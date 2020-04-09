using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Data
{
  public class DataContext : DbContext

  //IdentityDbContext<User, Role, int, IdentityUserClaim<int>,
  //  UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
  {
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);
      builder.Entity<UserRole>(userRole =>
      {
        userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

        userRole.HasOne(ur => ur.Role)
          .WithMany(r => r.UserRoles)
          .HasForeignKey(ur => ur.RoleId)
          .IsRequired();

        userRole.HasOne(ur => ur.User)
          .WithMany(r => r.UserRoles)
          .HasForeignKey(ur => ur.UserId)
          .IsRequired();
      });
    }

  }
}