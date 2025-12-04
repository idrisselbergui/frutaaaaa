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
                    var query = _context.EcartDirects
                        .Include(e => e.TypeEcart)
                        .Where(e => e.Codtype == codtype && e.Numvent == null);

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
                    var query = _context.EcartEs
                        .Join(_context.EcartDs, e => e.numpal, d => d.numpal, (e, d) => new { e, d })
                        .Where(x => x.d.codtype == codtype && x.e.numvent == null);

                    if (startDate.HasValue)
                    {
                        query = query.Where(x => x.e.dtepal >= startDate.Value.Date);
                    }
                    if (endDate.HasValue)
                    {
                        var endDateInclusive = endDate.Value.Date.AddDays(1).AddTicks(-1);
                        query = query.Where(x => x.e.dtepal <= endDateInclusive);
                    }

                    return await query.Select(x => x.e).Distinct().ToListAsync();
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
                                MontantTotal = request.MontantTotal
                            };
                            _context.Ventes.Add(vente);
                            await _context.SaveChangesAsync();

                            // Update selected ecarts
                            foreach (var selectedEcart in request.SelectedEcarts)
                            {
                                if (selectedEcart.Table == "ecart_direct")
                                {
                                    var ecart = await _context.EcartDirects.FindAsync(selectedEcart.Id);
                                    if (ecart != null)
                                    {
                                        ecart.Numvent = vente.Id;
                                        ecart.Pdsvent = selectedEcart.Pdsvent;
                                        _context.Entry(ecart).State = EntityState.Modified;
                                    }
                                }
                                else if (selectedEcart.Table == "ecart_e")
                                {
                                    var ecart = await _context.EcartEs.FindAsync(selectedEcart.Id);
                                    if (ecart != null)
                                    {
                                        ecart.numvent = vente.Id;
                                        ecart.pdsvent = selectedEcart.Pdsvent;
                                        _context.Entry(ecart).State = EntityState.Modified;
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
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
    }

    public class VenteEcartRequest
    {
        public int? Numbonvente { get; set; }
        public DateTime Date { get; set; }
        public double Price { get; set; }
        public double PoidsTotal { get; set; }
        public double MontantTotal { get; set; }
        public List<SelectedEcart> SelectedEcarts { get; set; }
    }

    public class SelectedEcart
    {
        public string Table { get; set; } // "ecart_direct" or "ecart_e"
        public int Id { get; set; } // Numpal for direct, numpal for E
        public double? Pdsvent { get; set; }
    }
}
