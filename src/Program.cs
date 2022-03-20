using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.Design;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:SqliteConnection"];
builder.Services.AddDbContext<MjAppDbContext>(options =>options.UseSqlite(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

#region middleware

if (app.Environment.IsDevelopment())
{
    //异常处理中间件
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

#endregion 

#region routers

app.MapPost("/signin", [AllowAnonymous] async (MjAppDbContext db, User user) =>
{
    var u = await db.Users.FirstOrDefaultAsync(k=>k.UserName == user.UserName && k.Password == user.Password);
    if(u == null){
        return Results.BadRequest();
    }
    var token = JwtToken.Build(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"], user);
    return Results.Ok(new { UserName = user.UserName,Token = token });
});

app.MapPost("/signup", [AllowAnonymous] async (MjAppDbContext db, User user) =>
{
    var existUser = await db.Users.FirstOrDefaultAsync(k => k.UserName == user.UserName);
    if(existUser != null)
    {
        return Results.BadRequest();
    }

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(user);
});

app.MapPost("/items",[Authorize] async (MjAppDbContext db, Item item) => {
    if( await db.Items.FirstOrDefaultAsync(x => x.Id == item.Id) != null)
    {
        return Results.BadRequest();
    }

    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created( $"/Items/{item.Id}",item);
});

app.MapGet("/items/{id}", [Authorize] async (MjAppDbContext db, int id) =>
{
    var item = await db.Items.FirstOrDefaultAsync(x => x.Id == id);

    return item == null ? Results.NotFound() : Results.Ok(item);
});

app.MapPut("/items/{id}", async (MjAppDbContext db, int id, Item item) =>
{
    var existItem = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    if(existItem == null)
    {
        return Results.BadRequest();
    }

    existItem.Title = item.Title;
    existItem.IsCompleted = item.IsCompleted;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/items/{id}", async (MjAppDbContext db, int id) => 
{
    var existItem = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    if(existItem == null)
    {
        return Results.BadRequest();
    }

    db.Items.Remove(existItem);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

// start 
app.MapGet("/", () => "Hello from Minimal API");
app.Run();

#region JWT
class JwtToken 
{
    public static string Build(string key, string issuer,string audience,  User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier,
            Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.Now.AddDays(150), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
#endregion

#region Models 
/*
    dotnet add package Microsoft.EntityFrameworkCore 
    dotnet add package Microsoft.EntityFrameworkCore.Design
    dotnet ef migrations add "initial migration"
    dotnet ef database update
*/

class MjAppDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
    public DbSet<User> Users { get; set; }

    public MjAppDbContext(DbContextOptions<MjAppDbContext> options) : base(options) {}
}

class MjAppDbContextFactory : IDesignTimeDbContextFactory<MjAppDbContext>
{
    public MjAppDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json")
                  .Build();

        var builder = new DbContextOptionsBuilder<MjAppDbContext>();
        var connectionString = configuration.GetConnectionString("SqliteConnection");
        builder.UseSqlite(connectionString);
        return new MjAppDbContext(builder.Options);
    }
}

public class User
{    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

class Item
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public bool IsCompleted { get; set; }
}
#endregion