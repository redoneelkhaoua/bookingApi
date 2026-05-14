namespace server.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class DirectoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public DirectoryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("businesses")]
    public async Task<IActionResult> GetBusinesses([FromQuery] string? search, [FromQuery] string? category)
    {
        var query = _db.Businesses.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.Name.Contains(search) || (b.Description != null && b.Description.Contains(search)));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(b => b.Category == category);
        }

        var businesses = await query
            .Select(b => new
            {
                b.Slug,
                b.Name,
                b.LogoUrl,
                b.Address,
                b.Category,
                b.Description,
                ServiceCount = b.Services.Count(s => s.IsActive),
                StaffCount = b.Staff.Count(s => s.IsActive)
            })
            .ToListAsync();

        return Ok(businesses);
    }
}
