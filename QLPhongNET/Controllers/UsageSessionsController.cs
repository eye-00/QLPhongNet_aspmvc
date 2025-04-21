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
    public class UsageSessionsController : Controller
    {
        private readonly QLPhongNetContext _context;

        public UsageSessionsController(QLPhongNetContext context)
        {
            _context = context;
        }

        // GET: UsageSessions
        public async Task<IActionResult> Index()
        {
            var usageSessions = await _context.UsageSessions
                .Include(u => u.Computer)
                .Include(u => u.User)
                .ToListAsync();
            return View(usageSessions);
        }

        // GET: UsageSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usageSession = await _context.UsageSessions
                .Include(u => u.Computer)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (usageSession == null)
            {
                return NotFound();
            }

            return View(usageSession);
        }

        // GET: UsageSessions/Create
        public IActionResult Create()
        {
            ViewData["ComputerID"] = new SelectList(_context.Computers, "ID", "Name");
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username");
            return View();
        }

        // POST: UsageSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,UserID,ComputerID,StartTime,EndTime,TotalCost")] UsageSession usageSession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usageSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ComputerID"] = new SelectList(_context.Computers, "ID", "Name", usageSession.ComputerID);
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username", usageSession.UserID);
            return View(usageSession);
        }

        // GET: UsageSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usageSession = await _context.UsageSessions.FindAsync(id);
            if (usageSession == null)
            {
                return NotFound();
            }
            ViewData["ComputerID"] = new SelectList(_context.Computers, "ID", "Name", usageSession.ComputerID);
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username", usageSession.UserID);
            return View(usageSession);
        }

        // POST: UsageSessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,UserID,ComputerID,StartTime,EndTime,TotalCost")] UsageSession usageSession)
        {
            if (id != usageSession.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usageSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsageSessionExists(usageSession.ID))
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
            ViewData["ComputerID"] = new SelectList(_context.Computers, "ID", "Name", usageSession.ComputerID);
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username", usageSession.UserID);
            return View(usageSession);
        }

        // GET: UsageSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usageSession = await _context.UsageSessions
                .Include(u => u.Computer)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (usageSession == null)
            {
                return NotFound();
            }

            return View(usageSession);
        }

        // POST: UsageSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usageSession = await _context.UsageSessions.FindAsync(id);
            if (usageSession != null)
            {
                _context.UsageSessions.Remove(usageSession);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsageSessionExists(int id)
        {
            return _context.UsageSessions.Any(e => e.ID == id);
        }
    }
}
