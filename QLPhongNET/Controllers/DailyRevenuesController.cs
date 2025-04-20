using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Models;

namespace QLPhongNET.Controllers
{
    public class DailyRevenuesController : Controller
    {
        private readonly QLPhongNetContext _context;

        public DailyRevenuesController(QLPhongNetContext context)
        {
            _context = context;
        }

        // GET: DailyRevenues
        public async Task<IActionResult> Index()
        {
            return View(await _context.DailyRevenues.ToListAsync());
        }

        // GET: DailyRevenues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRevenue = await _context.DailyRevenues
                .FirstOrDefaultAsync(m => m.ID == id);
            if (dailyRevenue == null)
            {
                return NotFound();
            }

            return View(dailyRevenue);
        }

        // GET: DailyRevenues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DailyRevenues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ReportDate,TotalUsageRevenue,TotalRecharge,TotalServiceRevenue")] DailyRevenue dailyRevenue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dailyRevenue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dailyRevenue);
        }

        // GET: DailyRevenues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRevenue = await _context.DailyRevenues.FindAsync(id);
            if (dailyRevenue == null)
            {
                return NotFound();
            }
            return View(dailyRevenue);
        }

        // POST: DailyRevenues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ReportDate,TotalUsageRevenue,TotalRecharge,TotalServiceRevenue")] DailyRevenue dailyRevenue)
        {
            if (id != dailyRevenue.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dailyRevenue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DailyRevenueExists(dailyRevenue.ID))
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
            return View(dailyRevenue);
        }

        // GET: DailyRevenues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyRevenue = await _context.DailyRevenues
                .FirstOrDefaultAsync(m => m.ID == id);
            if (dailyRevenue == null)
            {
                return NotFound();
            }

            return View(dailyRevenue);
        }

        // POST: DailyRevenues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dailyRevenue = await _context.DailyRevenues.FindAsync(id);
            if (dailyRevenue != null)
            {
                _context.DailyRevenues.Remove(dailyRevenue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DailyRevenueExists(int id)
        {
            return _context.DailyRevenues.Any(e => e.ID == id);
        }
    }
}
