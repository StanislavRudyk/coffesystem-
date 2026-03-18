using System;
using System.Collections.Generic;
using System.Text;

namespace Cafe.Shared.Models
{
    public class Order: BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int Status { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

    }
}
