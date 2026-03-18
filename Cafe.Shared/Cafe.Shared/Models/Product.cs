using System;
using System.Collections.Generic;
using System.Text;

namespace Cafe.Shared.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty; 
        public decimal Price { get; set; } 
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
