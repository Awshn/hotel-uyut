using System.ComponentModel.DataAnnotations;

namespace HotelUyutClean.Models
{
    public class HotelViewModel
    {
        public string? HotelName { get; set; }
        public string? WelcomeMessage { get; set; }
        public List<RoomImage>? HomeImages { get; set; }
        public List<GalleryImage>? AboutImages { get; set; }
        public List<GalleryImage>? ContactImages { get; set; }
        public ContactFormModel? ContactForm { get; set; }
    }

    public class RoomImage
    {
        public string? ImagePath { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }

    public class GalleryImage
    {
        public string? ImagePath { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    public class ContactFormModel
    {
        [Required(ErrorMessage = "Введите ваше имя")]
        public string? Name { get; set; }
        
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Введите телефон")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string? Phone { get; set; }
        
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int Guests { get; set; } = 1;
        public string? RoomType { get; set; }
        public string? Message { get; set; }
    }
}