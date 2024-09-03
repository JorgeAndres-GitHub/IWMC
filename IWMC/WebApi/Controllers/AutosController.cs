using AccesoDatos.Context;
using AccesoDatos.DTOs;
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
        private readonly string[] marcasCarros = { "Mercedes", "BMW", "Audi", "Ford", "Porshe", "Maserati" };
        private readonly string[] marcasCamionetas = { "Mercedes", "Cadillac", "Porshe", "Maserati", "Jeep" };

        public AutosController(AppCarrosContext appCarrosContext, AutoDAO autoDAO)
        {
            _appCarrosContext = appCarrosContext;
            _autoDAO = autoDAO;
        }

        [HttpGet("/auto/tipo/{tipo}")]
        public async Task<IActionResult> ObtenerAutosPorTipo(string tipo)
        {
            if (tipo.Equals("carro", StringComparison.OrdinalIgnoreCase)) 
            {
                return Ok(marcasCarros);
            }
            else if(tipo.Equals("camioneta", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(marcasCamionetas);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("/auto/marca/{marca}/{tipo}")]
        public async Task<IActionResult> ObtenerAutosPorMarca(string marca, string tipo)
        {
            var autos = await _appCarrosContext.Autos.Where(a => a.Vehiculo.Contains(marca) && a.Tipo.ToLower() == tipo.ToLower()).ToListAsync();
            if (autos.Count==0) return NotFound();
            return Ok(autos);
        }

        [Authorize(Policy = "SuperRol")]
        [HttpPost("/auto/post")]
        public async Task<IActionResult> AgregarAuto(AutoRequestDTO auto)
        {
            bool esMarca = false;
            if (auto.Tipo.Equals("Carro", StringComparison.OrdinalIgnoreCase))
            {
                foreach(var marca in marcasCarros)
                {
                    if (auto.Vehiculo.Contains(marca, StringComparison.OrdinalIgnoreCase))
                    {
                        esMarca = true;
                        break;
                    }
                }
            }
            else if (auto.Tipo.Equals("Camioneta", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var marca in marcasCamionetas)
                {
                    if (auto.Vehiculo.Contains(marca, StringComparison.OrdinalIgnoreCase))
                    {
                        esMarca = true;
                        break;
                    }
                }                
            }
            else
            {
                return BadRequest("Tipo de auto ingresado invalido");
            }

            if(!esMarca) return BadRequest("Debe ingresar un vehiculo con marca valida");

            var autoAgregado = await _autoDAO.AgregarAuto(auto.Vehiculo, auto.VersionVehiculo, auto.Precio, auto.Tipo);
            if (!autoAgregado) return BadRequest("Error al intentar agregar un auto");
            return CreatedAtAction("AgregarAuto", auto);
        }
    }
}
