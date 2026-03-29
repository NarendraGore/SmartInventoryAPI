namespace SmartInventoryAPI.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public string Type { get; set; }
        public DateTime Date { get; set; }
    }
}
