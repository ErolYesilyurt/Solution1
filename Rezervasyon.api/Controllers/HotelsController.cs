using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezervasyon.Api.Models;
using Rezervasyon.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Rezervasyon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bu controller'a sadece giriş yapmış kullanıcılar erişebilir
    public class HotelsController : ControllerBase
    {   private readonly DataContext _context;

        public HotelsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetHotels()
        {   
            // TODO: Bu veriler daha sonra veritabanından gelecek.
            // Şimdilik test için sabit bir liste döndürüyoruz.
           /* var hotels = new List<Hotel>
            {
                new Hotel { Id = 1, Name = "Grand Park Lara", City = "Antalya", Stars = 5 },
                new Hotel { Id = 2, Name = "The Marmara Pera", City = "İstanbul", Stars = 4 },
                new Hotel { Id = 3, Name = "Swissôtel Uludağ", City = "Bursa", Stars = 5 }
            };*/

            var hotels =_context.Oteller.ToList();

            return Ok(hotels);
        }

        [HttpPost]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> AddHotel([FromBody] Hotel hotel)
        {
            if (hotel ==null)
            { return BadRequest(); }

            _context.Oteller.Add(hotel);
            await _context.SaveChangesAsync();
            return Ok(hotel);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles="Admin")]

        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Oteller.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }
            _context.Oteller.Remove(hotel);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotel(int id)
        {
            var hotel = await _context.Oteller.FirstOrDefaultAsync(a => a.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }
            return Ok(hotel);
        }

        [HttpPut("{id}")]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] Hotel updatedHotel)
        {
           if(id != updatedHotel.Id)
            {
                return BadRequest();
            }
            
           _context.Entry(updatedHotel).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {  if(!_context.Oteller.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
       
            return NoContent();

        }

    }
}