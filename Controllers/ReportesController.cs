using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using back_tienda.Core.DTOs;
using back_tienda.Core.Interfaces;

namespace back_tienda.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;
    private readonly ITiendaService _tiendaService;

    public ReportesController(IReporteService reporteService, ITiendaService tiendaService)
    {
        _reporteService = reporteService;
        _tiendaService = tiendaService;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<ReporteDto>> GetResumen()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Buscar tiendas donde el usuario es dueño
        var tiendas = await _tiendaService.GetByDueñoAsync(userId);
        var tienda = tiendas.FirstOrDefault();

        if (tienda == null)
        {
            // TODO: Manejar caso donde es empleado pero no dueño (requeriría lógica adicional)
            return NotFound("No se encontró tienda asociada al usuario");
        }

        var reporte = await _reporteService.GetResumenAsync(tienda.IdTienda);
        return Ok(reporte);
    }

    [HttpGet("dashboard/{idTienda}")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(Guid idTienda)
    {
        var dashboard = await _reporteService.GetDashboardAsync(idTienda);
        return Ok(dashboard);
    }

    [HttpGet("ventas/{idTienda}")]
    public async Task<ActionResult<ReporteVentasDto>> GetReporteVentas(Guid idTienda, [FromQuery] DateTime inicio, [FromQuery] DateTime fin)
    {
        var reporte = await _reporteService.GetReporteVentasAsync(idTienda, inicio, fin);
        return Ok(reporte);
    }

    [HttpGet("stock-alertas/{idTienda}")]
    public async Task<ActionResult<IEnumerable<AlertaStockDto>>> GetAlertasStock(Guid idTienda)
    {
        var alertas = await _reporteService.GetAlertasStockAsync(idTienda);
        return Ok(alertas);
    }
}
