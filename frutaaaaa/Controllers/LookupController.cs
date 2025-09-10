using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // 1. Add this
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly IConfiguration _configuration; // 2. Change DbContext to IConfiguration

        public LookupController(IConfiguration configuration) // 3. Update constructor
        {
            _configuration = configuration;
        }

        // 4. Add the helper method
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

        // --- 5. Update all methods to use the header and the helper ---

        [HttpGet("destinations")]
        public async Task<ActionResult<IEnumerable<Destination>>> GetDestinations([FromHeader(Name = "X-Database-Name")] string database)
        {
            using (var _context = CreateDbContext(database))
            {
                return await _context.Destinations.ToListAsync();
            }
        }

        [HttpGet("tpalettes")]
        public async Task<ActionResult<IEnumerable<TPalette>>> GetTPalettes([FromHeader(Name = "X-Database-Name")] string database)
        {
            using (var _context = CreateDbContext(database))
            {
                return await _context.TPalettes.ToListAsync();
            }
        }

        [HttpGet("partenaires/{type}")]
        public async Task<ActionResult<IEnumerable<Partenaire>>> GetPartenaires([FromHeader(Name = "X-Database-Name")] string database, string type)
        {
            using (var _context = CreateDbContext(database))
            {
                return await _context.Partenaires
                                     .Where(p => p.type == type)
                                     .ToListAsync();
            }
        }

        [HttpGet("grpvars")]
        public async Task<ActionResult<IEnumerable<GrpVar>>> GetGrpVars([FromHeader(Name = "X-Database-Name")] string database)
        {
            using (var _context = CreateDbContext(database))
            {
                return await _context.grpvars.ToListAsync();
            }
        }

        [HttpGet("varietes")]
        public async Task<ActionResult<IEnumerable<Variete>>> GetVarietes([FromHeader(Name = "X-Database-Name")] string database)
        {
            using (var _context = CreateDbContext(database))
            {
                return await _context.Varietes.ToListAsync();
            }
        }

        [HttpGet("vergers")]
        public async Task<ActionResult<IEnumerable<Verger>>> GetVergers([FromHeader(Name = "X-Database-Name")] string database)
        {
            using (var _context = CreateDbContext(database))
            {
                return await _context.Vergers.ToListAsync();
            }
        }

        [HttpGet("typeecarts")]
        public async Task<ActionResult<IEnumerable<TypeEcart>>> GetTypeEcarts([FromHeader(Name = "X-Database-Name")] string database)
        {
            using (var _context = CreateDbContext(database))
            {
                return await _context.TypeEcarts.ToListAsync();
            }
        }

        [HttpGet("campagne-dates")]
        public async Task<ActionResult<object>> GetCampagneDates([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var result = await _context.Entreprises
                        .Select(e => new { StartDate = e.dtDebut, EndDate = e.dtFin })
                        .FirstOrDefaultAsync();

                    if (result == null)
                    {
                        return NotFound("No campaign data found in the entreprise table.");
                    }
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching campaign dates: {ex.Message}");
            }
        }
    }
}

