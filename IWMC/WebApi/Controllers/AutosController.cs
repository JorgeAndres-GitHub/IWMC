using AccesoDatos.Context;
using AccesoDatos.Models;
using AccesoDatos.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AutosController : ControllerBase
    {
        private readonly AppCarrosContext _appCarrosContext;
        private readonly AutoDAO _autoDAO;

        public AutosController(AppCarrosContext appCarrosContext, AutoDAO autoDAO)
        {
            _appCarrosContext = appCarrosContext;
            _autoDAO = autoDAO;
        }

        [HttpGet("{tipo}")]
        public async Task<IEnumerable<Auto>> ObtenerAutos(string tipo) => await _appCarrosContext.Autos.Where(a=>a.Tipo.Equals(tipo)).ToListAsync();

        [Authorize(Policy = "SuperRol")]
        [HttpPost]
        public async Task<IActionResult> AgregarAuto(Auto auto)
        {
            var autoAgregado = await _autoDAO.AgregarAuto(auto.Vehiculo, auto.VersionVehiculo, auto.Precio, auto.Tipo);
            if (!autoAgregado) return BadRequest("Error al intentar agregar un auto");
            return CreatedAtAction("AgregarAuto", auto.Id, auto);
        }
    }
}
