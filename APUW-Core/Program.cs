using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using User                  = GalleryIdentityUser;
using Role                  = Microsoft.AspNetCore.Identity.IdentityRole;
using DbContext             = SimpleDbContext;
using UserManager           = Microsoft.AspNetCore.Identity.UserManager<GalleryIdentityUser>;
using RoleManager           = Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>;
using SignInManager         = Microsoft.AspNetCore.Identity.SignInManager<GalleryIdentityUser>;

#region Builder & Services

//var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder();

builder.Services
    .AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.RequestProperties;
    })
    .AddDbContext<DbContext>(options =>
    {
        options.UseInMemoryDatabase("PreBaza");
    })
    //.AddAuthorization(options =>
    //{
    //    var singleRolePolicy = (string role) => options.AddPolicy(role, policy => { policy.RequireRole(role); });

    //    singleRolePolicy("Client");
    //})
    .AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<DbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/user/login";
        options.LogoutPath = "/api/user/logout";
    });

builder.Logging
    .AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Information);

#endregion

#region App Configuration

var application = builder.Build();

application
    .UseHttpLogging()
    //.UseWebAssemblyDebugging()
    .UseExceptionHandler("/api/error")
    .UseHsts()
    //.UseHttpsRedirection()
    //.UseBlazorFrameworkFiles()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthentication();
//.UseAuthorization();

#endregion

/*
 * Anonymous and User
 *  +GET:    /                           -> returns "[{username}] Home"
 *  +GET:    /user/register              -> mock register HTML page
 *  +GET:    /user/login                 -> mock login HTML page
 *  +POST:   /user/register              -> register request
 *  +POST:   /user/login                 -> login request
 *  +POST:   /logout                     -> logout request
 *  +GET:    /users                      -> list users request
 *  +GET:    /users/{username}           -> get profile request
 *  +GET:    /user                       -> redirects to GET /users/{username} if logged in, otherwise to login page
 *  +GET:    /photos                     -> list photos request
 *  +GET:    /photos/{photoID}           -> list photo if exists
 *  +GET:    /users/{username}/photos    -> list photos of user if user exists, otherwise Status 400
 *  +GET:    /photos/{photoID}/user      -> redirects to /users/{username} if photo exists, otherwise Status 400
 *  
 * User-Only
 *  +POST:   /user/update                -> updates user if logged in
 *  +POST:   /user/delete                -> deletes user if logged in
 *  +POST:   /photos/{photoName}/create  -> creates new photo
 *  +POST:   /photos/{photoID}/update    -> updates existing photo
 *  +POST:   /photos/{photoID}/delete    -> deletes photo
 */

#region Anonymous Requests

/// <summary>
/// Handles the root endpoint and displays user-specific information if authenticated.
/// </summary>
application.MapGet("/", (HttpContext context, UserManager userManager) =>
{
    context.Response.Redirect("/index.html");
});

/// <summary>
/// Displays a greeting for the login page.
/// </summary>
application.MapGet("/api/user/login", () => "Hello login!");

/// <summary>
/// Handles user registration.
/// </summary>
application.MapGet("/api/user/register", () => "Hello register!");

/// <summary>
/// Handles user registration.
/// </summary>
application.MapPost("/api/user/register", async (HttpContext context, RoleManager roleManager, UserManager userManager) =>
{
    var username = context.Request.Form["username"];
    var password = context.Request.Form["password"];
    
    if(StringValues.IsNullOrEmpty(username) || StringValues.IsNullOrEmpty(password))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Registration failed!");
        return;
    }

    var user = new User { UserName = username };
    var result = await userManager.CreateAsync(user, password);

    if(result.Succeeded)
    {
        await context.Response.WriteAsync("User Registered Successfully!");
    }
    else
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("User registration failed:\n\t - " + string.Join("\n\t - ", result.Errors.Select(e => e.Description)));
    };
});

/// <summary>
/// Handles user login.
/// </summary>
application.MapPost("/api/user/login", async (HttpContext context, UserManager UserManager, SignInManager signInManager) =>
{
    var username = context.Request.Form["username"];
    var password = context.Request.Form["password"];

    if(StringValues.IsNullOrEmpty(username) || StringValues.IsNullOrEmpty(password))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Log in failed!");
        return;
    }

    var user = await UserManager.FindByNameAsync(username);

    if(user == null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Log in failed!");
        return;
    }

    var result = await signInManager.PasswordSignInAsync(user, password, false, false);

    if(result.Succeeded)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Redirect("/api/user");
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Log in failed!");
    }
});

