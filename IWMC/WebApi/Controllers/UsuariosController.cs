using AccesoDatos.Configuration;
using AccesoDatos.Context;
using AccesoDatos.DTOs;
using AccesoDatos.Models;
using AccesoDatos.Operaciones;
using AccesoDatos.Respuesta;
using AccesoDatos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private AppCarrosContext _context;
        private readonly IEmailSender _emailSender;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly UsuarioDAO _usuarioDAO;
        private readonly IConfiguration _configuration;

        public UsuariosController(AppCarrosContext context, IEmailSender emailSender, TokenValidationParameters tokenValidationParameters, UsuarioDAO usuarioDAO, IConfiguration configuration)
        {
            _context = context;
            _emailSender = emailSender;
            _tokenValidationParameters = tokenValidationParameters;
            _usuarioDAO = usuarioDAO;
            _configuration = configuration;
        }

        [HttpPost("EnviarEmailActualizacion")]
        public async Task<IActionResult> EnviarEmailActualizacion([FromBody] string correo)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.Equals(correo));

                if (usuario == null) return BadRequest(new AuthResult
                {
                    Respuesta = false,
                    Mensaje = "Correo inexistente"
                });

                if ((bool)!usuario.ConfirmedEmail) return BadRequest(new AuthResult
                {
                    Respuesta = false,
                    Mensaje = "El email debe estar confirmado"
                });

                await SendConfirmUpdateEmailAsync(usuario);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }        

        [HttpPatch("ActualizarContraseña")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] PasswordUpdateRequestDTO request)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.Equals(request.Correo));

                if (request.CodigoConfirmacion != usuario.ConfirmUpdateCode) return BadRequest("Codigo de confirmacion no valido");

                usuario.Contrasenia = HashPassword.HashPasswordBD(request.Password);
                usuario.ConfirmUpdateCode = null;
                await _context.SaveChangesAsync();                

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new AuthResult
                {
                    Respuesta = false,
                    Mensaje = "Error al intentar actualizar la contraseña"
                });
            }
        }


        [HttpPost("Register")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] RegisterRequestDTO usuario)
        {
            bool esAdmin = false;
            if(!usuario.Rol.IsNullOrEmpty() && usuario.Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (UsuarioPuedeSerAdmin(usuario))
                {
                    esAdmin = true;
                }
            }
            var respuesta = await _usuarioDAO.RegistrarUsuario(usuario, esAdmin);
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

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDTO tokenRequest)
        {
            if (!ModelState.IsValid) return BadRequest(new AuthResult
            {
                Mensaje = "Invalid parameters",
                Respuesta = false
            });

            var results = await VerifyAndGenerateAsync(tokenRequest);

            if (results == null) return BadRequest(new AuthResult
            {
                Mensaje = "Invalid token"
            });

            return Ok(results);
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

        [Authorize]
        [HttpGet("Perfil")]
        public async Task<IActionResult> MostrarPerfil()
        {
            try
            {
                var userClaims = User.Claims;

                var userId = userClaims.FirstOrDefault(c => c.Type == "Id")?.Value;
                var respuesta = await _usuarioDAO.ObtenerPerfilDeUsuario(int.Parse(userId));
                if (respuesta == null) return NotFound("Usuario no encontrado");
                return Ok(respuesta);
            }
            catch (Exception)
            {
                return BadRequest("Error al intentar buscar datos de usuario");
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

        private async Task SendConfirmUpdateEmailAsync(Usuario usuario)
        {
            var codigoDeConfirmacion = RandomGenerator.GenerateRandomString(5);

            usuario.ConfirmUpdateCode = codigoDeConfirmacion;
            await _context.SaveChangesAsync();

            var emailBody = $"Este es tu codigo de confirmacion {codigoDeConfirmacion}";

            await _emailSender.SendEmailAsync(usuario.Email, "Actualizacion de datos", emailBody);
        }

        private async Task<AuthResult> VerifyAndGenerateAsync(TokenRequestDTO tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                //ACCESS TOKEN
                _tokenValidationParameters.ValidateLifetime = false; //talvez cambie en produccion
                var tokenBeingVerified = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                if(validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (!result || tokenBeingVerified == null) throw new Exception("Invalid token"); 
                }

                var utcExpiryDate = long.Parse(tokenBeingVerified.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate).UtcDateTime;

                if (expiryDate < DateTime.UtcNow) throw new Exception("Expired token");

                //REFRESH TOKEN
                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenRequest.RefreshToken);

                if (storedToken == null) throw new Exception("Invalid token");

                if (storedToken.IsRevoked || storedToken.IsUsed) throw new Exception("Invalid token");

                var jti = tokenBeingVerified.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
                if (jti != storedToken.JwtId) throw new Exception("Invalid token");

                if (storedToken.ExpiryDate < DateTime.UtcNow) throw new Exception("Expired token");

                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                var dbUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == storedToken.UsuarioId);

                return await _usuarioDAO.GenerateTokenAsync(dbUser);
            }
            catch (Exception e)
            {
                var message = e.Message == "Invalid token" || e.Message == "Expired token" ? e.Message : "Internal Server Error";
                return new AuthResult
                {
                    Respuesta = false,
                    Mensaje = message
                };
            }
        }

        private bool UsuarioPuedeSerAdmin(RegisterRequestDTO usuario)
        {
            var key = _configuration["CodigoAdmin:Key"];
            return usuario.CodigoInvitacion == key;
        }

    }
}
