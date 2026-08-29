using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezervasyon.Api.Data;
using Rezervasyon.Api.Models;
using System.Security.AccessControl;
using System.Security.Claims;


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


        [HttpGet("financial-summary")]
        public async Task<ActionResult<FinancialSummaryDto>> GetFinancialSummary()
        {

            var stats = await _context.Reservations
                .GroupBy(r => r.IsCancelled) 
                .Select(g => new
                {
                    IsCancelled = g.Key, 
                    Count = g.Count(), 
                    ToplamFiyat = g.Sum(r => r.ToplamFiyat), 
                    IptalKesintisi = g.Sum(r => r.IptalKesintisi) 
                })
                .ToListAsync(); 

            
            var activeStats = stats.FirstOrDefault(s => s.IsCancelled == false) ?? new { IsCancelled = false, Count = 0, ToplamFiyat = 0m, IptalKesintisi = 0m };
            var cancelledStats = stats.FirstOrDefault(s => s.IsCancelled == true) ?? new { IsCancelled = true, Count = 0, ToplamFiyat = 0m, IptalKesintisi = 0m };

            var summary = new FinancialSummaryDto
            {
                
                ToplamKazanc = activeStats.ToplamFiyat + cancelledStats.IptalKesintisi,
                AktifRezervasyonSayisi = activeStats.Count,

                IptalEdilenTutar = cancelledStats.IptalKesintisi,
                IptalEdilenRezervasyonSayisi = cancelledStats.Count,

                
                PotansiyelKazanc = activeStats.ToplamFiyat + cancelledStats.ToplamFiyat,
                ToplamRezervasyonKaydi = activeStats.Count + cancelledStats.Count
            };

            return Ok(summary);
        }
        [HttpGet]
        [Authorize(Roles = "Admin, Worker")] 
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations([FromQuery] int? hotelId = null)
        {
            var query = _context.Reservations.AsQueryable();

            if (hotelId.HasValue && hotelId.Value > 0)
            {
                query = query.Where(r => r.HotelId == hotelId.Value);
            }

            
            var reservationsDto = await query
                .OrderByDescending(r => r.GirisTarihi)
                .Select(r => new ReservationDto 
                {
                    Id = r.Id,
                    GirisTarihi = r.GirisTarihi,
                    CikisTarihi = r.CikisTarihi,
                    ToplamFiyat = r.ToplamFiyat,
                    Aciklama = r.Aciklama,
                    IsCancelled = r.IsCancelled,
                    CurrencyCode = r.CurrencyCode,


                    Hotel = r.Hotel,     
                    User = r.User,  

                  
                    Guests = r.Guests.Select(g => new GuestDto
                    {
                        Id = g.Id,
                        FirstName = g.FirstName,
                        LastName = g.LastName,
                        DateOfBirth = g.DateOfBirth
                    }).ToList()
                })
                .ToListAsync();

            return Ok(reservationsDto);
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

            int totalGuestsInReservation = reservation.Guests.Count;

           
            var applicablePrice = await _context.Prices
                .FirstOrDefaultAsync(p => p.HotelId == reservation.HotelId &&
                                           !reservation.IsCancelled &&
                                           reservation.GirisTarihi >= p.GecerlilikBaslangic &&
                                           reservation.CikisTarihi <= p.GecerlilikBitis &&
                                           p.MaxGuests-totalGuestsInReservation>=0);

          
            if (applicablePrice == null)
            {
                return BadRequest($"Seçilen otel/tarihler için geçerli bir fiyat tanımı bulunamadı.");
            }

           
            if (totalGuestsInReservation > applicablePrice.MaxGuests) 
            {
                
                return BadRequest($"Kişi sayısı ({totalGuestsInReservation}), bu oda/fiyat için izin verilen maksimum kapasiteyi ({applicablePrice.MaxGuests}) aşıyor.");
            }
            reservation.PriceId = applicablePrice.Id;
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
            if(!reservation.IsCancelled && reservation.PriceId.HasValue)
            {
                var price = await _context.Prices.FirstOrDefaultAsync(p => p.Id == reservation.PriceId);
                if(price != null)
                {
                    price.MaxGuests += reservation.Guests.Count();
                }
            }
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
                .Include(p=> p.Guests)
                .Include(p => p.CurrencyCode)
                .ToListAsync();
        }

        [HttpPut("{id}/cancel")]
        [Authorize] 
        public async Task<IActionResult> CancelReservation(int id, [FromBody] CancelRequestDto request)
        {
            int currentUserId = request.UserId;


            var tokenUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(tokenUserIdString) || !int.TryParse(tokenUserIdString, out int tokenUserId) || tokenUserId != currentUserId)
            {
                return Forbid($"Token'daki kullanıcı kimliği ile gönderilen kimlik uyuşmuyor.{tokenUserIdString}  current user ıd de bu {currentUserId}");
            }

            var reservation = await _context.Reservations.FirstOrDefaultAsync(x=>x.Id== id);
            if (reservation == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

            if (!User.IsInRole("Admin") && reservation.UserId != currentUserId)
            {
                return Forbid("Bu rezervasyonu iptal etme yetkiniz yok.");
            }

            if (reservation.IsCancelled)
            {
                return BadRequest("Bu rezervasyon zaten iptal edilmiş.");
            }

            decimal kesintiTutari = 0;
            TimeSpan kalanSure =reservation.GirisTarihi - DateTime.UtcNow;
            if(kalanSure.TotalDays > 2 && kalanSure.TotalDays<7) {
                kesintiTutari = reservation.ToplamFiyat * 0.5m;
            }
            else if(kalanSure.TotalDays <= 2) {
                kesintiTutari = reservation.ToplamFiyat;
            }


            reservation.IsCancelled = true;
            reservation.CancelledOn = DateTime.UtcNow;
            reservation.IptalKesintisi = kesintiTutari;

            _context.Entry(reservation).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}
