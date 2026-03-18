using System;
using System.Collections.Generic;
using System.Text;

namespace Cafe.Shared.Models
{
    public class Category: BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
