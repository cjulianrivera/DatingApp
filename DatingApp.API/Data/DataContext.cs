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
    public DbSet<Like> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      builder.Entity<Like>()
        .HasKey(k => new { k.LikerId, k.LikeeId });

      builder.Entity<Like>()
        .HasOne(u => u.Likee)
        .WithMany(u => u.Likers)
        .HasForeignKey(u => u.LikeeId)
        .OnDelete(DeleteBehavior.Restrict);

      builder.Entity<Like>()
        .HasOne(u => u.Liker)
        .WithMany(u => u.Likees)
        .HasForeignKey(u => u.LikerId)
        .OnDelete(DeleteBehavior.Restrict);
    }
  }
}