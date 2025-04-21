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
    public class ServiceUsagesController : Controller
    {
        private readonly QLPhongNetContext _context;

        public ServiceUsagesController(QLPhongNetContext context)
        {
            _context = context;
        }

        // GET: ServiceUsages
        public async Task<IActionResult> Index()
        {
            var serviceUsages = await _context.ServiceUsages
                .Include(s => s.Service)
                .Include(s => s.User)
                .ToListAsync();
            return View(serviceUsages);
        }

        // GET: ServiceUsages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsages
                .Include(s => s.Service)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (serviceUsage == null)
            {
                return NotFound();
            }

            return View(serviceUsage);
        }

        // GET: ServiceUsages/Create
        public IActionResult Create()
        {
            ViewData["ServiceID"] = new SelectList(_context.Services, "ID", "Name");
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username");
            return View();
        }

        // POST: ServiceUsages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,UserID,ServiceID,Quantity,UsageTime,TotalPrice")] ServiceUsage serviceUsage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serviceUsage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ServiceID"] = new SelectList(_context.Services, "ID", "Name", serviceUsage.ServiceID);
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username", serviceUsage.UserID);
            return View(serviceUsage);
        }

        // GET: ServiceUsages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsages.FindAsync(id);
            if (serviceUsage == null)
            {
                return NotFound();
            }
            ViewData["ServiceID"] = new SelectList(_context.Services, "ID", "Name", serviceUsage.ServiceID);
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username", serviceUsage.UserID);
            return View(serviceUsage);
        }

        // POST: ServiceUsages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,UserID,ServiceID,Quantity,UsageTime,TotalPrice")] ServiceUsage serviceUsage)
        {
            if (id != serviceUsage.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceUsage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceUsageExists(serviceUsage.ID))
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
            ViewData["ServiceID"] = new SelectList(_context.Services, "ID", "Name", serviceUsage.ServiceID);
            ViewData["UserID"] = new SelectList(_context.Users, "ID", "Username", serviceUsage.UserID);
            return View(serviceUsage);
        }

        // GET: ServiceUsages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceUsage = await _context.ServiceUsages
                .Include(s => s.Service)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (serviceUsage == null)
            {
                return NotFound();
            }

            return View(serviceUsage);
        }

        // POST: ServiceUsages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceUsage = await _context.ServiceUsages.FindAsync(id);
            if (serviceUsage != null)
            {
                _context.ServiceUsages.Remove(serviceUsage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceUsageExists(int id)
        {
            return _context.ServiceUsages.Any(e => e.ID == id);
        }
    }
}