/// <summary>
/// Handles user logout.
/// </summary>
application.MapPost("/api/user/logout", async (HttpContext context, UserManager userManager, SignInManager signInManager) =>
{
    if(context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return "Already logged out!";
    }
    var user = await userManager.GetUserAsync(context.User);
    if(user == null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return "Already logged out!";
    }

    await signInManager.SignOutAsync();
    return "Logged out successfully!";
});

/// <summary>
/// Gets currently logged in user profile.
/// </summary>
application.MapGet("/api/user", async (HttpContext context, UserManager userManager) =>
{
    if(context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/api/user/login");
        return;
    }
    var user = await userManager.GetUserAsync(context.User);
    if(user == null)
    {
        context.Response.Redirect("/api/user/login");
        return;
    }

    context.Response.Redirect($"/api/users/{user.UserName}");
});

/// <summary>
/// Retrieves a list of all users.
/// </summary>
application.MapGet("/api/users", async (HttpContext context, UserManager userManager) =>
{
    var users = await userManager.Users.ToListAsync();
    var userDTOs = users.ConvertAll(u => new UserDTO { Id = u.Id, UserName = u.UserName });
    await context.Response.WriteAsJsonAsync(userDTOs);
});

/// <summary>
/// Retrieves user information by username.
/// </summary>
application.MapGet("/api/users/{username}", async (string username, HttpContext context, UserManager userManager) =>
{
    var users = await userManager.Users.Where(u => u.UserName == username).ToListAsync();

    if(users.Count == 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"User {username} does not exist!");
        return;
    }

    await context.Response.WriteAsJsonAsync(users.ConvertAll(u => new UserDTO{ Id = u.Id, UserName = u.UserName })[0]);
});

/// <summary>
/// Retrieves user photos by username.
/// </summary>
application.MapGet("/api/users/{username}/photos", async (string username, HttpContext context, UserManager userManager, DbContext dbContext) =>
{
    var users = await userManager.Users.Where(u => u.UserName == username).ToListAsync();

    if(users.Count == 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"User {username} does not exist!");
        return;
    }

    var photos = dbContext.Pictures.Where(p => p.UserId == users[0].Id).ToList().ConvertAll(p => new PhotoDTO
    {
        Id = p.Id,
        UserId = p.UserId,
        Content = p.Content,
        ContentType = p.ContentType,
        FileName = p.FileName
    });

    await context.Response.WriteAsJsonAsync(photos);
});

/// <summary>
/// Retrieves a list of all photos.
/// </summary>
application.MapGet("/api/photos", async (HttpContext context, DbContext dbContext) =>
{
    var photos = dbContext.Pictures.ToList().ConvertAll(p => new PhotoDTO
    {
        Id = p.Id,
        UserId = p.UserId,
        Content = p.Content,
        ContentType = p.ContentType,
        FileName = p.FileName
    });
    await context.Response.WriteAsJsonAsync(photos);
});

/// <summary>
/// Retrieves a specific photo by ID.
/// </summary>
application.MapGet("/api/photos/{photoID}", async (int photoID, HttpContext context, DbContext dbContext) =>
{
    var photo = await dbContext.Pictures.Where(p => p.Id == photoID).ToListAsync();

    if(photo.Count == 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Photo with id: {photoID} does not exist!");
        return;
    }

    using(var ms = new MemoryStream(photo[0].Content))
    {
        context.Response.ContentType = photo[0].ContentType;
        await ms.CopyToAsync(context.Response.Body);
    }
});

/// <summary>
/// Retrieves the user associated with a specific photo.
/// </summary>
application.MapGet("/api/photos/{photoID}/user", async (int photoID, HttpContext context, DbContext dbContext) =>
{
    var photoUserId = await dbContext.Pictures.Where(p => p.Id == photoID).Select(p => p.UserId).ToListAsync();

    if(photoUserId.Count == 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Photo with id: {photoID} does not exist!");
        return;
    }

    var photoUser = await dbContext.Users.Where(u => u.Id == photoUserId[0]).ToListAsync();
    await context.Response.WriteAsJsonAsync(photoUser.ConvertAll(u => new UserDTO{ Id = u.Id, UserName = u.UserName }).First());
});

#endregion

#region User Requests

application.MapPost("/api/user/update", async (HttpContext context, UserManager userManager, DbContext dbContext) =>
{
    if(context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"User update failed!");
        return;
    }
    var user = await userManager.GetUserAsync(context.User);
    if(user == null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"User update failed!");
        return;
    }

    var username = context.Request.Form["username"];
    
    if(StringValues.IsNullOrEmpty(username))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("User update failed!");
        return;
    }
    else if(username == user.UserName || dbContext.Users.Where(u => u.UserName == username).Count() > 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("User update failed!");
        return;
    }

    user.UserName = username;
    dbContext.Update(user);
    await dbContext.SaveChangesAsync();
    await context.Response.WriteAsync("Username successfully updated!");
});

