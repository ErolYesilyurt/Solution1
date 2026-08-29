using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezervasyon.Api.Models;
using Rezervasyon.Api.Data;
using Microsoft.Identity.Client;

namespace Rezervasyon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StopSalesController : ControllerBase
    {
        private readonly DataContext _context;

        public StopSalesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("forHotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<StopSale>>> GetStopSalesForHotel(int hotelId)
        {
            return await _context.StopSales
                .Where(ss => ss.HotelId == hotelId)
                .OrderBy(ss => ss.BaslangicTarihi)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> AddStopSale(StopSale stopSale)
        {
            if (stopSale == null)
                return BadRequest();
            _context.StopSales.Add(stopSale);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStopSale(int id)
        {
            var stopSale = _context.StopSales.FirstOrDefault(x => x.Id == id);
            if (stopSale == null)
                return NotFound();
            _context.StopSales.Remove(stopSale);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAllStopSales()
        {
            var stopSales = _context.StopSales.ToList();
            return Ok(stopSales);
        }

        [HttpPut("{id}")]

        public IActionResult UpdateStopSale(int id, [FromBody] StopSale stopsale)
        {   if (stopsale == null || stopsale.Id != id)
                return BadRequest();
            _context.Entry(stopsale).State = EntityState.Modified;
            try
            {
                _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            { if (_context.StopSales.Any(x => x.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();



            
            
        }
    }
}
