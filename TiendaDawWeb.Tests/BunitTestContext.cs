using Bunit;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace TiendaDawWeb.Tests;

/// <summary>
/// OBJETIVO: Proveer un contexto de renderizado de Blazor (bUnit) para tests unitarios.
/// UBICACIÓN: Raíz del proyecto de Tests.
/// RAZÓN: Es una clase de infraestructura de testing. Centraliza la creación y destrucción del BunitContext 
/// para evitar fugas de memoria y duplicidad de código en los tests de componentes.
/// </summary>
public abstract class BunitTestContext : IDisposable
{
    private BunitContext? _context;

    protected BunitContext Context => _context ?? throw new InvalidOperationException("BunitContext is not initialized.");

    protected IServiceCollection Services => Context.Services;

    [SetUp]
    public void SetupContext()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        _context = new BunitContext();
    }

    [TearDown]
    public void TearDownContext()
    {
        _context?.Dispose();
        _context = null;
    }

    public IRenderedComponent<TComponent> RenderComponent<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        return parameterBuilder is null
            ? Context.Render<TComponent>()
            : Context.Render<TComponent>(parameterBuilder);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
