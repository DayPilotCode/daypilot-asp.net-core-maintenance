using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Types : ControllerBase
    {
        private readonly MaintenanceDbContext _context;

        public Types(MaintenanceDbContext context)
        {
            _context = context;
        }
        
        // GET: api/Types
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTypes()
        {
            if (_context.Types == null)
            {
                return NotFound();
            }

            var types = await _context.Types
                .Include(t => t.TypeItems)
                .Select(t => new 
                {
                    name = t.Name,
                    id = t.Id,
                    checklist = t.TypeItems.Select(ti => new 
                    {
                        name = ti.Name,
                        id = ti.Id
                    }),
                    color = t.Color,
                    period = t.Period
                })
                .ToListAsync();

            return types;
        }

    }
}
