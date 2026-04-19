using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelUyutClean.Data;
using HotelUyutClean.Models;

namespace HotelUyutClean.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RoomsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Rooms - доступно всем
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.ToListAsync();
            return View(rooms);
        }

        // GET: Rooms/Details/5 - доступно всем
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (room == null)
                return NotFound();

            return View(room);
        }

        // GET: Rooms/Create - только админ
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rooms/Create - только админ с загрузкой фото
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            if (ModelState.IsValid)
            {
                // Обработка загрузки фото
                if (room.ImageFile != null && room.ImageFile.Length > 0)
                {
                    // Создаем уникальное имя файла
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(room.ImageFile.FileName);
                    
                    // Путь к папке wwwroot/images/rooms
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms");
                    
                    // Создаем папку если не существует
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);
                    
                    // Полный путь к файлу
                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    // Сохраняем файл
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await room.ImageFile.CopyToAsync(stream);
                    }
                    
                    // Сохраняем относительный путь в БД
                    room.ImageUrl = $"/images/rooms/{fileName}";
                }
                else
                {
                    // Фото по умолчанию
                    room.ImageUrl = "/images/rooms/default.jpg";
                }

                room.CreatedAt = DateTime.Now;
                _context.Add(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Номер успешно добавлен!";
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Edit/5 - только админ
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            return View(room);
        }

        // POST: Rooms/Edit/5 - только админ с загрузкой фото
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoom = await _context.Rooms.FindAsync(id);
                    if (existingRoom == null)
                        return NotFound();

                    // Если загружено новое фото
                    if (room.ImageFile != null && room.ImageFile.Length > 0)
                    {
                        // Удаляем старое фото если它不是 дефолтное
                        if (!string.IsNullOrEmpty(existingRoom.ImageUrl) && 
                            existingRoom.ImageUrl != "/images/rooms/default.jpg")
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                                existingRoom.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        // Сохраняем новое фото
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(room.ImageFile.FileName);
                        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms");
                        
                        if (!Directory.Exists(uploadPath))
                            Directory.CreateDirectory(uploadPath);
                        
                        var filePath = Path.Combine(uploadPath, fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await room.ImageFile.CopyToAsync(stream);
                        }
                        
                        existingRoom.ImageUrl = $"/images/rooms/{fileName}";
                    }

                    // Обновляем остальные поля
                    existingRoom.Title = room.Title;
                    existingRoom.Description = room.Description;
                    existingRoom.Price = room.Price;
                    existingRoom.Area = room.Area;
                    existingRoom.MaxGuests = room.MaxGuests;
                    existingRoom.Beds = room.Beds;
                    existingRoom.Amenities = room.Amenities;
                    existingRoom.Category = room.Category;
                    existingRoom.IsAvailable = room.IsAvailable;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Номер успешно обновлен!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Delete/5 - только админ
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
                return NotFound();

            return View(room);
        }

        // POST: Rooms/Delete/5 - только админ
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                // Удаляем файл фото
                if (!string.IsNullOrEmpty(room.ImageUrl) && 
                    room.ImageUrl != "/images/rooms/default.jpg")
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                        room.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Номер успешно удален!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}