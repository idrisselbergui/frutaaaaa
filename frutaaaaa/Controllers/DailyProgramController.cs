using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // 1. Add this

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyProgramController : ControllerBase
    {
        private readonly IConfiguration _configuration; // 2. Change DbContext to IConfiguration

        public DailyProgramController(IConfiguration configuration) // 3. Update constructor
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

        [HttpGet("dates")]
        public async Task<ActionResult<IEnumerable<string>>> GetProgramDates([FromHeader(Name = "X-Database-Name")] string database)
        {
            using (var _context = CreateDbContext(database))
            {
                var dates = await _context.DailyPrograms
                    .OrderByDescending(p => p.Dteprog)
                    .Select(p => p.Dteprog.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .ToListAsync();
                return dates;
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyProgram>>> GetDailyPrograms(
            [FromHeader(Name = "X-Database-Name")] string database,
            [FromQuery] string date)
        {
            using (var _context = CreateDbContext(database))
            {
                if (!DateTime.TryParse(date, out DateTime parsedDate))
                {
                    return BadRequest("Invalid date format. Please use YYYY-MM-DD.");
                }

                var programs = await _context.DailyPrograms
                    .Include(p => p.Details)
                    .Where(p => p.Dteprog >= parsedDate.Date && p.Dteprog < parsedDate.Date.AddDays(1))
                    .OrderByDescending(p => p.Dteprog)
                    .ToListAsync();

                return programs;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DailyProgram>> GetDailyProgram(
            [FromHeader(Name = "X-Database-Name")] string database,
            int id)
        {
            using (var _context = CreateDbContext(database))
            {
                var dailyProgram = await _context.DailyPrograms
                    .Include(p => p.Details)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (dailyProgram == null)
                {
                    return NotFound();
                }

                return dailyProgram;
            }
        }

        [HttpPost]
        public async Task<ActionResult<DailyProgram>> PostDailyProgram(
            [FromHeader(Name = "X-Database-Name")] string database,
            DailyProgram dailyProgram)
        {
            using (var _context = CreateDbContext(database))
            {
                _context.DailyPrograms.Add(dailyProgram);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDailyProgram), new { id = dailyProgram.Id, database = database }, dailyProgram);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDailyProgram(
            [FromHeader(Name = "X-Database-Name")] string database,
            int id,
            DailyProgram dailyProgram)
        {
            using (var _context = CreateDbContext(database))
            {
                if (id != dailyProgram.Id)
                {
                    return BadRequest();
                }

                var existingDetails = _context.DailyProgramDetails
                                              .Where(d => d.NumProg == dailyProgram.NumProg);
                _context.DailyProgramDetails.RemoveRange(existingDetails);

                if (dailyProgram.Details != null && dailyProgram.Details.Any())
                {
                    foreach (var detail in dailyProgram.Details)
                    {
                        _context.DailyProgramDetails.Add(detail);
                    }
                }

                _context.Entry(dailyProgram).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DailyPrograms.Any(e => e.Id == id)) { return NotFound(); }
                    else { throw; }
                }

                return NoContent();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyProgram(
            [FromHeader(Name = "X-Database-Name")] string database,
            int id)
        {
            using (var _context = CreateDbContext(database))
            {
                var dailyProgram = await _context.DailyPrograms
                                                 .Include(p => p.Details)
                                                 .FirstOrDefaultAsync(p => p.Id == id);
                if (dailyProgram == null)
                {
                    return NotFound();
                }

                _context.DailyPrograms.Remove(dailyProgram);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
    }
}

