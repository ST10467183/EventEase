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

        
        //Detail
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Location");
            return View();
        }

        // POST: Bookings/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,VenueId,EventId,BookingDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Check for double booking
                bool isDoubleBooked = await _context.Bookings
                    .AnyAsync(b => b.VenueId == booking.VenueId
                                && b.BookingDate == booking.BookingDate
                                && b.BookingId != booking.BookingId);

                if (isDoubleBooked)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected date and time. Please choose a different date or venue.");
                }
                else
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.Event.EventName.Contains(searchString));
            }

            return View(await bookings.ToListAsync());
        }

        // POST: Bookings

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,VenueId,EventId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check for double booking
                bool isDoubleBooked = await _context.Bookings
                    .AnyAsync(b => b.VenueId == booking.VenueId
                                && b.BookingDate == booking.BookingDate
                                && b.BookingId != booking.BookingId);

                if (isDoubleBooked)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected date and time. Please choose a different date or venue.");
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
                        if (!BookingExists(booking.BookingId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
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
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
