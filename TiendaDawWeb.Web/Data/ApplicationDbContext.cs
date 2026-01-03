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

    /// <summary>
    /// Sobrescribe el guardado de cambios para interceptar las entidades y rellenar 
    /// automáticamente los campos de auditoría (CreatedAt, UpdatedAt).
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => (e.Entity is AuditableEntity || e.Entity is User) && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var now = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                // Si es una entidad nueva, establecemos la fecha de creación
                if (entityEntry.Entity is AuditableEntity auditable)
                {
                    auditable.CreatedAt = now;
                }
                else if (entityEntry.Entity is User user)
                {
                    user.CreatedAt = now;
                }
            }
            else
            {
                // Si es una modificación, establecemos la fecha de actualización
                if (entityEntry.Entity is AuditableEntity auditable)
                {
                    auditable.UpdatedAt = now;
                }
                else if (entityEntry.Entity is User user)
                {
                    user.UpdatedAt = now;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

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

        // ========================================
        // USER CONFIGURATION (Identity already configures HasKey)
        // ========================================
        builder.Entity<User>(entity =>
        {
            // entity.HasKey(e => e.Id); // No es necesario, Identity ya lo configura
            
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Rol).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            
            entity.HasMany(e => e.Products)
                .WithOne(p => p.Propietario)
                .HasForeignKey(p => p.PropietarioId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(e => e.Purchases)
                .WithOne(p => p.Comprador)
                .HasForeignKey(p => p.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // PRODUCT CONFIGURATION
        // ========================================
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Precio).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Categoria).IsRequired();
            entity.Property(e => e.Imagen).HasMaxLength(500);
            entity.Property(e => e.Deleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Propietario)
                .WithMany(u => u.Products)
                .HasForeignKey(e => e.PropietarioId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Compra)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CompraId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            
            // Apply global query filter for soft delete
            entity.HasQueryFilter(p => !p.Deleted);
        });

        // ========================================
        // PURCHASE CONFIGURATION
        // ========================================
        builder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FechaCompra).IsRequired();
            entity.Property(e => e.Total).IsRequired().HasPrecision(18, 2);
            
            entity.HasOne(e => e.Comprador)
                .WithMany(u => u.Purchases)
                .HasForeignKey(e => e.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(e => e.Products)
                .WithOne(p => p.Compra)
                .HasForeignKey(p => p.CompraId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // RATING CONFIGURATION
        // ========================================
        builder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Puntuacion).IsRequired();
            entity.Property(e => e.Comentario).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Ratings)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Producto)
                .WithMany(p => p.Ratings)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Índice único: un usuario solo puede valorar un producto una vez
            entity.HasIndex(e => new { e.UsuarioId, e.ProductoId }).IsUnique();
            
            // Apply matching query filter to avoid EF warning
            entity.HasQueryFilter(r => !r.Producto.Deleted);
        });

        // ========================================
        // FAVORITE CONFIGURATION
        // ========================================
        builder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Favorites)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany(p => p.Favorites)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único: un usuario no puede marcar el mismo producto como favorito dos veces
            entity.HasIndex(e => new { e.UsuarioId, e.ProductoId }).IsUnique();
            
            // Apply matching query filter to avoid EF warning
            entity.HasQueryFilter(f => !f.Producto.Deleted);
        });

        // ========================================
        // CARRITO ITEM CONFIGURATION
        // ========================================
        builder.Entity<CarritoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Precio).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.CarritoItems)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único por usuario y producto para evitar duplicados
            entity.HasIndex(e => new { e.UsuarioId, e.ProductoId }).IsUnique();
            
            // Apply matching query filter to avoid EF warning
            entity.HasQueryFilter(c => !c.Producto.Deleted);
        });
    }
}
