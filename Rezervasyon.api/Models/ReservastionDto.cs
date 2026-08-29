using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Rezervasyon.Api.Models;

namespace Rezervasyon.Api.Models
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public DateTime GirisTarihi { get; set; }
        public DateTime CikisTarihi { get; set; }
        public decimal ToplamFiyat { get; set; }
        public string? Aciklama { get; set; }

     

        public int? UserId { get; set; }
        [ValidateNever]
        public virtual User? User { get; set; }
     
        public int HotelId { get; set; }
        [ValidateNever]
        public virtual Hotel Hotel { get; set; }
        // Misafir listesi (ama DTO olarak)
        public List<GuestDto> Guests { get; set; } = new List<GuestDto>();

        public bool IsCancelled { get; set; } = false;

        public string CurrencyCode { get; set; }
    }
}