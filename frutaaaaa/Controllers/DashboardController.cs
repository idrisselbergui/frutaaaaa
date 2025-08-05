using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // --- MÉTHODE POUR LES DONNÉES PRINCIPALES DU TABLEAU DE BORD (SANS FILTRE DESTINATION) ---
    [HttpGet("data")]
    public async Task<ActionResult<DashboardDataDto>> GetDashboardData(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? vergerId,
        [FromQuery] int? varieteId)
    {
        var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);

        // --- 1. Filtrer les requêtes de base ---
        var palBrutQuery = _context.palbruts.AsQueryable()
            .Where(p => p.etat == "R")
            .Where(p => p.dterec >= startDate.Date && p.dterec <= endDateInclusive);

        var paletteDQuery = _context.Palette_ds
            .Join(_context.Palettes, pd => pd.numpal, p => p.numpal, (pd, p) => new { PaletteD = pd, Palette = p })
            .Where(x => x.Palette.dtepal >= startDate.Date && x.Palette.dtepal <= endDateInclusive);

        var ecartQuery = _context.EcartEs.AsQueryable()
            .Where(e => e.dtepal <= endDateInclusive);

        // Appliquer les filtres optionnels
        if (vergerId.HasValue)
        {
            palBrutQuery = palBrutQuery.Where(p => p.refver == vergerId.Value);
            paletteDQuery = paletteDQuery.Where(x => x.PaletteD.refver == vergerId.Value);
            ecartQuery = ecartQuery.Where(e => e.refver == vergerId.Value);
        }
        if (varieteId.HasValue)
        {
            palBrutQuery = palBrutQuery.Where(p => p.codvar == varieteId.Value);
            paletteDQuery = paletteDQuery.Where(x => x.PaletteD.codvar == varieteId.Value);
            ecartQuery = ecartQuery.Where(e => e.codvar == varieteId.Value);
        }

        // --- 2. Calculer les KPIs ---
        var totalPdsfru = await palBrutQuery.SumAsync(p => p.pdsfru ?? 0);
        var totalPdscom = await paletteDQuery.SumAsync(x => x.PaletteD.pdscom ?? 0);
        var totalEcart = await ecartQuery.SumAsync(e => e.pdsfru);
        double exportPercentage = (totalPdsfru > 0) ? ((double)totalPdscom / totalPdsfru) * 100 : 0;
        double ecartPercentage = (totalPdsfru > 0) ? (totalEcart / totalPdsfru) * 100 : 0;

        // --- 3. Charger les données brutes en mémoire ---
        var palBrutList = await palBrutQuery.Select(p => new { p.refver, p.codvar, p.pdsfru }).ToListAsync();
        var paletteDList = await paletteDQuery.Select(p => new { p.PaletteD.refver, p.PaletteD.codvar, p.PaletteD.pdscom }).ToListAsync();
        var ecartList = await ecartQuery.Select(e => new { e.refver, e.codvar, e.pdsfru }).ToListAsync();
        var vergers = await _context.Vergers.ToDictionaryAsync(v => v.refver, v => v.nomver ?? "N/A");
        var varietes = await _context.Varietes.ToDictionaryAsync(v => v.codvar, v => v.nomvar ?? "N/A");

        // --- 4. Calculer les données des graphiques (en mémoire) ---
        var receptionChartData = palBrutList.GroupBy(p => p.refver).Select(g => new ChartDataDto { RefVer = g.Key.ToString(), Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})", Value = (decimal)g.Sum(p => p.pdsfru ?? 0) }).OrderByDescending(x => x.Value).ToList();
        var exportChartData = paletteDList.GroupBy(x => x.refver).Select(g => new ChartDataDto { RefVer = g.Key.ToString(), Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})", Value = g.Sum(x => x.pdscom ?? 0) }).OrderByDescending(x => x.Value).ToList();

        // --- 5. Calculer les données de la table (en mémoire) ---
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

    // --- MÉTHODE CORRIGÉE POUR LE GRAPHIQUE EMPILÉ ---
    [HttpGet("destination-chart")]
    public async Task<ActionResult<object>> GetDestinationChartData(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? vergerId,
        [FromQuery] int? varieteId,
        [FromQuery] int? destinationId)
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
        if (varieteId.HasValue) query = query.Where(x => x.pd.codvar == varieteId.Value);
        if (destinationId.HasValue) query = query.Where(x => x.d.coddes == destinationId.Value);

        var flatData = await query
            .GroupBy(x => new { x.v.refver, x.v.nomver, x.va.nomvar })
            .Select(g => new {
                RefVer = g.Key.refver,
                VergerName = g.Key.nomver,
                VarieteName = g.Key.nomvar,
                TotalPdscom = g.Sum(item => item.pd.pdscom ?? 0)
            })
            .ToListAsync();

        // --- CORRECTION ICI : Regrouper par RefVer et VergerName ---

        var groupedData = flatData
            .GroupBy(x => new { x.RefVer, x.VergerName })
            .Select(g => new {
                GroupKey = g.Key,
                Items = g.ToList(),
                TotalPdscomForVerger = g.Sum(item => item.TotalPdscom) // Calculer le total pour le tri
            })
            .OrderByDescending(x => x.TotalPdscomForVerger) // Trier par le total
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
    // In your DashboardController.cs file
    // Add this new method to your DashboardController.cs

    // --- NEW METHOD FOR THE REQUIRED CHART ---
    // This endpoint shows sales by destination, for a specific, required orchard.
    [HttpGet("destination-by-variety-chart")]
    public async Task<ActionResult<object>> GetDestinationByVarietyChart(
     [FromQuery] DateTime startDate,
     [FromQuery] DateTime endDate,
     [FromQuery] int vergerId, // Required
     [FromQuery] int? varieteId) // Optional
    {
        var endDateInclusive = endDate.Date.AddDays(1).AddTicks(-1);

        var query = from pd in _context.Palette_ds
                    join p in _context.Palettes on pd.numpal equals p.numpal
                    join b in _context.Bdqs on pd.numbdq equals b.numbdq
                    join d in _context.Dossiers on b.numdos equals d.numdos
                    join dest in _context.Destinations on d.coddes equals dest.coddes
                    join va in _context.Varietes on pd.codvar equals va.codvar
                    where p.dtepal >= startDate.Date && p.dtepal <= endDateInclusive
                    && pd.refver == vergerId // Filter by the required orchard
                    select new { pd, dest, va };

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

        // --- MODIFICATION START ---
        var groupedData = flatData
            .GroupBy(x => x.DestinationName)
            // 1. Create an intermediate object with the total weight for sorting
            .Select(g => new {
                DestinationName = g.Key,
                Items = g.ToList(),
                TotalWeightForDestination = g.Sum(item => item.TotalWeight)
            })
            // 2. Order by the calculated total weight in descending order
            .OrderByDescending(x => x.TotalWeightForDestination)
            // 3. Select the final object shape
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
        // --- MODIFICATION END ---

        var allVarietes = flatData.Select(x => x.VarieteName).Distinct().ToList();

        return Ok(new { data = groupedData, keys = allVarietes });
    }



}
