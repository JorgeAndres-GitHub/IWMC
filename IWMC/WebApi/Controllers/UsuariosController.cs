using AccesoDatos.DTOs;
using AccesoDatos.Models;
using AccesoDatos.Operaciones;
using AccesoDatos.Respuesta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioDAO _usuarioDAO;

        public UsuariosController(UsuarioDAO usuarioDAO)
        {
            _usuarioDAO = usuarioDAO;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] RegisterRequestDTO usuario)
        {
            var respuesta = await _usuarioDAO.RegistrarUsuario(usuario);
            if (respuesta.Mensaje.Equals("El email/cedula ingresado ya existe") && respuesta.Mensaje != "Usuario Ingresado Correctamente") return BadRequest(respuesta.Mensaje);

            return Ok(respuesta);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO usuario)
        {
            var respuesta = await _usuarioDAO.Login(usuario);
            if(respuesta.Mensaje.Equals("Email y/o contraseña invalidos") || respuesta.Mensaje.Equals("El email debe estar confirmado")) return BadRequest(respuesta.Mensaje);

            return Ok(respuesta);
        }
    }
}
