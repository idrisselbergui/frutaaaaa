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
                        Permission = request.Permission
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "User registered successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
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
                using (var dynamicContext = CreateDbContext(request.Database))
                {
                    // This will now throw an exception if the database or table does not exist.
                    var user = await dynamicContext.Users.FirstOrDefaultAsync(u =>
                        u.Username == request.Username &&
                        u.Password == request.Password);

                    if (user == null)
                    {
                        return Unauthorized(new { message = "Invalid credentials for the specified database." });
                    }

                    return Ok(new
                    {
                        message = "Login successful",
                        userId = user.Id,
                        permission = user.Permission,
                        database = request.Database
                    });
                }
            }
            catch (Exception ex)
            {
                // This catch block will now handle bad database names.
                Console.WriteLine(ex.Message); // Log the real error for debugging.
                return StatusCode(500, new { message = "Could not connect to the specified database. Please check the name and try again." });
            }
        }
    }
}

