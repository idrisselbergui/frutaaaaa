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
    public class TraitController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TraitController(IConfiguration configuration)
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

        // GET: api/trait
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trait>>> GetTraits([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    return await _context.Traits.ToListAsync();
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/trait/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Trait>> GetTrait([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var trait = await _context.Traits.FindAsync(id);

                    if (trait == null)
                    {
                        return NotFound();
                    }

                    return trait;
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/trait
        [HttpPost]
        public async Task<ActionResult<Trait>> PostTrait([FromHeader(Name = "X-Database-Name")] string database, [FromBody] Trait trait)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    _context.Traits.Add(trait);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetTrait), new { id = trait.Ref }, trait);
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT: api/trait/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTrait([FromHeader(Name = "X-Database-Name")] string database, int id, [FromBody] Trait trait)
        {
            if (id != trait.Ref)
            {
                return BadRequest();
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    _context.Entry(trait).State = EntityState.Modified;
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

        // DELETE: api/trait/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrait([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var trait = await _context.Traits.FindAsync(id);
                    if (trait == null)
                    {
                        return NotFound();
                    }

                    _context.Traits.Remove(trait);
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

