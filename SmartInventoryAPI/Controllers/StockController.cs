using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.DTOs.Stock;
using SmartInventoryAPI.Models;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public StockController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost("add-stock")]
        public async Task<IActionResult> AddStock(AddStockDto dtos ) 
        {
            var product = await _appDbContext.Products.FirstOrDefaultAsync(p=>p.Id == dtos.ProductId);
            if (dtos.Quantity <= 0) {

                return BadRequest("Invalid Quantity");
            }

            if (product == null) { 
            return NotFound($"Product with ID {dtos.ProductId} not found.");
            }
            //stock Added
            var stock = new StockTransaction
            {
                ProductId = dtos.ProductId,
                Quantity = dtos.Quantity,
                Type = "IN",
                Date = DateTime.UtcNow
            };
            _appDbContext.StockTransactions.Add(stock);
            await _appDbContext.SaveChangesAsync();

            return Ok(
                new { 
                    success=true,
                    message="Stock Added Successfully" });
        }
       
    }
}
