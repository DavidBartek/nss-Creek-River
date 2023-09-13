using Microsoft.EntityFrameworkCore;
using CreekRiver.Models;

public class CreekRiverDbContext : DbContext
{

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Campsite> Campsites { get; set; }
    public DbSet<CampsiteType> CampsiteTypes { get; set; }

    public CreekRiverDbContext(DbContextOptions<CreekRiverDbContext> context) : base(context)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // seed data with campsite types
        modelBuilder.Entity<CampsiteType>().HasData(new CampsiteType[]
        {
            new CampsiteType {Id = 1, CampsiteTypeName = "Tent", FeePerNight = 15.99M, MaxReservationDays = 7},
            new CampsiteType {Id = 2, CampsiteTypeName = "RV", FeePerNight = 26.50M, MaxReservationDays = 14},
            new CampsiteType {Id = 3, CampsiteTypeName = "Primitive", FeePerNight = 10.00M, MaxReservationDays = 3},
            new CampsiteType {Id = 4, CampsiteTypeName = "Hammock", FeePerNight = 12M, MaxReservationDays = 7}
        });

        modelBuilder.Entity<Campsite>().HasData(new Campsite[]
        {
            new Campsite {Id = 1, CampsiteTypeId = 1, Nickname = "Barred Owl", ImageUrl="https://tnstateparks.com/assets/images/content-images/campgrounds/249/colsp-area2-site73.jpg"},
            new Campsite {Id = 2, CampsiteTypeId = 3, Nickname = "Bucklesby", ImageUrl="https://www.reddit.com/media?url=https%3A%2F%2Fi.redd.it%2Fvl0elvghb1j31.jpg"},
            new Campsite {Id = 3, CampsiteTypeId = 2, Nickname = "Trolls", ImageUrl="https://www.reddit.com/media?url=https%3A%2F%2Fi.redd.it%2F64rtmh28xx261.jpg"},
            new Campsite {Id = 4, CampsiteTypeId = 2, Nickname = "Parking Lot", ImageUrl="https://www.reddit.com/media?url=https%3A%2F%2Fi.redd.it%2Ftlgt4sxf2b051.png"},
            new Campsite {Id = 5, CampsiteTypeId = 4, Nickname = "Tropical Paradise", ImageUrl="https://i.redd.it/ucc6k6ou95y41.jpg"},
            new Campsite {Id = 6, CampsiteTypeId = 3, Nickname = "Primitive Indeed", ImageUrl="https://i.redd.it/02ty6pivu8q31.jpg"}
        });

        modelBuilder.Entity<UserProfile>().HasData(new UserProfile []
        {
            new UserProfile {Id = 1, FirstName = "David", LastName = "McDavid", Email = "davidmcdavid@davidmcdavid.com"}
        });

        modelBuilder.Entity<Reservation>().HasData(new Reservation []
        {
            new Reservation {Id = 1, CampsiteId = 4, UserProfileId = 1, CheckinDate = new DateTime(2023, 9, 12), CheckoutDate = new DateTime(2023, 9, 13)}
        });

    }

}

// for thorough explanation: https://github.com/nashville-software-school/server-side-dotnet-curriculum/blob/main/book-3-sql-efcore/chapters/creek-river-db-context.md