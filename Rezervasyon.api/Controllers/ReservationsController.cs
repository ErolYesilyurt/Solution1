using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
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

            bool isStopSaleActive = await _context.StopSales
            .AnyAsync(ss => ss.HotelId == reservation.HotelId &&
                            reservation.GirisTarihi < ss.BitisTarihi &&
                            reservation.CikisTarihi > ss.BaslangicTarihi);

            if (isStopSaleActive)
            {
              
                return BadRequest("Seçilen tarihlerde otel rezervasyona kapalıdır (Stop Sale).");
            }

            int totalGuestsInReservation = reservation.YetiskinSayisi + reservation.CocukSayisi;

           
            var applicablePrice = await _context.Prices
                .FirstOrDefaultAsync(p => p.HotelId == reservation.HotelId &&
                                           reservation.GirisTarihi >= p.GecerlilikBaslangic &&
                                           reservation.CikisTarihi <= p.GecerlilikBitis);

          
            if (applicablePrice == null)
            {
                return BadRequest($"Seçilen otel/tarihler için geçerli bir fiyat tanımı bulunamadı.");
            }

           
            if (totalGuestsInReservation > applicablePrice.MaxGuests) 
            {
                
                return BadRequest($"Kişi sayısı ({totalGuestsInReservation}), bu oda/fiyat için izin verilen maksimum kapasiteyi ({applicablePrice.MaxGuests}) aşıyor.");
            }
            applicablePrice.MaxGuests -= totalGuestsInReservation;
            _context.Entry(applicablePrice).State = EntityState.Modified;
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

        [HttpGet("forUser/{UserId}")]
        
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservationsForUser(int UserId)
        {
            return await _context.Reservations
                .Where(p => p.UserId == UserId)
                .Include(p => p.Hotel)
                .ToListAsync();
        }

    }
}
