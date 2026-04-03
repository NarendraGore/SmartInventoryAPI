 using System.Runtime.CompilerServices;
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

        //Bulk Insert

        [HttpPost("bulk-insert")]

        public async Task<IActionResult> BulkInsert(List<CreateProductDto> dto) {

            var products = dto
                .Select(d => new Product {
                    Name = d.Name,
                    Price = d.Price,
                    CategoryId = d.CategoryId,
                    SupplierId = d.SupplierId
                }).ToList();

            await _appDbContext.Products.AddRangeAsync(products);
            await _appDbContext.SaveChangesAsync();

            return Ok(
                new 
                {
                success = true,
                count=  products.Count

                });

        }

        [HttpGet]

        public async Task<IActionResult> GetAllProducts(int page=1, int pagesize=10) {


            var query = _appDbContext.Products
                .AsNoTracking()
                .AsQueryable();

            var totalrecords = await query.CountAsync();


            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    SupplierName = p.Supplier.Name,

                    AvailableStock = _appDbContext.StockTransactions
            .Where(s => s.ProductId == p.Id)
            .Sum(s => s.Type == "IN" ? s.Quantity : -s.Quantity)
                }).ToListAsync();

            //var products = await _appDbContext.Products
            //    .Include(p => p.Category)
            //    .Include(p => p.Supplier)
            //    .Select(p => new ProductResponseDto {
            //        Id = p.Id,
            //        Name = p.Name,
            //        Price = p.Price,
            //        CategoryName = p.Category.Name,
            //        SupplierName = p.Supplier.Name
            //    })
            //    .ToListAsync();

            return Ok(new 
            { 
            totalrecords,
           data= products,
           page,
           pagesize

            });
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
                SupplierName=p.Supplier.Name,
                AvailableStock = _appDbContext.StockTransactions
            .Where(s => s.ProductId == p.Id)
            .Sum(s => s.Type == "IN" ? s.Quantity : -s.Quantity)
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

        //Get By Price Range

        [HttpGet("price-range")]
        public async Task<IActionResult> GetByPriceRange(decimal minPrice, decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                return BadRequest("Invalid price range");
            var products = await _appDbContext.Products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
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
                return NotFound("No products found in this price range");
            return Ok(new
            {
                success = true,
                message = "Products fetched successfully",
                data = products
            });
        }


        //Get Product By Search

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            string? sortOrder,
            int page=1,
            int pageSize =10
            ) {

            var query = _appDbContext.Products
                .AsNoTracking()
                .AsQueryable();

            //search by name
            if (!string.IsNullOrWhiteSpace(search)) {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            //filter by category Id
            if (categoryId.HasValue) {

                query = query.Where(p => p.CategoryId == categoryId);
            }

            //filter by price range

            if (minPrice.HasValue) {

                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice);
            
            }

            //Sorting

            switch (sortBy?.ToLower()) {
                case "price":

                    if (sortOrder == "asc")
                    {
                        query = query.OrderBy(p => p.Price);
                    }
                    else 
                    {

                        query = query.OrderByDescending(p => p.Price); 
                    }
                    break;

                case "latest":

                    if (sortOrder == "asc")
                    {

                        query = query.OrderBy(p => p.Id);

                    }
                    else 
                    {
                        query = query.OrderByDescending(p=>p.Id);
                    }
                    break;
                default:

                    query = query.OrderBy(p => p.Id);
                    break;
            
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    SupplierName = p.Supplier.Name
                }).ToListAsync();




            return Ok(new 
            {
                totalRecords,
                page,
                pageSize,
                data
            }
                
                
                );
        }

        //Update Product By Id
        [HttpPut("{id}")]
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

        //Bulk Price Update
        [HttpPut("bulk-update-price")]

        public async Task<IActionResult> BulkPriceUpdate(List<BulkPriceUpdateDto> dtos) {
            var ids = dtos.Select(d=>d.ProductId).ToList();

            var products = await _appDbContext.Products
                .Where(p => ids.Contains(p.Id)).ToListAsync();

            if (!products.Any()) {

                return BadRequest("Products are not fetched");
            }

            foreach (var product in products) { 
            
            var dto =dtos.First(x=> x.ProductId == product.Id);
                product.Price = dto.NewPrice;
            }
            await _appDbContext.SaveChangesAsync();

            return Ok(new
            { 
            success= true,
            message= "Prices Updated Successfully",
            });
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
