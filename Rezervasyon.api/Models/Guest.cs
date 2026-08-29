using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Rezervasyon.Api.Models
{
    public class Guest
    {
        
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }

        public int ReservationId { get; set; }
        [ValidateNever]
        public virtual Reservation Reservation { get; set; }
    }
}
