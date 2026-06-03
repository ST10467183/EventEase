using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? eventTypeId, int? venueId, DateTime? startDate, DateTime? endDate)
        {
            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .ThenInclude(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.Event.EventName.Contains(searchString));
            }

            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventTypeId == eventTypeId);
            }

            if (venueId.HasValue)
            {
                bookings = bookings.Where(b => b.VenueId == venueId);
            }

            if (startDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate >= startDate);
            }

            if (endDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate <= endDate);
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "TypeName", eventTypeId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", venueId);
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(await bookings.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,VenueId,EventId,BookingDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == booking.VenueId);
                var eventExists = await _context.Events.AnyAsync(e => e.EventId == booking.EventId);

                if (!venueExists) ModelState.AddModelError("VenueId", "The selected venue does not exist.");
                if (!eventExists) ModelState.AddModelError("EventId", "The selected event does not exist.");

                if (venueExists && eventExists)
                {
                    bool isDoubleBooked = await _context.Bookings
                        .AnyAsync(b => b.VenueId == booking.VenueId
                                    && b.BookingDate == booking.BookingDate
                                    && b.BookingId != booking.BookingId);

                    if (isDoubleBooked)
                    {
                        ModelState.AddModelError("", "This venue is already booked for the selected date and time.");
                    }
                    else
                    {
                        try
                        {
                            _context.Add(booking);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        catch (DbUpdateException)
                        {
                            ModelState.AddModelError("", "Unable to save the booking.");
                        }
                    }
                }
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,VenueId,EventId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == booking.VenueId);
                var eventExists = await _context.Events.AnyAsync(e => e.EventId == booking.EventId);

                if (!venueExists) ModelState.AddModelError("VenueId", "The selected venue does not exist.");
                if (!eventExists) ModelState.AddModelError("EventId", "The selected event does not exist.");

                if (venueExists && eventExists)
                {
                    bool isDoubleBooked = await _context.Bookings
                        .AnyAsync(b => b.VenueId == booking.VenueId
                                    && b.BookingDate == booking.BookingDate
                                    && b.BookingId != booking.BookingId);

                    if (isDoubleBooked)
                    {
                        ModelState.AddModelError("", "This venue is already booked for the selected date and time.");
                    }
                    else
                    {
                        try
                        {
                            _context.Update(booking);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!BookingExists(booking.BookingId)) return NotFound();
                            else throw;
                        }
                        catch (DbUpdateException)
                        {
                            ModelState.AddModelError("", "Unable to save the booking.");
                        }
                    }
                }
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                try
                {
                    _context.Bookings.Remove(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Unable to delete the booking.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}