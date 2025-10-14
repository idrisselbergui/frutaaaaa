using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DashboardController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [NonAction]
    public ApplicationDbContext CreateDbContext(string dbName)
    {
        var baseConnectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(baseConnectionString))
        {
            throw new ArgumentException("Database name or connection string is missing.");
        }
        var dynamicConnectionString = baseConnectionString.Replace("frutaaaaa_db", dbName);
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(dynamicConnectionString, ServerVersion.AutoDetect(dynamicConnectionString));
        return new ApplicationDbContext(optionsBuilder.Options);
    }

    [HttpGet("data")]
    public async Task<ActionResult<DashboardDataDto>> GetDashboardData(
        [FromHeader(Name = "X-Database-Name")] string database,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? vergerId,
        [FromQuery] int? grpVarId,
        [FromQuery] int? varieteId)
    {
        try
        {
            using (var _context = CreateDbContext(database))
            {
                var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);

                var palBrutQuery = _context.palbruts.AsQueryable()
                    .Where(p => p.etat == "R")
                    .Where(p => p.dterec >= startDate.Date && p.dterec <= endDateInclusive);

                var paletteDQuery = _context.Palette_ds
                    .Join(_context.Palettes, pd => pd.numpal, p => p.numpal, (pd, p) => new { PaletteD = pd, Palette = p })
                    .Where(x => x.Palette.dtepal >= startDate.Date && x.Palette.dtepal <= endDateInclusive);

                var ecartQuery = _context.EcartEs.AsQueryable()
                    .Where(e => e.dtepal <= endDateInclusive);

                if (vergerId.HasValue)
                {
                    palBrutQuery = palBrutQuery.Where(p => p.refver == vergerId.Value);
                    paletteDQuery = paletteDQuery.Where(x => x.PaletteD.refver == vergerId.Value);
                    ecartQuery = ecartQuery.Where(e => e.refver == vergerId.Value);
                }
                if (grpVarId.HasValue)
                {
                    var varietiesInGroup = await _context.Varietes
                        .Where(v => v.codgrv == grpVarId.Value)
                        .Select(v => v.codvar)
                        .ToListAsync();

                    palBrutQuery = palBrutQuery.Where(p => p.codvar.HasValue && varietiesInGroup.Contains(p.codvar.Value));
                    paletteDQuery = paletteDQuery.Where(x => x.PaletteD.codvar.HasValue && varietiesInGroup.Contains(x.PaletteD.codvar.Value));
                    ecartQuery = ecartQuery.Where(e => varietiesInGroup.Contains(e.codvar));
                }
                if (varieteId.HasValue)
                {
                    palBrutQuery = palBrutQuery.Where(p => p.codvar == varieteId.Value);
                    paletteDQuery = paletteDQuery.Where(x => x.PaletteD.codvar == varieteId.Value);
                    ecartQuery = ecartQuery.Where(e => e.codvar == varieteId.Value);
                }

                var totalPdsfru = await palBrutQuery.SumAsync(p => p.pdsfru ?? 0);
                var totalPdscom = await paletteDQuery.SumAsync(x => x.PaletteD.pdscom ?? 0);
                var totalEcart = await ecartQuery.SumAsync(e => e.pdsfru);
                double exportPercentage = (totalPdsfru > 0) ? ((double)totalPdscom / totalPdsfru) * 100 : 0;
                double ecartPercentage = (totalPdsfru > 0) ? (totalEcart / totalPdsfru) * 100 : 0;

                var palBrutList = await palBrutQuery.Select(p => new { p.refver, p.codvar, p.pdsfru }).ToListAsync();
                var paletteDList = await paletteDQuery.Select(p => new { p.PaletteD.refver, p.PaletteD.codvar, p.PaletteD.pdscom }).ToListAsync();
                var ecartList = await ecartQuery.Select(e => new { e.refver, e.codvar, e.pdsfru }).ToListAsync();
                var vergers = await _context.Vergers.ToDictionaryAsync(v => v.refver, v => v.nomver ?? "N/A");
                var varietes = await _context.Varietes.ToDictionaryAsync(v => v.codvar, v => v.nomvar ?? "N/A");

                var receptionChartData = palBrutList.GroupBy(p => p.refver).Select(g => new ChartDataDto { RefVer = g.Key.ToString(), Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})", Value = (decimal)g.Sum(p => p.pdsfru ?? 0) }).OrderByDescending(x => x.Value).ToList();
                var exportChartData = paletteDList.GroupBy(x => x.refver).Select(g => new ChartDataDto { RefVer = g.Key.ToString(), Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})", Value = g.Sum(x => x.pdscom ?? 0) }).OrderByDescending(x => x.Value).ToList();

                var allData = new Dictionary<(int?, int?), DashboardTableRowDto>();

                foreach (var group in palBrutList.GroupBy(p => new { p.refver, p.codvar }))
                {
                    var key = (group.Key.refver, group.Key.codvar);
                    if (!allData.ContainsKey(key))
                    {
                        allData[key] = new DashboardTableRowDto { VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A", VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A" };
                    }
                    allData[key].TotalPdsfru = group.Sum(p => p.pdsfru ?? 0);
                }
                foreach (var group in paletteDList.GroupBy(p => new { p.refver, p.codvar }))
                {
                    var key = (group.Key.refver, group.Key.codvar);
                    if (!allData.ContainsKey(key))
                    {
                        allData[key] = new DashboardTableRowDto { VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A", VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A" };
                    }
                    allData[key].TotalPdscom = group.Sum(p => p.pdscom ?? 0);
                }
                foreach (var group in ecartList.GroupBy(p => new { p.refver, p.codvar }))
                {
                    var key = (group.Key.refver, (int?)group.Key.codvar);
                    if (!allData.ContainsKey(key))
                    {
                        allData[key] = new DashboardTableRowDto { VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A", VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A" };
                    }
                    allData[key].TotalEcart = group.Sum(p => p.pdsfru);
                }

                var result = new DashboardDataDto
                {
                    TotalPdsfru = totalPdsfru,
                    TotalPdscom = totalPdscom,
                    ExportPercentage = exportPercentage,
                    TotalEcart = totalEcart,
                    EcartPercentage = ecartPercentage,
                    TableRows = allData.Values.ToList(),
                    ReceptionByVergerChart = receptionChartData,
                    ExportByVergerChart = exportChartData
                };
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("destination-chart")]
    public async Task<ActionResult<object>> GetDestinationChartData(
        [FromHeader(Name = "X-Database-Name")] string database,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? vergerId,
        [FromQuery] int? grpVarId, // --- ADD THIS ---
        [FromQuery] int? varieteId,
        [FromQuery] int? destinationId)
    {
        try
        {
            using (var _context = CreateDbContext(database))
            {
                var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);

                var query = from pd in _context.Palette_ds
                            join p in _context.Palettes on pd.numpal equals p.numpal
                            join b in _context.Bdqs on pd.numbdq equals b.numbdq
                            join d in _context.Dossiers on b.numdos equals d.numdos
                            join v in _context.Vergers on pd.refver equals v.refver
                            join va in _context.Varietes on pd.codvar equals va.codvar
                            where p.dtepal >= startDate.Date && p.dtepal <= endDateInclusive
                            select new { pd, p, d, v, va };

                if (vergerId.HasValue) query = query.Where(x => x.pd.refver == vergerId.Value);
                if (destinationId.HasValue) query = query.Where(x => x.d.coddes == destinationId.Value);

                // --- ADD THIS BLOCK ---
                if (grpVarId.HasValue)
                {
                    var varietiesInGroup = await _context.Varietes
                        .Where(v => v.codgrv == grpVarId.Value)
                        .Select(v => v.codvar)
                        .ToListAsync();
                    query = query.Where(x => x.pd.codvar.HasValue && varietiesInGroup.Contains(x.pd.codvar.Value));
                }
                // --- END BLOCK ---

                if (varieteId.HasValue) query = query.Where(x => x.pd.codvar == varieteId.Value);

                var flatData = await query
                    .GroupBy(x => new { x.v.refver, x.v.nomver, x.va.nomvar })
                    .Select(g => new {
                        RefVer = g.Key.refver,
                        VergerName = g.Key.nomver,
                        VarieteName = g.Key.nomvar,
                        TotalPdscom = g.Sum(item => item.pd.pdscom ?? 0)
                    })
                    .ToListAsync();

                var groupedData = flatData
                    .GroupBy(x => new { x.RefVer, x.VergerName })
                    .Select(g => new {
                        GroupKey = g.Key,
                        Items = g.ToList(),
                        TotalPdscomForVerger = g.Sum(item => item.TotalPdscom)
                    })
                    .OrderByDescending(x => x.TotalPdscomForVerger)
                    .Select(g => {
                        var resultObject = new Dictionary<string, object>
                        {
                            ["refver"] = g.GroupKey.RefVer,
                            ["name"] = g.GroupKey.VergerName
                        };
                        foreach (var item in g.Items)
                        {
                            resultObject[item.VarieteName] = item.TotalPdscom;
                        }
                        return resultObject;
                    })
                    .ToList();

                var allVarietes = flatData.Select(x => x.VarieteName).Distinct().ToList();
                return Ok(new { data = groupedData, keys = allVarietes });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("destination-by-variety-chart")]
    public async Task<ActionResult<object>> GetDestinationByVarietyChart(
        [FromHeader(Name = "X-Database-Name")] string database,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int vergerId,
        [FromQuery] int? grpVarId, // --- ADD THIS ---
        [FromQuery] int? varieteId)
    {
        try
        {
            using (var _context = CreateDbContext(database))
            {
                var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);

                var query = from pd in _context.Palette_ds
                            join p in _context.Palettes on pd.numpal equals p.numpal
                            join b in _context.Bdqs on pd.numbdq equals b.numbdq
                            join d in _context.Dossiers on b.numdos equals d.numdos
                            join dest in _context.Destinations on d.coddes equals dest.coddes
                            join va in _context.Varietes on pd.codvar equals va.codvar
                            where p.dtepal >= startDate.Date && p.dtepal <= endDateInclusive
                            && pd.refver == vergerId
                            select new { pd, dest, va };

                // --- ADD THIS BLOCK ---
                if (grpVarId.HasValue)
                {
                    var varietiesInGroup = await _context.Varietes
                        .Where(v => v.codgrv == grpVarId.Value)
                        .Select(v => v.codvar)
                        .ToListAsync();
                    query = query.Where(x => x.pd.codvar.HasValue && varietiesInGroup.Contains(x.pd.codvar.Value));
                }
                // --- END BLOCK ---

                if (varieteId.HasValue)
                {
                    query = query.Where(x => x.pd.codvar == varieteId.Value);
                }

                var flatData = await query
                    .GroupBy(x => new {
                        DestinationName = x.dest.vildes,
                        VarieteName = x.va.nomvar
                    })
                    .Select(g => new {
                        g.Key.DestinationName,
                        g.Key.VarieteName,
                        TotalWeight = g.Sum(item => item.pd.pdscom ?? 0)
                    })
                    .Where(x => x.TotalWeight > 0)
                    .ToListAsync();

                var groupedData = flatData
                    .GroupBy(x => x.DestinationName)
                    .Select(g => new {
                        DestinationName = g.Key,
                        Items = g.ToList(),
                        TotalWeightForDestination = g.Sum(item => item.TotalWeight)
                    })
                    .OrderByDescending(x => x.TotalWeightForDestination)
                    .Select(g =>
                    {
                        var resultObject = new Dictionary<string, object> { ["name"] = g.DestinationName };
                        foreach (var item in g.Items)
                        {
                            resultObject[item.VarieteName] = item.TotalWeight;
                        }
                        return resultObject;
                    })
                    .ToList();

                var allVarietes = flatData.Select(x => x.VarieteName).Distinct().ToList();
                return Ok(new { data = groupedData, keys = allVarietes });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("periodic-trends")]
    public async Task<ActionResult<object>> GetPeriodicTrends(
    [FromHeader(Name = "X-Database-Name")] string database,
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] string chartType,
    [FromQuery] string timePeriod,
    [FromQuery] int vergerId,
    [FromQuery] int? grpVarId,
    [FromQuery] int? varieteId)
    {
        try
        {
            using (var _context = CreateDbContext(database))
            {
                var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);
                var trendsData = new List<Dictionary<string, object>>();

                if (chartType.ToLower() == "reception")
                {
                    var receptionQuery = _context.palbruts.AsQueryable()
                        .Where(pb => pb.etat == "R"
                            && pb.dterec >= startDate.Date
                            && pb.dterec <= endDateInclusive
                            && pb.refver == vergerId);

                    // Apply variety group filter
                    if (grpVarId.HasValue)
                    {
                        var varietiesInGroup = await _context.Varietes
                            .Where(v => v.codgrv == grpVarId.Value)
                            .Select(v => v.codvar)
                            .ToListAsync();
                        receptionQuery = receptionQuery.Where(pb => pb.codvar.HasValue && varietiesInGroup.Contains(pb.codvar.Value));
                    }

                    // Apply variety filter
                    if (varieteId.HasValue)
                    {
                        receptionQuery = receptionQuery.Where(pb => pb.codvar == varieteId.Value);
                    }

                    var data = await receptionQuery
                        .Select(pb => new { Date = pb.dterec, Value = pb.pdsfru ?? 0 })
                        .ToListAsync();

                    trendsData = GroupByTimePeriod(data, timePeriod, startDate, endDate);
                }
                else if (chartType.ToLower() == "export")
                {
                    var exportQuery = _context.Palette_ds.AsQueryable()
                        .Join(_context.Palettes,
                              pd => pd.numpal,
                              p => p.numpal,
                              (pd, p) => new { PaletteD = pd, Palette = p })
                        .Where(x => x.Palette.dtepal >= startDate.Date
                            && x.Palette.dtepal <= endDateInclusive
                            && x.PaletteD.refver == vergerId);

                    // Apply variety group filter
                    if (grpVarId.HasValue)
                    {
                        var varietiesInGroup = await _context.Varietes
                            .Where(v => v.codgrv == grpVarId.Value)
                            .Select(v => v.codvar)
                            .ToListAsync();
                        exportQuery = exportQuery.Where(x => x.PaletteD.codvar.HasValue && varietiesInGroup.Contains(x.PaletteD.codvar.Value));
                    }

                    // Apply variety filter
                    if (varieteId.HasValue)
                    {
                        exportQuery = exportQuery.Where(x => x.PaletteD.codvar == varieteId.Value);
                    }

                    var data = await exportQuery
                        .Select(x => new { Date = x.Palette.dtepal, Value = x.PaletteD.pdscom ?? 0 })
                        .ToListAsync();

                    trendsData = GroupByTimePeriod(data, timePeriod, startDate, endDate);
                }
                else if (chartType.ToLower() == "ecart")
                {
                    var ecartQuery = _context.EcartEs.AsQueryable()
                        .Where(e => e.dtepal >= startDate.Date
                            && e.dtepal <= endDateInclusive
                            && e.refver == vergerId);

                    // Apply variety group filter
                    if (grpVarId.HasValue)
                    {
                        var varietiesInGroup = await _context.Varietes
                            .Where(v => v.codgrv == grpVarId.Value)
                            .Select(v => v.codvar)
                            .ToListAsync();
                        ecartQuery = ecartQuery.Where(e => varietiesInGroup.Contains(e.codvar));
                    }

                    // Apply variety filter
                    if (varieteId.HasValue)
                    {
                        ecartQuery = ecartQuery.Where(e => e.codvar == varieteId.Value);
                    }

                    var data = await ecartQuery
                        .Select(e => new { Date = e.dtepal, Value = e.pdsfru })
                        .ToListAsync();

                    trendsData = GroupByTimePeriod(data, timePeriod, startDate, endDate);
                }

                return Ok(new { trends = trendsData, chartType, timePeriod });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }



    [NonAction]
    private List<Dictionary<string, object>> GroupByTimePeriod(
    IEnumerable<dynamic> data,
    string timePeriod,
    DateTime startDate,
    DateTime endDate)
    {
        var groupedData = new List<Dictionary<string, object>>();

        switch (timePeriod.ToLower())
        {

            // Fix the date format for JavaScript compatibility
            case "daily":
                groupedData = data.GroupBy(d => d.Date.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new Dictionary<string, object>
                    {
                        ["date"] = g.Key.ToString("yyyy-MM-dd"), // Always ISO format for JS
                        ["value"] = g.Sum(d => (decimal)d.Value),
                        ["label"] = g.Key.ToString("dd/MM/yyyy") // Always include year
                    }).ToList();
                break;


            case "monthly":
                groupedData = data.GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new Dictionary<string, object>
                    {
                        ["date"] = g.Key.ToString("yyyy-MM-01"), // First day of month
                        ["value"] = g.Sum(d => (decimal)d.Value),
                        ["label"] = g.Key.ToString("MM/yyyy")
                    }).ToList();
                break;

            case "weekly":
                groupedData = data.GroupBy(d => {
                    var date = d.Date.Date;
                    int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
                    return date.AddDays(-1 * diff);
                })
                .OrderBy(g => g.Key)
                .Select(g => new Dictionary<string, object>
                {
                    ["date"] = g.Key.ToString("yyyy-MM-dd"),
                    ["value"] = g.Sum(d => (decimal)d.Value),
                    ["label"] = $"Week of {g.Key.ToString("dd/MM/yyyy")}"
                }).ToList();
                break;

            case "biweekly":
                groupedData = data.GroupBy(d => {
                    var date = d.Date.Date;
                    var dayOfMonth = date.Day;

                    // First half: 1st-15th, Second half: 16th-End of month
                    if (dayOfMonth <= 15)
                    {
                        // First half of month: 1st-15th
                        return new DateTime(date.Year, date.Month, 1);
                    }
                    else
                    {
                        // Second half of month: 16th-End
                        return new DateTime(date.Year, date.Month, 16);
                    }
                })
                .OrderBy(g => g.Key)
                .Select(g => {
                    var firstDate = g.Key;
                    var isFirstHalf = firstDate.Day == 1;
                    var monthName = firstDate.ToString("MM/yyyy");

                    return new Dictionary<string, object>
                    {
                        ["date"] = firstDate.ToString("yyyy-MM-dd"),
                        ["value"] = g.Sum(d => (decimal)d.Value),
                        ["label"] = isFirstHalf ? $"1-15 {monthName}" : $"16-End {monthName}"
                    };
                }).ToList();
                break;


            

            case "yearly":
                groupedData = data.GroupBy(d => new DateTime(d.Date.Year, 1, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new Dictionary<string, object>
                    {
                        ["date"] = g.Key.ToString("yyyy-MM-dd"),
                        ["value"] = g.Sum(d => (decimal)d.Value),
                        ["label"] = g.Key.Year.ToString()
                    }).ToList();
                break;
        }

        return groupedData;
    }


    [HttpGet("ecart-details")]
    public async Task<ActionResult<EcartDetailsResponseDto>> GetEcartDetails(
        [FromHeader(Name = "X-Database-Name")] string database,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? vergerId,
        [FromQuery] int? grpVarId, // --- ADD THIS ---
        [FromQuery] int? varieteId,
        [FromQuery] int? ecartTypeId)
    {
        try
        {
            using (var _context = CreateDbContext(database))
            {
                var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);

                var query = from d in _context.EcartDs
                            join e in _context.EcartEs on d.numpal equals e.numpal
                            where e.dtepal >= startDate.Date && e.dtepal <= endDateInclusive
                            select new { d, e };

                if (vergerId.HasValue)
                {
                    query = query.Where(x => x.e.refver == vergerId.Value);
                }

                // --- ADD THIS BLOCK ---
                if (grpVarId.HasValue)
                {
                    var varietiesInGroup = await _context.Varietes
                        .Where(v => v.codgrv == grpVarId.Value)
                        .Select(v => v.codvar)
                        .ToListAsync();
                    query = query.Where(x => varietiesInGroup.Contains(x.e.codvar));
                }
                // --- END BLOCK ---

                if (varieteId.HasValue)
                {
                    query = query.Where(x => x.e.codvar == varieteId.Value);
                }
                if (ecartTypeId.HasValue)
                {
                    query = query.Where(x => x.d.codtype == ecartTypeId.Value);
                }

                var result = await (
                    from q in query
                    join v in _context.Varietes on q.e.codvar equals v.codvar
                    join t in _context.TypeEcarts on q.d.codtype equals t.codtype
                    join g in _context.Vergers on q.e.refver equals g.refver into vergerJoin
                    from g in vergerJoin.DefaultIfEmpty()
                    group new { q.d, q.e } by new { VergerName = g.nomver, v.nomvar, EcartType = t.destype } into grouped
                    select new EcartDetailsDto
                    {
                        VergerName = grouped.Key.VergerName ?? "N/A",
                        VarieteName = grouped.Key.nomvar,
                        EcartType = grouped.Key.EcartType,
                        TotalPdsfru = grouped.Sum(x => x.d.pdsfru),
                        TotalNbrcai = grouped.Sum(x => x.d.nbrcai)
                    }
                ).ToListAsync();

                var total = result.Sum(r => r.TotalPdsfru);

                var response = new EcartDetailsResponseDto
                {
                    Data = result,
                    TotalPdsfru = total
                };

                return Ok(response);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}

