using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HamedStack.AspNetCore.Identity
{
    public class AdvIdentityDbContext : IdentityDbContext<AdvIdentityUser>
    {
        public AdvIdentityDbContext(DbContextOptions<AdvIdentityDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var identityUser = new AdvIdentityUser
            {
                UserName = "test",
                PasswordHash = "test@123",
                Email = "test@test.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            modelBuilder.Entity<AdvIdentityUser>().HasData(identityUser);
        }
    }
}
