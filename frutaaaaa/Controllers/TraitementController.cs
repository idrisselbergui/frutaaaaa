using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraitementController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TraitementController(IConfiguration configuration)
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

        // GET: api/traitement
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTraitements([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var traitements = await _context.Traitements
                        .OrderByDescending(t => t.Numtrait) // Sort by Numtrait
                        .Join(_context.Vergers, t => t.Refver, v => v.refver, (t, v) => new { Traitement = t, Verger = v })
                        .Join(_context.Traits, tv => tv.Traitement.Ref, p => p.Ref, (tv, p) => new { tv.Traitement, tv.Verger, Trait = p })
                        .Join(_context.grpvars, tvp => tvp.Traitement.Codgrp, g => g.codgrv, (tvp, g) => new { tvp.Traitement, tvp.Verger, tvp.Trait, GrpVar = g })
                        .Join(_context.Varietes, tvpg => tvpg.Traitement.Codvar, va => va.codvar, (tvpg, va) => new
                        {
                            tvpg.Traitement.Numtrait,
                            tvpg.Traitement.Dateappli,
                            tvpg.Traitement.Dateprecolte,
                            VergerName = tvpg.Verger.nomver,
                            TraitName = tvpg.Trait.Nomcom,
                            GrpVarName = tvpg.GrpVar.nomgrv,
                            VarieteName = va.nomvar
                        })
                        .ToListAsync();

                    return Ok(traitements);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/traitement
        [HttpPost]
        public async Task<ActionResult<Traitement>> PostTraitement([FromHeader(Name = "X-Database-Name")] string database, [FromBody] Traitement traitement)
        {
            if (!traitement.Ref.HasValue || !traitement.Dateappli.HasValue || !traitement.Refver.HasValue || !traitement.Codgrp.HasValue || !traitement.Codvar.HasValue)
            {
                return BadRequest("Orchard, Group, Variety, Product, and Application Date are required.");
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var traitProduct = await _context.Traits.FindAsync(traitement.Ref.Value);
                    if (traitProduct == null || !traitProduct.Dar.HasValue)
                    {
                        return BadRequest("Invalid Trait product selected or DAR value is missing.");
                    }

                    traitement.Dateprecolte = traitement.Dateappli.Value.AddDays(traitProduct.Dar.Value);
                    _context.Traitements.Add(traitement);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetTraitements), new { id = traitement.Numtrait }, traitement);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE: api/traitement/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTraitement([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var traitement = await _context.Traitements.FindAsync(id);
                    if (traitement == null)
                    {
                        return NotFound();
                    }

                    // --- NEW BUSINESS RULE CHECK ---
                    // Check if the Traitement is being used in any Reception records.
                    var isInUse = await _context.Receptions.AnyAsync(r => r.Numtrait == id);
                    if (isInUse)
                    {
                        // If it is in use, return a 400 Bad Request error with a clear message.
                        return BadRequest(new { message = "This treatment cannot be deleted because it is linked to a reception record." });
                    }
                    // --- END OF CHECK ---

                    _context.Traitements.Remove(traitement);
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

