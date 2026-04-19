using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelUyutClean.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название номера обязательно")]
        [Display(Name = "Название номера")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Описание обязательно")]
        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Display(Name = "Цена за ночь")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Площадь (м²)")]
        public int Area { get; set; }

        [Display(Name = "Максимум гостей")]
        public int MaxGuests { get; set; }

        [Display(Name = "Количество кроватей")]
        public int Beds { get; set; }

        [Display(Name = "Фото номера")]
        public string? ImageUrl { get; set; }

        // Это поле для загрузки файла (не сохраняется в БД)
        [NotMapped]
        [Display(Name = "Загрузить фото")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Удобства")]
        public string? Amenities { get; set; }

        [Display(Name = "Доступен")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Категория")]
        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}