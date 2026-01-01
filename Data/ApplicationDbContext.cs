using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Data;

/// <summary>
/// Contexto de base de datos de la aplicación con soporte para Identity
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<long>, long>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Rating> Ratings => Set<Rating>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de Favorite (Many-to-Many)
        builder.Entity<Favorite>(entity =>
        {
            entity.HasKey(f => f.Id);
            
            entity.HasOne(f => f.Usuario)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Producto)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => new { f.UsuarioId, f.ProductoId }).IsUnique();
        });

        // Configuración de Product
        builder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Propietario)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.PropietarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.Precio).HasPrecision(18, 2);
            entity.HasQueryFilter(p => !p.Deleted);
        });

        // Configuración de Purchase
        builder.Entity<Purchase>(entity =>
        {
            entity.Property(p => p.Total).HasPrecision(18, 2);
        });

        // Configuración de Rating
        builder.Entity<Rating>(entity =>
        {
            entity.HasOne(r => r.Usuario)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Producto)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
