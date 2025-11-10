// Controllers/DefautController.cs
using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DefautController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DefautController(IConfiguration configuration)
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
                throw new System.ArgumentException("Database name or connection string is missing.");
            }
            var dynamicConnectionString = baseConnectionString.Replace("frutaaaaa_db", dbName);
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(dynamicConnectionString, ServerVersion.AutoDetect(dynamicConnectionString));
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        // GET: api/defaut
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Defaut>>> GetDefauts([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    return await _context.Defauts.ToListAsync();
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/defaut/5
        [HttpGet("{coddef}")]
        public async Task<ActionResult<Defaut>> GetDefaut([FromHeader(Name = "X-Database-Name")] string database, short coddef)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var defaut = await _context.Defauts.FindAsync(coddef);

                    if (defaut == null)
                    {
                        return NotFound();
                    }

                    return defaut;
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/defaut
        [HttpPost]
        public async Task<ActionResult<Defaut>> PostDefaut([FromHeader(Name = "X-Database-Name")] string database, [FromBody] Defaut defaut)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // If coddef is not provided or is 0, auto-assign the next available coddef
                    if (defaut.Coddef == 0)
                    {
                        var maxCoddef = await _context.Defauts.MaxAsync(d => (short?)d.Coddef) ?? 0;
                        defaut.Coddef = (short)(maxCoddef + 1);
                    }

                    _context.Defauts.Add(defaut);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetDefaut), new { coddef = defaut.Coddef }, defaut);
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT: api/defaut/5
        [HttpPut("{coddef}")]
        public async Task<IActionResult> PutDefaut([FromHeader(Name = "X-Database-Name")] string database, short coddef, [FromBody] Defaut defaut)
        {
            if (coddef != defaut.Coddef)
            {
                return BadRequest();
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    _context.Entry(defaut).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/defaut/5
        [HttpDelete("{coddef}")]
        public async Task<IActionResult> DeleteDefaut([FromHeader(Name = "X-Database-Name")] string database, short coddef)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var defaut = await _context.Defauts.FindAsync(coddef);
                    if (defaut == null)
                    {
                        return NotFound();
                    }

                    // TODO: Add business rule checks if needed
                    // For example, check if defaut is used in other tables

                    _context.Defauts.Remove(defaut);
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
