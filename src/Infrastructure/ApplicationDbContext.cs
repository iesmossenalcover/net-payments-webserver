using Application.Common.Services;
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
            base.OnModelCreating(modelBuilder);

            // Auths
            modelBuilder.Entity<Domain.Authentication.User>()
                .ToTable("user", "authentication")
                .HasIndex(x => x.Username).IsUnique();
            
            modelBuilder.Entity<Domain.Authentication.User>()
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Authentication.User>()
                .Property(x => x.Username)
                .HasMaxLength(100);

            modelBuilder.Entity<Domain.Authentication.UserClaim>()
                .ToTable("user_claim", "authentication")
                .HasKey(pc => new { pc.Type, pc.UserId });
            modelBuilder.Entity<Domain.Authentication.UserClaim>()
                .ToTable("user_claim", "authentication")
                .HasIndex(x => x.UserId);
            modelBuilder.Entity<Domain.Authentication.UserClaim>()
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Authentication.UserClaim>()
                .HasOne(x => x.User).WithMany(x => x.UserClaims);
        }
    }
}