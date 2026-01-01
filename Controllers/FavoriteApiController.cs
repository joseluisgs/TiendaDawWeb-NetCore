using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// REST API Controller for favorite operations (AJAX)
/// </summary>
[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoriteApiController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<FavoriteApiController> _logger;

    public FavoriteApiController(
        IFavoriteService favoriteService,
        UserManager<User> userManager,
        ILogger<FavoriteApiController> logger)
    {
        _favoriteService = favoriteService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/favorites - Add product to favorites
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { success = false, message = "Usuario no autenticado" });
        }

        var result = await _favoriteService.AddFavoriteAsync(user.Id, request.ProductId);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, message = result.Error.Message });
        }

        return Ok(new { success = true, message = "Producto añadido a favoritos" });
    }

    /// <summary>
    /// DELETE /api/favorites/{productId} - Remove product from favorites
    /// </summary>
    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFavorite(long productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { success = false, message = "Usuario no autenticado" });
        }

        var result = await _favoriteService.RemoveFavoriteAsync(user.Id, productId);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, message = result.Error.Message });
        }

        return Ok(new { success = true, message = "Producto eliminado de favoritos" });
    }

    /// <summary>
    /// GET /api/favorites/check/{productId} - Check if product is favorited
    /// </summary>
    [HttpGet("check/{productId}")]
    public async Task<IActionResult> CheckFavorite(long productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { success = false, message = "Usuario no autenticado" });
        }

        var result = await _favoriteService.IsFavoriteAsync(user.Id, productId);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, message = result.Error.Message });
        }

        return Ok(new { success = true, isFavorite = result.Value });
    }

    /// <summary>
    /// POST /api/favorites/toggle - Toggle favorite status
    /// </summary>
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleFavorite([FromBody] AddFavoriteRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { success = false, message = "Usuario no autenticado" });
        }

        var checkResult = await _favoriteService.IsFavoriteAsync(user.Id, request.ProductId);
        
        if (checkResult.IsFailure)
        {
            return BadRequest(new { success = false, message = checkResult.Error.Message });
        }

        if (checkResult.Value)
        {
            // Remove from favorites
            var result = await _favoriteService.RemoveFavoriteAsync(user.Id, request.ProductId);
            if (result.IsFailure)
            {
                return BadRequest(new { success = false, message = result.Error.Message });
            }
            return Ok(new { success = true, isFavorite = false, message = "Eliminado de favoritos" });
        }
        else
        {
            // Add to favorites
            var result = await _favoriteService.AddFavoriteAsync(user.Id, request.ProductId);
            if (result.IsFailure)
            {
                return BadRequest(new { success = false, message = result.Error.Message });
            }
            return Ok(new { success = true, isFavorite = true, message = "Añadido a favoritos" });
        }
    }
}

public record AddFavoriteRequest(long ProductId);