application.MapPost("/api/user/delete", async (HttpContext context, UserManager userManager, DbContext dbContext) =>
{
    if(context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"User delete failed!");
        return;
    }
    var user = await userManager.GetUserAsync(context.User);
    if(user == null)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"User delete failed!");
        return;
    }

    await dbContext.Entry(user).Collection(u => u.Pictures).LoadAsync();

    var result = await userManager.DeleteAsync(user);

    if(result.Succeeded)
    {
        await dbContext.SaveChangesAsync();
        await context.Response.WriteAsync("User deleted Successfully!");
    }
    else
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("User deletion failed:\n\t - " + string.Join("\n\t - ", result.Errors.Select(e => e.Description)));
    };
});

application.MapPost("/api/photos/{photoName}/create", async (string photoName, HttpContext context, UserManager userManager, DbContext dbContext) =>
{
    if(context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"Photo creation failed!");
        return;
    }
    var user = await userManager.GetUserAsync(context.User);
    if(user == null)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"Photo creation failed!");
        return;
    }
    if(context.Request.ContentLength == null || context.Request.ContentType == null || context.Request.ContentLength.Value == 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Photo creation failed!");
        return;
    }

    using (var ms = new MemoryStream())
    {
        await context.Request.Body.CopyToAsync(ms);
        var content = ms.ToArray();

        Photo pic = new Photo(user, photoName, content, context.Request.ContentType);
        dbContext.Add(pic);
        user.Pictures.Add(pic);
        await dbContext.SaveChangesAsync();
        await context.Response.WriteAsync($"Photo {photoName} successfully created!");
    }
});

application.MapPost("/api/photos/{photoID}/update", async (int photoID, HttpContext context, UserManager userManager, DbContext dbContext) =>
{
    if(context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"Photo update forbidden!");
        return;
    }
    var user = await userManager.GetUserAsync(context.User);
    if(user == null)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"Photo update forbidden!");
        return;
    }
    if(context.Request.ContentLength == null || context.Request.ContentType == null || context.Request.ContentLength.Value == 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Photo update failed!");
        return;
    }

    var pic = await dbContext.Pictures.Where(p => p.Id == photoID).FirstOrDefaultAsync();
    if(pic == null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Photo does not exist!");
        return;
    }
    if(pic.UserId != user.Id)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"Photo update forbidden!");
        return;
    }

    using (var ms = new MemoryStream())
    {
        await context.Request.Body.CopyToAsync(ms);
        pic.Content = ms.ToArray();
        pic.ContentType = context.Request.ContentType;

        dbContext.Update(pic);
        await dbContext.SaveChangesAsync();
        await context.Response.WriteAsync($"Photo {pic.FileName} successfully updated!");
    }
});

application.MapPost("/api/photos/{photoID}/delete", async (int photoID, HttpContext context, UserManager userManager, DbContext dbContext) =>
{
    if(context.User.Identity == null || !context.User.Identity.IsAuthenticated)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"Photo deletion failed!");
        return;
    }
    var user = await userManager.GetUserAsync(context.User);
    if(user == null)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Photo deletion failed!");
        return;
    }

    var photos = await dbContext.Pictures.Where(p => p.Id == photoID).ToListAsync();

    if(photos.Count == 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync($"Photo {photoID} deletion failed!");
        return;
    }
    else if(photos.Count > 1 || photos[0].UserId != user.Id)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync($"Photo {photoID} deletion forbidden!");
        return;
    }

    dbContext.Pictures.RemoveRange(photos);
    await dbContext.SaveChangesAsync();
    await context.Response.WriteAsync($"Photo {photoID} deleted!");
});

application.Run();

#endregion

#region Wrappers

public record PhotoDTO
{
    public int Id { get; init; }
    public string UserId { get; init; }
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public byte[] Content { get; init; }
}

public record UserDTO
{
    public string Id { get; init; }
    public string UserName {get; init; }
}

public class Photo
{
    public Photo() {}
    public Photo(User user, string filename, byte[] content, string contentType)
    {
        _User = user;
        FileName = filename;
        Content = content;
        ContentType = contentType;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string FileName { get; set; }
    public byte[] Content { get; set; }
    public string ContentType { get; set; }

    [ForeignKey("UserId")]
    public string UserId { get; set; }
    public User _User { get; set; }
}

public class GalleryIdentityUser : IdentityUser
{
    public ICollection<Photo> Pictures { get; set; } = new List<Photo>();
}

public class SimpleDbContext : IdentityDbContext<User, Role, string>
{
    public SimpleDbContext(DbContextOptions<SimpleDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder
            .Entity<User>()
            .HasMany(u => u.Pictures)
            .WithOne(p => p._User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Photo> Pictures { get; set; }
}

#endregion