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

                    var sampleDtos = new List<SampleTestDto>();

                    foreach (var s in samples)
                    {
                        var isCheckedToday = await _context.DailyChecks
                            .AnyAsync(dc => dc.SampleTestId == s.Id && dc.CheckDate.Date == today);
                        
                        // Fetch Verger Name
                        var vergerName = await _context.palbruts
                            .Where(p => p.numpal == s.Numpal)
                            .Join(_context.Vergers,
                                p => p.refver,
                                v => v.refver,
                                (p, v) => v.nomver)
                            .FirstOrDefaultAsync();

                        sampleDtos.Add(new SampleTestDto
                        {
                            Id = s.Id,
                            Numpal = s.Numpal,
                            Coddes = s.Coddes,
                            Codvar = s.Codvar,
                            StartDate = s.StartDate,
                            VergerName = vergerName,
                            InitialFruitCount = s.InitialFruitCount,
                            Pdsfru = s.Pdsfru,
                            Couleur1 = s.Couleur1,
                            Couleur2 = s.Couleur2,
                            Status = s.Status,
                            IsCheckedToday = isCheckedToday
                        });
                    }

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

                    var sampleDtos = new List<SampleTestDto>();

                    foreach (var s in samples)
                    {
                        var isCheckedToday = await _context.DailyChecks
                            .AnyAsync(dc => dc.SampleTestId == s.Id && dc.CheckDate.Date == today);

                        // Fetch Verger Name
                        var vergerName = await _context.palbruts
                            .Where(p => p.numpal == s.Numpal)
                            .Join(_context.Vergers,
                                p => p.refver,
                                v => v.refver,
                                (p, v) => v.nomver)
                            .FirstOrDefaultAsync();

                        sampleDtos.Add(new SampleTestDto
                        {
                            Id = s.Id,
                            Numpal = s.Numpal,
                            Coddes = s.Coddes,
                            Codvar = s.Codvar,
                            StartDate = s.StartDate,
                            VergerName = vergerName,
                            InitialFruitCount = s.InitialFruitCount,
                            Pdsfru = s.Pdsfru,
                            Couleur1 = s.Couleur1,
                            Couleur2 = s.Couleur2,
                            Status = s.Status,
                            IsCheckedToday = isCheckedToday
                        });
                    }

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
            if (request == null)
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

                    // Check if daily check already exists for this sample and date
                    var existingDailyCheck = await _context.DailyChecks
                        .FirstOrDefaultAsync(dc => dc.SampleTestId == id && dc.CheckDate.Date == request.CheckDate.Date);

                    DailyCheck dailyCheck;

                    if (existingDailyCheck != null)
                    {
                        // Update existing daily check
                        existingDailyCheck.Pdsfru = (double)(request.Pdsfru ?? 0);
                        existingDailyCheck.Couleur1 = request.Couleur1 ?? 0;
                        existingDailyCheck.Couleur2 = request.Couleur2 ?? 0;
                        dailyCheck = existingDailyCheck;
                    }
                    else
                    {
                        // Create new daily check
                        dailyCheck = new DailyCheck
                        {
                            SampleTestId = id,
                            CheckDate = request.CheckDate.Date,
                            Pdsfru = (double)(request.Pdsfru ?? 0),
                            Couleur1 = request.Couleur1 ?? 0,
                            Couleur2 = request.Couleur2 ?? 0
                        };
                        _context.DailyChecks.Add(dailyCheck);
                    }

                    await _context.SaveChangesAsync();

                    // Handle defect details (only if there are defects to add)
                    if (request.Defects != null && request.Defects.Count > 0)
                    {
                        Console.WriteLine($"Processing {request.Defects.Count} defects for daily check {dailyCheck.Id}");
                            // Remove existing defect details for this daily check
                            var existingDetails = await _context.DailyCheckDetails
                                .Where(d => d.DailyCheckId == dailyCheck.Id)
                                .ToListAsync();
                            _context.DailyCheckDetails.RemoveRange(existingDetails);

                            // Add new defect details
                            foreach (var defect in request.Defects)
                            {
                                var detail = new DailyCheckDetail
                                {
                                    DailyCheckId = dailyCheck.Id,
                                    DefectId = defect.DefectId,
                                    Quantity = defect.Quantity
                                };
                                _context.DailyCheckDetails.Add(detail);
                            }

                            await _context.SaveChangesAsync();
                            Console.WriteLine("Defect details saved successfully");
                    }
                    else
                    {
                        Console.WriteLine("No defects to process (Defects is null or empty)");
                    }

                    return Ok(new { Message = "Daily check saved successfully.", DailyCheckId = dailyCheck.Id });
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"ERROR in PostDailyCheck: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, new { 
                    Message = $"An error occurred: {ex.Message}",
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
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

        // GET: api/samples/{id}/daily-check/{date}
        [HttpGet("{id}/daily-check/{date}")]
        public async Task<ActionResult<DailyCheck>> GetDailyCheck(int id, string date, [FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // Check if sample exists first
                    var sample = await _context.SampleTests.FindAsync(id);
                    if (sample == null)
                    {
                        return Ok(null); // Return null instead of 404
                    }

                    var checkDate = DateTime.Parse(date).Date;
                    Console.WriteLine($"Looking for daily check: SampleId={id}, Date={checkDate}");

                    // Query for daily check - let errors show on console
                    var dailyCheck = await _context.DailyChecks
                        .Include(dc => dc.Details)
                        .FirstOrDefaultAsync(dc => dc.SampleTestId == id && dc.CheckDate.Date == checkDate);

                    if (dailyCheck != null)
                    {
                        Console.WriteLine($"Found daily check: Id={dailyCheck.Id}, Pdsfru={dailyCheck.Pdsfru}, Couleur1={dailyCheck.Couleur1}, Couleur2={dailyCheck.Couleur2}");
                    }
                    else
                    {
                        Console.WriteLine("No daily check found for this sample and date");
                    }

                    // Return the daily check data (null if not found)
                    return Ok(dailyCheck);
                }
            }
            catch (System.Exception ex)
            {
                // Log the error and return null instead of 500
                Console.WriteLine($"Error in GetDailyCheck: {ex.Message}");
                return Ok(null);
            }
        }

        // GET: api/samples/{id}/history
        [HttpGet("{id}/history")]
        public async Task<ActionResult<SampleHistoryDto>> GetSampleHistory(int id, [FromHeader(Name = "X-Database-Name")] string database)
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

                    var dailyChecks = await _context.DailyChecks
                        .Include(dc => dc.Details)
                        .Where(dc => dc.SampleTestId == id)
                        .OrderBy(dc => dc.CheckDate)
                        .ToListAsync();

                    // Fetch Verger Name
                    var vergerName = await _context.palbruts
                        .Where(p => p.numpal == sample.Numpal)
                        .Join(_context.Vergers,
                            p => p.refver,
                            v => v.refver,
                            (p, v) => v.nomver)
                        .FirstOrDefaultAsync();

                    var historyDto = new SampleHistoryDto
                    {
                        Sample = new SampleTestDto
                        {
                            Id = sample.Id,
                            Numpal = sample.Numpal,
                            Coddes = sample.Coddes,
                            Codvar = sample.Codvar,
                            StartDate = sample.StartDate,
                            VergerName = vergerName,
                            InitialFruitCount = sample.InitialFruitCount,
                            Pdsfru = sample.Pdsfru,
                            Couleur1 = sample.Couleur1,
                            Couleur2 = sample.Couleur2,
                            Status = sample.Status,
                            IsCheckedToday = dailyChecks.Any(dc => dc.CheckDate.Date == DateTime.Today)
                        },
                        DailyChecks = dailyChecks.Select(dc => new DailyCheckHistoryDto
                        {
                            Id = dc.Id,
                            CheckDate = dc.CheckDate,
                            Pdsfru = dc.Pdsfru,
                            Couleur1 = dc.Couleur1,
                            Couleur2 = dc.Couleur2,
                            Defects = dc.Details.Select(d => new DefectHistoryDto
                            {
                                DefectId = d.DefectId,
                                Quantity = d.Quantity
                            }).ToList()
                        }).ToList()
                    };

                    return Ok(historyDto);
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
