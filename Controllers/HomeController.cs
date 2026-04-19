using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelUyutClean.Data;
using HotelUyutClean.Models;

namespace HotelUyutClean.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new HotelViewModel
            {
                HotelName = "Гостиница Уют",
                WelcomeMessage = "Добро пожаловать в нашу гостиницу!",
                HomeImages = new List<RoomImage>
                {
                    new RoomImage { ImagePath = "/images/home/home1.jpg", Title = "Стандарт", Description = "Уютный номер со всеми удобствами", Price = 3500 },
                    new RoomImage { ImagePath = "/images/home/home2.jpg", Title = "Люкс", Description = "Просторный номер с панорамным видом", Price = 6500 },
                    new RoomImage { ImagePath = "/images/home/home3.jpg", Title = "Семейный", Description = "Большая комната для всей семьи", Price = 5000 }
                }
            };
            return View(model);
        }

        public IActionResult About()
        {
            var model = new HotelViewModel
            {
                AboutImages = new List<GalleryImage>
                {
                    new GalleryImage { ImagePath = "/images/about/about1.jpg", Title = "Лобби" },
                    new GalleryImage { ImagePath = "/images/about/about2.jpg", Title = "Ресторан" },
                    new GalleryImage { ImagePath = "/images/about/about3.jpg", Title = "Спа-зона" },
                    new GalleryImage { ImagePath = "/images/about/about4.jpg", Title = "Бассейн" },
                    new GalleryImage { ImagePath = "/images/about/about5.jpg", Title = "Терраса" }
                }
            };
            return View(model);
        }

        public IActionResult Contacts()
        {
            var model = new HotelViewModel
            {
                ContactImages = new List<GalleryImage>
                {
                    new GalleryImage { ImagePath = "/images/contacts/contact1.jpg", Title = "Вид снаружи" },
                    new GalleryImage { ImagePath = "/images/contacts/contact2.jpg", Title = "Номер люкс" },
                    new GalleryImage { ImagePath = "/images/contacts/contact3.jpg", Title = "Конференц-зал" },
                    new GalleryImage { ImagePath = "/images/contacts/contact4.jpg", Title = "Фитнес-центр" },
                    new GalleryImage { ImagePath = "/images/contacts/contact5.jpg", Title = "Парковка" },
                    new GalleryImage { ImagePath = "/images/contacts/contact6.jpg", Title = "Ночной вид" }
                },
                ContactForm = new ContactFormModel()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Contacts(HotelViewModel model)
        {
            if (ModelState.IsValid && model.ContactForm != null)
            {
                TempData["SuccessMessage"] = "Заявка успешно отправлена! Мы свяжемся с вами.";
                return RedirectToAction("Contacts");
            }
            return View(model);
        }

        // ДОБАВЬТЕ ЭТОТ МЕТОД:
        public async Task<IActionResult> Rooms()
        {
            var rooms = await _context.Rooms.Where(r => r.IsAvailable).ToListAsync();
            return View(rooms);
        }
    }
}