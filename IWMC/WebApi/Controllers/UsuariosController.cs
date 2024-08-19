using AccesoDatos.Context;
using AccesoDatos.DTOs;
using AccesoDatos.Models;
using AccesoDatos.Operaciones;
using AccesoDatos.Respuesta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Encodings.Web;

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppCarrosContext _context;
        private readonly UsuarioDAO _usuarioDAO;
        private readonly IEmailSender _emailSender;

        public UsuariosController(AppCarrosContext context, UsuarioDAO usuarioDAO, IEmailSender emailSender)
        {
            _context = context;
            _usuarioDAO = usuarioDAO;
            _emailSender = emailSender;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] RegisterRequestDTO usuario)
        {
            var respuesta = await _usuarioDAO.RegistrarUsuario(usuario);
            if (respuesta.Mensaje.Equals("El email/cedula ingresado ya existe") && respuesta.Mensaje != "Usuario Ingresado Correctamente") return BadRequest(respuesta.Mensaje);
            var usuarioRegistrado = await _context.Usuarios.Where(u => u.Email.Equals(usuario.Email)).FirstOrDefaultAsync();
            await SendVerificationEmailAsync(usuarioRegistrado);
            return Ok(respuesta);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO usuario)
        {
            var respuesta = await _usuarioDAO.Login(usuario);
            if(respuesta.Mensaje.Equals("Email y/o contraseña invalidos") || respuesta.Mensaje.Equals("El email debe estar confirmado")) return BadRequest(respuesta.Mensaje);

            return Ok(respuesta);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(int userId, string code)
        {
            if (string.IsNullOrEmpty(userId.ToString()) || string.IsNullOrEmpty(code)) return BadRequest(new AuthResult { Mensaje = "Invalid email confirmation url", Respuesta = false });

            var user = await _context.Usuarios.FirstOrDefaultAsync(u=>u.Id== userId);

            if (user == null)
                return NotFound($"Unable to load user with Id '{userId}'.");

            if (user.VerificationCode != code) return BadRequest(new AuthResult
            {
                Mensaje = "Invalid verification code",
                Respuesta = false
            });

            try
            {
                user.ConfirmedEmail = true;
                await _context.SaveChangesAsync();

                return Ok("Thank you for confirm your email.");
            }
            catch (Exception)
            {
                return BadRequest("There has been an error confirming your email");
            }
            

        }

        private async Task SendVerificationEmailAsync(Usuario usuario)
        {
            var verificationCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationCode));

            usuario.VerificationCode = verificationCode;
            await _context.SaveChangesAsync();

            //Example: https://localhost:8080/api/authentication/verifyEmail/userId=exampleUserId&code=exampleCode
            var callBackUrl = $"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Usuarios", new { userId = usuario.Id, code = verificationCode })}";
            var emailBody = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>";

            await _emailSender.SendEmailAsync(usuario.Email, "Confirm your email", emailBody);
        }
    }
}
