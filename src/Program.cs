using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

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

app.UseStaticFiles(new StaticFileOptions()                                       
{
    FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, @"images")),
    RequestPath = new PathString("/images")
});

#endregion 

#region routers

app.MapPost("/signin", [AllowAnonymous] async (MjAppDbContext db, User user) =>
{
    var et = await db.Users.FirstOrDefaultAsync(k=>k.UserName == user.UserName && k.Password == user.Password);
    if(et == null){
        return Results.BadRequest();
    }
    var token = JwtToken.Build(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"], user);
    return Results.Ok(new { UserName = user.UserName,Token = token });
});

app.MapPost("/signup", [AllowAnonymous] async (MjAppDbContext db, User user) =>
{
    var et = await db.Users.FirstOrDefaultAsync(k => k.UserName == user.UserName);
    if(et != null)
    {
        return Results.BadRequest();
    }

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(user);
});

app.MapPost("/upload", async Task<IResult> (HttpRequest request) =>
 {
     if (!request.HasFormContentType)
         return Results.BadRequest();

     var form = await request.ReadFormAsync();
     var fi = form.Files["fi"];

     if (fi is null || fi.Length == 0)
         return Results.BadRequest();

     var svrFn = $"{DateTime.Now.Ticks}{Path.GetExtension(fi.FileName)}";
     var svrpath = Path.Combine(AppContext.BaseDirectory, "images", svrFn);
     await using var stream = fi.OpenReadStream();
     using var fs = File.Create(svrpath);
     await stream.CopyToAsync(fs);
     return Results.Created($"/images/{svrFn}",string.Empty);
 });

app.MapPost("/item", async (MjAppDbContext db, Item item) => {
    if(await db.Items.FirstOrDefaultAsync(x => x.Id == item.Id) != null)
    {
        return Results.BadRequest();
    }

    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created( $"/item/{item.Id}",item);
});

app.MapGet("/item", async (MjAppDbContext db, PagingData pageData) => {
    var ets = from et in db.Items
              select et;
    if (!string.IsNullOrEmpty(pageData.SearchString))
    {
        ets = ets.Where(s => s.ItemName.Contains(pageData.SearchString)
                               || s.ItemCode.Contains(pageData.SearchString));
    }
    ets = ets.OrderByDescending(s => s.Id);
    var p = await Page<Item>.CreateAsync(ets, pageData.CurrentPage);
    return Results.Ok(p);
});

app.MapGet("/item/{id}",async (MjAppDbContext db, int id) =>
{
    var et = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    return et == null ? Results.NotFound() : Results.Ok(et);
});

app.MapPut("/item/{id}", async (MjAppDbContext db, int id, Item item) =>
{
    var et = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    if(et == null)
    {
        return Results.BadRequest();
    }
    if(await db.Items.FirstOrDefaultAsync(x => x.Id != id && x.ItemCode == item.ItemCode) != null)
    {
        return Results.BadRequest("same [itemcode]");
    }
    et.ItemCate = item.ItemCate;
    et.ItemCode = item.ItemCode;
    et.ItemModel = item.ItemModel;
    et.ItemName = item.ItemName;
    et.Volume = item.Volume;
    et.PackageQty = item.PackageQty;
    et.Price = item.Price;
    et.PicPath = item.PicPath;
    await db.SaveChangesAsync();
    return Results.Ok(et);
});

app.MapDelete("/item/{id}", async (MjAppDbContext db, int id) => 
{
    var et = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    if(et == null)
    {
        return Results.BadRequest();
    }

    db.Items.Remove(et);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapPost("/supplier", async (MjAppDbContext db, Supplier supplier) => {
    if (await db.Suppliers.FirstOrDefaultAsync(x => x.Id == supplier.Id) != null)
    {
        return Results.BadRequest();
    }

    db.Suppliers.Add(supplier);
    await db.SaveChangesAsync();
    return Results.Created($"/supplier/{supplier.Id}", supplier);
});

app.MapGet("/supplier/{id}", async (MjAppDbContext db, int id) =>
{
    var et = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
    return et == null ? Results.NotFound() : Results.Ok(et);
});

app.MapPut("/supplier/{id}", async (MjAppDbContext db, int id, Supplier supplier) =>
{
    var et = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
    if (et == null)
    {
        return Results.BadRequest();
    }
    et.SupplierName = supplier.SupplierName;
    et.SupplierMobile = supplier.SupplierMobile;
    et.SupplierCode = supplier.SupplierCode;   
    et.SupplierAddress = supplier.SupplierAddress;
    await db.SaveChangesAsync();
    return Results.Ok(et);
});

app.MapDelete("/supplier/{id}", async (MjAppDbContext db, int id) =>
{
    var et = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
    if (et == null)
    {
        return Results.BadRequest();
    }

    db.Suppliers.Remove(et);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPost("/supplieritem", async (MjAppDbContext db, SupplierItem supplierItem) => {
    if (await db.SupplierItems.FirstOrDefaultAsync(x => x.Id == supplierItem.Id) != null)
    {
        return Results.BadRequest();
    }

    db.SupplierItems.Add(supplierItem);
    await db.SaveChangesAsync();
    return Results.Created($"/supplier/{supplierItem.Id}", supplierItem);
});


app.MapGet("/supplieritem", async (MjAppDbContext db, PagingData pageData) => {
    var ets = db.SupplierItems.Join(
                db.Items,
                sit => sit.ItemCode,
                it => it.ItemCode,
                (sit, it) => new SupplierItemDto
                {
                    Id = sit.Id,
                    SupplierName = sit.SupplierName,
                    ItemCate = it.ItemCate,
                    CreateTime = sit.CreateTime,
                    Creator = sit.Creator,
                    ItemCode = sit.ItemCode,
                    ItemModel = sit.ItemModel,
                    ItemName = it.ItemName,
                    ItemStyle = it.ItemCate,
                    PackageQty = sit.PackageQty,
                    PicPath = it.PicPath,
                    Price = it.Price,
                    Volume = it.Volume,
                    CostPrice= sit.CostPrice
                }
            );
    if (!string.IsNullOrEmpty(pageData.SearchString))
    {
        ets = ets.Where(s => s.ItemModel.Contains(pageData.SearchString)
                             || s.ItemCode.Contains(pageData.SearchString)
                             || s.ItemName.Contains(pageData.SearchString));
    }
    ets = ets.OrderByDescending(s => s.Id);
    var p = await Page<SupplierItemDto>.CreateAsync(ets, pageData.CurrentPage);
    return Results.Ok(p);
});


app.MapGet("/supplieritem/{id}", async (MjAppDbContext db, int id) =>
{
    var et = await db.SupplierItems.FirstOrDefaultAsync(x => x.Id == id);
    return et == null ? Results.NotFound() : Results.Ok(et);
});

app.MapPut("/supplieritem/{id}", async (MjAppDbContext db, int id, SupplierItem supplierItem) =>
{
    var et = await db.SupplierItems.FirstOrDefaultAsync(x => x.Id == id);
    if (et == null)
    {
        return Results.BadRequest();
    }
    et.ItemModel = supplierItem.ItemModel;
    et.PackageQty = supplierItem.PackageQty;
    et.CostPrice = supplierItem.CostPrice;
    await db.SaveChangesAsync();
    return Results.Ok(et);
});

app.MapDelete("/supplieritem/{id}", async (MjAppDbContext db, int id) =>
{
    var et = await db.SupplierItems.FirstOrDefaultAsync(x => x.Id == id);
    if (et == null)
    {
        return Results.BadRequest();
    }

    db.SupplierItems.Remove(et);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

// start 
app.MapGet("/", () => "Dotnet Minimal API");
app.Run();


class PagingData
{
    // GET /products?SortBy=xyz&SortDir=Desc&Page=99&wd=foo
    public string? SortBy { get; init; }
    public SortDirection SortDirection { get; init; }
    public int CurrentPage { get; init; } = 1;

    public string? SearchString { get; init; }

    public static ValueTask<PagingData?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        const string sortByKey = "sortBy";
        const string sortDirectionKey = "sortDir";
        const string currentPageKey = "page";
        const string searchStringKey = "wd";

        Enum.TryParse<SortDirection>(context.Request.Query[sortDirectionKey],
                                     ignoreCase: true, out var sortDirection);
        int.TryParse(context.Request.Query[currentPageKey], out var page);
        page = page == 0 ? 1 : page;


        var result = new PagingData
        {
            SortBy = context.Request.Query[sortByKey],
            SortDirection = sortDirection,
            CurrentPage = page,
            SearchString = context.Request.Query[searchStringKey]
        };

        return ValueTask.FromResult<PagingData?>(result);
    }
}

enum SortDirection
{
    Default,
    Asc,
    Desc
}


#region Page

class Page<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }
    public List<T> Data { get; set; }

    public Page(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Data = items;
    }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<Page<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize = 50)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new Page<T>(items, count, pageIndex, pageSize);
    }
}

#endregion

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
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierItem> SupplierItems { get; set; }
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



class User
{    
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? Creator { get; set; }
}

class Item
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public decimal Volume { get; set; } = 0;
    public decimal PackageQty { get; set; } = 0;
    public decimal Price { get; set; } = 0;
    public decimal CostPrice { get; set; } = 0;
    public string? ItemCate { get; set; }
    public string? ItemStyle { get; set; }
    public string? ItemModel { get; set; }
    public string? PicPath { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? Creator { get; set; }
}

class Supplier 
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string? SupplierMobile { get; set; }
    public string? SupplierAddress { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? Creator { get; set; }
}

class SupplierItem
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public decimal PackageQty { get; set; } = 0;
    public decimal CostPrice { get; set; } = 0;
    public string ItemModel { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? Creator { get; set; }
}

class SupplierItemDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public decimal Volume { get; set; } = 0;
    public decimal PackageQty { get; set; } = 0;
    public decimal Price { get; set; } = 0;
    public decimal CostPrice { get; set; } = 0;
    public string? ItemCate { get; set; }
    public string? ItemStyle { get; set; }
    public string ItemModel { get; set; } = string.Empty;
    public string? PicPath { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public string? Creator { get; set; }
}


#endregion
