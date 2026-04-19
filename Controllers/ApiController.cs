using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelUyutClean.Data;
using HotelUyutClean.Models;

namespace HotelUyutClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== НОМЕРА (ROOMS) ====================

        // GET: api/api/rooms - получить все номера
        [HttpGet("rooms")]
        public async Task<ActionResult<IEnumerable<object>>> GetRooms()
        {
            var rooms = await _context.Rooms
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.Price,
                    r.Area,
                    r.MaxGuests,
                    r.Beds,
                    r.ImageUrl,
                    r.Amenities,
                    r.IsAvailable,
                    r.Category,
                    r.CreatedAt
                })
                .ToListAsync();
            return Ok(rooms);
        }

        // GET: api/api/rooms/5 - получить номер по ID
        [HttpGet("rooms/{id}")]
        public async Task<ActionResult<object>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.Price,
                    r.Area,
                    r.MaxGuests,
                    r.Beds,
                    r.ImageUrl,
                    r.Amenities,
                    r.IsAvailable,
                    r.Category,
                    r.CreatedAt
                })
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (room == null)
                return NotFound(new { message = "Номер не найден" });
            
            return Ok(room);
        }

        // POST: api/api/rooms - создать номер
        [HttpPost("rooms")]
        public async Task<ActionResult<Room>> CreateRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            room.CreatedAt = DateTime.Now;
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }

        // PUT: api/api/rooms/5 - обновить номер
        [HttpPut("rooms/{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room room)
        {
            if (id != room.Id)
                return BadRequest(new { message = "ID не совпадают" });

            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null)
                return NotFound(new { message = "Номер не найден" });

            existingRoom.Title = room.Title;
            existingRoom.Description = room.Description;
            existingRoom.Price = room.Price;
            existingRoom.Area = room.Area;
            existingRoom.MaxGuests = room.MaxGuests;
            existingRoom.Beds = room.Beds;
            existingRoom.ImageUrl = room.ImageUrl;
            existingRoom.Amenities = room.Amenities;
            existingRoom.IsAvailable = room.IsAvailable;
            existingRoom.Category = room.Category;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Номер обновлен", room = existingRoom });
        }

        // DELETE: api/api/rooms/5 - удалить номер
        [HttpDelete("rooms/{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound(new { message = "Номер не найден" });

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Номер удален" });
        }

        // ==================== БРОНИРОВАНИЯ (BOOKINGS) ====================

        // GET: api/api/bookings - получить все бронирования
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Select(b => new
                {
                    b.Id,
                    b.UserId,
                    UserEmail = b.User != null ? b.User.Email : null,
                    UserName = b.User != null ? b.User.FirstName + " " + b.User.LastName : null,
                    Room = b.Room != null ? new
                    {
                        b.Room.Id,
                        b.Room.Title,
                        b.Room.Price
                    } : null,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.Guests,
                    b.SpecialRequests,
                    b.BookingDate,
                    b.TotalPrice,
                    b.Status,
                    b.ContactPhone,
                    b.ContactEmail
                })
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return Ok(bookings);
        }

        // GET: api/api/bookings/5 - получить бронирование по ID
        [HttpGet("bookings/{id}")]
        public async Task<ActionResult<object>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Select(b => new
                {
                    b.Id,
                    b.UserId,
                    UserEmail = b.User != null ? b.User.Email : null,
                    UserName = b.User != null ? b.User.FirstName + " " + b.User.LastName : null,
                    Room = b.Room != null ? new
                    {
                        b.Room.Id,
                        b.Room.Title,
                        b.Room.Price,
                        b.Room.ImageUrl
                    } : null,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.Guests,
                    b.SpecialRequests,
                    b.BookingDate,
                    b.TotalPrice,
                    b.Status,
                    b.ContactPhone,
                    b.ContactEmail
                })
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (booking == null)
                return NotFound(new { message = "Бронирование не найдено" });
            
            return Ok(booking);
        }

        // GET: api/api/bookings/user/{userId} - получить бронирования пользователя
        [HttpGet("bookings/user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserBookings(string userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == userId)
                .Select(b => new
                {
                    b.Id,
                    b.RoomId,
                    RoomTitle = b.Room != null ? b.Room.Title : null,
                    RoomPrice = b.Room != null ? b.Room.Price : 0,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.Guests,
                    b.SpecialRequests,
                    b.BookingDate,
                    b.TotalPrice,
                    b.Status,
                    b.ContactPhone,
                    b.ContactEmail
                })
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            
            return Ok(bookings);
        }

        // POST: api/api/bookings - создать бронирование
        [HttpPost("bookings")]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] Booking booking)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room == null)
                return BadRequest(new { message = "Номер не найден" });

            // Рассчитываем стоимость
            var days = (booking.CheckOutDate - booking.CheckInDate).Days;
            booking.TotalPrice = days * room.Price;
            booking.BookingDate = DateTime.Now;
            booking.Status = "Confirmed";

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        // PUT: api/api/bookings/5 - обновить бронирование
        [HttpPut("bookings/{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] Booking booking)
        {
            if (id != booking.Id)
                return BadRequest(new { message = "ID не совпадают" });

            var existingBooking = await _context.Bookings.FindAsync(id);
            if (existingBooking == null)
                return NotFound(new { message = "Бронирование не найдено" });

            var room = await _context.Rooms.FindAsync(booking.RoomId);
            var days = (booking.CheckOutDate - booking.CheckInDate).Days;
            
            existingBooking.RoomId = booking.RoomId;
            existingBooking.CheckInDate = booking.CheckInDate;
            existingBooking.CheckOutDate = booking.CheckOutDate;
            existingBooking.Guests = booking.Guests;
            existingBooking.SpecialRequests = booking.SpecialRequests;
            existingBooking.Status = booking.Status;
            existingBooking.ContactPhone = booking.ContactPhone;
            existingBooking.ContactEmail = booking.ContactEmail;
            existingBooking.TotalPrice = days * (room?.Price ?? 0);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Бронирование обновлено", booking = existingBooking });
        }

        // DELETE: api/api/bookings/5 - удалить бронирование
        [HttpDelete("bookings/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { message = "Бронирование не найдено" });

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Бронирование удалено" });
        }

        // ==================== СТАТИСТИКА ====================

        // GET: api/api/stats - получить статистику
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var roomsCount = await _context.Rooms.CountAsync();
            var availableRoomsCount = await _context.Rooms.CountAsync(r => r.IsAvailable);
            var bookingsCount = await _context.Bookings.CountAsync();
            var confirmedBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");
            var totalRevenue = await _context.Bookings.Where(b => b.Status == "Confirmed").SumAsync(b => b.TotalPrice);

            return Ok(new
            {
                rooms = new
                {
                    total = roomsCount,
                    available = availableRoomsCount,
                    occupied = roomsCount - availableRoomsCount
                },
                bookings = new
                {
                    total = bookingsCount,
                    confirmed = confirmedBookings,
                    cancelled = await _context.Bookings.CountAsync(b => b.Status == "Cancelled")
                },
                revenue = totalRevenue
            });
        }
    }
}