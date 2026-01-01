using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public DbSet<CarritoItem> CarritoItems => Set<CarritoItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Suppress InMemoryDatabase transaction warning
        optionsBuilder.ConfigureWarnings(w => 
            w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }

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
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(f => new { f.UsuarioId, f.ProductoId }).IsUnique();
            
            // Apply matching query filter to avoid EF warning
            entity.HasQueryFilter(f => !f.Producto.Deleted);
        });

        // Configuración de Product
        builder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Propietario)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.PropietarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Compra)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CompraId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.Property(p => p.Precio).HasPrecision(18, 2);
            
            // Apply global query filter for soft delete
            entity.HasQueryFilter(p => !p.Deleted);
        });

        // Configuración de Purchase
        builder.Entity<Purchase>(entity =>
        {
            entity.HasOne(p => p.Comprador)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);

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
                .OnDelete(DeleteBehavior.Restrict);
            
            // Apply matching query filter to avoid EF warning
            entity.HasQueryFilter(r => !r.Producto.Deleted);
        });

        // Configuración de CarritoItem
        builder.Entity<CarritoItem>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Usuario)
                .WithMany(u => u.CarritoItems)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Producto)
                .WithMany()
                .HasForeignKey(c => c.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(c => c.Precio).HasPrecision(18, 2);
            
            // Índice único por usuario y producto para evitar duplicados
            entity.HasIndex(c => new { c.UsuarioId, c.ProductoId }).IsUnique();
            
            // Apply matching query filter to avoid EF warning
            entity.HasQueryFilter(c => !c.Producto.Deleted);
        });
    }
}
