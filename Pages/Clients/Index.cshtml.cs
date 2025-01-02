using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ids.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly ConfigurationDbContext _context;

        public IndexModel(ConfigurationDbContext context)
        {
            _context = context;
        }

        public List<Client> Clients { get; set; } = new List<Client>();

        public async Task OnGetAsync()
        {
            Clients = await _context.Clients.Include(c => c.RedirectUris).ToListAsync();
        }

    }
}
