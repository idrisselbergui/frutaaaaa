using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

        // This endpoint remains the same
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
        // This endpoint has been corrected
        [HttpGet("partenaires/{type}")]
        public async Task<ActionResult<IEnumerable<Partenaire>>> GetPartenaires(string type)
        {
            // This now filters by the type provided in the URL
            return await _context.Partenaires
                                 .Where(p => p.type == type)
                                 .ToListAsync();
        }
        // Add this new method inside the LookupController class
        [HttpGet("grpvars")]
        public async Task<ActionResult<IEnumerable<grpvar>>> GetGrpVars()
        {
            return await _context.GrpVars.ToListAsync();
        }
    }
}