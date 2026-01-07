using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SampleController(IConfiguration configuration)
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

        // GET: api/samples/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SampleTestDto>>> GetActiveSamples([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var today = DateTime.Today;

                    var samples = await _context.SampleTests
                        .Where(s => s.Status == SampleTestStatus.Active)
                        .ToListAsync();

                    var sampleDtos = samples.Select(s =>
                    {
                        var isCheckedToday = _context.DailyChecks
                            .Any(dc => dc.SampleTestId == s.Id && dc.CheckDate.Date == today);

                        return new SampleTestDto
                        {
                            Id = s.Id,
                            Numrec = s.Numrec,
                            Coddes = s.Coddes,
                            Codvar = s.Codvar,
                            StartDate = s.StartDate,
                            InitialFruitCount = s.InitialFruitCount,
                            Status = s.Status,
                            IsCheckedToday = isCheckedToday
                        };
                    }).ToList();

                    return sampleDtos;
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/samples/{id}/check
        [HttpPost("{id}/check")]
        public async Task<IActionResult> PostDailyCheck(int id, [FromHeader(Name = "X-Database-Name")] string database, [FromBody] DailyCheckRequest request)
        {
            if (request == null || request.Defects == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // Check if sample exists and is active
                    var sample = await _context.SampleTests.FindAsync(id);
                    if (sample == null)
                    {
                        return NotFound("Sample not found.");
                    }
                    if (sample.Status != SampleTestStatus.Active)
                    {
                        return BadRequest("Sample is not active.");
                    }

                    // Check if a check already exists for this date
                    var existingCheck = await _context.DailyChecks
                        .FirstOrDefaultAsync(dc => dc.SampleTestId == id && dc.CheckDate.Date == request.CheckDate.Date);

                    if (existingCheck != null)
                    {
                        return BadRequest("A check already exists for this date.");
                    }

                    // Create the daily check
                    var dailyCheck = new DailyCheck
                    {
                        SampleTestId = id,
                        CheckDate = request.CheckDate.Date
                    };

                    _context.DailyChecks.Add(dailyCheck);
                    await _context.SaveChangesAsync();

                    // Add defect details
                    foreach (var defect in request.Defects)
                    {
                        var detail = new DailyCheckDetail
                        {
                            DailyCheckId = dailyCheck.Id,
                            DefectType = defect.Type,
                            Quantity = defect.Quantity
                        };
                        _context.DailyCheckDetails.Add(detail);
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new { Message = "Daily check saved successfully.", DailyCheckId = dailyCheck.Id });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/samples/receptions
        [HttpGet("receptions")]
        public async Task<ActionResult<IEnumerable<object>>> GetReceptions([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // Get all receptions that don't already have sample tests
                    var receptions = await _context.Receptions
                        .Where(r => !_context.SampleTests.Any(st => st.Numrec == r.Numrec))
                        .Select(r => new {
                            Numrec = r.Numrec,
                            Codvar = r.Codvar,
                            Refver = r.Refver,
                            Nbrfru = r.Nbrfru,
                            Dterec = r.Dterec
                        })
                        .OrderByDescending(r => r.Numrec)
                        .ToListAsync();

                    return receptions;
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/samples
        [HttpPost]
        public async Task<ActionResult<SampleTestDto>> PostSampleTest([FromHeader(Name = "X-Database-Name")] string database, [FromBody] CreateSampleTestRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // Check if a sample test already exists for this reception
                    var existingSample = await _context.SampleTests
                        .FirstOrDefaultAsync(st => st.Numrec == request.Numrec);

                    if (existingSample != null)
                    {
                        return BadRequest("A sample test already exists for this reception number.");
                    }

                    // Create the sample test
                    var sampleTest = new SampleTest
                    {
                        Numrec = request.Numrec,
                        Coddes = request.Coddes,
                        Codvar = request.Codvar,
                        StartDate = request.StartDate,
                        InitialFruitCount = request.InitialFruitCount,
                        Status = request.Status
                    };

                    _context.SampleTests.Add(sampleTest);
                    await _context.SaveChangesAsync();

                    // Return the created sample test
                    var dto = new SampleTestDto
                    {
                        Id = sampleTest.Id,
                        Numrec = sampleTest.Numrec,
                        Coddes = sampleTest.Coddes,
                        Codvar = sampleTest.Codvar,
                        StartDate = sampleTest.StartDate,
                        InitialFruitCount = sampleTest.InitialFruitCount,
                        Status = sampleTest.Status,
                        IsCheckedToday = false
                    };

                    return CreatedAtAction(nameof(GetSampleTest), new { id = sampleTest.Id }, dto);
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/samples/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SampleTestDto>> GetSampleTest([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var sample = await _context.SampleTests.FindAsync(id);

                    if (sample == null)
                    {
                        return NotFound("Sample test not found.");
                    }

                    var today = DateTime.Today;
                    var isCheckedToday = _context.DailyChecks
                        .Any(dc => dc.SampleTestId == sample.Id && dc.CheckDate.Date == today);

                    var dto = new SampleTestDto
                    {
                        Id = sample.Id,
                        Numrec = sample.Numrec,
                        Coddes = sample.Coddes,
                        Codvar = sample.Codvar,
                        StartDate = sample.StartDate,
                        InitialFruitCount = sample.InitialFruitCount,
                        Status = sample.Status,
                        IsCheckedToday = isCheckedToday
                    };

                    return dto;
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
