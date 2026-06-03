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
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public VenuesController(ApplicationDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // shows all venues (with search)
        public async Task<IActionResult> Index(string searchString)
        {
            var venues = _context.Venues.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                venues = venues.Where(v => v.VenueName.Contains(searchString)
                                         || v.Location.Contains(searchString));
            }

            return View(await venues.ToListAsync());
        }

        // details page
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // create form
        public IActionResult Create()
        {
            return View();
        }

        // create logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity")] Venue venue)
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
                            venue.ImageUrl = await _blobStorageService.UploadImageAsync(stream, file.FileName);
                        }
                    }
                }

                try
                {
                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Unable to create venue. Please check your input and try again.";
                }
            }
            return View(venue);
        }

        // edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // edit logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // handle new image
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            venue.ImageUrl = await _blobStorageService.UploadImageAsync(stream, file.FileName);
                        }
                    }
                }

                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
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
            return View(venue);
        }

        // delete page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // delete logic
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            // can't delete if there's bookings
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings. Please delete the bookings first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Unable to delete the venue. Please try again later.";
            }

            return RedirectToAction(nameof(Index));
        }

        // shows image from blob storage
        public async Task<IActionResult> VenueImage(string imageUrl)
        {
            var (stream, contentType) = await _blobStorageService.GetImageAsync(imageUrl);

            if (stream == null)
                return NotFound();

            return File(stream, contentType ?? "image/jpeg");
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}