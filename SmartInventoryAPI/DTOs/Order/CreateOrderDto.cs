namespace SmartInventoryAPI.DTOs.Order
{
    public class CreateOrderDto
    {
        public string CustomerName { get; set; } 
        public List<OrderItemDto> Items { get; set; }
    }
}
