using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.Models;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public SupplierController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //Create Supplier
        [HttpPost]
        public async Task<IActionResult> CreateSupplier( Supplier supplier) {

            if (string.IsNullOrWhiteSpace(supplier.Name)) {

                return BadRequest("Supplier Name is Required");
            }

            var exists = await _appDbContext.Suppliers
                .AnyAsync(s => s.Name.ToLower() == supplier.Name);
            if (exists) {
                return BadRequest("Supplier Name Already exists");
            }

            _appDbContext.Suppliers.Add(supplier);
            await _appDbContext.SaveChangesAsync();
            return Ok(
                new { 
                success= true,
                message="Supplier Added Successfully",
                data= supplier
                });

        }

        //Get Supplier
        [HttpGet]

        public async Task<IActionResult> GetAllSupplier() {

            var supplier = await _appDbContext.Suppliers.Select(s => new
            {
                s.Id,
                s.Name
            }).ToListAsync();

            return Ok(
                new { 
                
                success = true,
                data= supplier
                
                });
        }

        //Update Supplier
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateSupplier([FromRoute] int id, [FromBody] Supplier supplier) {

            var supplier1 = await _appDbContext.Suppliers.FindAsync(id);

            if(supplier1 == null)
            {

                return NotFound("Supplier Not Found");
            }

            supplier1.Name = supplier.Name;
            await _appDbContext.SaveChangesAsync();


            return Ok(supplier1);
        }

        //Delete Supplier
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteSupplier(int id) {
            var supplier = await _appDbContext.Suppliers.FindAsync(id);
            if (supplier == null) {

                return NotFound("Supplier Not Found");
            }

            var hasProducts= await _appDbContext.Products
                .AnyAsync(p=>p.SupplierId == id);

            if (hasProducts) {

                return BadRequest("can not delete category with existing products");
            }
            _appDbContext.Suppliers.Remove(supplier);
            await _appDbContext.SaveChangesAsync();
        
        return Ok(
            new { 
            succes = true,
            message="Supplier Deleted Successfully."

            });    
        }

    }
}
