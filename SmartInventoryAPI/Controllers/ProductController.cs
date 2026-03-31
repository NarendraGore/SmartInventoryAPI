using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.DTOs.Product;
using SmartInventoryAPI.Models;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public ProductController(AppDbContext appDbContext) {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto) {
            if (dto.Price <= 0) {
                return BadRequest("Price must be greater than zero");
            }
            var exist = await _appDbContext.Products
                .AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower());

            if (exist) {
                return BadRequest("Product with the same name already exists");
            }

            var product = new Product {


                Name = dto.Name,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId
            };

            _appDbContext.Products.Add(product);
            await _appDbContext.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpGet]

        public async Task<IActionResult> GetAllProducts() {
            var products = await _appDbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Select(p => new ProductResponseDto {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    SupplierName = p.Supplier.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetProductByID(int id) {

            var product = await _appDbContext.Products
                .Include (p => p.Category)
                .Include (p => p.Supplier)
                .Where (p => p.Id == id)
                .Select(p=> new ProductResponseDto 
                {
                Id=p.Id,
                Name =p.Name,
                Price=p.Price,
                CategoryName=p.Category.Name, 
                SupplierName=p.Supplier.Name
                }).FirstOrDefaultAsync();

            if (product == null) { 
            return NotFound("Product Not Found");
            }
            return Ok(product);
        }

        // Get By category Id

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _appDbContext.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    SupplierName = p.Supplier.Name
                })
                .ToListAsync();

            if (!products.Any())
                return NotFound("No products found for this category");

            return Ok(new
            {
                success = true,
                message = "Products fetched successfully",
                data = products
            });
        }

        //Update Product By Id
        public async Task<IActionResult> UpdateProductByID(int id, UpdateProductDto dto)
        {
            var product = await _appDbContext.Products.FindAsync (id);

            if (product == null) {
                return NotFound("Product Not Found");
            }
            if (dto.Price <= 0) {
                return BadRequest("Price Must Be Greater than Zero..");
            }
            product.Name = dto.Name;
            product.Price = dto.Price;

            await _appDbContext.SaveChangesAsync();
            return Ok(dto);
        }

        //Delete Product
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteProductById(int id) {

            var product = await _appDbContext.Products.FindAsync(id);

            if (product == null) {
                return NotFound("Product Not Found");
            
            }
            _appDbContext.Products.Remove(product);
            await _appDbContext.SaveChangesAsync();


            return Ok("Product Deleted Successfully");
        }

    }
}
