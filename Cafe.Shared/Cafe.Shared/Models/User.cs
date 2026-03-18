using System;
using System.Collections.Generic;
using System.Text;

namespace Cafe.Shared.Models
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty; 
        public string PasswordHash { get; set; } = string.Empty; 
        public string Role { get; set; } = "Waiter"; 
        public string FullName { get; set; } = string.Empty; 
    }
}
