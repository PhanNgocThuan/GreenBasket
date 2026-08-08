using GreenBasket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenBasket.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Products.AnyAsync())
            {
                return;
            }

            // 1. FARMS
            var farms = new List<Farm>
            {
                new Farm { Name = "Green Valley Farm", Location = "Dalat, Lam Dong", ContactInfo = "0901-234-567" },
                new Farm { Name = "Mekong Orchard", Location = "Can Tho", ContactInfo = "0987-654-321" },
                new Farm { Name = "Sunrise Organic Farm", Location = "Hanoi", ContactInfo = "0911-222-333" }
            };

            context.Farms.AddRange(farms);
            await context.SaveChangesAsync();

            // 2. PRODUCTS
            var products = new List<Product>
            {
                // LeafyGreens
                new Product { Name = "Crystal Lettuce", Category = ProductCategory.LeafyGreens, Description = "Crisp hydroponic lettuce.", Price = 1.50m, Unit = "kg", Organic = true, ImageUrl = "/img/lettuce.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Green Broccoli", Category = ProductCategory.LeafyGreens, Description = "Freshly cut broccoli, same-day harvest.", Price = 1.70m, Unit = "kg", Organic = true, ImageUrl = "/img/broccoli.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Celery Stalks", Category = ProductCategory.LeafyGreens, Description = "Thick celery stalks, great for juicing.", Price = 1.20m, Unit = "kg", Organic = true, ImageUrl = "/img/celery.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Purple Cabbage", Category = ProductCategory.LeafyGreens, Description = "Crunchy purple cabbage, rich in vitamins.", Price = 1.30m, Unit = "kg", Organic = true, ImageUrl = "/img/purple-cabbage.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Straw Mushroom", Category = ProductCategory.LeafyGreens, Description = "Freshly picked straw mushrooms.", Price = 3.80m, Unit = "kg", Organic = true, ImageUrl = "/img/mushroom.jpg", IsActive = true, StockQty = 0 },

                // RootVeggies
                new Product { Name = "Baby Carrot", Category = ProductCategory.RootVeggies, Description = "Naturally sweet Dalat carrots.", Price = 1.10m, Unit = "kg", Organic = false, ImageUrl = "/img/carrot.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Golden Potato", Category = ProductCategory.RootVeggies, Description = "Soft golden potatoes, great for soups.", Price = 0.90m, Unit = "kg", Organic = false, ImageUrl = "/img/potato.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Honey Sweet Potato", Category = ProductCategory.RootVeggies, Description = "Sweet potatoes that ooze honey when roasted.", Price = 1.50m, Unit = "kg", Organic = true, ImageUrl = "/img/sweet-potato.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Cherry Tomato", Category = ProductCategory.RootVeggies, Description = "Juicy, naturally sweet cherry tomatoes.", Price = 2.00m, Unit = "kg", Organic = true, ImageUrl = "/img/cherry-tomato.jpg", IsActive = true, StockQty = 0 },

                // TropicalFruit
                new Product { Name = "Ri6 Durian", Category = ProductCategory.TropicalFruit, Description = "Seedless durian with golden flesh.", Price = 6.50m, Unit = "kg", Organic = false, ImageUrl = "/img/durian.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "034 Avocado", Category = ProductCategory.TropicalFruit, Description = "Creamy avocado with a small seed.", Price = 2.80m, Unit = "kg", Organic = true, ImageUrl = "/img/avocado.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Laba Banana", Category = ProductCategory.TropicalFruit, Description = "Sweet, fragrant heirloom banana.", Price = 1.10m, Unit = "bunch", Organic = true, ImageUrl = "/img/banana.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Royal Cantaloupe", Category = ProductCategory.TropicalFruit, Description = "Orange-fleshed melon, richly sweet.", Price = 3.50m, Unit = "each", Organic = true, ImageUrl = "/img/cantaloupe.jpg", IsActive = true, StockQty = 0 },

                // SeasonalFruit
                new Product { Name = "New Zealand Strawberry", Category = ProductCategory.SeasonalFruit, Description = "Large, mildly sweet strawberries.", Price = 10.50m, Unit = "kg", Organic = true, ImageUrl = "/img/strawberry.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Vinh Long Orange", Category = ProductCategory.SeasonalFruit, Description = "Juicy oranges, perfect for fresh juice.", Price = 1.50m, Unit = "kg", Organic = false, ImageUrl = "/img/orange.jpg", IsActive = true, StockQty = 0 },
                new Product { Name = "Ha Giang Rock Apple", Category = ProductCategory.SeasonalFruit, Description = "Crisp, sweet apple with red skin.", Price = 1.90m, Unit = "kg", Organic = true, ImageUrl = "/img/apple.jpg", IsActive = true, StockQty = 0 }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // 3. BATCHES
            var batches = new List<Batch>();
            var random = new Random();

            foreach (var product in products)
            {
                var randomFarm = farms[random.Next(farms.Count)];
                int quantity = random.Next(50, 201);

                batches.Add(new Batch
                {
                    ProductId = product.Id,
                    FarmId = randomFarm.Id,
                    HarvestDate = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                    ReceivedDate = DateTime.UtcNow,
                    QuantityReceived = quantity,
                    QuantityRemaining = quantity,
                    CostPrice = product.Price * 0.6m
                });

                product.StockQty += quantity;
                product.StockStatus = StockStatus.InStock;
            }

            context.Batches.AddRange(batches);
            await context.SaveChangesAsync();
        }
    }
}