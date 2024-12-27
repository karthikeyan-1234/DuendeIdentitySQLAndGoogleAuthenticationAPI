using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ids.Pages.Clients
{
    public class EditModel : PageModel
    {
        private readonly ConfigurationDbContext _context;

        public EditModel(ConfigurationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client? Client { get; set; }

        public async Task<IActionResult> OnGetAsync(string? ClientId)
        {
            if (ClientId == null)
                return RedirectToPage("./Index");

            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == ClientId)!;

            if (client == null)
                return RedirectToPage("./Index");
            else
            {
                Client = client;
                return Page();
            }

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Entry(Client!).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
