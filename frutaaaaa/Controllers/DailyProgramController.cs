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

        // GET: api/dailyprogram
        // Gets all programs for the datagridview
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyProgram>>> GetDailyPrograms()
        {
            return await _context.DailyPrograms.ToListAsync();
        }

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