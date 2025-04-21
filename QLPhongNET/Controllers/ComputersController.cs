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
    public class ComputersController : Controller
    {
        private readonly QLPhongNetContext _context;

        public ComputersController(QLPhongNetContext context)
        {
            _context = context;
        }

        // GET: Computers
        public async Task<IActionResult> Index()
        {
            var computers = await _context.Computers
                .Include(c => c.Category)
                .ToListAsync();
            return View(computers);
        }

        // GET: Computers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var computer = await _context.Computers
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (computer == null)
            {
                return NotFound();
            }

            return View(computer);
        }

        // GET: Computers/Create
        public IActionResult Create()
        {
            var categories = _context.ComputerCategories.ToList();
            if (!categories.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm loại máy trước khi thêm máy tính");
                return RedirectToAction("Create", "ComputerCategories");
            }

            ViewData["Categories"] = new SelectList(categories, "ID", "Name");
            return View();
        }

        // POST: Computers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Status,CatID")] Computer computer)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem loại máy có tồn tại không
                var category = await _context.ComputerCategories.FindAsync(computer.CatID);
                if (category == null)
                {
                    ModelState.AddModelError("CatID", "Loại máy không tồn tại");
                    ViewData["Categories"] = new SelectList(_context.ComputerCategories, "ID", "Name");
                    return View(computer);
                }

                _context.Add(computer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = new SelectList(_context.ComputerCategories, "ID", "Name", computer.CatID);
            return View(computer);
        }

        // GET: Computers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
            {
                return NotFound();
            }

            var categories = await _context.ComputerCategories.ToListAsync();
            if (!categories.Any())
            {
                ModelState.AddModelError("", "Không tìm thấy loại máy nào");
                return View(computer);
            }

            ViewData["Categories"] = new SelectList(categories, "ID", "Name", computer.CatID);
            return View(computer);
        }

        // POST: Computers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Status,CatID")] Computer computer)
        {
            if (id != computer.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra xem loại máy có tồn tại không
                var category = await _context.ComputerCategories.FindAsync(computer.CatID);
                if (category == null)
                {
                    ModelState.AddModelError("CatID", "Loại máy không tồn tại");
                    ViewData["Categories"] = new SelectList(_context.ComputerCategories, "ID", "Name", computer.CatID);
                    return View(computer);
                }

                try
                {
                    _context.Update(computer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComputerExists(computer.ID))
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

            ViewData["Categories"] = new SelectList(_context.ComputerCategories, "ID", "Name", computer.CatID);
            return View(computer);
        }

        // GET: Computers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var computer = await _context.Computers
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (computer == null)
            {
                return NotFound();
            }

            return View(computer);
        }

        // POST: Computers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var computer = await _context.Computers.FindAsync(id);
            if (computer != null)
            {
                _context.Computers.Remove(computer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComputerExists(int id)
        {
            return _context.Computers.Any(e => e.ID == id);
        }
    }
}
