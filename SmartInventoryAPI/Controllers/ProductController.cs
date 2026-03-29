using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.DTOs;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public ProductController(AppDbContext _appDbContext) {
            appDbContext = _appDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto ) {
            if (dto.Price <= 0) {
                return BadRequest("Price must be greater than zero");
            }
        
        return Ok();
        }
    }
}
