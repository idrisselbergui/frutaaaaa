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
    public class GestionAvancesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GestionAvancesController(IConfiguration configuration)
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

        // GET: api/GestionAvances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGestionAvances([FromHeader(Name = "X-Database-Name")] string database)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var result = await _context.GestionAvances
                        .OrderByDescending(g => g.Id)
                        .Select(g => new
                        {
                            g.Id,
                            g.Refadh,
                            g.Date,
                            g.Annee,
                            g.Mois,
                            g.Ttdecompte,
                            g.Ttcharges,
                            g.TgExport,
                            g.PrixEstemeMois,
                            g.DecaompteEsteme,
                            g.S1, g.S2, g.S3, g.S4, g.S5,
                            g.RealTS1, g.RealTS2, g.RealTS3, g.RealTS4, g.RealTS5,
                            g.RealDecS1, g.RealDecS2, g.RealDecS3, g.RealDecS4, g.RealDecS5,
                            g.Montant
                        })
                        .ToListAsync();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: api/GestionAvances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetGestionAvance([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var gestionAvance = await _context.GestionAvances
                        .FirstOrDefaultAsync(g => g.Id == id);

                    if (gestionAvance == null)
                    {
                        return NotFound();
                    }

                    var result = new
                    {
                        avance = new
                        {
                            gestionAvance.Id,
                            gestionAvance.Refadh,
                            gestionAvance.Date,
                            gestionAvance.Annee,
                            gestionAvance.Mois,
                            gestionAvance.Ttdecompte,
                            gestionAvance.Ttcharges,
                            gestionAvance.TgExport,
                            gestionAvance.PrixEstemeMois,
                            gestionAvance.DecaompteEsteme,
                            gestionAvance.S1, gestionAvance.S2, gestionAvance.S3, gestionAvance.S4, gestionAvance.S5,
                            gestionAvance.RealTS1, gestionAvance.RealTS2, gestionAvance.RealTS3, gestionAvance.RealTS4, gestionAvance.RealTS5,
                            gestionAvance.RealDecS1, gestionAvance.RealDecS2, gestionAvance.RealDecS3, gestionAvance.RealDecS4, gestionAvance.RealDecS5,
                            gestionAvance.Montant
                        }
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // POST: api/GestionAvances
        [HttpPost]
        public async Task<ActionResult<GestionAvance>> PostGestionAvance([FromHeader(Name = "X-Database-Name")] string database, GestionAvanceRequestDto request)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // --- Duplicate check: one décompte per adherent per month ---
                        bool alreadyExists = await _context.GestionAvances.AnyAsync(g =>
                            g.Refadh == request.Refadh &&
                            g.Annee == request.Annee &&
                            g.Mois == request.Mois);

                        if (alreadyExists)
                        {
                            await transaction.RollbackAsync();
                            return Conflict($"Un décompte existe déjà pour cet adhérent ({request.Refadh}) pour le mois {request.Mois}/{request.Annee}. Veuillez le modifier au lieu d'en créer un nouveau.");
                        }

                        var avance = new GestionAvance
                        {
                            Refadh = request.Refadh,
                            Date = request.Date,
                            Annee = request.Annee,
                            Mois = request.Mois,
                            Ttdecompte = request.Ttdecompte,
                            Ttcharges = request.Ttcharges,
                            TgExport = request.TgExport,
                            PrixEstemeMois = request.PrixEstemeMois,
                            DecaompteEsteme = request.DecaompteEsteme,
                            S1 = request.S1, S2 = request.S2, S3 = request.S3, S4 = request.S4, S5 = request.S5,
                            RealTS1 = request.RealTS1, RealTS2 = request.RealTS2, RealTS3 = request.RealTS3,
                            RealTS4 = request.RealTS4, RealTS5 = request.RealTS5,
                            RealDecS1 = request.RealDecS1, RealDecS2 = request.RealDecS2, RealDecS3 = request.RealDecS3,
                            RealDecS4 = request.RealDecS4, RealDecS5 = request.RealDecS5,
                            Montant = request.Montant
                        };

                        _context.GestionAvances.Add(avance);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return CreatedAtAction(nameof(GetGestionAvance), new { id = avance.Id }, avance);
                    }
                    catch (Exception ex)
                    {
                        var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        await transaction.RollbackAsync();
                        return StatusCode(500, $"Internal server error during save: {innerMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred connecting to database: {ex.Message}");
            }
        }

        // PUT: api/GestionAvances/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGestionAvance([FromHeader(Name = "X-Database-Name")] string database, int id, GestionAvanceRequestDto request)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var existingAvance = await _context.GestionAvances
                            .FirstOrDefaultAsync(g => g.Id == id);

                        if (existingAvance == null)
                            return NotFound();

                        // Update properties
                        existingAvance.Refadh = request.Refadh;
                        existingAvance.Date = request.Date;
                        existingAvance.Annee = request.Annee;
                        existingAvance.Mois = request.Mois;
                        existingAvance.Ttdecompte = request.Ttdecompte;
                        existingAvance.Ttcharges = request.Ttcharges;
                        existingAvance.TgExport = request.TgExport;
                        existingAvance.PrixEstemeMois = request.PrixEstemeMois;
                        existingAvance.DecaompteEsteme = request.DecaompteEsteme;
                        existingAvance.S1 = request.S1; existingAvance.S2 = request.S2; existingAvance.S3 = request.S3;
                        existingAvance.S4 = request.S4; existingAvance.S5 = request.S5;
                        existingAvance.RealTS1 = request.RealTS1; existingAvance.RealTS2 = request.RealTS2;
                        existingAvance.RealTS3 = request.RealTS3; existingAvance.RealTS4 = request.RealTS4;
                        existingAvance.RealTS5 = request.RealTS5;
                        existingAvance.RealDecS1 = request.RealDecS1; existingAvance.RealDecS2 = request.RealDecS2;
                        existingAvance.RealDecS3 = request.RealDecS3; existingAvance.RealDecS4 = request.RealDecS4;
                        existingAvance.RealDecS5 = request.RealDecS5;
                        existingAvance.Montant = request.Montant;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return NoContent();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, $"Internal server error during update: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred connecting to database: {ex.Message}");
            }
        }

        // GET: api/GestionAvances/yearly-report
        [HttpGet("yearly-report")]
        public async Task<ActionResult<object>> GetYearlyReport(
            [FromHeader(Name = "X-Database-Name")] string database,
            [FromQuery] int refadh,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // Adherent name
                    var adherent = await _context.Adherents.FirstOrDefaultAsync(a => a.Refadh == refadh);

                    int yearDebut = dateDebut.Year, yearFin = dateFin.Year;

                    // All gestionavances for this adherent across the full date range years
                    var avances = await _context.GestionAvances
                        .Where(g => g.Refadh == refadh && g.Annee >= yearDebut && g.Annee <= yearFin)
                        .ToListAsync();

                    // All charges joined with charge definitions across the full date range
                    var rawCharges = await _context.AdherentCharges
                        .Where(ac => ac.Refadh == refadh
                                  && ac.Date >= dateDebut && ac.Date <= dateFin)
                        .Join(_context.Charges,
                              ac => ac.Idcharge,
                              c => c.Idcharge,
                              (ac, c) => new { ac.Date, ac.Montant, c.Label, c.Typecharge })
                        .ToListAsync();

                    // Distinct charge types
                    var chargeTypes = rawCharges
                        .Select(c => string.IsNullOrEmpty(c.Typecharge) ? c.Label : c.Typecharge)
                        .Distinct().OrderBy(t => t).ToList();

                    // ── Pre-fetch export data for auto-estimation (months with no saved décompte) ──
                    var validVergerIds = await _context.Vergers
                        .Where(v => v.refadh == refadh)
                        .Select(v => v.refver)
                        .ToListAsync();

                    // All palette details for this adherent in the date range
                    var allExportsQuery = from pd in _context.Palette_ds
                                         join p in _context.Palettes on pd.numpal equals p.numpal
                                         join v in _context.Varietes on pd.codvar equals v.codvar
                                         where pd.refver.HasValue && validVergerIds.Contains(pd.refver.Value)
                                            && p.dtepal >= dateDebut && p.dtepal <= dateFin
                                         select new { pd.pdscom, p.dtepal, v.codgrv };

                    var allExports = validVergerIds.Any()
                        ? await allExportsQuery.ToListAsync()
                        : await allExportsQuery.Where(x => false).ToListAsync();

                    // All prix estimatifs for the full period
                    var allPrices = await _context.PrixEstimatifs
                        .Where(pe => pe.Annee >= yearDebut && pe.Annee <= yearFin)
                        .ToListAsync();

                    // Helper: compute week-bucketed tonnage totals from export data for a given month
                    (double s1, double s2, double s3, double s4, double s5) ComputeExportTonnage(int yr, int mois)
                    {
                        if (!validVergerIds.Any()) return (0, 0, 0, 0, 0);
                        int daysInMonth = DateTime.DaysInMonth(yr, mois);
                        DateTime firstDay = new DateTime(yr, mois, 1);
                        DateTime lastDay = new DateTime(yr, mois, daysInMonth);
                        int off = ((int)firstDay.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                        var monthWeeks = new List<DateTime>();
                        DateTime mon = firstDay.AddDays(-off);
                        while (mon <= lastDay)
                        {
                            DateTime sun = mon.AddDays(6);
                            DateTime os = mon < firstDay ? firstDay : mon;
                            DateTime oe = sun > lastDay ? lastDay : sun;
                            if ((int)(oe - os).TotalDays + 1 >= 4) monthWeeks.Add(mon);
                            mon = mon.AddDays(7);
                        }
                        var weekToSemaine = new Dictionary<DateTime, int>();
                        for (int i = 0; i < monthWeeks.Count; i++) weekToSemaine[monthWeeks[i]] = i + 1;
                        double ts1 = 0, ts2 = 0, ts3 = 0, ts4 = 0, ts5 = 0;
                        foreach (var item in allExports)
                        {
                            if (!item.dtepal.HasValue) continue;
                            DateTime dtepal = ((DateTime)item.dtepal.Value).Date;
                            if (dtepal.Year != yr || dtepal.Month != mois) continue;
                            int dayOff = ((int)dtepal.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                            DateTime itemMon = dtepal.AddDays(-dayOff);
                            if (!weekToSemaine.TryGetValue(itemMon, out int sem)) continue;
                            double kg = (double)(item.pdscom ?? 0);
                            switch (sem) { case 1: ts1 += kg; break; case 2: ts2 += kg; break; case 3: ts3 += kg; break; case 4: ts4 += kg; break; case 5: ts5 += kg; break; }
                        }
                        return (ts1, ts2, ts3, ts4, ts5);
                    }

                    // Helper: average prix estimatif for a given month across all variety groups
                    double ComputeEstimatedPrix(int yr, int mois)
                    {
                        var monthPrices = allPrices.Where(pe => pe.Annee == yr && pe.Mois == mois).ToList();
                        if (!monthPrices.Any()) return 0;
                        return monthPrices.Average(pe => (double)pe.PrixEstime);
                    }

                    // Helper: valid Mon-Sun week ranges for a given year/month
                    // Uses the full Mon-Sun span (not clipped to month) so charges on spill-over
                    // days (e.g. Feb 1 in a Jan-majority week) are still attributed correctly.
                    List<(DateTime Start, DateTime End)> GetValidRanges(int yr, int mois)
                    {
                        int dim = DateTime.DaysInMonth(yr, mois);
                        var firstDay = new DateTime(yr, mois, 1);
                        var lastDay  = new DateTime(yr, mois, dim);
                        int off = ((int)firstDay.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                        var ranges = new List<(DateTime, DateTime)>();
                        var monday = firstDay.AddDays(-off);
                        while (monday <= lastDay)
                        {
                            var sunday = monday.AddDays(6);
                            // Clipped overlap used only for the majority-day (>= 4) check
                            var os = monday < firstDay ? firstDay : monday;
                            var oe = sunday > lastDay  ? lastDay  : sunday;
                            if ((int)(oe - os).TotalDays + 1 >= 4)
                                ranges.Add((monday, sunday)); // store FULL week, not clipped
                            monday = monday.AddDays(7);
                        }
                        return ranges;
                    }

                    // Build one entry per month in the date range
                    var monthEntries = new List<(int Year, int Mois)>();
                    var cur = new DateTime(yearDebut, dateDebut.Month, 1);
                    var end = new DateTime(yearFin, dateFin.Month, 1);
                    while (cur <= end)
                    {
                        monthEntries.Add((cur.Year, cur.Month));
                        cur = cur.AddMonths(1);
                    }

                    var months = monthEntries.Select(e =>
                    {
                        int yr = e.Year; int mois = e.Mois;
                        var ga = avances.FirstOrDefault(a => a.Annee == yr && a.Mois == mois);

                        var validRanges = GetValidRanges(yr, mois);
                        var mc = rawCharges
                            .Where(c => c.Date.Year == yr
                                     && validRanges.Any(r => c.Date.Date >= r.Start && c.Date.Date <= r.End))
                            .ToList();

                        var chargesByType = mc
                            .GroupBy(c => string.IsNullOrEmpty(c.Typecharge) ? c.Label : c.Typecharge)
                            .ToDictionary(g => g.Key, g => g.Sum(x => x.Montant));

                        var totalCharges = mc.Sum(c => c.Montant);

                        if (ga != null)
                        {
                            // ── Saved record exists: use stored values ──
                            double storedPrice = ga.PrixEstemeMois ?? 0;
                            double tgTotal = ga.TgExport ?? 0;
                            double decompteEst = ga.DecaompteEsteme ?? 0;
                            double pricePerKg = storedPrice > 0 ? storedPrice
                                              : (tgTotal > 0 ? decompteEst / tgTotal : 0);

                            double WeekDh(double? realDec, double? tonnage) =>
                                (realDec ?? 0) > 0 ? realDec!.Value : (tonnage ?? 0) * pricePerKg;

                            double realDecS1 = WeekDh(ga.RealDecS1, ga.S1);
                            double realDecS2 = WeekDh(ga.RealDecS2, ga.S2);
                            double realDecS3 = WeekDh(ga.RealDecS3, ga.S3);
                            double realDecS4 = WeekDh(ga.RealDecS4, ga.S4);
                            double realDecS5 = WeekDh(ga.RealDecS5, ga.S5);
                            double realDecTotal = realDecS1 + realDecS2 + realDecS3 + realDecS4 + realDecS5;

                            double realTS1 = ga.RealTS1 ?? 0, realTS2 = ga.RealTS2 ?? 0,
                                   realTS3 = ga.RealTS3 ?? 0, realTS4 = ga.RealTS4 ?? 0,
                                   realTS5 = ga.RealTS5 ?? 0;
                            double realTonnageSum = realTS1 + realTS2 + realTS3 + realTS4 + realTS5;
                            double effectiveTonnage = realTonnageSum > 0 ? realTonnageSum : tgTotal;

                            // Calculate true estimated accompt from exact db exports & estimated prices
                            var (ts1, ts2, ts3, ts4, ts5) = ComputeExportTonnage(yr, mois);
                            double pricePerKgEst = ComputeEstimatedPrix(yr, mois);
                            double trueEstimatedAccompt = (ts1 + ts2 + ts3 + ts4 + ts5) * pricePerKgEst;

                            return new
                            {
                                Mois = mois, Annee = yr, IsEstimated = false,
                                Decompte = realDecTotal,
                                TgExport = effectiveTonnage,
                                AccomptEstime = trueEstimatedAccompt,
                                S1 = ga.S1 ?? 0, S2 = ga.S2 ?? 0, S3 = ga.S3 ?? 0,
                                S4 = ga.S4 ?? 0, S5 = ga.S5 ?? 0,
                                RealTS1 = realTS1, RealTS2 = realTS2, RealTS3 = realTS3,
                                RealTS4 = realTS4, RealTS5 = realTS5,
                                RealDecS1 = realDecS1, RealDecS2 = realDecS2,
                                RealDecS3 = realDecS3, RealDecS4 = realDecS4,
                                RealDecS5 = realDecS5,
                                IsRealDecS1 = (ga.RealDecS1 ?? 0) > 0,
                                IsRealDecS2 = (ga.RealDecS2 ?? 0) > 0,
                                IsRealDecS3 = (ga.RealDecS3 ?? 0) > 0,
                                IsRealDecS4 = (ga.RealDecS4 ?? 0) > 0,
                                IsRealDecS5 = (ga.RealDecS5 ?? 0) > 0,
                                ChargesByType = chargesByType,
                                TotalCharges = totalCharges,
                                Resultat = realDecTotal - totalCharges
                            };
                        }
                        else
                        {
                            // ── No saved record: auto-estimate from export data + prix estimatifs ──
                            var (ts1, ts2, ts3, ts4, ts5) = ComputeExportTonnage(yr, mois);
                            double tgTotal = ts1 + ts2 + ts3 + ts4 + ts5;
                            double pricePerKg = ComputeEstimatedPrix(yr, mois);

                            double rdS1 = ts1 * pricePerKg, rdS2 = ts2 * pricePerKg,
                                   rdS3 = ts3 * pricePerKg, rdS4 = ts4 * pricePerKg,
                                   rdS5 = ts5 * pricePerKg;
                            double realDecTotal = rdS1 + rdS2 + rdS3 + rdS4 + rdS5;

                            return new
                            {
                                Mois = mois, Annee = yr, IsEstimated = true,
                                Decompte = realDecTotal,
                                TgExport = tgTotal,
                                AccomptEstime = realDecTotal,
                                S1 = ts1, S2 = ts2, S3 = ts3, S4 = ts4, S5 = ts5,
                                RealTS1 = ts1, RealTS2 = ts2, RealTS3 = ts3, RealTS4 = ts4, RealTS5 = ts5,
                                RealDecS1 = rdS1, RealDecS2 = rdS2, RealDecS3 = rdS3,
                                RealDecS4 = rdS4, RealDecS5 = rdS5,
                                IsRealDecS1 = false, IsRealDecS2 = false,
                                IsRealDecS3 = false, IsRealDecS4 = false, IsRealDecS5 = false,
                                ChargesByType = chargesByType,
                                TotalCharges = totalCharges,
                                Resultat = realDecTotal - totalCharges
                            };
                        }
                    }).ToList();

                    return Ok(new
                    {
                        AdherentName = adherent?.Nomadh ?? $"Adhérent {refadh}",
                        DateDebut = dateDebut.ToString("yyyy-MM-dd"),
                        DateFin = dateFin.ToString("yyyy-MM-dd"),
                        ChargeTypes = chargeTypes,
                        Months = months
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // DELETE: api/GestionAvances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGestionAvance([FromHeader(Name = "X-Database-Name")] string database, int id)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    var gestionAvance = await _context.GestionAvances.FindAsync(id);
                    if (gestionAvance == null)
                    {
                        return NotFound();
                    }

                    _context.GestionAvances.Remove(gestionAvance);
                    await _context.SaveChangesAsync();

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred connecting to database: {ex.Message}");
            }
        }
    
        // GET: api/GestionAvances/wizard-details
        [HttpGet("wizard-details")]
        public async Task<ActionResult<object>> GetWizardDetails(
            [FromHeader(Name = "X-Database-Name")] string database,
            [FromQuery] int refadh,
            [FromQuery] int annee,
            [FromQuery] int mois)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    // Find all vergers for this adherent
                    var validVergerIds = await _context.Vergers
                        .Where(v => v.refadh == refadh)
                        .Select(v => v.refver)
                        .ToListAsync();

                    if (!validVergerIds.Any())
                    {
                        return Ok(new List<object>()); // empty
                    }

                    int daysInMonth = DateTime.DaysInMonth(annee, mois);
                    DateTime dteStart = new DateTime(annee, mois, 1);
                    DateTime dteEnd = new DateTime(annee, mois, daysInMonth).AddDays(1).AddTicks(-1);

                    var exportQuery = from pd in _context.Palette_ds
                                      join p in _context.Palettes on pd.numpal equals p.numpal
                                      join v in _context.Varietes on pd.codvar equals v.codvar
                                      join g in _context.grpvars on v.codgrv equals g.codgrv
                                      where pd.refver.HasValue && validVergerIds.Contains(pd.refver.Value)
                                         && p.dtepal >= dteStart && p.dtepal <= dteEnd
                                      select new { pd, p, v, g };

                    var exports = await exportQuery.ToListAsync();

                    // --- Real calendar week bucketing (Mon-Sun, majority rule) ---
                    // Step 1: Find all Mon-Sun weeks that "belong" to this month.
                    // A week belongs to a month if >= 4 of its days fall in that month.
                    DateTime firstDayOfMonth = new DateTime(annee, mois, 1);
                    DateTime lastDayOfMonth = new DateTime(annee, mois, daysInMonth);

                    // Find the Monday of the week containing the 1st of the month
                    int offset = ((int)firstDayOfMonth.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    DateTime firstMonday = firstDayOfMonth.AddDays(-offset);

                    // Collect weeks (by their Monday) that belong to this month
                    var monthWeeks = new List<DateTime>();
                    DateTime weekMonday = firstMonday;
                    while (weekMonday <= lastDayOfMonth)
                    {
                        DateTime weekSunday = weekMonday.AddDays(6);
                        // Count how many days of this Mon-Sun week fall in the target month
                        DateTime overlapStart = weekMonday < firstDayOfMonth ? firstDayOfMonth : weekMonday;
                        DateTime overlapEnd = weekSunday > lastDayOfMonth ? lastDayOfMonth : weekSunday;
                        int daysInTargetMonth = (int)(overlapEnd - overlapStart).TotalDays + 1;
                        if (daysInTargetMonth >= 4)
                            monthWeeks.Add(weekMonday); // This week belongs to this month
                        weekMonday = weekMonday.AddDays(7);
                    }

                    // Build a lookup: Monday date -> semaine number (1-5)
                    var weekToSemaine = new Dictionary<DateTime, int>();
                    for (int i = 0; i < monthWeeks.Count; i++)
                        weekToSemaine[monthWeeks[i]] = i + 1;

                    // Step 2: Group export tonnage by variety group and semaine
                    var dict = new Dictionary<int, WizardWeeklyDetailDto>();

                    foreach (var item in exports)
                    {
                        if (!item.p.dtepal.HasValue) continue;

                        // Find the Monday of the week this export date belongs to
                        var dtepal = item.p.dtepal.Value.Date;
                        int dayOffset = ((int)dtepal.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                        DateTime itemWeekMonday = dtepal.AddDays(-dayOffset);

                        // Skip if this week belongs to a neighbouring month
                        if (!weekToSemaine.TryGetValue(itemWeekMonday, out int semaine))
                            continue;

                        var codgrv = item.v.codgrv;
                        if (!dict.ContainsKey(codgrv))
                        {
                            dict[codgrv] = new WizardWeeklyDetailDto
                            {
                                CodGrv = codgrv,
                                NomGrv = item.g.nomgrv,
                                TonnageS1 = 0, TonnageS2 = 0, TonnageS3 = 0,
                                TonnageS4 = 0, TonnageS5 = 0,
                                PrixEstime = 0
                            };
                        }

                        var pdscom = (double)(item.pd.pdscom ?? 0);
                        switch (semaine)
                        {
                            case 1: dict[codgrv].TonnageS1 += pdscom; break;
                            case 2: dict[codgrv].TonnageS2 += pdscom; break;
                            case 3: dict[codgrv].TonnageS3 += pdscom; break;
                            case 4: dict[codgrv].TonnageS4 += pdscom; break;
                            case 5: dict[codgrv].TonnageS5 += pdscom; break;
                        }
                    }

                    // Get Prices
                    var codgrvs = dict.Keys.ToList();
                    var prices = await _context.PrixEstimatifs
                        .Where(pe => pe.Annee == annee && pe.Mois == mois && codgrvs.Contains(pe.CodGrv))
                        .ToListAsync();

                    foreach (var price in prices)
                    {
                        if (dict.ContainsKey(price.CodGrv))
                        {
                            dict[price.CodGrv].PrixEstime = price.PrixEstime;
                        }
                    }

                    return Ok(dict.Values.OrderBy(v => v.NomGrv));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }

    public class WizardWeeklyDetailDto
    {
        public int CodGrv { get; set; }
        public string NomGrv { get; set; }
        public double TonnageS1 { get; set; }
        public double TonnageS2 { get; set; }
        public double TonnageS3 { get; set; }
        public double TonnageS4 { get; set; }
        public double TonnageS5 { get; set; }
        public double PrixEstime { get; set; }
    }

    public class GestionAvanceRequestDto
    {
        public int? Refadh { get; set; }
        public DateTime? Date { get; set; }
        public int? Annee { get; set; }
        public int? Mois { get; set; }
        public double? Ttdecompte { get; set; }
        public double? Ttcharges { get; set; }
        public double? TgExport { get; set; }
        public double? PrixEstemeMois { get; set; }
        public double? DecaompteEsteme { get; set; }
        public double? S1 { get; set; }
        public double? S2 { get; set; }
        public double? S3 { get; set; }
        public double? S4 { get; set; }
        public double? S5 { get; set; }
        public double? RealTS1 { get; set; }
        public double? RealTS2 { get; set; }
        public double? RealTS3 { get; set; }
        public double? RealTS4 { get; set; }
        public double? RealTS5 { get; set; }
        public double? RealDecS1 { get; set; }
        public double? RealDecS2 { get; set; }
        public double? RealDecS3 { get; set; }
        public double? RealDecS4 { get; set; }
        public double? RealDecS5 { get; set; }
        public double? Montant { get; set; }
    }
}
