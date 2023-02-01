using Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ApplicationDbContext : DbContext
    {

        #region constructor
        public ApplicationDbContext(DbContextOptions options) : base(options)
        { }
        #endregion

        #region sets
        public DbSet<Domain.Authentication.User> Users { get; set; } = default!;
        public DbSet<Domain.Authentication.UserClaim> UserClaims { get; set; } = default!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Auths
            modelBuilder.Entity<Domain.Authentication.User>()
                .ToTable("user", "authentication")
                .HasIndex(x => x.Username).IsUnique();

            modelBuilder.Entity<Domain.Authentication.UserClaim>()
                .ToTable("user_claim", "authentication");

            // modelBuilder.Entity<Domain.Restaurants.Entities.RestaurantLanguage>()
            //     .ToTable("restaurant_language", "restaurants")
            //     .HasKey(rl => new { rl.RestaurantId, rl.LanguageId });

            // modelBuilder.Entity<Domain.Products.Entities.ProductInfo>()
            //     .ToTable("product_info", "products");
            // modelBuilder.Entity<Domain.Products.Entities.ProductInfo>().HasIndex(x => x.NormalizedName);
            // modelBuilder.Entity<Domain.Products.Entities.ProductInfo>().HasIndex(x => x.SlugName);
            // modelBuilder.Entity<Domain.Products.Entities.ProductInfo>().HasIndex(x => x.ProductId);
            // modelBuilder.Entity<Domain.Products.Entities.ProductInfo>().HasIndex(x => new { x.SlugName, x.ProductId, x.LanguageId });
            // modelBuilder.Entity<Domain.Products.Entities.ProductInfo>().HasIndex(x => new { x.SlugName, x.ProductId, x.LanguageId, x.Deleted }).IsUnique();
            // modelBuilder.Entity<Domain.Products.Entities.ProductCategory>()
            //     .ToTable("product_category", "products")
            //     .HasKey(pc => new { pc.ProductId, pc.CategoryId });
            // modelBuilder.Entity<Domain.Products.Entities.ProductAllergen>()
            //     .ToTable("product_allergen", "products")
            //     .HasKey(pa => new { pa.ProductId, pa.AllergenId });

            // modelBuilder.ToSnakeCase();
        }
    }
}