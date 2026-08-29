using System.ComponentModel.DataAnnotations;

namespace Rezervasyon.Api.Models // Veya .Dtos
{
    public class PriceCalculationRequest
    {
        [Required]
        public int HotelId { get; set; }
        [Required]
        public DateTime CheckInDate { get; set; }
        [Required]
        public DateTime CheckOutDate { get; set; }

        // Sadece doğum tarihlerini alıyoruz
        public List<DateTime> GuestBirthDates { get; set; } = new List<DateTime>();
    }
}