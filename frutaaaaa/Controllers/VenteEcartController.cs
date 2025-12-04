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

        // PUT: api/vente-ecart/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVente([FromHeader(Name = "X-Database-Name")] string database, int id, [FromBody] VenteUpdateRequest request)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var vente = await _context.Ventes.FindAsync(id);
                    if (vente == null) return NotFound();

                    vente.Numbonvente = request.Numbonvente ?? vente.Numbonvente;
                    vente.Date = request.Date ?? vente.Date;
                    vente.Price = request.Price ?? vente.Price;
                    vente.PoidsTotal = request.PoidsTotal ?? vente.PoidsTotal;
                    vente.MontantTotal = request.MontantTotal ?? vente.MontantTotal;

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Vente updated successfully" });
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

                            // Reset numvent on related ecarts
                            var directEcarts = await _context.EcartDirects.Where(e => e.Numvent == id).ToListAsync();
                            foreach (var ecart in directEcarts)
                            {
                                ecart.Numvent = null;
                                ecart.Pdsvent = null;
                                _context.Entry(ecart).State = EntityState.Modified;
                            }

                            var eEcarts = await _context.EcartEs.Where(e => e.numvent == id).ToListAsync();
                            foreach (var ecart in eEcarts)
                            {
                                ecart.numvent = null;
                                ecart.pdsvent = null;
                                _context.Entry(ecart).State = EntityState.Modified;
                            }

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
        public List<SelectedEcart> SelectedEcarts { get; set; }
    }

    public class SelectedEcart
    {
        public string Table { get; set; } // "ecart_direct" or "ecart_e"
        public int Id { get; set; } // Numpal for direct, numpal for E
        public double? Pdsvent { get; set; }
    }

    public class VenteUpdateRequest
    {
        public int? Numbonvente { get; set; }
        public DateTime? Date { get; set; }
        public double? Price { get; set; }
        public double? PoidsTotal { get; set; }
        public double? MontantTotal { get; set; }
    }
}
