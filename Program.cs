using CreekRiver.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<CreekRiverDbContext>(builder.Configuration["CreekRiverDbConnectionString"]);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// get campsites collection
app.MapGet("/api/campsites", (CreekRiverDbContext db) =>
{
    return db.Campsites.ToList();
});

// get campsite by specified ID - includes related data (.Include)
app.MapGet("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    try
    {
        Campsite campsite = db.Campsites.Include(c => c.CampsiteType).Single(c => c.Id == id);
        return Results.Ok(campsite);
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
    // alternate method to try/catch: use .SingleOrDefault instead of .Single, handle error in if/else block
});

// create campsite
app.MapPost("/api/campsites", (CreekRiverDbContext db, Campsite campsite) =>
{
    db.Campsites.Add(campsite);
    db.SaveChanges();
    return Results.Created($"/api/campsites/{campsite.Id}", campsite);
});

// delete campsite by id
app.MapDelete("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    Campsite campsite = db.Campsites.SingleOrDefault(campsite => campsite.Id == id);
    if (campsite == null)
    {
        return Results.NotFound();
    }
    db.Campsites.Remove(campsite);
    db.SaveChanges();
    return Results.NoContent();

    // note: by default, EF Core will configure related reservation rows (containing related data to campsites) to be deleted as well.
    // ^ "cascade delete"

});

// update campsite by id
app.MapPut("/api/campsites/{id}", (CreekRiverDbContext db, int id, Campsite campsite) =>
{
    Campsite campsiteToUpdate = db.Campsites.SingleOrDefault(campsite => campsite.Id == id);
    if (campsiteToUpdate == null)
    {
        return Results.NotFound();
    }
    campsiteToUpdate.Nickname = campsite.Nickname;
    campsiteToUpdate.CampsiteTypeId = campsite.CampsiteTypeId;
    campsiteToUpdate.ImageUrl = campsite.ImageUrl;

    db.SaveChanges();
    return Results.NoContent();
});

// get reservations - includes related data (.Include, .ThenInclude, .OrderBy)
app.MapGet("/api/reservations", (CreekRiverDbContext db) =>
{
    return db.Reservations
        .Include(r => r.UserProfile) // JOIN the UserProfiles table
        .Include(r => r.Campsite) // JOIN the Campsites table
        .ThenInclude(c => c.CampsiteType) // CampsiteType only accessible through Campsite; .ThenInclude JOINs through Campsite
        .OrderBy(res => res.CheckinDate) // .OrderBy: regular Linq method, but corresponds to SQL's ORDER BY
        .ToList();
});

// ^^ equivalent SQL query:
// SELECT r.Id,
//        r.CampsiteId,
//        r.UserProfileId,
//        r.CheckinDate,
//        r.CheckoutDate,
//        up.Id,
//        up.FirstName,
//        up.LastName,
//        up.Email,
//        c.Id, 
//        c.Nickname
//        c.ImageUrl,
//        c.CampsiteTypeId, 
//        ct.Id,
//        ct.CampsiteTypeName,
//        ct.MaxReservationDays,
//        ct.FeePerNight
// FROM Reservations r
// LEFT JOIN UserProfiles up ON up.Id = r.UserProfileId
// LEFT JOIN Campsites c ON c.Id = r.CampsiteId
// LEFT JOIN CampsiteTypes ct ON ct.Id = c.CampsiteTypeId
// ORDER BY r.CheckinDate;

// post new reservation
// error handling example here
app.MapPost("/api/reservations", (CreekRiverDbContext db, Reservation newRes) =>
{
    // Check if reservation checkout is before or the same day as checkin
    if (newRes.CheckoutDate <= newRes.CheckinDate)
    {
        return Results.BadRequest("Reservation checkout must be at least one day after checkin");
    }

    try
    {
        db.Reservations.Add(newRes);
        db.SaveChanges();
        return Results.Created($"/api/reservations/{newRes.Id}", newRes);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Invalid data submitted");
    }
    
});

// delete reservation by id
app.MapDelete("/api/reservations/{id}", (CreekRiverDbContext db, int id) =>
{
    Reservation reservation = db.Reservations.SingleOrDefault(r => r.Id == id);
    if (reservation == null)
    {
        return Results.NotFound();
    }
    db.Reservations.Remove(reservation);
    db.SaveChanges();
    return Results.NoContent();

    // note: by default, EF Core will configure related reservation rows (containing related data to campsites) to be deleted as well.
    // ^ "cascade delete"

});

app.Run();