
using AccesoDatos.Configuration;
using AccesoDatos.Context;
using AccesoDatos.DTOs;
using AccesoDatos.Models;
using AccesoDatos.Respuesta;
using AccesoDatos.Services;
using Azure.Core;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.Operaciones
{
    public class UsuarioDAO
    {
        private readonly AppCarrosContext _context;
        private readonly JwtConfig _jwtConfig;

        public UsuarioDAO(AppCarrosContext context, IOptions<JwtConfig> jwtConfig)
        {
            _context = context;
            _jwtConfig = jwtConfig.Value;
        }

        public async Task<AuthResult> RegistrarUsuario(RegisterRequestDTO usuario)
        {
            try
            {
                var cedulaEmailExist = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.Equals(usuario.Email) || u.Cedula.Equals(usuario.Cedula));
                if (cedulaEmailExist != null) return new AuthResult
                {
                    Respuesta = false,
                    Mensaje = "El email/cedula ingresado ya existe"
                };

                var contraseniaEncriptada = HashPassword.HashPasswordBD(usuario.Contrasenia);

                var usuarioR = new Usuario
                {
                    Nombre = usuario.Nombre,
                    Apellidos = usuario.Apellidos,
                    Cedula = usuario.Cedula,
                    Direccion = usuario.Direccion,
                    Telefono = usuario.Telefono,
                    Email = usuario.Email,
                    Contrasenia = contraseniaEncriptada,
                    ConfirmedEmail = false
                };

                await _context.Usuarios.AddAsync(usuarioR);
                await _context.SaveChangesAsync();
                                
                return new AuthResult
                {
                    Respuesta = true,
                    Mensaje = "Usuario Ingresado Correctamente"
                };
            }
            catch (Exception e)
            {
                return new AuthResult
                {
                    Respuesta = false,
                    Mensaje = e.Message
                };
            }

        }

        public async Task<AuthResult> Login(LoginRequestDTO loginRequestDTO)
        {
            var contraseñaEncriptada = HashPassword.HashPasswordBD(loginRequestDTO.Contrasenia);
            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.Equals(loginRequestDTO.Email) && u.Contrasenia.Equals(contraseñaEncriptada));
            if (existingUser == null) return new AuthResult
            {
                Respuesta = false,
                Mensaje = "Email y/o contraseña invalidos"
            };

            if ((bool)!existingUser.ConfirmedEmail) return new AuthResult
            {
                Mensaje = "El email debe estar confirmado",
                Respuesta = false
            };

            var token = GenerateTokenAsync(existingUser).Result;
            return new AuthResult
            {
                Respuesta = true,
                Token = token,
                Mensaje = "Inicio de sesion exitoso"
            };

        }

        private async Task<string> GenerateTokenAsync(Usuario usuario)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new ClaimsIdentity(new[]
                {
                    new Claim("Id", usuario.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Nombre+usuario.Apellidos),
                    new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                })),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
            
        }        
    }
}
