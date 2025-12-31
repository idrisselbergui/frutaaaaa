using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarqueController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MarqueController(IConfiguration configuration)
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

        [HttpGet]
        public async Task<IActionResult> GetMarques()
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var marques = await _context.Marques.ToListAsync();
                return Ok(marques);
            }
        }

        [HttpGet("{codmar}")]
        public async Task<IActionResult> GetMarque(short codmar)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var marque = await _context.Marques.FindAsync(codmar);
                if (marque == null) return NotFound();
                return Ok(marque);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMarque([FromBody] Marque marque)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                // Check for duplicate codmar
                var existing = await _context.Marques.FindAsync(marque.codmar);
                if (existing != null)
                {
                    return BadRequest(new { message = "Marque with this code already exists" });
                }

                _context.Marques.Add(marque);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMarque), new { codmar = marque.codmar }, marque);
            }
        }

        [HttpPut("{codmar}")]
        public async Task<IActionResult> UpdateMarque(short codmar, [FromBody] Marque marque)
        {
            if (codmar != marque.codmar) return BadRequest();

            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                _context.Entry(marque).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        [HttpDelete("{codmar}")]
        public async Task<IActionResult> DeleteMarque(short codmar)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var marque = await _context.Marques.FindAsync(codmar);
                if (marque == null) return NotFound();

                // Check if marque has assignments
                var hasAssignments = await _context.MarqueAssignments.AnyAsync(ma => ma.Codmar == codmar);
                if (hasAssignments)
                {
                    return BadRequest(new { message = "Cannot delete marque that has assignments" });
                }

                _context.Marques.Remove(marque);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}
