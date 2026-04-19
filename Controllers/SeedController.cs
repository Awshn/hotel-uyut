using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelUyutClean.Data;
using HotelUyutClean.Models;
using Microsoft.AspNetCore.Identity;
namespace HotelUyutClean.Controllers
{
    public class SeedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Главная страница с кнопками
        public IActionResult Index()
        {
            return View();
        }

        // Создание 100 номеров
        public async Task<IActionResult> CreateRooms()
        {
            if (_context.Rooms.Any())
            {
                return Content("⚠️ Номера уже есть в базе данных. Используйте /Seed/ClearAll для очистки.");
            }

            var rooms = new List<Room>();
            var categories = new[] { "Стандарт", "Стандарт+", "Бизнес", "Семейный", "Люкс", "Президентский", "Эконом" };
            var images = new[] 
            { 
                "/images/rooms/standard.jpg",
                "/images/rooms/standard_plus.jpg", 
                "/images/rooms/lux.jpg",
                "/images/rooms/family.jpg",
                "/images/rooms/business.jpg",
                "/images/rooms/econom.jpg",
                "/images/rooms/lux_premium.jpg",
                "/images/rooms/family_lux.jpg",
                "/images/rooms/romantic.jpg",
                "/images/rooms/president.jpg"
            };
            var amenitiesList = new[]
            {
                "Wi-Fi, TV, Кондиционер",
                "Wi-Fi, TV, Кондиционер, Холодильник",
                "Wi-Fi, TV, Кондиционер, Холодильник, Джакузи",
                "Wi-Fi, TV, Кондиционер, Холодильник, Мини-бар",
                "Wi-Fi, TV, Кондиционер, Детская кроватка",
                "Wi-Fi, TV, Кондиционер, Сауна, Бильярд"
            };
            var descriptions = new[]
            {
                "Просторный и уютный номер, идеально подходящий для комфортного отдыха.",
                "Светлый номер с современным ремонтом и всей необходимой техникой.",
                "Номер с панорамным видом на город. Отличный выбор для романтического отдыха.",
                "Отличный вариант для деловых поездок. В номере есть рабочая зона.",
                "Большой семейный номер с детской кроваткой и игровой зоной.",
                "Роскошный номер с джакузи и мини-баром. Для тех, кто ценит комфорт."
            };

            for (int i = 1; i <= 100; i++)
            {
                var category = categories[i % categories.Length];
                var imageIndex = i % images.Length;
                var amenitiesIndex = i % amenitiesList.Length;
                var descriptionIndex = i % descriptions.Length;
                
                decimal price = category switch
                {
                    "Эконом" => 2000 + (i % 5) * 100,
                    "Стандарт" => 3000 + (i % 10) * 100,
                    "Стандарт+" => 4000 + (i % 10) * 100,
                    "Бизнес" => 5000 + (i % 15) * 100,
                    "Семейный" => 6000 + (i % 15) * 100,
                    "Люкс" => 8000 + (i % 20) * 100,
                    "Президентский" => 15000 + (i % 30) * 100,
                    _ => 3500
                };

                var room = new Room
                {
                    Title = $"{category} {i}",
                    Description = $"{descriptions[descriptionIndex]} Категория: {category}. Номер #{i}.",
                    Price = price,
                    Area = 15 + (i % 50),
                    MaxGuests = 1 + (i % 8),
                    Beds = 1 + (i % 4),
                    ImageUrl = images[imageIndex],
                    Amenities = amenitiesList[amenitiesIndex],
                    Category = category,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now.AddDays(-i)
                };
                rooms.Add(room);
            }

            await _context.Rooms.AddRangeAsync(rooms);
            await _context.SaveChangesAsync();

            return Content($"✅ Добавлено {rooms.Count} номеров!");
        }

        // Создание 100 бронирований
        public async Task<IActionResult> CreateBookings()
        {
            // Проверяем, есть ли уже бронирования
            if (_context.Bookings.Any())
            {
                return Content("⚠️ Бронирования уже есть в базе данных. Используйте /Seed/ClearAll для очистки.");
            }

            // Получаем всех пользователей
            var users = await _context.Users.ToListAsync();
            if (!users.Any())
            {
                return Content("❌ Сначала зарегистрируйте пользователей! Админ создается автоматически, но нужно запустить проект хотя бы раз.");
            }

            // Получаем все номера
            var rooms = await _context.Rooms.ToListAsync();
            if (!rooms.Any())
            {
                return Content("❌ Сначала добавьте номера через /Seed/CreateRooms");
            }

            var statuses = new[] { "Confirmed", "Cancelled", "Pending", "Confirmed", "Confirmed" };
            var random = new Random();
            var bookings = new List<Booking>();

            for (int i = 1; i <= 100; i++)
            {
                var user = users[random.Next(users.Count)];
                var room = rooms[random.Next(rooms.Count)];
                var checkIn = DateTime.Now.AddDays(random.Next(-60, 60));
                var checkOut = checkIn.AddDays(random.Next(1, 15));
                var days = (checkOut - checkIn).Days;
                var status = statuses[random.Next(statuses.Length)];
                
                // Для будущих бронирований ставим статус Confirmed или Pending
                if (checkIn > DateTime.Now && status == "Cancelled")
                {
                    status = "Confirmed";
                }

                var booking = new Booking
                {
                    UserId = user.Id,
                    RoomId = room.Id,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    Guests = random.Next(1, Math.Min(room.MaxGuests, 8) + 1),
                    SpecialRequests = random.Next(4) == 0 ? GenerateRandomRequest(random) : null,
                    BookingDate = checkIn.AddDays(-random.Next(1, 60)),
                    TotalPrice = days * room.Price,
                    Status = status,
                    ContactPhone = GenerateRandomPhone(random),
                    ContactEmail = user.Email
                };
                bookings.Add(booking);
            }

            await _context.Bookings.AddRangeAsync(bookings);
            await _context.SaveChangesAsync();

            return Content($"✅ Добавлено {bookings.Count} бронирований!\n\n" +
                          $"📊 Статистика:\n" +
                          $"- Подтверждено: {bookings.Count(b => b.Status == "Confirmed")}\n" +
                          $"- Отменено: {bookings.Count(b => b.Status == "Cancelled")}\n" +
                          $"- В ожидании: {bookings.Count(b => b.Status == "Pending")}");
        }

