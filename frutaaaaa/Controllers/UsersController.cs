using frutaaaaa.Data;
using frutaaaaa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace frutaaaaa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [NonAction]
        public ApplicationDbContext CreateDbContext(string dbName)
        {
            var baseConnectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(baseConnectionString))
            {
                throw new ArgumentException("Database name or connection string is missing.");
            }
            var dynamicConnectionString = baseConnectionString.Replace("frutaaaaa_db", dbName);
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(dynamicConnectionString, ServerVersion.AutoDetect(dynamicConnectionString));
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromHeader(Name = "X-Database-Name")] string database,
            [FromBody] UserRequest request)
        {
            try
            {
                using (var _context = CreateDbContext(database))
                {
                    if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                    {
                        return BadRequest(new { message = "Username already exists" });
                    }

                    var user = new User
                    {
                        Username = request.Username,
                        Password = request.Password,
                        // REMOVED: Permission = request.Permission
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Define available pages
                    var availablePages = new[] { "home", "dashboard", "programs", "program-new", "program-edit", "traits", "traitements", "ecart-direct", "admin" };

                    // Create default permissions (all false) for new user
                    foreach (var page in availablePages)
                    {
                        _context.UserPagePermissions.Add(new UserPagePermission
                        {
                            UserId = user.Id,
                            PageName = page,
                            Allowed = false
                        });
                    }

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "User registered successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserRequest request)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound();

                user.Username = request.Username;
                user.Password = request.Password;
                // REMOVED: user.Permission = request.Permission;

                await _context.SaveChangesAsync();
                return Ok(user);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound();

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserRequest request)
        {
            if (string.IsNullOrEmpty(request.Database))
            {
                return BadRequest(new { message = "Database name is required." });
            }

            try
            {
                Console.WriteLine($"Login attempt: Username={request.Username}, DB={request.Database}");
                using (var dynamicContext = CreateDbContext(request.Database))
                {
                    var user = await dynamicContext.Users.FirstOrDefaultAsync(u =>
                        u.Username == request.Username &&
                        u.Password == request.Password);

                    Console.WriteLine($"User lookup result: User found={user != null}, Id={user?.Id}");
                    if (user == null)
                    {
                        Console.WriteLine("Unauthorized: Invalid credentials.");
                        return Unauthorized(new { message = "Invalid credentials for the specified database." });
                    }

                    // Get user's page permissions
                    var permissions = await dynamicContext.UserPagePermissions
                        .Where(p => p.UserId == user.Id)
                        .ToListAsync();

                    Console.WriteLine($"Permissions loaded: Count={permissions.Count}");
                    permissions.ForEach(p => Console.WriteLine($"  - {p.PageName}: {p.Allowed}"));

                    return Ok(new
                    {
                        message = "Login successful",
                        userId = user.Id,
                        // REMOVED: permission = user.Permission,
                        database = request.Database,
                        permissions = permissions.Select(p => new {
                            page_name = p.PageName,
                            allowed = p.Allowed ? 1 : 0
                        }).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Login: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return StatusCode(500, new { message = "Could not connect to the specified database. Please check the name and try again." });
            }
        }

        [HttpGet("permissions/{userId}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                var permissions = await _context.UserPagePermissions
                    .Where(p => p.UserId == userId)
                    .Select(p => new {
                        page_name = p.PageName,
                        allowed = p.Allowed
                    })
                    .ToListAsync();

                return Ok(permissions);
            }
        }

        [HttpPost("permissions/{userId}")]
        public async Task<IActionResult> UpdateUserPermissions(int userId, [FromBody] UpdatePermissionsRequest request)
        {
            using (var _context = CreateDbContext(Request.Headers["X-Database-Name"].ToString()))
            {
                Console.WriteLine($"Starting UpdateUserPermissions for userId={userId}, permissions count={request.Permissions.Count}");

                request.Permissions.ForEach(p => Console.WriteLine($" - {p.PageName}: {p.Allowed}"));

                // Use raw SQL delete to avoid EF tracking issues
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM user_page_permissions WHERE user_id = ?", userId);
                Console.WriteLine("Deleted existing permissions");

                // Add new permissions
                foreach (var perm in request.Permissions)
                {
                    var newPerm = new UserPagePermission
                    {
                        UserId = userId,
                        PageName = perm.PageName,
                        Allowed = perm.Allowed
                    };
                    Console.WriteLine($"Adding new permission: UserId={newPerm.UserId}, PageName={newPerm.PageName}, Allowed={newPerm.Allowed}");
                    _context.UserPagePermissions.Add(newPerm);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Permissions saved successfully");
                }
                catch (Exception saveEx)
                {
                    Console.WriteLine($"SaveChanges failed: {saveEx.Message}");
                    if (saveEx.InnerException != null) Console.WriteLine($"Inner: {saveEx.InnerException.Message}");
                    throw;
                }

                return Ok(new
                {
                    message = "Permissions updated successfully",
                    updated_count = request.Permissions.Count
                });
            }
        }
    }
}
 