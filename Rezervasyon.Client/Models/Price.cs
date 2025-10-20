using Rezervasyon.Client.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Rezervasyon.Client.Models
{
    public class Price
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } 

        
        public DateTime GecerlilikBaslangic { get; set; } 
        public DateTime GecerlilikBitis { get; set; }   




        public int HotelId { get; set; }
        public virtual Hotel Hotel { get; set; }


        public int CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }
    }
}