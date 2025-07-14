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

        // This endpoint has been corrected
        [HttpGet("partenaires")]
        public async Task<ActionResult<IEnumerable<Partenaire>>> GetPartenaires()
        {
            // Uses the corrected DbSet name: Partenaires
            return await _context.Partenaires.ToListAsync();
        }
        // Add this new method inside the LookupController class
        [HttpGet("varietes")]
        public async Task<ActionResult<IEnumerable<variete>>> GetVarietes()
        {
            return await _context.varietes.ToListAsync();
        }
    }
}