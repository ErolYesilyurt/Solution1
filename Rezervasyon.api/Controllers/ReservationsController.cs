using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezervasyon.Api.Data;
using Rezervasyon.Api.Models;

namespace Rezervasyon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController : ControllerBase
    {   private readonly DataContext _context;
            public ReservationsController(DataContext context) {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetReservations([FromQuery] int? id=null)
        {   if(id != null)
                {
                var reservation = _context.Reservations
                    .Include(x => x.Hotel)
                    .Include(x => x.User)
                    .FirstOrDefault(x => x.Id == id);
                if (reservation == null)
                    return NotFound();
                return Ok(reservation);
            }
            var reservations = _context.Reservations
                .Include(x=> x.Hotel)
                .Include(x=> x.User).ToList();
            return Ok(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> AddReservation([FromBody] Reservation reservation)
        {
            if (reservation == null)
                return BadRequest();

            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        { var reservation = _context.Reservations.FirstOrDefault(x => x.Id==id);
            if(reservation == null)
                return BadRequest();
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditReservation(int id, [FromBody]Reservation reservation)
        {   
            if (reservation == null)
                return BadRequest();

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reservations.Any(x => x.Id == reservation.Id))
                    return NotFound();
                else
                    throw;
                
            }
            
            return NoContent();


        }

    }
}
