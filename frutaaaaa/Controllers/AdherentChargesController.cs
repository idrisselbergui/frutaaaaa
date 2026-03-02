using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdherentChargesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdherentChargesController(IConfiguration configuration)
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

        // GET: api/AdherentCharges
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAdherentCharges([FromHeader(Name = "X-Database-Name")] string database, [FromQuery] int? refadh = null, [FromQuery] string date = null)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var query = _context.AdherentCharges.AsQueryable();

                    if (refadh.HasValue && refadh.Value > 0)
                    {
                        query = query.Where(ac => ac.Refadh == refadh.Value);
                    }

                    if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                    {
                        query = query.Where(ac => ac.Date.Date == parsedDate.Date);
                    }

                    var result = await query.ToListAsync();
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/AdherentCharges
        [HttpPost]
        public async Task<ActionResult<AdherentCharge>> PostAdherentCharge([FromHeader(Name = "X-Database-Name")] string database, AdherentCharge adherentCharge)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    _context.AdherentCharges.Add(adherentCharge);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetAdherentCharges), new { id = adherentCharge.Id }, adherentCharge);
                }
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"Internal server error during save: {innerMsg}");
            }
        }

        // DELETE: api/AdherentCharges/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdherentCharge([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var adherentCharge = await _context.AdherentCharges.FindAsync(id);
                    if (adherentCharge == null)
                    {
                        return NotFound();
                    }

                    _context.AdherentCharges.Remove(adherentCharge);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"An error occurred during deletion: {innerMsg}");
            }
        }

        // GET: api/AdherentCharges/sum
        [HttpGet("sum")]
        public async Task<ActionResult<double>> GetChargeSum([FromHeader(Name = "X-Database-Name")] string database, [FromQuery] int refadh, [FromQuery] int annee, [FromQuery] int mois)
        {
            try
            {
                if (refadh <= 0 || annee <= 0 || mois <= 0)
                {
                    return BadRequest("Missing or invalid parameters.");
                }

                using (var _context = CreateDbContext(database))
                {
                    // Compute the Mon-Sun weeks that "belong" to this month (same rule as tonnage bucketing)
                    int daysInMonth = DateTime.DaysInMonth(annee, mois);
                    DateTime firstDayOfMonth = new DateTime(annee, mois, 1);
                    DateTime lastDayOfMonth = new DateTime(annee, mois, daysInMonth);

                    // Find the Monday of the week containing the 1st of the month
                    int offset = ((int)firstDayOfMonth.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    DateTime firstMonday = firstDayOfMonth.AddDays(-offset);

                    // Build a list of valid date ranges (Mon-Sun) where >= 4 days are in this month
                    var validRanges = new List<(DateTime Start, DateTime End)>();
                    DateTime weekMonday = firstMonday;
                    while (weekMonday <= lastDayOfMonth)
                    {
                        DateTime weekSunday = weekMonday.AddDays(6);
                        DateTime overlapStart = weekMonday < firstDayOfMonth ? firstDayOfMonth : weekMonday;
                        DateTime overlapEnd = weekSunday > lastDayOfMonth ? lastDayOfMonth : weekSunday;
                        int daysInTargetMonth = (int)(overlapEnd - overlapStart).TotalDays + 1;
                        if (daysInTargetMonth >= 4)
                            validRanges.Add((overlapStart, overlapEnd));
                        weekMonday = weekMonday.AddDays(7);
                    }

                    // Fetch all charges for this adherent in the calendar month, then filter client-side by valid ranges
                    var charges = await _context.AdherentCharges
                        .Where(ac => ac.Refadh == refadh
                                  && ac.Date.Year == annee
                                  && ac.Date.Month == mois)
                        .ToListAsync();

                    double total = charges
                        .Where(ac => validRanges.Any(r => ac.Date.Date >= r.Start && ac.Date.Date <= r.End))
                        .Sum(ac => ac.Montant);

                    return Ok(total);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
