using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourLibrary.Models;

namespace YourLibrary.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<UserBook> UserBooks { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.Review)
            .WithMany()
            .HasForeignKey(ub => ub.ReviewId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserBook>()
                .HasOne(ub => ub.ApplicationUser)
                .WithMany(u => u.UserBooks)
                .HasForeignKey(ub => ub.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Requister
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Requester)
                .WithMany(u => u.FriendshipRequesters) 
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Receiver
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Receiver) 
                .WithMany(u => u.FriendshipReceivers) 
                .HasForeignKey(f => f.ReceiverId) 
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
