using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LookupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- UPDATED METHOD WITH ERROR HANDLING ---
        [HttpGet("campagne-dates")]
        public async Task<ActionResult<object>> GetCampagneDates()
        {
            try
            {
                var dates = await _context.Entreprises
                    .Select(e => new { e.dtDebut, e.dtFin })
                    .FirstOrDefaultAsync();

                if (dates == null)
                {
                    return NotFound("No entreprise record found.");
                }

                var startDate = dates.dtDebut ?? System.DateTime.MinValue;
                var endDate = dates.dtFin ?? System.DateTime.UtcNow;

                return Ok(new
                {
                    startDate = startDate,
                    endDate = endDate
                });
            }
            catch (System.Exception ex)
            {
                // This will return the specific database error to the browser console for debugging.
                return StatusCode(500, $"An error occurred: {ex.Message} | Inner Exception: {ex.InnerException?.Message}");
            }
        }


        [HttpGet("typeecarts")]
        public async Task<ActionResult<IEnumerable<TypeEcart>>> GetTypeEcarts()
        {
            return await _context.TypeEcarts.ToListAsync();
        }

        [HttpGet("destinations")]
        public async Task<ActionResult<IEnumerable<Destination>>> GetDestinations()
        {
            return await _context.Destinations.ToListAsync();
        }
        [HttpGet("tpalettes")]
        public async Task<ActionResult<IEnumerable<TPalette>>> GetTPalettes()
        {
            return await _context.TPalettes.ToListAsync();
        }

        [HttpGet("partenaires/{type}")]
        public async Task<ActionResult<IEnumerable<Partenaire>>> GetPartenaires(string type)
        {
            return await _context.Partenaires
                                 .Where(p => p.type == type)
                                 .ToListAsync();
        }

        [HttpGet("grpvars")]
        public async Task<ActionResult<IEnumerable<GrpVar>>> GetGrpVars()
        {
            return await _context.grpvars.ToListAsync();
        }
        [HttpGet("varietes")]
        public async Task<ActionResult<IEnumerable<Variete>>> GetVarietes()
        {
            return await _context.Varietes.ToListAsync();
        }

        [HttpGet("vergers")]
        public async Task<ActionResult<IEnumerable<Verger>>> GetVergers()
        {
            return await _context.Vergers.ToListAsync();
        }
    }
}
