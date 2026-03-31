namespace SmartInventoryAPI.DTOs.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string CategoryName { get; set; }

        public string SupplierName { get; set; }
    }
}
