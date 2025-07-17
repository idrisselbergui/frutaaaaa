using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyProgramController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DailyProgramController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // 1. GET: api/dailyprogram/dates
        // This method returns a sorted list of unique dates that have programs.
        [HttpGet("dates")]
        public async Task<ActionResult<IEnumerable<string>>> GetProgramDates()
        {
            var dates = await _context.DailyPrograms
                .OrderByDescending(p => p.Dteprog)
                .Select(p => p.Dteprog.ToString("yyyy-MM-dd"))
                .Distinct()
                .ToListAsync();
            return dates;
        }

        // 2. GET: api/dailyprogram?date=YYYY-MM-DD
        // This gets programs for a single, specific date using the original DailyProgram model.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyProgram>>> GetDailyPrograms([FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format. Please use YYYY-MM-DD.");
            }

            var programs = await _context.DailyPrograms
                .Include(p => p.Details) // We still include details for the edit form
                .Where(p => p.Dteprog >= parsedDate.Date && p.Dteprog < parsedDate.Date.AddDays(1))
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return programs;
        }

        // GET: api/dailyprogram
        // Gets all programs for the datagridview
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<DailyProgram>>> GetDailyPrograms()
        //{
        //    return await _context.DailyPrograms.ToListAsync();
        //}

        // GET: api/dailyprogram/5
        // Gets a single program with its details to populate the form for editing
        [HttpGet("{id}")]
        public async Task<ActionResult<DailyProgram>> GetDailyProgram(int id)
        {
            var dailyProgram = await _context.DailyPrograms
                .Include(p => p.Details) // Include the details
                .FirstOrDefaultAsync(p => p.Id == id);

            if (dailyProgram == null)
            {
                return NotFound();
            }

            return dailyProgram;
        }

        // POST: api/dailyprogram
        // Creates a new program and its details
        [HttpPost]
        public async Task<ActionResult<DailyProgram>> PostDailyProgram(DailyProgram dailyProgram)
        {
            _context.DailyPrograms.Add(dailyProgram);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDailyProgram), new { id = dailyProgram.Id }, dailyProgram);
        }

        // PUT: api/dailyprogram/5
        // Updates a program and its details
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDailyProgram(int id, DailyProgram dailyProgram)
        {
            if (id != dailyProgram.Id)
            {
                return BadRequest();
            }

            // Find existing details for this program to remove them
            var existingDetails = _context.DailyProgramDetails
                                          .Where(d => d.NumProg == dailyProgram.NumProg);
            _context.DailyProgramDetails.RemoveRange(existingDetails);

            // Add the new/updated details from the request
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

        // DELETE: api/dailyprogram/5
        // Deletes a program and its details
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyProgram(int id)
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