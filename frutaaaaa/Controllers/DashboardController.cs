using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

                // Apply filters (same as before)
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

                // Calculate totals (same as before)
                var totalPdsfru = await palBrutQuery.SumAsync(p => p.pdsfru ?? 0);
                var totalPdscom = await paletteDQuery.SumAsync(x => x.PaletteD.pdscom ?? 0);
                var totalEcart = await ecartQuery.SumAsync(e => e.pdsfru);
                double exportPercentage = (totalPdsfru > 0) ? ((double)totalPdscom / totalPdsfru) * 100 : 0;
                double ecartPercentage = (totalPdsfru > 0) ? (totalEcart / totalPdsfru) * 100 : 0;

                // Keep full data with dates for calculations
                var palBrutList = await palBrutQuery.Select(p => new { p.refver, p.codvar, p.pdsfru, p.dterec }).ToListAsync();
                var paletteDList = await paletteDQuery.Select(p => new { p.PaletteD.refver, p.PaletteD.codvar, p.PaletteD.pdscom, p.Palette.dtepal }).ToListAsync();
                var ecartList = await ecartQuery.Select(e => new { e.refver, e.codvar, e.pdsfru, e.dtepal }).ToListAsync();

                var vergers = await _context.Vergers.ToDictionaryAsync(v => v.refver, v => v.nomver ?? "N/A");
                var varietes = await _context.Varietes.ToDictionaryAsync(v => v.codvar, v => v.nomvar ?? "N/A");

                // Build chart data (same as before)
                var receptionChartData = palBrutList.GroupBy(p => p.refver)
                    .Select(g => new ChartDataDto
                    {
                        RefVer = g.Key.ToString(),
                        Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})",
                        Value = (decimal)g.Sum(p => p.pdsfru ?? 0)
                    })
                    .OrderByDescending(x => x.Value).ToList();

                var exportChartData = paletteDList.GroupBy(x => x.refver)
                    .Select(g => new ChartDataDto
                    {
                        RefVer = g.Key.ToString(),
                        Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})",
                        Value = g.Sum(x => x.pdscom ?? 0)
                    })
                    .OrderByDescending(x => x.Value).ToList();

                // Build the table data dictionary (same logic)
                var allData = new Dictionary<(int?, int?), DashboardTableRowDto>();

                // Fill from receptions
                foreach (var group in palBrutList.GroupBy(p => new { p.refver, p.codvar }))
                {
                    var key = (group.Key.refver, group.Key.codvar);
                    if (!allData.ContainsKey(key))
                    {
                        allData[key] = new DashboardTableRowDto
                        {
                            VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A",
                            VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A"
                        };
                    }
                    allData[key].TotalPdsfru = group.Sum(p => p.pdsfru ?? 0);
                }

                // Fill from exports  
                foreach (var group in paletteDList.GroupBy(p => new { p.refver, p.codvar }))
                {
                    var key = (group.Key.refver, group.Key.codvar);
                    if (!allData.ContainsKey(key))
                    {
                        allData[key] = new DashboardTableRowDto
                        {
                            VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A",
                            VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A"
                        };
                    }
                    allData[key].TotalPdscom = group.Sum(p => p.pdscom ?? 0);
                }

                // Fill from ecarts
                foreach (var group in ecartList.GroupBy(p => new { p.refver, p.codvar }))
                {
                    var key = (group.Key.refver, (int?)group.Key.codvar);
                    if (!allData.ContainsKey(key))
                    {
                        allData[key] = new DashboardTableRowDto
                        {
                            VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A",
                            VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A"
                        };
                    }
                    allData[key].TotalEcart = group.Sum(p => p.pdsfru);
                }

                // # IMRPOVED: Optimized date calculations using the in-memory data
                foreach (var kvp in allData)
                {
                    var key = kvp.Key;
                    var rowDto = kvp.Value;
                    var vergerIdKey = key.Item1;
                    var varieteIdKey = key.Item2;

                    // Use already loaded data instead of extra DB queries
                    var minReceptionDate = palBrutList
                        .Where(p => p.refver == vergerIdKey && p.codvar == varieteIdKey && p.dterec != null)
                        .Select(p => (DateTime?)p.dterec)
                        .Min();

                    var maxExportDate = paletteDList
                        .Where(p => p.refver == vergerIdKey && p.codvar == varieteIdKey && p.dtepal != null)
                        .Select(p => (DateTime?)p.dtepal)
                        .Max();

                    rowDto.MinReceptionDate = minReceptionDate;
                    rowDto.MaxExportDate = maxExportDate;
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
                    .Select(g => new
                    {
                        RefVer = g.Key.refver,
                        VergerName = g.Key.nomver,
                        VarieteName = g.Key.nomvar,
                        TotalPdscom = g.Sum(item => item.pd.pdscom ?? 0)
                    })
                    .ToListAsync();

                var groupedData = flatData
                    .GroupBy(x => new { x.RefVer, x.VergerName })
                    .Select(g => new
                    {
                        GroupKey = g.Key,
                        Items = g.ToList(),
                        TotalPdscomForVerger = g.Sum(item => item.TotalPdscom)
                    })
                    .OrderByDescending(x => x.TotalPdscomForVerger)
                    .Select(g =>
                    {
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
                    .GroupBy(x => new
                    {
                        DestinationName = x.dest.vildes,
                        VarieteName = x.va.nomvar
                    })
                    .Select(g => new
                    {
                        g.Key.DestinationName,
                        g.Key.VarieteName,
                        TotalWeight = g.Sum(item => item.pd.pdscom ?? 0)
                    })
                    .Where(x => x.TotalWeight > 0)
                    .ToListAsync();

                var groupedData = flatData
                    .GroupBy(x => x.DestinationName)
                    .Select(g => new
                    {
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
                groupedData = data.GroupBy(d =>
                {
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
                groupedData = data.GroupBy(d =>
                {
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
                .Select(g =>
                {
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
      [FromQuery] int? grpVarId,
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

                if (grpVarId.HasValue)
                {
                    var varietiesInGroup = await _context.Varietes
                        .Where(v => v.codgrv == grpVarId.Value)
                        .Select(v => v.codvar)
                        .ToListAsync();
                    query = query.Where(x => varietiesInGroup.Contains(x.e.codvar));
                }

                if (varieteId.HasValue)
                {
                    query = query.Where(x => x.e.codvar == varieteId.Value);
                }
                if (ecartTypeId.HasValue)
                {
                    query = query.Where(x => x.d.codtype == ecartTypeId.Value);
                }

                // ===== SAVE RAW DATA FOR DATE CALCULATIONS =====
                var rawQueryData = await query
                    .Select(x => new { x.e.refver, x.e.codvar, x.e.dtepal, x.d.codtype })
                    .ToListAsync();

                var result = await (
                    from q in query
                    join v in _context.Varietes on q.e.codvar equals v.codvar
                    join t in _context.TypeEcarts on q.d.codtype equals t.codtype
                    join g in _context.Vergers on q.e.refver equals g.refver into vergerJoin
                    from g in vergerJoin.DefaultIfEmpty()
                    group new { q.d, q.e } by new { VergerName = g.nomver, v.nomvar, EcartType = t.destype, VergerId = g.refver, VarieteId = v.codvar, EcartTypeId = t.codtype } into grouped
                    select new EcartDetailsDto
                    {
                        VergerName = grouped.Key.VergerName ?? "N/A",
                        VarieteName = grouped.Key.nomvar,
                        EcartType = grouped.Key.EcartType,
                        TotalPdsfru = grouped.Sum(x => x.d.pdsfru),
                        TotalNbrcai = grouped.Sum(x => x.d.nbrcai),
                        // ===== NEW PROPERTIES =====
                        MinEcartDate = null, // Will be set below
                        MaxEcartDate = null  // Will be set below
                    }
                ).ToListAsync();

                // ===== DATE CALCULATIONS FOR EACH ECART ROW =====
                foreach (var ecartRow in result)
                {
                    // Find min/max ecart dates for this specific combination
                    var ecartDates = rawQueryData
                        .Where(r => r.refver == ecartRow.VergerId &&
                                   r.codvar == ecartRow.VarieteId &&
                                   r.codtype == ecartRow.EcartTypeId) // You'll need to map EcartType back to ID
                        .Select(r => r.dtepal)
                        .Where(d => d != null)
                        .ToList();

                    ecartRow.MinEcartDate = ecartDates.Any() ? (DateTime?)ecartDates.Min() : null;
                    ecartRow.MaxEcartDate = ecartDates.Any() ? (DateTime?)ecartDates.Max() : null;
                }

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
    [HttpGet("data-grouped-by-variety-group")]
    public async Task<ActionResult<object>> GetDashboardDataGroupedByVarietyGroup(
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

                // preload mapping codvar -> codgrv (codvar assumed non-null in Varietes table)
                var varietyToGroupList = await _context.Varietes
                    .Select(v => new { CodVar = v.codvar, CodGrv = v.codgrv })
                    .ToListAsync();
                var varietyToGroupDict = varietyToGroupList.ToDictionary(x => (int?)x.CodVar, x => (int?)x.CodGrv);

                // build base queries
                var receptionQuery = _context.palbruts
                    .Where(p => p.etat == "R" && p.dterec >= startDate.Date && p.dterec <= endDateInclusive)
                    .Join(_context.Varietes, p => p.codvar, v => v.codvar, (p, v) => new { p, v });

                var exportQuery = _context.Palette_ds
                    .Join(_context.Palettes, pd => pd.numpal, p => p.numpal, (pd, p) => new { pd, p })
                    .Where(x => x.p.dtepal >= startDate.Date && x.p.dtepal <= endDateInclusive)
                    .Join(_context.Varietes, x => x.pd.codvar, v => v.codvar, (x, v) => new { x.pd, x.p, v });

                if (vergerId.HasValue)
                {
                    receptionQuery = receptionQuery.Where(x => x.p.refver == vergerId.Value);
                    exportQuery = exportQuery.Where(x => x.pd.refver == vergerId.Value);
                }

                IQueryable<int> varietiesInGroupQuery = null;
                if (grpVarId.HasValue)
                {
                    varietiesInGroupQuery = _context.Varietes.Where(v => v.codgrv == grpVarId.Value).Select(v => v.codvar);
                }

                if (varieteId.HasValue)
                {
                    receptionQuery = receptionQuery.Where(x => x.p.codvar == varieteId.Value);
                    exportQuery = exportQuery.Where(x => x.pd.codvar == varieteId.Value);
                }
                else if (grpVarId.HasValue && varietiesInGroupQuery != null)
                {
                    receptionQuery = receptionQuery.Where(x => x.p.codvar != null && varietiesInGroupQuery.Contains(x.p.codvar.Value));
                    exportQuery = exportQuery.Where(x => x.pd.codvar != null && varietiesInGroupQuery.Contains(x.pd.codvar.Value));
                }

                // SNAPSHOTS: raw date rows
                var rawReceptionData = await receptionQuery
                    .Select(x => new { RefVer = (int?)x.p.refver, CodVar = (int?)x.v.codvar, DteRec = (DateTime?)x.p.dterec })
                    .ToListAsync();

                var rawExportData = await exportQuery
                    .Select(x => new { RefVer = (int?)x.pd.refver, CodVar = (int?)x.v.codvar, DtePal = (DateTime?)x.p.dtepal })
                    .ToListAsync();

                // aggregated totals grouped by refver + codgrv
                var receptionData = receptionQuery.Select(x => new { RefVer = (int?)x.p.refver, CodGrv = (int?)x.v.codgrv, Pdsfru = (decimal?)x.p.pdsfru ?? 0m, Pdscom = 0m, Ecart = 0m });
                var exportData = exportQuery.Select(x => new { RefVer = (int?)x.pd.refver, CodGrv = (int?)x.v.codgrv, Pdsfru = 0m, Pdscom = (decimal?)x.pd.pdscom ?? 0m, Ecart = 0m });
                var allData = receptionData.Concat(exportData);

                var aggregated = await allData
                    .GroupBy(x => new { x.RefVer, x.CodGrv })
                    .Select(g => new
                    {
                        VergerId = g.Key.RefVer,
                        GroupId = g.Key.CodGrv,
                        TotalPdsfru = (double)g.Sum(x => x.Pdsfru),
                        TotalPdscom = g.Sum(x => x.Pdscom),
                        TotalEcart = g.Sum(x => x.Ecart)
                    })
                    .ToListAsync();

                // load verger & group names for found ids
                var vergerIds = aggregated.Select(a => a.VergerId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
                var groupIds = aggregated.Select(a => a.GroupId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();

                var vergers = await _context.Vergers.Where(v => vergerIds.Contains(v.refver)).ToDictionaryAsync(v => v.refver, v => v.nomver);
                var groups = await _context.grpvars.Where(g => groupIds.Contains(g.codgrv)).ToDictionaryAsync(g => g.codgrv, g => g.nomgrv);

                var rows = aggregated.Select(a => new GroupVarietyTableRowDto
                {
                    VergerName = a.VergerId.HasValue && vergers.TryGetValue(a.VergerId.Value, out var vn) ? vn : "N/A",
                    GroupVarieteName = a.GroupId.HasValue && groups.TryGetValue(a.GroupId.Value, out var gn) ? gn : "N/A",
                    TotalPdsfru = a.TotalPdsfru,
                    TotalPdscom = a.TotalPdscom,
                    TotalEcart = a.TotalEcart,
                    VergerId = a.VergerId,
                    GroupId = a.GroupId
                }).ToList();

                // build lookups (keys are (int? VergerId, int? GroupId))
                var minReceptionLookup = rawReceptionData
                    .Where(r => r.RefVer.HasValue && r.CodVar.HasValue && varietyToGroupDict.ContainsKey(r.CodVar))
                    .GroupBy(r => new { RefVer = r.RefVer, CodGrv = varietyToGroupDict[r.CodVar] })
                    .ToDictionary(g => (g.Key.RefVer, g.Key.CodGrv), g => g.Min(x => x.DteRec));

                var maxExportLookup = rawExportData
                    .Where(r => r.RefVer.HasValue && r.CodVar.HasValue && varietyToGroupDict.ContainsKey(r.CodVar))
                    .GroupBy(r => new { RefVer = r.RefVer, CodGrv = varietyToGroupDict[r.CodVar] })
                    .ToDictionary(g => (g.Key.RefVer, g.Key.CodGrv), g => g.Max(x => x.DtePal));

                // assign dates
                foreach (var row in rows)
                {
                    if (minReceptionLookup.TryGetValue((row.VergerId, row.GroupId), out var minRec))
                        row.MinReceptionDate = minRec;
                    if (maxExportLookup.TryGetValue((row.VergerId, row.GroupId), out var maxExp))
                        row.MaxExportDate = maxExp;
                }

                return Ok(new GroupedDashboardDataDto { TableRows = rows });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message} -> {ex.InnerException?.Message}");
        }
    }
    [HttpGet("ecart-details-grouped")]
    public async Task<ActionResult<EcartGroupDetailsResponseDto>> GetEcartDetailsGrouped(
     [FromHeader(Name = "X-Database-Name")] string database,
     [FromQuery] DateTime startDate,
     [FromQuery] DateTime endDate,
     [FromQuery] int? vergerId,
     [FromQuery] int? grpVarId,
     [FromQuery] int? varieteId,
     [FromQuery] int? ecartTypeId)
    {
        try
        {
            using (var _context = CreateDbContext(database))
            {
                var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);

                // ----------------------------
                // 1) Preload variety -> group (int -> int)
                // ----------------------------
                var varietyToGroupList = await _context.Varietes
                    .Select(v => new { CodVar = v.codvar, CodGrv = v.codgrv })
                    .ToListAsync();

                var varietyToGroupDict = varietyToGroupList.ToDictionary(x => x.CodVar, x => x.CodGrv);
                // Optional debug (remove if no logger)
                // _logger?.LogDebug("Variety->Group count: {count}", varietyToGroupDict.Count);

                // ----------------------------
                // 2) Build base filtered query
                // ----------------------------
                var baseQuery = from d in _context.EcartDs
                                join e in _context.EcartEs on d.numpal equals e.numpal
                                where e.dtepal >= startDate.Date && e.dtepal <= endDateInclusive
                                select new { d, e };

                if (vergerId.HasValue) baseQuery = baseQuery.Where(x => x.e.refver == vergerId.Value);

                if (grpVarId.HasValue)
                {
                    var varInGroup = _context.Varietes.Where(v => v.codgrv == grpVarId.Value).Select(v => v.codvar);
                    baseQuery = baseQuery.Where(x => varInGroup.Contains(x.e.codvar));
                }

                if (varieteId.HasValue) baseQuery = baseQuery.Where(x => x.e.codvar == varieteId.Value);
                if (ecartTypeId.HasValue) baseQuery = baseQuery.Where(x => x.d.codtype == ecartTypeId.Value);

                // ----------------------------
                // 3) Snapshot raw data for date calculations (use non-nullable ints)
                // ----------------------------
                var rawGroupedData = await baseQuery
                    .Select(x => new
                    {
                        RefVer = x.e.refver,   // int
                        CodVar = x.e.codvar,   // int
                        DtePal = x.e.dtepal,   // DateTime
                        CodType = x.d.codtype  // int
                    })
                    .ToListAsync();

                // Quick debug counts (optional logger)
                // _logger?.LogDebug("rawGroupedData rows: {n}", rawGroupedData.Count);

                // ----------------------------
                // 4) Aggregated grouping for DTOs (use inner joins to ensure keys are present)
                // ----------------------------
                var groupedAgg = await (
                    from q in baseQuery
                    join v in _context.Varietes on q.e.codvar equals v.codvar
                    join g in _context.Vergers on q.e.refver equals g.refver
                    join grp in _context.grpvars on v.codgrv equals grp.codgrv
                    join t in _context.TypeEcarts on q.d.codtype equals t.codtype
                    group new { q.d, q.e, v, grp, g, t } by new
                    {
                        VergerName = g.nomver,
                        GroupName = grp.nomgrv,
                        GroupId = grp.codgrv,
                        EcartType = t.destype,
                        VergerId = g.refver,
                        EcartTypeId = t.codtype
                    } into gg
                    select new EcartGroupDetailsDto
                    {
                        VergerName = gg.Key.VergerName ?? "N/A",
                        GroupVarieteName = gg.Key.GroupName,
                        GroupId = gg.Key.GroupId,
                        EcartType = gg.Key.EcartType,
                        VergerId = gg.Key.VergerId,
                        EcartTypeId = gg.Key.EcartTypeId,
                        TotalPdsfru = (double)gg.Sum(x => x.d.pdsfru),
                        TotalNbrcai = gg.Sum(x => x.d.nbrcai),
                        MinEcartDate = null,
                        MaxEcartDate = null
                    })
                    .OrderBy(x => x.VergerName)
                    .ThenBy(x => x.GroupVarieteName)
                    .ToListAsync();

                // Debug: how many grouped rows?
                // _logger?.LogDebug("groupedAgg rows: {n}", groupedAgg.Count);

                // ----------------------------
                // 5) Build dictionary keyed by (refver:int, codgrv:int, codtype:int) => (Min,Max)
                // ----------------------------
                var keyedDateGroups = rawGroupedData
                  .Where(r => r.CodVar != null && varietyToGroupDict.ContainsKey(r.CodVar))
                  .GroupBy(r => new
                  {
                      RefVer = (int?)r.RefVer,                           // cast en int?
                      CodGrv = (int?)varietyToGroupDict[r.CodVar],       // cast en int?
                      CodType = (int?)r.CodType
                  })
                  .ToDictionary(
                      g => (g.Key.RefVer, g.Key.CodGrv, g.Key.CodType),  // key type is (int?, int?, int?)
                      g => new
                      {
                          Min = g.Min(x => x.DtePal),
                          Max = g.Max(x => x.DtePal)
                      });

                // Optional debug: sample some keys
                // _logger?.LogDebug("keyedDateGroups count: {n}", keyedDateGroups.Count);

                // ----------------------------
                // 6) Assign Min/Max to DTOs using matching int tuple keys
                // ----------------------------
                foreach (var dto in groupedAgg)
                {
                    // Here dto.VergerId / dto.GroupId / dto.EcartTypeId are int (non-nullable)
                    var key = (dto.VergerId, dto.GroupId, dto.EcartTypeId);

                    if (keyedDateGroups.TryGetValue(key, out var minmax))
                    {
                        dto.MinEcartDate = minmax.Min;
                        dto.MaxEcartDate = minmax.Max;
                    }
                    else
                    {
                        // Optional debug to catch mismatches
                        // _logger?.LogDebug("No date match for key: VergerId={v}, GroupId={g}, EcartTypeId={t}",
                        //     dto.VergerId, dto.GroupId, dto.EcartTypeId);
                    }
                }

                var total = groupedAgg.Sum(r => r.TotalPdsfru);

                var response = new EcartGroupDetailsResponseDto
                {
                    Data = groupedAgg,
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


