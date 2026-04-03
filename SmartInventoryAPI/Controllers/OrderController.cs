using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.DTOs.Order;
using SmartInventoryAPI.Models;

namespace SmartInventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public OrderController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //  Get Available Stock
        private async Task<int> GetAvailableStock(int productId)
        {
            var stockIn = await _appDbContext.StockTransactions
                .Where(s => s.ProductId == productId && s.Type == "IN")
                .SumAsync(s => (int?)s.Quantity) ?? 0;

            var stockOut = await _appDbContext.StockTransactions
                .Where(s => s.ProductId == productId && s.Type == "OUT")
                .SumAsync(s => (int?)s.Quantity) ?? 0;

            return stockIn - stockOut;
        }

        // Create Order
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Order must have at least one item");

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    CustomerName = dto.CustomerName,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _appDbContext.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                        return BadRequest($"Product with ID {item.ProductId} not found");

                    //  Stock validation
                    var availableStock = await GetAvailableStock(product.Id);

                    if (availableStock < item.Quantity)
                        return BadRequest($"Insufficient stock for product {product.Name}");

                    var itemTotal = product.Price * item.Quantity;
                    totalAmount += itemTotal;

                    //  Add Order Item
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Price = product.Price
                    });

                  //stock Out
                    _appDbContext.StockTransactions.Add(new StockTransaction
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Type = "OUT", 
                        Date = DateTime.UtcNow
                    });
                }

                _appDbContext.Orders.Add(order);
                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Order Created Successfully",
                    orderId = order.Id,
                    totalAmount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}