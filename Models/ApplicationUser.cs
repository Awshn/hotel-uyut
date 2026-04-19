using Microsoft.AspNetCore.Identity;

namespace HotelUyutClean.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Связь с бронированиями
        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}