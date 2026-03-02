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
    public class PrixEstimatifsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PrixEstimatifsController(IConfiguration configuration)
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

        // GET: api/PrixEstimatifs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrixEstimatif>>> GetPrixEstimatifs([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    return await _context.PrixEstimatifs.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/PrixEstimatifs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PrixEstimatif>> GetPrixEstimatif([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var result = await _context.PrixEstimatifs.FindAsync(id);

                    if (result == null)
                    {
                        return NotFound();
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        // GET: api/PrixEstimatifs/by-month?annee=2026&mois=3
        [HttpGet("by-month")]
        public async Task<ActionResult<IEnumerable<PrixEstimatif>>> GetPrixEstimatifsByMonth([FromHeader(Name = "X-Database-Name")] string database, [FromQuery] int annee, [FromQuery] int mois)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    return await _context.PrixEstimatifs
                                         .Where(p => p.Annee == annee && p.Mois == mois)
                                         .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/PrixEstimatifs/upsert
        [HttpPost("upsert")]
        public async Task<ActionResult<PrixEstimatif>> UpsertPrixEstimatif([FromHeader(Name = "X-Database-Name")] string database, PrixEstimatif dto)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var existing = await _context.PrixEstimatifs
                        .FirstOrDefaultAsync(p => p.Annee == dto.Annee && p.Mois == dto.Mois && p.CodGrv == dto.CodGrv);

                    if (existing != null)
                    {
                        existing.PrixEstime = dto.PrixEstime;
                    }
                    else
                    {
                        _context.PrixEstimatifs.Add(new PrixEstimatif 
                        {
                             Annee = dto.Annee,
                             Mois = dto.Mois,
                             CodGrv = dto.CodGrv,
                             PrixEstime = dto.PrixEstime
                        });
                    }
                    
                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
