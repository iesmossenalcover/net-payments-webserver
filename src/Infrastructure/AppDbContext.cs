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

        public DbSet<Domain.Entities.People.Person> People { get; set; } = default!;
        public DbSet<Domain.Entities.People.Group> Groups { get; set; } = default!;
        public DbSet<Domain.Entities.People.Course> Courses { get; set; } = default!;
        public DbSet<Domain.Entities.People.PersonGroupCourse> PersonGroupCourses { get; set; } = default!;

        public DbSet<Domain.Entities.Events.Event> Events { get; set; } = default!;
        public DbSet<Domain.Entities.Events.EventPerson> EventPersons { get; set; } = default!;

        public DbSet<Domain.Entities.Orders.Order> Orders { get; set; } = default!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                .Property(x => x.ContactMail).HasMaxLength(100);
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
                .HasIndex(x => new { x.PersonId, x.EventId, x.OrderId }).IsUnique();
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .HasOne(x => x.Person);
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .HasOne(x => x.Event);
            modelBuilder.Entity<Domain.Entities.Events.EventPerson>()
                .HasOne(x => x.Order);



            //Orders


            modelBuilder.Entity<Domain.Entities.Orders.Order>()
                .ToTable("order", "main")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Entities.Orders.Order>()
                .HasIndex(x => x.Created).IsDescending();



        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<Person>())
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                {
                    entry.Entity.DocumentId = entry.Entity.DocumentId.ToUpperInvariant();
                }
            }
            
            return base.SaveChanges();
        }
    }
}