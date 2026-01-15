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
    [Route("api/vente-ecart")]
    [ApiController]
    public class VenteEcartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public VenteEcartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [NonAction]
        public ApplicationDbContext CreateDbContext(string dbName)
        {
            var baseConnectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(baseConnectionString))
            {
                return null;
            }
            var dynamicConnectionString = baseConnectionString.Replace("frutaaaaa_db", dbName);
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(dynamicConnectionString, ServerVersion.AutoDetect(dynamicConnectionString));
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        // GET: api/vente-ecart/ecartdirect/unsold?codtype=1&startDate=2023-01-01&endDate=2023-12-31
        [HttpGet("ecartdirect/unsold")]
        public async Task<ActionResult<IEnumerable<EcartDirect>>> GetUnsoldEcartDirects([FromHeader(Name = "X-Database-Name")] string database, [FromQuery] int codtype, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    IQueryable<EcartDirect> query = _context.EcartDirects
                        .Include(e => e.TypeEcart)
                        .Where(e => e.Numvent == null && e.Codtype == codtype);

                    if (startDate.HasValue)
                    {
                        query = query.Where(e => e.Dtepal >= startDate.Value.Date);
                    }
                    if (endDate.HasValue)
                    {
                        var endDateInclusive = endDate.Value.Date.AddDays(1).AddTicks(-1);
                        query = query.Where(e => e.Dtepal <= endDateInclusive);
                    }

                    return await query.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/vente-ecart/ecart-e/unsold?codtype=1&startDate=2023-01-01&endDate=2023-12-31
        [HttpGet("ecart-e/unsold")]
        public async Task<ActionResult<IEnumerable<EcartE>>> GetUnsoldEcartEs([FromHeader(Name = "X-Database-Name")] string database, [FromQuery] int codtype, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    IQueryable<EcartE> query = _context.EcartEs
                        .Join(_context.EcartDs, e => e.numpal, d => d.numpal, (e, d) => new { e, d })
                        .Where(x => x.e.numvent == null && x.d.codtype == codtype)
                        .Select(x => x.e)
                        .Distinct();

                    if (startDate.HasValue)
                    {
                        query = query.Where(e => e.dtepal >= startDate.Value.Date);
                    }
                    if (endDate.HasValue)
                    {
                        var endDateInclusive = endDate.Value.Date.AddDays(1).AddTicks(-1);
                        query = query.Where(e => e.dtepal <= endDateInclusive);
                    }

                    return await query.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/vente-ecart
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vente>>> GetVentes([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    return await _context.Ventes.Where(v => v.Date != null).OrderByDescending(v => v.Date).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/vente-ecart
        [HttpPost]
        public async Task<IActionResult> CreateVenteEcart([FromHeader(Name = "X-Database-Name")] string database, [FromBody] VenteEcartRequest request)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Create Vente record
                            var vente = new Vente
                            {
                                Numbonvente = request.Numbonvente,
                                Date = request.Date,
                                Price = request.Price,
                                PoidsTotal = request.PoidsTotal,
                                MontantTotal = request.MontantTotal,
                                Numlot = request.Numlot,
                                Codtype = request.Codtype // Map Codtype
                            };
                            _context.Ventes.Add(vente);
                            await _context.SaveChangesAsync();

                            Console.WriteLine($"Created Vente with ID: {vente.Id}");

                            // Create details in vecart_d
                            if (request.Details != null && request.Details.Any())
                            {
                                var details = request.Details.Select(d => new VecartD
                                {
                                    Numvnt = vente.Id,
                                    Refver = d.Refver,
                                    Codgrv = d.Codgrv,
                                    Pds = d.Pds,
                                    Nbrcol = d.Nbrcol,
                                    Numpal = d.Numpal
                                }).ToList();

                                _context.VecartDs.AddRange(details);
                                await _context.SaveChangesAsync();
                            }
                            
                            await transaction.CommitAsync();

                            return Ok(new { message = "Vente created successfully", venteId = vente.Id });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/vente-ecart/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVente([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var vente = await _context.Ventes.FindAsync(id);
                    if (vente == null) return NotFound();

                    var details = await _context.VecartDs
                        .Where(d => d.Numvnt == id)
                        .ToListAsync();

                    return Ok(new { vente, details });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT: api/vente-ecart/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVenteEcart([FromHeader(Name = "X-Database-Name")] string database, int id, [FromBody] VenteEcartRequest request)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var vente = await _context.Ventes.FindAsync(id);
                            if (vente == null) return NotFound();

                            // Update Vente record
                            vente.Numbonvente = request.Numbonvente ?? vente.Numbonvente;
                            vente.Date = request.Date;
                            vente.Price = request.Price;
                            vente.PoidsTotal = request.PoidsTotal;
                            vente.MontantTotal = request.MontantTotal;
                            vente.Numlot = request.Numlot ?? vente.Numlot;
                            vente.Codtype = request.Codtype ?? vente.Codtype; // Update Codtype

                            // Remove old details
                            var oldDetails = await _context.VecartDs.Where(d => d.Numvnt == id).ToListAsync();
                            _context.VecartDs.RemoveRange(oldDetails);
                            await _context.SaveChangesAsync();

                            // Add new details
                            if (request.Details != null && request.Details.Any())
                            {
                                var newDetails = request.Details.Select(d => new VecartD
                                {
                                    Numvnt = vente.Id, // Ensure link
                                    Refver = d.Refver,
                                    Codgrv = d.Codgrv,
                                    Pds = d.Pds,
                                    Nbrcol = d.Nbrcol,
                                    Numpal = d.Numpal
                                }).ToList();

                                _context.VecartDs.AddRange(newDetails);
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            return Ok(new { message = "Vente updated successfully" });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE: api/vente-ecart/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVente([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var vente = await _context.Ventes.FindAsync(id);
                            if (vente == null) return NotFound();

                            // Remove related details
                            var details = await _context.VecartDs.Where(d => d.Numvnt == id).ToListAsync();
                            _context.VecartDs.RemoveRange(details);

                            _context.Ventes.Remove(vente);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            return Ok(new { message = "Vente deleted successfully" });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

    public class VenteEcartRequest
    {
        public int? Numbonvente { get; set; }
        public DateTime Date { get; set; }
        public double Price { get; set; }
        public double PoidsTotal { get; set; }
        public double MontantTotal { get; set; }
        public int? Numlot { get; set; }
        public int? Codtype { get; set; } // Add Codtype
        public List<VenteEcartDetailDto> Details { get; set; }
    }

    public class VenteEcartDetailDto
    {
        public int? Refver { get; set; }
        public int? Codgrv { get; set; } // Changed from Variete
        public double? Pds { get; set; }
        public int? Nbrcol { get; set; }
        public int? Numpal { get; set; }
    }
}
