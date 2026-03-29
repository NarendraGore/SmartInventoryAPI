namespace SmartInventoryAPI.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; } 
        public decimal Price { get; set; }
        public string categoryId { get; set; }

        public string supplierId { get; set; }
    }
}
