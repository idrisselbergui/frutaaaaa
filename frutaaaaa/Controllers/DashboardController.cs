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

        // --- 3. Calculer les données groupées pour la table ---
        var pdsfruGroups = await palBrutQuery.GroupBy(p => new { p.refver, p.codvar }).Select(g => new { VergerId = g.Key.refver, VarieteId = g.Key.codvar, Total = g.Sum(p => p.pdsfru ?? 0) }).ToListAsync();
        var pdscomGroups = await paletteDQuery.GroupBy(x => new { x.PaletteD.refver, x.PaletteD.codvar }).Select(g => new { VergerId = g.Key.refver, VarieteId = g.Key.codvar, Total = g.Sum(x => x.PaletteD.pdscom ?? 0) }).ToListAsync();
        var ecartGroups = await ecartQuery.GroupBy(e => new { e.refver, e.codvar }).Select(g => new { VergerId = g.Key.refver, VarieteId = (int?)g.Key.codvar, Total = g.Sum(e => e.pdsfru) }).ToListAsync();

        // --- 4. Calculer les données des graphiques (MÉTHODE CORRIGÉE) ---
        var vergers = await _context.Vergers.ToDictionaryAsync(v => v.refver, v => v.nomver);

        var receptionChartData = pdsfruGroups
            .GroupBy(p => p.VergerId)
            .Select(g => new ChartDataDto
            {
                RefVer = g.Key.ToString(),
                Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})",
                Value = (decimal)g.Sum(p => p.Total)
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        var exportChartData = pdscomGroups
            .GroupBy(p => p.VergerId)
            .Select(g => new ChartDataDto
            {
                RefVer = g.Key.ToString(),
                Name = g.Key.HasValue && vergers.ContainsKey(g.Key.Value) ? vergers[g.Key.Value] : $"Verger inconnu ({g.Key})",
                Value = g.Sum(p => p.Total)
            })
            .OrderByDescending(x => x.Value)
            .ToList();

        // --- 5. Fusionner les résultats en mémoire ---
        var allData = new Dictionary<(int?, int?), DashboardTableRowDto>();
        var varietes = await _context.Varietes.ToDictionaryAsync(v => v.codvar, v => v.nomvar);

        foreach (var group in pdsfruGroups)
        {
            var key = (group.VergerId, group.VarieteId);
            if (!allData.ContainsKey(key))
            {
                allData[key] = new DashboardTableRowDto { VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A", VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A" };
            }
            allData[key].TotalPdsfru = group.Total;
        }
        foreach (var group in pdscomGroups)
        {
            var key = (group.VergerId, group.VarieteId);
            if (!allData.ContainsKey(key))
            {
                allData[key] = new DashboardTableRowDto { VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A", VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A" };
            }
            allData[key].TotalPdscom = group.Total;
        }
        foreach (var group in ecartGroups)
        {
            var key = (group.VergerId, group.VarieteId);
            if (!allData.ContainsKey(key))
            {
                allData[key] = new DashboardTableRowDto { VergerName = key.Item1.HasValue && vergers.ContainsKey(key.Item1.Value) ? vergers[key.Item1.Value] : "N/A", VarieteName = key.Item2.HasValue && varietes.ContainsKey(key.Item2.Value) ? varietes[key.Item2.Value] : "N/A" };
            }
            allData[key].TotalEcart = group.Total;
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
