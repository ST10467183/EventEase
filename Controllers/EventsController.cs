using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public EventsController(ApplicationDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // show all events (with search)
        public async Task<IActionResult> Index(string searchString)
        {
            var events = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.EventName.Contains(searchString)
                                         || e.Description.Contains(searchString));
            }

            return View(await events.ToListAsync());
        }

        // event details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // create form
        public IActionResult Create()
        {
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "TypeName");
            return View();
        }

        // create logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,Description,StartDate,EndDate,EventTypeId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                // upload image if there is one
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            @event.ImageUrl = await _blobStorageService.UploadImageAsync(stream, file.FileName);
                        }
                    }
                }

                try
                {
                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Unable to create event. Please check your input and try again.";
                }
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        // edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        // edit logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,Description,StartDate,EndDate,EventTypeId")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // keep old image if no new one uploaded
                var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == @event.EventId);

                if (Request.Form.Files.Count > 0 && Request.Form.Files[0].Length > 0)
                {
                    var file = Request.Form.Files[0];
                    using (var stream = file.OpenReadStream())
                    {
                        @event.ImageUrl = await _blobStorageService.UploadImageAsync(stream, file.FileName);
                    }
                }
                else
                {
                    @event.ImageUrl = existingEvent?.ImageUrl;
                }

                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Unable to save changes. Please try again.";
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        // delete page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // delete logic
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            // can't delete if there's bookings
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings. Please delete the bookings first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Unable to delete the event. Please try again later.";
            }

            return RedirectToAction(nameof(Index));
        }

        // shows image from blob storage
        public async Task<IActionResult> EventImage(string imageUrl)
        {
            var (stream, contentType) = await _blobStorageService.GetImageAsync(imageUrl);

            if (stream == null)
                return NotFound();

            return File(stream, contentType ?? "image/jpeg");
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}