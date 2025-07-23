using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
     [FromQuery] int? varieteId) // Changed from grpVarId
    {
        // --- Filter palbrut data ---
        var palBrutQuery = _context.palbruts.AsQueryable();
        palBrutQuery = palBrutQuery.Where(p => p.dterec >= startDate && p.dterec <= endDate);

        if (vergerId.HasValue)
        {
            palBrutQuery = palBrutQuery.Where(p => p.refver == vergerId.Value);
        }
        if (varieteId.HasValue)
        {
            palBrutQuery = palBrutQuery.Where(p => p.codvar == varieteId.Value); // Changed to codvar
        }

        // --- Filter palette_d data ---
        var paletteDQuery = _context.Palette_ds
            .Join(_context.Palettes, pd => pd.numpal, p => p.numpal, (pd, p) => new { PaletteD = pd, Palette = p })
            .Where(x => x.Palette.dtepal >= startDate && x.Palette.dtepal <= endDate);

        if (vergerId.HasValue)
        {
            paletteDQuery = paletteDQuery.Where(x => x.PaletteD.refver == vergerId.Value);
        }
        if (varieteId.HasValue)
        {
            paletteDQuery = paletteDQuery.Where(x => x.PaletteD.codvar == varieteId.Value); // Changed to codvar
        }

        // --- Calculate KPIs ---
        var totalPdsfru = await palBrutQuery.SumAsync(p => p.pdsfru ?? 0);
        var totalPdscom = await paletteDQuery.SumAsync(x => x.PaletteD.pdscom ?? 0);

        // --- Calculate Table Data ---
        var tableData = await palBrutQuery
            .GroupBy(pb => new { pb.refver, pb.codvar }) // Group by verger and variety
            .Select(g => new
            {
                VergerId = g.Key.refver,
                VarieteId = g.Key.codvar, // Changed to VarieteId
                TotalPdsfru = g.Sum(pb => pb.pdsfru ?? 0)
            })
            .ToListAsync();

        // You will need to join with lookup tables to get the names
        var result = new DashboardDataDto
        {
            TotalPdsfru = totalPdsfru,
            TotalPdscom = totalPdscom,
            TableRows = tableData.Select(d => new DashboardTableRowDto
            {
                VergerName = $"Orchard {d.VergerId}",
                GrpVarName = $"Variety {d.VarieteId}", // Changed to Variety
                TotalPdsfru = d.TotalPdsfru,
                TotalPdscom = 0
            }).ToList()
        };

        return Ok(result);
    }


}