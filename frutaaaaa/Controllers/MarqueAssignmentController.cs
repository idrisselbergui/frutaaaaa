using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarqueAssignmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MarqueAssignmentController(IConfiguration configuration)
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
        public async Task<IActionResult> GetMarqueAssignments()
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var assignments = await _context.MarqueAssignments.ToListAsync();

                // Join with related tables to get names
                var result = await (from ma in _context.MarqueAssignments
                                   join m in _context.Marques on ma.Codmar equals m.codmar into marques
                                   from m in marques.DefaultIfEmpty()
                                   join v in _context.Vergers on ma.Refver equals v.refver into vergers
                                   from v in vergers.DefaultIfEmpty()
                                   join var in _context.Varietes on ma.Codvar equals var.codvar into varietes
                                   from var in varietes.DefaultIfEmpty()
                                   select new
                                   {
                                       ma.Id,
                                       ma.Codmar,
                                       ma.Refver,
                                       ma.Codvar,
                                       MarqueName = m != null ? m.desmar : null,
                                       VergerName = v != null ? v.nomver : null,
                                       VarieteName = var != null ? var.nomvar : null
                                   }).ToListAsync();

                return Ok(result);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMarqueAssignment(int id)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var result = await (from ma in _context.MarqueAssignments
                                   where ma.Id == id
                                   join m in _context.Marques on ma.Codmar equals m.codmar into marques
                                   from m in marques.DefaultIfEmpty()
                                   join v in _context.Vergers on ma.Refver equals v.refver into vergers
                                   from v in vergers.DefaultIfEmpty()
                                   join var in _context.Varietes on ma.Codvar equals var.codvar into varietes
                                   from var in varietes.DefaultIfEmpty()
                                   select new
                                   {
                                       ma.Id,
                                       ma.Codmar,
                                       ma.Refver,
                                       ma.Codvar,
                                       MarqueName = m != null ? m.desmar : null,
                                       VergerName = v != null ? v.nomver : null,
                                       VarieteName = var != null ? var.nomvar : null
                                   }).FirstOrDefaultAsync();

                if (result == null) return NotFound();
                return Ok(result);
            }
        }

        [HttpGet("marque/{codmar}")]
        public async Task<IActionResult> GetAssignmentsByMarque(short codmar)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var result = await (from ma in _context.MarqueAssignments
                                   where ma.Codmar == codmar
                                   join m in _context.Marques on ma.Codmar equals m.codmar into marques
                                   from m in marques.DefaultIfEmpty()
                                   join v in _context.Vergers on ma.Refver equals v.refver into vergers
                                   from v in vergers.DefaultIfEmpty()
                                   join var in _context.Varietes on ma.Codvar equals var.codvar into varietes
                                   from var in varietes.DefaultIfEmpty()
                                   select new
                                   {
                                       ma.Id,
                                       ma.Codmar,
                                       ma.Refver,
                                       ma.Codvar,
                                       MarqueName = m != null ? m.desmar : null,
                                       VergerName = v != null ? v.nomver : null,
                                       VarieteName = var != null ? var.nomvar : null
                                   }).ToListAsync();

                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMarqueAssignment([FromBody] MarqueAssignment assignment)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                // Check for duplicate assignment
                var existing = await _context.MarqueAssignments
                    .AnyAsync(ma => ma.Codmar == assignment.Codmar &&
                                   ma.Refver == assignment.Refver &&
                                   ma.Codvar == assignment.Codvar);

                if (existing)
                {
                    return BadRequest(new { message = "Assignment already exists for this marque-verger-variete combination" });
                }

                _context.MarqueAssignments.Add(assignment);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMarqueAssignment), new { id = assignment.Id }, assignment);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMarqueAssignment(int id, [FromBody] MarqueAssignment assignment)
        {
            if (id != assignment.Id) return BadRequest();

            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                // Check for duplicate assignment (excluding current)
                var existing = await _context.MarqueAssignments
                    .AnyAsync(ma => ma.Id != id &&
                                   ma.Codmar == assignment.Codmar &&
                                   ma.Refver == assignment.Refver &&
                                   ma.Codvar == assignment.Codvar);

                if (existing)
                {
                    return BadRequest(new { message = "Assignment already exists for this marque-verger-variete combination" });
                }

                _context.Entry(assignment).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarqueAssignment(int id)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var assignment = await _context.MarqueAssignments.FindAsync(id);
                if (assignment == null) return NotFound();

                _context.MarqueAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}
