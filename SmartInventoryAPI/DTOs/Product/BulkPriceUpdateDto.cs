namespace SmartInventoryAPI.DTOs.Product
{
    public class BulkPriceUpdateDto
    {
        public int ProductId { get; set; }
        public decimal NewPrice { get; set; }
    }
}
