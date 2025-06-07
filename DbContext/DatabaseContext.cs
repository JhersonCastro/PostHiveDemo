using DbContext.Models;
using Microsoft.EntityFrameworkCore;

namespace DbContext
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> options)
        : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        public DbSet<User> Users { get; init; }
        public DbSet<Credential> Credentials { get; init; }
        public DbSet<Post> Posts { get; init; }
        public DbSet<Relationship> Relationship { get; init; }
        public DbSet<Files> Files { get; init; }
        public DbSet<Comments> Comments { get; init; }
        public DbSet<CookiesResearch> Cookies { get; init; }
        public DbSet<Report> Reports { get; init; }
        public DbSet<ReportReasons> ReportReasons { get; init; }
        public DbSet<Ban> Ban { get; init; }
        public DbSet<Notifications> Notifications { get; init; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración para Comments
            modelBuilder.Entity<Comments>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Cambiado a Restrict

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Mantener cascada aquí

            // Configuración para Relationship
            modelBuilder.Entity<Relationship>()
                .HasOne(r => r.User)
                .WithMany(u => u.RelationshipsInitiated)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Relationship>()
                .HasOne(r => r.RelatedUser)
                .WithMany(u => u.RelationshipsReceived)
                .HasForeignKey(r => r.RelationshipUserIdA)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Ban>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Evita cascada

            modelBuilder.Entity<Ban>()
                .HasOne(b => b.Report)
                .WithMany()
                .HasForeignKey(b => b.ReportId)
                .OnDelete(DeleteBehavior.Restrict); // Evita cascada
            modelBuilder.Entity<User>()
                .ToTable(tb => tb.HasTrigger("NickNameAlreadyExists"));
        }



    }
}
