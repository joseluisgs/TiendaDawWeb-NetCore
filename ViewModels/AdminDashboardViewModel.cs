namespace TiendaDawWeb.ViewModels;

/// <summary>
/// ViewModel para el dashboard de administración
/// </summary>
public class AdminDashboardViewModel
{
    public int TotalUsuarios { get; set; }
    public int TotalProductos { get; set; }
    public int TotalCompras { get; set; }
    public decimal TotalVentas { get; set; }
    
    public int UsuariosActivos { get; set; }
    public int ProductosDisponibles { get; set; }
    public int ComprasHoy { get; set; }
    public int ComprasSemana { get; set; }
    public int ComprasMes { get; set; }
    
    public decimal VentasHoy { get; set; }
    public decimal VentasSemana { get; set; }
    public decimal VentasMes { get; set; }
}
