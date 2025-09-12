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

        // Helper method to create a DbContext for the specified database
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
        public async Task<ActionResult<IEnumerable<Traitement>>> GetTraitements([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    return await _context.Traitements.OrderByDescending(t => t.Dateappli).ToListAsync();
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
            if (!traitement.Ref.HasValue || !traitement.Dateappli.HasValue)
            {
                return BadRequest("Trait (ref) and application date are required.");
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // Find the selected Trait to get its DAR value
                    var traitProduct = await _context.Traits.FindAsync(traitement.Ref.Value);
                    if (traitProduct == null || !traitProduct.Dar.HasValue)
                    {
                        return BadRequest("Invalid Trait product selected or DAR value is missing.");
                    }

                    // --- AUTOMATIC CALCULATION LOGIC ---
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
