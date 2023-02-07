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

        public DbSet<Domain.People.Person> People { get; set; } = default!;
        public DbSet<Domain.People.Teacher> Teachers { get; set; } = default!;
        public DbSet<Domain.People.Student> Students { get; set; } = default!;
        public DbSet<Domain.People.Group> Groups { get; set; } = default!;
        public DbSet<Domain.People.Course> Courses { get; set; } = default!;
        public DbSet<Domain.People.PersonGroupCourse> PersonGroupCourses { get; set; } = default!;

        public DbSet<Domain.Events.Event> Events { get; set; } = default!;
        public DbSet<Domain.Events.EventPerson> EventPersons { get; set; } = default!;

        public DbSet<Domain.Orders.Order> Orders { get; set; } = default!;
        public DbSet<Domain.Orders.Item> Items { get; set; } = default!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // People
            modelBuilder.Entity<Domain.People.Person>()
                .ToTable("person", "people")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.People.Person>()
                .HasIndex(x => x.DocumentId).IsUnique();
            modelBuilder.Entity<Domain.People.Person>()
                .HasIndex(x => x.Name);
            modelBuilder.Entity<Domain.People.Person>()
                .Property(x => x.ContactMail).HasMaxLength(100);
            modelBuilder.Entity<Domain.People.Person>()
                .Property(x => x.DocumentId).HasMaxLength(50);
            modelBuilder.Entity<Domain.People.Person>()
                .Property(x => x.ContactPhone).HasMaxLength(15);


            modelBuilder.Entity<Domain.People.Teacher>()
                .ToTable("teacher", "people")
                .Property(x => x.Id).ValueGeneratedOnAdd(); ;
            modelBuilder.Entity<Domain.People.Teacher>()
                .HasIndex(x => x.PersonId).IsUnique();
            modelBuilder.Entity<Domain.People.Teacher>()
                .HasOne(x => x.Person);


            modelBuilder.Entity<Domain.People.Student>()
                .ToTable("student", "people")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.People.Student>()
                .HasIndex(x => x.PersonId).IsUnique();
            modelBuilder.Entity<Domain.People.Student>()
                .HasOne(x => x.Person);
            modelBuilder.Entity<Domain.People.Student>()
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.People.Student>()
                .HasIndex(x => x.AcademicRecordNumber).IsUnique();


            modelBuilder.Entity<Domain.People.Group>()
                .ToTable("group", "people")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.People.Group>()
            .HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Domain.People.Group>()
                .Property(x => x.Name).HasMaxLength(50);
            modelBuilder.Entity<Domain.People.Group>()
                .Property(x => x.Name).HasMaxLength(50);

            modelBuilder.Entity<Domain.People.Course>()
                .ToTable("course", "people")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.People.Course>()
                .HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Domain.People.Course>()
                .HasIndex(x => x.Active);
            modelBuilder.Entity<Domain.People.Course>()
                .HasIndex(x => x.StartDate).IsDescending();
            modelBuilder.Entity<Domain.People.Course>()
                .Property(x => x.Name).HasMaxLength(50);


            modelBuilder.Entity<Domain.People.PersonGroupCourse>()
                .ToTable("person_group_course", "people")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.People.PersonGroupCourse>()
                .HasIndex(x => new { x.PersonId, x.GroupId, x.CourseId }).IsUnique();
            modelBuilder.Entity<Domain.People.PersonGroupCourse>()
                .HasOne(x => x.Person);
            modelBuilder.Entity<Domain.People.PersonGroupCourse>()
                .HasOne(x => x.Group);
            modelBuilder.Entity<Domain.People.PersonGroupCourse>()
                .HasOne(x => x.Course);


            //Events
            modelBuilder.Entity<Domain.Events.Event>()
                .ToTable("event", "event")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Events.Event>()
                .HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<Domain.Events.Event>()
                .Property(x => x.Code).HasMaxLength(20);
            modelBuilder.Entity<Domain.Events.Event>()
                .HasIndex(x => x.CreationDate).IsDescending();
            modelBuilder.Entity<Domain.Events.Event>()
                .HasIndex(x => x.PublishDate).IsDescending();
            modelBuilder.Entity<Domain.Events.Event>()
                .HasIndex(x => x.UnpublishDate).IsDescending();

            
            modelBuilder.Entity<Domain.Events.EventPerson>()
                .ToTable("event_person", "event")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Events.EventPerson>()
                .HasIndex(x => new { x.PersonId, x.EventId, x.ItemId }).IsUnique();
            modelBuilder.Entity<Domain.Events.EventPerson>()
                .HasOne(x => x.Person);
            modelBuilder.Entity<Domain.Events.EventPerson>()
                .HasOne(x => x.Event);
            modelBuilder.Entity<Domain.Events.EventPerson>()
                .HasOne(x => x.Item);



            //Orders
            modelBuilder.Entity<Domain.Orders.Item>()
                .ToTable("item", "order")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Orders.Item>()
                .HasIndex(x => x.OrderId).IsUnique();
            modelBuilder.Entity<Domain.Orders.Item>()
                .HasOne(x => x.Order);

            
            modelBuilder.Entity<Domain.Orders.Order>()
                .ToTable("order", "order")
                .Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Domain.Orders.Order>()
                .HasIndex(x => x.Created).IsDescending();



        }
    }
}