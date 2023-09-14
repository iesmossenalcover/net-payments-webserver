using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AppDbContext : DbContext
    {

        #region constructor
        public AppDbContext(DbContextOptions options) : base(options)
        { }
        #endregion

        #region sets
        public DbSet<Domain.Entities.Authentication.User> Users { get; set; } = default!;
        public DbSet<Domain.Entities.Authentication.UserClaim> UserClaims { get; set; } = default!;
        public DbSet<Domain.Entities.Authentication.OAuthUser> OAuthUsers { get; set; } = default!;
        // public DbSet<Domain.Entities.Authentication.GoogleGroupClaimRelation> GoogleGroupClaimRelations { get; set; } = default!;


        public DbSet<Domain.Entities.Configuration.AppConfig> AppConfigs { get; set; } = default!;


        public DbSet<Domain.Entities.People.Person> People { get; set; } = default!;
        public DbSet<Domain.Entities.People.Group> Groups { get; set; } = default!;
        public DbSet<Domain.Entities.People.Course> Courses { get; set; } = default!;
        public DbSet<Domain.Entities.People.PersonGroupCourse> PersonGroupCourses { get; set; } = default!;

        public DbSet<Domain.Entities.Events.Event> Events { get; set; } = default!;
        public DbSet<Domain.Entities.Events.EventPerson> EventPersons { get; set; } = default!;

        public DbSet<Domain.Entities.Orders.Order> Orders { get; set; } = default!;

        public DbSet<Domain.Entities.GoogleApi.UoGroupRelation> UoGroupRelations { get; set; } = default!;

        public DbSet<Domain.Entities.Tasks.Task> Tasks { get; set; } = default!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasCollation("no_accent", locale: "und-u-ks-level1-kc-true", provider: "icu", deterministic: false);

            // Auths
            modelBuilder.Entity<Domain.Entities.Authentication.User>()
                .ToTable("user", "main")
                .HasIndex(x => x.Username).IsUnique();
            modelBuilder.Entity<Domain.Entities.Authentication.User>()
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.Authentication.User>()
                .Property(x => x.Username)
                .HasMaxLength(100);

            modelBuilder.Entity<Domain.Entities.Authentication.UserClaim>()
                .ToTable("user_claim", "main")
                .HasKey(pc => new { pc.Type, pc.UserId });
            modelBuilder.Entity<Domain.Entities.Authentication.UserClaim>()
                .ToTable("user_claim", "main")
                .HasIndex(x => x.UserId);
            modelBuilder.Entity<Domain.Entities.Authentication.UserClaim>()
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.Authentication.UserClaim>()
                .HasOne(x => x.User).WithMany(x => x.UserClaims);


            modelBuilder.Entity<Domain.Entities.Authentication.OAuthUser>()
                .ToTable("oauth_user", "main")
                .HasIndex(x => new { x.Subject, x.OAuthProviderCode }).IsUnique();

            // modelBuilder.Entity<Domain.Entities.Authentication.GoogleGroupClaimRelation>()
            //     .ToTable("google_group_claim_relation", "main")
            //     .HasIndex(x => x.GroupEmail).IsUnique();

            // People
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .ToTable("person", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .HasIndex(x => x.DocumentId).IsUnique();
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .HasIndex(x => x.AcademicRecordNumber).IsUnique();
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .HasIndex(x => x.Name);
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .HasIndex(x => x.Surname1);
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .HasIndex(x => x.Surname2);
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .Property(x => x.ContactMail).HasMaxLength(100);
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .HasIndex(x => x.ContactMail).IsUnique();
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .Property(x => x.DocumentId).HasMaxLength(50);
            modelBuilder.Entity<Domain.Entities.People.Person>()
                .Property(x => x.ContactPhone).HasMaxLength(15);

            modelBuilder.Entity<Domain.Entities.People.Group>()
                .ToTable("group", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.People.Group>()
                .HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Domain.Entities.People.Group>()
                .Property(x => x.Name).HasMaxLength(50);

            modelBuilder.Entity<Domain.Entities.People.Course>()
                .ToTable("course", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.People.Course>()
                .HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Domain.Entities.People.Course>()
                .HasIndex(x => x.Active);
            modelBuilder.Entity<Domain.Entities.People.Course>()
                .HasIndex(x => x.StartDate).IsDescending();
            modelBuilder.Entity<Domain.Entities.People.Course>()
                .Property(x => x.Name).HasMaxLength(50);


            modelBuilder.Entity<Domain.Entities.People.PersonGroupCourse>()
                .ToTable("person_group_course", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.People.PersonGroupCourse>()
                .HasIndex(x => new { x.PersonId, x.GroupId, x.CourseId }).IsUnique();
            modelBuilder.Entity<Domain.Entities.People.PersonGroupCourse>()
                .HasOne(x => x.Person);
            modelBuilder.Entity<Domain.Entities.People.PersonGroupCourse>()
                .HasOne(x => x.Group);
            modelBuilder.Entity<Domain.Entities.People.PersonGroupCourse>()
                .HasOne(x => x.Course);


            //Events
            modelBuilder.Entity<Domain.Entities.Events.Event>()
                .ToTable("event", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.Events.Event>()
                .HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<Domain.Entities.Events.Event>()
                .Property(x => x.MaxQuantity).HasDefaultValue(1);
            modelBuilder.Entity<Domain.Entities.Events.Event>()
                .Property(x => x.Code).HasMaxLength(20);
            modelBuilder.Entity<Domain.Entities.Events.Event>()
                .HasIndex(x => x.CreationDate).IsDescending();
            modelBuilder.Entity<Domain.Entities.Events.Event>()
                .HasIndex(x => x.PublishDate).IsDescending();
            modelBuilder.Entity<Domain.Entities.Events.Event>()
                .HasIndex(x => x.UnpublishDate).IsDescending();


            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .ToTable("event_person", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .Property(x => x.Quantity).HasDefaultValue(1);
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .HasIndex(x => new { x.PersonId, x.EventId, x.OrderId }).IsUnique();
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .HasOne(x => x.Person);
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .HasOne(x => x.Event);
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .HasOne(x => x.Order);



            // Orders
            modelBuilder.Entity<Domain.Entities.Orders.Order>()
                .ToTable("order", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.Orders.Order>()
                .HasIndex(x => x.Created).IsDescending();
            modelBuilder.Entity<Domain.Entities.Orders.Order>()
                .HasIndex(x => new { x.PaidDate, x.Status }).IsDescending();


            // App config
            modelBuilder.Entity<Domain.Entities.Configuration.AppConfig>()
                .ToTable("app_config", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();

            // GoogleApi
            modelBuilder.Entity<Domain.Entities.GoogleApi.UoGroupRelation>()
                .ToTable("uo_group_relation", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.GoogleApi.UoGroupRelation>()
                .HasIndex(x => x.GroupId).IsDescending();

            // Tasks
            modelBuilder.Entity<Domain.Entities.Tasks.Task>()
                .ToTable("task", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.Tasks.Task>()
                .HasIndex(x => new { x.Type, x.Status }).IsDescending();

            // Log
            modelBuilder.Entity<Domain.Entities.Tasks.LogStoreInfo>()
                .ToTable("log", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Person>())
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                {
                    entry.Entity.Name = entry.Entity.Name.Trim().ToUpperInvariant();
                    entry.Entity.Surname1 = entry.Entity.Surname1.Trim().ToUpperInvariant();
                    entry.Entity.DocumentId = entry.Entity.DocumentId.Trim().ToUpperInvariant();
                    entry.Entity.Surname2 = !string.IsNullOrEmpty(entry.Entity.Surname2) ? entry.Entity.Surname2.Trim().ToUpperInvariant() : null;
                    entry.Entity.ContactMail = !string.IsNullOrEmpty(entry.Entity.ContactMail) ? entry.Entity.ContactMail.Trim() : null;
                }
            }

            foreach (var entry in ChangeTracker.Entries<PersonGroupCourse>())
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                {
                    entry.Entity.SubjectsInfo = !string.IsNullOrEmpty(entry.Entity.SubjectsInfo) ? entry.Entity.SubjectsInfo.Trim() : null;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}