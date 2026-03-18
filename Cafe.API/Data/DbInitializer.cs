using Cafe.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Cafe.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();


            var admin = context.Users.FirstOrDefault(u => u.Username == "admin");
            if (admin == null)
            {
                context.Users.Add(new User { Username = "admin", PasswordHash = "admin123", Role = "Admin", FullName = "Главный Администратор" });
            }
            else if (string.IsNullOrEmpty(admin.PasswordHash) || admin.PasswordHash == "")
            {
                admin.PasswordHash = "admin123";
            }


            var bUser = context.Users.FirstOrDefault(u => u.Username == "user");
            if (bUser == null)
            {
                context.Users.Add(new User { Username = "user", PasswordHash = "user123", Role = "Manager", FullName = "Александр В. (Бариста)" });
            }
            else if (string.IsNullOrEmpty(bUser.PasswordHash) || bUser.PasswordHash == "")
            {
                bUser.PasswordHash = "user123";
            }
            context.SaveChanges();

            var coffeeCategory = context.Categories.FirstOrDefault(c => c.Name == "Кава");
            if (coffeeCategory == null)
            {
                coffeeCategory = new Category { Name = "Кава" };
                context.Categories.Add(coffeeCategory);
                context.SaveChanges();
            }

            var dessertCategory = context.Categories.FirstOrDefault(c => c.Name == "Десерти");
            if (dessertCategory == null)
            {
                dessertCategory = new Category { Name = "Десерти" };
                context.Categories.Add(dessertCategory);
                context.SaveChanges();
            }

            var coldCategory = context.Categories.FirstOrDefault(c => c.Name == "Холодні напої");
            if (coldCategory == null)
            {
                coldCategory = new Category { Name = "Холодні напої" };
                context.Categories.Add(coldCategory);
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { Name = "Еспресо", Price = 45, CategoryId = coffeeCategory.Id, IsActive = true, Description = "Класична бадьорість" },
                    new Product { Name = "Капучино", Price = 60, CategoryId = coffeeCategory.Id, IsActive = true, Description = "Ніжна пінка" },
                    new Product { Name = "Айс Лате", Price = 75, CategoryId = coldCategory.Id, IsActive = true, Description = "З льодом та сиропом" },
                    new Product { Name = "Чизкейк", Price = 85, CategoryId = dessertCategory.Id, IsActive = true, Description = "Класичний десерт" }
                );
                context.SaveChanges();
            }
        }
    }
}