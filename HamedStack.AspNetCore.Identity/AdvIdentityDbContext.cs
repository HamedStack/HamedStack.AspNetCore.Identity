using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HamedStack.AspNetCore.Identity
{
    public class AdvIdentityDbContext : IdentityDbContext<AdvIdentityUser>
    {
        public AdvIdentityDbContext(DbContextOptions options) : base(options) { }

    }
}
