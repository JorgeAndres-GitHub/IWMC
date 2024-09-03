using AccesoDatos.Context;
using AccesoDatos.Models;
using AccesoDatos.Respuesta;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.Operaciones
{
    public class AutoCompradoDAO
    {
        private readonly AppCarrosContext _appCarrosContext;

        public AutoCompradoDAO(AppCarrosContext appCarrosContext) 
        {
            _appCarrosContext = appCarrosContext;            
        }

        public async Task<ResultadoOperacion> ComprarAuto(int idUsuario, int idAuto, string numeroDeCuenta)
        {
            using (var transaction = await _appCarrosContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var usuarioExistente = await _appCarrosContext.Usuarios.FindAsync(idUsuario);
                    if (usuarioExistente == null) return new ResultadoOperacion
                    {
                        Mensaje = "El usuario no existe",
                        Respuesta = false
                    };

                    var autoExistente = await _appCarrosContext.Autos.FindAsync(idAuto);
                    if (autoExistente == null) return new ResultadoOperacion
                    {
                        Mensaje = "El auto seleccionado no existe",
                        Respuesta = false
                    };

                    var cuentaExistente = await _appCarrosContext.CuentasBancarias.Where(cb => cb.NumeroDeCuenta.Equals(numeroDeCuenta) && cb.UsuarioId == idUsuario).FirstOrDefaultAsync();
                    if (cuentaExistente == null) return new ResultadoOperacion
                    {
                        Mensaje = "La cuenta bancaria no existe",
                        Respuesta = false
                    };

                    if (cuentaExistente.Dinero < autoExistente.Precio) return new ResultadoOperacion
                    {
                        Mensaje = "Dinero insuficiente en la cuenta",
                        Respuesta = false
                    };

                    var compra = new AutosDeUsuario
                    {
                        UsuarioId = idUsuario,
                        AutoId = idAuto
                    };

                    cuentaExistente.Dinero -= autoExistente.Precio;

                    await _appCarrosContext.AutosDeUsuarios.AddAsync(compra);
                    await _appCarrosContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return new ResultadoOperacion
                    {
                        Mensaje = "El auto ha sido comprado con exito",
                        Respuesta = true
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return new ResultadoOperacion
                    {
                        Mensaje = "Error al realizar la compra",
                        Respuesta = false
                    };
                }
            }
        }
    }
}
