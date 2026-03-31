using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.Models;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public CategoryController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //Create Category
        [HttpPost]

        public async Task<IActionResult> CreateCategory(Category category) {

            if (string.IsNullOrWhiteSpace(category.Name)) {
                return BadRequest("Category Name Is Required");

            }
            var exists = await _appDbContext.Categories
            .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());

            if (exists) {

                return BadRequest("category already Exists");
            }
            _appDbContext.Categories.Add(category);
            await _appDbContext.SaveChangesAsync();
            return Ok(new {
                success = true,
                message = "Category Created Successfully",
                data = category
            });
        }

        //Get Category
        [HttpGet]
        public async Task<IActionResult> GetAllCategory() {

            var categories = await _appDbContext.Categories
                .Select(c => new {
                    c.Id,
                    c.Name })
                .ToListAsync();


            return Ok(
                new {
                    succes = true,
                    data = categories

                });
        }


        //Update Category
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateCategory([FromRoute]int id, [FromBody]Category category) {
            var category1 = await _appDbContext.Categories.FindAsync(id);
            if (category1 == null) {
                return NotFound("Product Not Found");
            }

            category1.Name = category.Name;

            await _appDbContext.SaveChangesAsync();

            return Ok(category1);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id) {

            var category = await _appDbContext.Categories.FindAsync(id);

            if (category == null) { 
            return NotFound("Category Not Found");
            }

            var hasProduct = await _appDbContext.Products.AnyAsync(p => p.CategoryId == id);

            if (hasProduct) {
                return BadRequest("can not delete category with existing products");
            }

            _appDbContext.Categories.Remove(category);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { 
            
            success = true,
            message="category Deleted Successfully"});
        
        }


    }
}