        // Создание тестовых пользователей
        public async Task<IActionResult> CreateUsers()
        {
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            
            // Проверяем, есть ли уже пользователи (кроме админа)
            var existingUsers = await _context.Users.Where(u => u.Email != "admin@hoteluyut.com").ToListAsync();
            if (existingUsers.Any())
            {
                return Content($"⚠️ Пользователи уже есть в базе данных ({existingUsers.Count} шт.)");
            }

            int createdCount = 0;
            var random = new Random();
            var firstNames = new[] { "Алексей", "Мария", "Иван", "Елена", "Дмитрий", "Ольга", "Сергей", "Анна", "Владимир", "Татьяна" };
            var lastNames = new[] { "Иванов", "Петрова", "Сидоров", "Кузнецова", "Смирнов", "Васильева", "Попов", "Соколова", "Михайлов", "Федорова" };

            for (int i = 1; i <= 20; i++)
            {
                var userEmail = $"user{i}@test.com";
                if (await userManager.FindByEmailAsync(userEmail) == null)
                {
                    var firstName = firstNames[random.Next(firstNames.Length)];
                    var lastName = lastNames[random.Next(lastNames.Length)];
                    
                    var user = new ApplicationUser
                    {
                        UserName = userEmail,
                        Email = userEmail,
                        FirstName = firstName,
                        LastName = lastName,
                        RegistrationDate = DateTime.Now.AddDays(-random.Next(1, 365))
                    };
                    await userManager.CreateAsync(user, "User123!");
                    await userManager.AddToRoleAsync(user, "User");
                    createdCount++;
                }
            }

            return Content($"✅ Создано {createdCount} тестовых пользователей!\n" +
                          $"📧 Email: user1@test.com ... user20@test.com\n" +
                          $"🔑 Пароль для всех: User123!");
        }

        // Очистка всех данных
        public async Task<IActionResult> ClearAll()
        {
            var bookingsCount = _context.Bookings.Count();
            var roomsCount = _context.Rooms.Count();
            
            _context.Bookings.RemoveRange(_context.Bookings);
            _context.Rooms.RemoveRange(_context.Rooms);
            await _context.SaveChangesAsync();
            
            return Content($"✅ Очищено!\n" +
                          $"- Удалено бронирований: {bookingsCount}\n" +
                          $"- Удалено номеров: {roomsCount}");
        }

        // Полная инициализация (все сразу)
        public async Task<IActionResult> FullSeed()
        {
            var result = new List<string>();
            
            // Очищаем
            _context.Bookings.RemoveRange(_context.Bookings);
            _context.Rooms.RemoveRange(_context.Rooms);
            await _context.SaveChangesAsync();
            result.Add("✅ База очищена");
            
            // Создаем номера
            await CreateRooms();
            result.Add("✅ 100 номеров создано");
            
            // Создаем пользователей
            var userResult = await CreateUsers();
            result.Add(userResult.ToString()?.Replace("\n", " "));
            
            // Создаем бронирования
            var bookingResult = await CreateBookings();
            result.Add(bookingResult.ToString()?.Replace("\n", " "));
            
            return Content(string.Join("\n\n", result));
        }

        private string GenerateRandomRequest(Random random)
        {
            var requests = new[]
            {
                "Нужна детская кроватка",
                "Приготовьте шампанское в номер",
                "Вегетарианское питание",
                "Вид на город, пожалуйста",
                "Поздний заезд после 22:00",
                "Дополнительное полотенце и халат",
                "Глютен-фри питание",
                "Номер рядом с лифтом",
                "Тихий номер, пожалуйста",
                "Установите дополнительную кровать"
            };
            return requests[random.Next(requests.Length)];
        }

        private string GenerateRandomPhone(Random random)
        {
            return $"+7 (9{random.Next(10)}{random.Next(10)}{random.Next(10)}) {random.Next(100, 999)}-{random.Next(10)}{random.Next(10)}-{random.Next(10)}{random.Next(10)}";
        }
    }
}