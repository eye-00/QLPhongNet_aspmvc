using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLPhongNET.Data;
using QLPhongNET.Models;

namespace QLPhongNET.Controllers
{
    public class ComputerCategoriesController : Controller
    {
        private readonly QLPhongNetContext _context;

        public ComputerCategoriesController(QLPhongNetContext context)
        {
            _context = context;
        }

        // GET: ComputerCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ComputerCategories.ToListAsync());
        }

        // GET: ComputerCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var computerCategory = await _context.ComputerCategories
                .FirstOrDefaultAsync(m => m.ID == id);
            if (computerCategory == null)
            {
                return NotFound();
            }

            return View(computerCategory);
        }

        // GET: ComputerCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ComputerCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,PricePerHour")] ComputerCategory computerCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(computerCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(computerCategory);
        }

        // GET: ComputerCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var computerCategory = await _context.ComputerCategories.FindAsync(id);
            if (computerCategory == null)
            {
                return NotFound();
            }
            return View(computerCategory);
        }

        // POST: ComputerCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,PricePerHour")] ComputerCategory computerCategory)
        {
            if (id != computerCategory.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(computerCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComputerCategoryExists(computerCategory.ID))
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
            return View(computerCategory);
        }

        // GET: ComputerCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var computerCategory = await _context.ComputerCategories
                .FirstOrDefaultAsync(m => m.ID == id);
            if (computerCategory == null)
            {
                return NotFound();
            }

            return View(computerCategory);
        }

        // POST: ComputerCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var computerCategory = await _context.ComputerCategories.FindAsync(id);
            if (computerCategory != null)
            {
                _context.ComputerCategories.Remove(computerCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComputerCategoryExists(int id)
        {
            return _context.ComputerCategories.Any(e => e.ID == id);
        }
    }
}
