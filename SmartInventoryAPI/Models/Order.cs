using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace SmartInventoryAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public  string CustomerName { get; set; }

        public DateTime OrderDate { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
