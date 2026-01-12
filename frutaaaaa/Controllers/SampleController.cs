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
                            Numpal = s.Numpal,
                            Coddes = s.Coddes,
                            Codvar = s.Codvar,
                            StartDate = s.StartDate,
                            InitialFruitCount = s.InitialFruitCount,
                            Pdsfru = s.Pdsfru,
                            Couleur1 = s.Couleur1,
                            Couleur2 = s.Couleur2,
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

        // GET: api/samples/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<SampleTestDto>>> GetAllSamples([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var today = DateTime.Today;

                    var samples = await _context.SampleTests
                        .OrderByDescending(s => s.StartDate)
                        .ToListAsync();

                    var sampleDtos = samples.Select(s =>
                    {
                        var isCheckedToday = _context.DailyChecks
                            .Any(dc => dc.SampleTestId == s.Id && dc.CheckDate.Date == today);

                        return new SampleTestDto
                        {
                            Id = s.Id,
                            Numpal = s.Numpal,
                            Coddes = s.Coddes,
                            Codvar = s.Codvar,
                            StartDate = s.StartDate,
                            InitialFruitCount = s.InitialFruitCount,
                            Pdsfru = s.Pdsfru,
                            Couleur1 = s.Couleur1,
                            Couleur2 = s.Couleur2,
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
                    // Get all palettes from the last 3 days that don't already have sample tests
                    var threeDaysAgo = DateTime.Now.AddDays(-3);
                    var palettes = await _context.palbruts
                        .Where(p => p.dterec >= threeDaysAgo && !_context.SampleTests.Any(st => st.Numpal == p.numpal))
                        .Join(_context.Vergers,
                            p => p.refver,
                            v => v.refver,
                            (p, v) => new { Palette = p, Verger = v })
                        .Join(_context.TPalettes,
                            pv => pv.Palette.codtyp,
                            t => t.codtyp,
                            (pv, t) => new {
                                numpal = pv.Palette.numpal,
                                numrec = pv.Palette.numrec,
                                codvar = pv.Palette.codvar,
                                refadh = pv.Palette.refadh,
                                refver = pv.Palette.refver,
                                nbrcai = pv.Palette.nbrcai,
                                pdsfru = pv.Palette.pdsfru,
                                dterec = pv.Palette.dterec,
                                nomver = pv.Verger.nomver,
                                nomemb = t.nomemb
                            })
                        .OrderByDescending(p => p.numpal)
                        .ToListAsync();

                    return palettes;
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
                    // Check if a sample test already exists for this palette
                    var existingSample = await _context.SampleTests
                        .FirstOrDefaultAsync(st => st.Numpal == request.Numpal);

                    if (existingSample != null)
                    {
                        return BadRequest("A sample test already exists for this palette number.");
                    }

                    // Create the sample test
                    var sampleTest = new SampleTest
                    {
                        Numpal = request.Numpal,
                        Coddes = request.Coddes,
                        Codvar = request.Codvar,
                        StartDate = request.StartDate,
                        InitialFruitCount = request.InitialFruitCount,
                        Pdsfru = request.Pdsfru,
                        Couleur1 = request.Couleur1,
                        Couleur2 = request.Couleur2,
                        Status = request.Status
                    };

                    _context.SampleTests.Add(sampleTest);
                    await _context.SaveChangesAsync();

                    // Return the created sample test
                    var dto = new SampleTestDto
                    {
                        Id = sampleTest.Id,
                        Numpal = sampleTest.Numpal,
                        Coddes = sampleTest.Coddes,
                        Codvar = sampleTest.Codvar,
                        StartDate = sampleTest.StartDate,
                        InitialFruitCount = sampleTest.InitialFruitCount,
                        Pdsfru = sampleTest.Pdsfru,
                        Couleur1 = sampleTest.Couleur1,
                        Couleur2 = sampleTest.Couleur2,
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
                        Numpal = sample.Numpal,
                        Coddes = sample.Coddes,
                        Codvar = sample.Codvar,
                        StartDate = sample.StartDate,
                        InitialFruitCount = sample.InitialFruitCount,
                        Pdsfru = sample.Pdsfru,
                        Couleur1 = sample.Couleur1,
                        Couleur2 = sample.Couleur2,
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

        // PUT: api/samples/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateSampleStatus(int id, [FromHeader(Name = "X-Database-Name")] string database, [FromBody] UpdateSampleStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var sample = await _context.SampleTests.FindAsync(id);
                    if (sample == null)
                    {
                        return NotFound("Sample test not found.");
                    }

                    // Update the status
                    sample.Status = request.Status;
                    await _context.SaveChangesAsync();

                    return Ok(new { Message = "Sample status updated successfully." });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
