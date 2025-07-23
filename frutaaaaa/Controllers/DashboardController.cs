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
        var pdsfruGroups = await palBrutQuery
            .GroupBy(p => new { p.refver, p.codvar })
            .Select(g => new { VergerId = g.Key.refver, VarieteId = g.Key.codvar, Total = g.Sum(p => p.pdsfru ?? 0) })
            .ToListAsync();

        var pdscomGroups = await paletteDQuery
            .GroupBy(x => new { x.PaletteD.refver, x.PaletteD.codvar })
            .Select(g => new { VergerId = g.Key.refver, VarieteId = g.Key.codvar, Total = g.Sum(x => x.PaletteD.pdscom ?? 0) })
            .ToListAsync();

        var ecartGroups = await ecartQuery
            .GroupBy(e => new { e.refver, e.codvar })
            .Select(g => new { VergerId = g.Key.refver, VarieteId = g.Key.codvar, Total = g.Sum(e => e.pdsfru) })
            .ToListAsync();

        // --- 4. Fusionner les résultats en mémoire ---
        var allKeys = pdsfruGroups.Select(p => (p.VergerId, p.VarieteId))
            .Union(pdscomGroups.Select(p => (p.VergerId, p.VarieteId)))
            .Union(ecartGroups.Select(p => (p.VergerId, p.VarieteId)))
            .Distinct();

        var vergers = await _context.Vergers.ToDictionaryAsync(v => v.refver, v => v.nomver);
        var varietes = await _context.Varietes.ToDictionaryAsync(v => v.codvar, v => v.nomvar);

        var tableRows = allKeys.Select(key => new DashboardTableRowDto
        {
            VergerName = key.VergerId.HasValue && vergers.ContainsKey(key.VergerId.Value) ? vergers[key.VergerId.Value] : "N/A",
            VarieteName = key.VarieteId.HasValue && varietes.ContainsKey(key.VarieteId.Value) ? varietes[key.VarieteId.Value] : "N/A",
            TotalPdsfru = pdsfruGroups.FirstOrDefault(f => f.VergerId == key.VergerId && f.VarieteId == key.VarieteId)?.Total ?? 0,
            TotalPdscom = pdscomGroups.FirstOrDefault(c => c.VergerId == key.VergerId && c.VarieteId == key.VarieteId)?.Total ?? 0,
            TotalEcart = ecartGroups.FirstOrDefault(e => e.VergerId == key.VergerId && e.VarieteId == key.VarieteId)?.Total ?? 0
        }).ToList();

        var result = new DashboardDataDto
        {
            TotalPdsfru = totalPdsfru,
            TotalPdscom = totalPdscom,
            ExportPercentage = exportPercentage,
            TotalEcart = totalEcart,
            EcartPercentage = ecartPercentage,
            TableRows = tableRows
        };

        return Ok(result);
    }
}
