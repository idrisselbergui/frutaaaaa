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
                                Numlot = request.Numlot
                            };
                            _context.Ventes.Add(vente);
                            await _context.SaveChangesAsync();

                            Console.WriteLine($"Created Vente with ID: {vente.Id}");

                            // Update selected ecarts
                            foreach (var selectedEcart in request.SelectedEcarts)
                            {
                                if (selectedEcart.Table == "ecart_direct")
                                {
                                    var ecart = await _context.EcartDirects.FindAsync(selectedEcart.Id);
                                    if (ecart != null)
                                    {
                                        Console.WriteLine($"Found EcartDirect with ID: {selectedEcart.Id}, associating with Vente ID: {vente.Id}");
                                        ecart.Numvent = vente.Id;
                                        ecart.Pdsvent = selectedEcart.Pdsvent;
                                        _context.Entry(ecart).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"EcartDirect with ID: {selectedEcart.Id} not found.");
                                    }
                                }
                                else if (selectedEcart.Table == "ecart_e")
                                {
                                    var ecart = await _context.EcartEs.FindAsync(selectedEcart.Id);
                                    if (ecart != null)
                                    {
                                        Console.WriteLine($"Found EcartE with ID: {selectedEcart.Id}, associating with Vente ID: {vente.Id}");
                                        ecart.numvent = vente.Id;
                                        ecart.pdsvent = selectedEcart.Pdsvent;
                                        _context.Entry(ecart).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"EcartE with ID: {selectedEcart.Id} not found.");
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

                    var directEcarts = await _context.EcartDirects
                        .Where(e => e.Numvent == id)
                        .Select(e => new {
                            table = "ecart_direct",
                            id = e.Numpal,
                            pdsvent = e.Pdsvent,
                            refver = e.Refver,
                            codvar = e.Codvar,
                            numbl = e.Numbl
                        })
                        .ToListAsync();

                    var eEcarts = await _context.EcartEs
                        .Where(e => e.numvent == id)
                        .Select(e => new {
                            table = "ecart_e",
                            id = e.numpal,
                            pdsvent = e.pdsvent,
                            refver = e.refver,
                            codvar = e.codvar
                        })
                        .ToListAsync();

                    var ecarts = directEcarts.Cast<object>().Concat(eEcarts.Cast<object>()).ToList();

                    Console.WriteLine($"Found {ecarts.Count} ecarts for Vente ID: {id}");

                    var codtype = 0;
                    var directType = await _context.EcartDirects.Where(e => e.Numvent == id).Select(e => e.Codtype).FirstOrDefaultAsync();
                    if (directType != null && directType != 0)
                    {
                        codtype = directType.Value;
                    }
                    else
                    {
                        var eType = await _context.EcartEs
                            .Join(_context.EcartDs, e => e.numpal, d => d.numpal, (e, d) => new { e, d })
                            .Where(x => x.e.numvent == id)
                            .Select(x => x.d.codtype)
                            .FirstOrDefaultAsync();
                        codtype = eType;
                    }

                    return Ok(new { vente, ecarts, codtype });
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

                            // Reset old ecarts
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
                            await _context.SaveChangesAsync();

                            // Set new selected ecarts
                            foreach (var selectedEcart in request.SelectedEcarts)
                            {
                                if (selectedEcart.Table == "ecart_direct")
                                {
                                    var ecart = await _context.EcartDirects.FindAsync(selectedEcart.Id);
                                    if (ecart != null && ecart.Numvent == null) // Check not taken
                                    {
                                        ecart.Numvent = vente.Id;
                                        ecart.Pdsvent = selectedEcart.Pdsvent;
                                        _context.Entry(ecart).State = EntityState.Modified;
                                    }
                                }
                                else if (selectedEcart.Table == "ecart_e")
                                {
                                    var ecart = await _context.EcartEs.FindAsync(selectedEcart.Id);
                                    if (ecart != null && ecart.numvent == null) // Check not taken
                                    {
                                        ecart.numvent = vente.Id;
                                        ecart.pdsvent = selectedEcart.Pdsvent;
                                        _context.Entry(ecart).State = EntityState.Modified;
                                    }
                                }
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
        public int? Numlot { get; set; }
        public List<SelectedEcart> SelectedEcarts { get; set; }
    }

    public class SelectedEcart
    {
        public string Table { get; set; } // "ecart_direct" or "ecart_e"
        public int Id { get; set; } // Numpal for direct, numpal for E
        public double? Pdsvent { get; set; }
    }
}
