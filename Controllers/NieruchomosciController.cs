using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProperTax.Data;
using ProperTax.Models;

namespace ProperTax.Controllers
{
    [Authorize]
    public class NieruchomosciController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NieruchomosciController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Nieruchomosci
        public async Task<IActionResult> Index()
        {
            return View(await _context.Nieruchomosci.ToListAsync());
        }

        // GET: Nieruchomosci/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nieruchomosc = await _context.Nieruchomosci
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nieruchomosc == null)
            {
                return NotFound();
            }

            return View(nieruchomosc);
        }

        // GET: Nieruchomosci/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Nieruchomosci/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NrKsiegiWieczystej,Adres,NrObrebu,IdDzialki,Udzial100m,PowierzchniaUzytkowaBudynku,KategoriaGruntyPowierzchniaDzialkiMieszkalnej,KategoriaGruntyPowierzchniaDzialkiNiemieszkalnej,KategoriaBudynkiPowierzchniaUzytkowaMieszkalna,KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna,KategoriaWartoscBudowli,FormaWladania,DataKupienia,DataSprzedania,Komentarz")] Nieruchomosc nieruchomosc)
        {
            //Dzieki tej ifologii w formularzu zostaną przesłane domyślne wartości gdy użytkownik zostawi te pola puste
            //Probowalem ustawic domyslne wartosci w klasie Nieruchomosc ale nie dzialalo i do bazy przesylane byly wartosci null
            if (!nieruchomosc.NrObrebu.HasValue)
            {
                nieruchomosc.NrObrebu = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.IdDzialki.HasValue)
            {
                nieruchomosc.IdDzialki = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.PowierzchniaUzytkowaBudynku.HasValue)
            {
                nieruchomosc.PowierzchniaUzytkowaBudynku = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaGruntyPowierzchniaDzialkiMieszkalnej.HasValue)
            {
                nieruchomosc.KategoriaGruntyPowierzchniaDzialkiMieszkalnej = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaGruntyPowierzchniaDzialkiNiemieszkalnej.HasValue)
            {
                nieruchomosc.KategoriaGruntyPowierzchniaDzialkiNiemieszkalnej = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaMieszkalna.HasValue)
            {
                nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaMieszkalna = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna.HasValue)
            {
                nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaWartoscBudowli.HasValue)
            {
                nieruchomosc.KategoriaWartoscBudowli = 0; // Ustaw domyślną wartość
            }
            if (string.IsNullOrWhiteSpace(nieruchomosc.Komentarz))
            {
                nieruchomosc.Komentarz = ""; // Ustaw domyślną wartość
            }
            if (string.IsNullOrWhiteSpace(nieruchomosc.FormaWladania))
            {
                nieruchomosc.FormaWladania = "własność"; // Ustaw domyślną wartość
            }
            nieruchomosc.Udzial100m = string.Concat((Math.Max(nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaMieszkalna ?? 0, nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna ?? 0) * 100).ToString(), "/", (nieruchomosc.PowierzchniaUzytkowaBudynku * 100).ToString());

            //Sprawdza czy data kupienia jest mniejsza niz sprzedania
            if (nieruchomosc.DataSprzedania.HasValue)
            {
                if (nieruchomosc.DataSprzedania < nieruchomosc.DataKupienia)
                {
                    ModelState.AddModelError("DataSprzedania", "Data sprzedaży nie może być wcześniejsza niż data kupienia.");
                }
            }

            // Sprawdzenie, czy rok zakupu istnieje w tabeli StawkiPodatkow
            bool rokZakupuIstnieje = _context.StawkiPodatkow.Any(s => s.Rok == nieruchomosc.DataKupienia.Year);
            if (!rokZakupuIstnieje)
            {
                ModelState.AddModelError("DataKupienia", "Przed wpisaniem daty z tym rokiem musisz dodać go w Stawki podatków.");
            }

            // Sprawdzenie, czy rok sprzedaży istnieje w tabeli StawkiPodatkow
            if (nieruchomosc.DataSprzedania.HasValue)
            {
                bool rokSprzedazyIstnieje = _context.StawkiPodatkow.Any(s => s.Rok == nieruchomosc.DataSprzedania.Value.Year);
                if (!rokSprzedazyIstnieje)
                {
                    ModelState.AddModelError("DataSprzedania", "Przed wpisaniem daty z tym rokiem musisz dodać go w Stawki podatków.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(nieruchomosc);
                await AktualizujWszystkieSumyPowierzchni();
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nieruchomosc);
        }

        // GET: Nieruchomosci/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nieruchomosc = await _context.Nieruchomosci.FindAsync(id);
            if (nieruchomosc == null)
            {
                return NotFound();
            }
            return View(nieruchomosc);
        }

        // POST: Nieruchomosci/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NrKsiegiWieczystej,Adres,NrObrebu,IdDzialki,Udzial100m,PowierzchniaUzytkowaBudynku,KategoriaGruntyPowierzchniaDzialkiMieszkalnej,KategoriaGruntyPowierzchniaDzialkiNiemieszkalnej,KategoriaBudynkiPowierzchniaUzytkowaMieszkalna,KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna,KategoriaWartoscBudowli,FormaWladania,DataKupienia,DataSprzedania,Komentarz")] Nieruchomosc nieruchomosc)
        {
            if (id != nieruchomosc.Id)
            {
                return NotFound();
            }

            //Dzieki tej ifologii w formularzu zostaną przesłane domyślne wartości gdy użytkownik zostawi te pola puste
            //Probowalem ustawic domyslne wartosci w klasie Nieruchomosc ale nie dzialalo i do bazy przesylane byly wartosci null
            if (!nieruchomosc.NrObrebu.HasValue)
            {
                nieruchomosc.NrObrebu = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.IdDzialki.HasValue)
            {
                nieruchomosc.IdDzialki = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.PowierzchniaUzytkowaBudynku.HasValue)
            {
                nieruchomosc.PowierzchniaUzytkowaBudynku = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaGruntyPowierzchniaDzialkiMieszkalnej.HasValue)
            {
                nieruchomosc.KategoriaGruntyPowierzchniaDzialkiMieszkalnej = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaGruntyPowierzchniaDzialkiNiemieszkalnej.HasValue)
            {
                nieruchomosc.KategoriaGruntyPowierzchniaDzialkiNiemieszkalnej = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaMieszkalna.HasValue)
            {
                nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaMieszkalna = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna.HasValue)
            {
                nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna = 0; // Ustaw domyślną wartość
            }
            if (!nieruchomosc.KategoriaWartoscBudowli.HasValue)
            {
                nieruchomosc.KategoriaWartoscBudowli = 0; // Ustaw domyślną wartość
            }
            if (string.IsNullOrWhiteSpace(nieruchomosc.Komentarz))
            {
                nieruchomosc.Komentarz = ""; // Ustaw domyślną wartość
            }
            if (string.IsNullOrWhiteSpace(nieruchomosc.FormaWladania))
            {
                nieruchomosc.FormaWladania = "własność"; // Ustaw domyślną wartość
            }
            nieruchomosc.Udzial100m = string.Concat((Math.Max(nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaMieszkalna ?? 0, nieruchomosc.KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna ?? 0) * 100).ToString(), "/", (nieruchomosc.PowierzchniaUzytkowaBudynku * 100).ToString());

            //Sprawdza czy data kupienia jest mniejsza niz sprzedania
            if (nieruchomosc.DataSprzedania.HasValue)
            {
                if (nieruchomosc.DataSprzedania < nieruchomosc.DataKupienia)
                {
                    ModelState.AddModelError("DataSprzedania", "Data sprzedaży nie może być wcześniejsza niż data kupienia.");
                }
            }

            // Sprawdzenie, czy rok zakupu istnieje w tabeli StawkiPodatkow
            bool rokZakupuIstnieje = _context.StawkiPodatkow.Any(s => s.Rok == nieruchomosc.DataKupienia.Year);
            if (!rokZakupuIstnieje)
            {
                ModelState.AddModelError("DataKupienia", "Przed wpisaniem daty z tym rokiem musisz dodać go w Stawki podatków.");
            }

            // Sprawdzenie, czy rok sprzedaży istnieje w tabeli StawkiPodatkow
            if (nieruchomosc.DataSprzedania.HasValue)
            {
                bool rokSprzedazyIstnieje = _context.StawkiPodatkow.Any(s => s.Rok == nieruchomosc.DataSprzedania.Value.Year);
                if (!rokSprzedazyIstnieje)
                {
                    ModelState.AddModelError("DataSprzedania", "Przed wpisaniem daty z tym rokiem musisz dodać go w Stawki podatków.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nieruchomosc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NieruchomoscExists(nieruchomosc.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(nieruchomosc);
        }

        // GET: Nieruchomosci/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nieruchomosc = await _context.Nieruchomosci
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nieruchomosc == null)
            {
                return NotFound();
            }

            return View(nieruchomosc);
        }

        // POST: Nieruchomosci/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nieruchomosc = await _context.Nieruchomosci.FindAsync(id);
            if (nieruchomosc != null)
            {
                _context.Nieruchomosci.Remove(nieruchomosc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NieruchomoscExists(int id)
        {
            return _context.Nieruchomosci.Any(e => e.Id == id);
        }

        public async Task AktualizujWszystkieSumyPowierzchni()
        {
            // Pobierz wszystkie nieruchomości
            var wszystkieMiesiace = await _context.SumyPowierzchni.ToListAsync();

            foreach (var miesiac in wszystkieMiesiace)
            {
                DateTime aktualnyRokMiesiacPoczatek = new DateTime(miesiac.RokMiesiac.Year, miesiac.RokMiesiac.Month, 1);
                DateTime aktualnyRokMiesiacKoniec = aktualnyRokMiesiacPoczatek.AddMonths(1).AddDays(-1);

                // Przechwytujemy wszystkie nieruchomości, które były posiadane w danym miesiącu
                /*var nieruchomosci = await _context.Nieruchomosci.Where(n =>
                    n.DataKupienia <= aktualnyRokMiesiacPoczatek &&
                    (n.DataSprzedania == null || n.DataSprzedania >= aktualnyRokMiesiacKoniec))
                    .ToListAsync();*/

                var nieruchomosci = await _context.Nieruchomosci
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM Nieruchomosci
                        WHERE 
                            DATEFROMPARTS(YEAR(DataKupienia), MONTH(DataKupienia), 1) <= {aktualnyRokMiesiacPoczatek}
                            AND 
                            (DataSprzedania IS NULL OR 
                             DataSprzedania >= {aktualnyRokMiesiacKoniec})")
                    .ToListAsync();



                Console.WriteLine($"Nieruchomości w miesiącu {miesiac.RokMiesiac}:");
                foreach (var n in nieruchomosci)
                {
                    Console.WriteLine($"Id: {n.Id}, DataKupienia: {n.DataKupienia}, DataSprzedania: {n.DataSprzedania}");
                }


                // Sumowanie wartości
                double SumaPowierzchniKategoriaGruntyPowierzchniaDzialkiMieszkalnej = nieruchomosci.Sum(n => n.KategoriaGruntyPowierzchniaDzialkiMieszkalnej ?? 0);
                double SumaPowierzchniKategoriaGruntyPowierzchniaDzialkiNiemieszkalnej = nieruchomosci.Sum(n => n.KategoriaGruntyPowierzchniaDzialkiNiemieszkalnej ?? 0);
                double SumaPowierzchniKategoriaBudynkiPowierzchniaUzytkowaMieszkalna = nieruchomosci.Sum(n => n.KategoriaBudynkiPowierzchniaUzytkowaMieszkalna ?? 0);
                double SumaPowierzchniKategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna = nieruchomosci.Sum(n => n.KategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna ?? 0);
                double SumaPowierzchniKategoriaWartoscBudowli = nieruchomosci.Sum(n => n.KategoriaWartoscBudowli ?? 0);

                // Przypisanie sum powierzchni w aktualnym miesiacu
                miesiac.SumaPowierzchniKategoriaGruntyPowierzchniaDzialkiMieszkalnej = SumaPowierzchniKategoriaGruntyPowierzchniaDzialkiMieszkalnej;
                miesiac.SumaPowierzchniKategoriaGruntyPowierzchniaDzialkiNiemieszkalnej = SumaPowierzchniKategoriaGruntyPowierzchniaDzialkiNiemieszkalnej;
                miesiac.SumaPowierzchniKategoriaBudynkiPowierzchniaUzytkowaMieszkalna = SumaPowierzchniKategoriaBudynkiPowierzchniaUzytkowaMieszkalna;
                miesiac.SumaPowierzchniKategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna = SumaPowierzchniKategoriaBudynkiPowierzchniaUzytkowaNiemieszkalna;
                miesiac.SumaPowierzchniKategoriaWartoscBudowli = SumaPowierzchniKategoriaWartoscBudowli;
            }

            // Zapisz zmiany w bazie danych
            await _context.SaveChangesAsync();
        }
    }
}
