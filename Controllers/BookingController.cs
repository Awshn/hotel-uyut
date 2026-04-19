using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelUyutClean.Data;
using HotelUyutClean.Models;

namespace HotelUyutClean.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings (для админа - все бронирования)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return View(bookings);
        }

        // GET: Bookings/Details/5 (для админа)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create/5
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var booking = new Booking
            {
                RoomId = room.Id,
                Room = room,
                CheckInDate = DateTime.Now.AddDays(1),
                CheckOutDate = DateTime.Now.AddDays(2),
                Guests = 1,
                ContactEmail = User.Identity?.Name
            };

            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var room = await _context.Rooms.FindAsync(booking.RoomId);
                if (room == null)
                {
                    return NotFound();
                }

                var days = (booking.CheckOutDate - booking.CheckInDate).Days;
                booking.TotalPrice = days * room.Price;
                booking.UserId = user.Id;
                booking.BookingDate = DateTime.Now;
                booking.Status = "Confirmed";

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Номер '{room.Title}' успешно забронирован! Стоимость: {booking.TotalPrice} ₽";
                return RedirectToAction("MyBookings");
            }

            var roomForView = await _context.Rooms.FindAsync(booking.RoomId);
            booking.Room = roomForView;
            return View(booking);
        }

        // GET: Bookings/MyBookings
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Bookings/Edit/5 (для админа)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: Bookings/Edit/5 (для админа)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBooking = await _context.Bookings.FindAsync(id);
                    if (existingBooking == null)
                        return NotFound();

                    existingBooking.Status = booking.Status;
                    existingBooking.CheckInDate = booking.CheckInDate;
                    existingBooking.CheckOutDate = booking.CheckOutDate;
                    existingBooking.Guests = booking.Guests;
                    existingBooking.SpecialRequests = booking.SpecialRequests;
                    existingBooking.ContactPhone = booking.ContactPhone;
                    existingBooking.ContactEmail = booking.ContactEmail;

                    // Пересчет стоимости
                    var room = await _context.Rooms.FindAsync(existingBooking.RoomId);
                    var days = (existingBooking.CheckOutDate - existingBooking.CheckInDate).Days;
                    existingBooking.TotalPrice = days * (room?.Price ?? 0);

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Бронирование обновлено!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: Bookings/Delete/5 (для админа)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5 (для админа)
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Бронирование удалено!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Cancel/5 (для пользователя)
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: Bookings/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Бронирование отменено";
            }
            return RedirectToAction("MyBookings");
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}