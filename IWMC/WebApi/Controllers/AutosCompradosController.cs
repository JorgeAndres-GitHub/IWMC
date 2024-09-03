using AccesoDatos.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AutosCompradosController : ControllerBase
    {
        private readonly AutoCompradoDAO _autoCompradoDAO;

        public AutosCompradosController(AutoCompradoDAO autoCompradoDAO)
        {
            _autoCompradoDAO= autoCompradoDAO;
        }

        [HttpPost("{idAuto}")]
        public async Task<IActionResult> ComprarAuto([FromBody] string numeroDeCuenta, int idAuto)
        {
            int usuarioId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value);
            var response = await _autoCompradoDAO.ComprarAuto(usuarioId, idAuto, numeroDeCuenta);
            if (!response.Respuesta) return BadRequest(response.Mensaje);
            return Ok(response);
        }
    }
}
