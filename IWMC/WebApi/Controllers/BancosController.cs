using AccesoDatos.DTOs;
using AccesoDatos.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BancosController : ControllerBase
    {
        private readonly CuentasBancariasDAO _cuentasBancariasDAO;

        public BancosController(CuentasBancariasDAO cuentasBancariasDAO)
        {
            _cuentasBancariasDAO = cuentasBancariasDAO;
        }

        [HttpPost("Banco")]
        public async Task<IActionResult> InsertarCuentaDeBanco([FromBody] BancoRequestDTO bancoRequestDTO)
        {
            var usuarioId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            var respuesta = await _cuentasBancariasDAO?.PonerCuentaBancaria(bancoRequestDTO, usuarioId!)!;
            if(!respuesta.Respuesta) return BadRequest(respuesta.Mensaje);
            return CreatedAtAction("InsertarCuentaDeBanco", usuarioId, respuesta);
        }

        [HttpPatch("Dinero")]
        public async Task<IActionResult> AgregarDinero([FromBody] DineroRequestDTO request)
        {
            int usuarioId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value);
            var response = await _cuentasBancariasDAO.AgregarDinero(usuarioId, request.NumeroDeCuenta, request.Dinero);
            if(response.Mensaje.Equals("Usuario no encontrado") || response.Mensaje.Equals("Error al agregar dinero a la cuenta")) return BadRequest(response);
            return NoContent();
        }

        [HttpGet("Cuentas")]
        public async Task<IActionResult> MostrarCuentas()
        {
            int usuarioId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value);
            var cuentas = await _cuentasBancariasDAO.ObtenerCuentas(usuarioId);
            if (cuentas == null) return NotFound();
            return Ok(cuentas);
        }
    }
}
