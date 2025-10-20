using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rezervasyon.Api.Models
{
    public class Price
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }// Gecelik Ücret

       
        public DateTime GecerlilikBaslangic { get; set; } 
        public DateTime GecerlilikBitis { get; set; } 

        // --- İlişkiler ---

     
        public int HotelId { get; set; }
        [ValidateNever]
        public virtual Hotel Hotel { get; set; }

   
        public int CurrencyId { get; set; }
        [ValidateNever]
        public virtual Currency Currency { get; set; }
    }
}