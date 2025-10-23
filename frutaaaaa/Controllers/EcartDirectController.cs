using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EcartDirectController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EcartDirectController(IConfiguration configuration)
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
                return null;
            }
            var dynamicConnectionString = baseConnectionString.Replace("frutaaaaa_db", dbName);
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(dynamicConnectionString, ServerVersion.AutoDetect(dynamicConnectionString));
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        // GET: api/ecartdirect
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EcartDirect>>> GetEcartDirects([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    return await _context.EcartDirects.Include(e => e.TypeEcart).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/ecartdirect/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EcartDirect>> GetEcartDirect([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var ecartDirect = await _context.EcartDirects.Include(e => e.TypeEcart).FirstOrDefaultAsync(e => e.Numpal == id);

                    if (ecartDirect == null)
                    {
                        return NotFound();
                    }

                    return ecartDirect;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/ecartdirect
        [HttpPost]
        public async Task<ActionResult<EcartDirect>> PostEcartDirect([FromHeader(Name = "X-Database-Name")] string database, [FromBody] EcartDirect ecartDirect)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    _context.EcartDirects.Add(ecartDirect);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetEcartDirect), new { id = ecartDirect.Numpal }, ecartDirect);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT: api/ecartdirect/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEcartDirect([FromHeader(Name = "X-Database-Name")] string database, int id, [FromBody] EcartDirect ecartDirect)
        {
            if (id != ecartDirect.Numpal)
            {
                return BadRequest();
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    _context.Entry(ecartDirect).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/ecartdirect/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEcartDirect([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var ecartDirect = await _context.EcartDirects.FindAsync(id);
                    if (ecartDirect == null)
                    {
                        return NotFound();
                    }

                    _context.EcartDirects.Remove(ecartDirect);
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
