using FurnitureManagementSystem.Data;
using FurnitureManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // FirstOrDefaultAsync üçün
using System.Threading.Tasks;

namespace FurnitureManagementSystem.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Contact
        public async Task<IActionResult> Index()
        {
            var contact = await _context.Contacts.FirstOrDefaultAsync();
           

            return View(contact);
        }
    }
}
