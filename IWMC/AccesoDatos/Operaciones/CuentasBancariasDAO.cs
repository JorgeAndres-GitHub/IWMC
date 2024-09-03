using AccesoDatos.Context;
using AccesoDatos.DTOs;
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
    public class CuentasBancariasDAO
    {
        private readonly AppCarrosContext _appCarrosContext;
        private readonly string[] bancosColombia =
        {
            "Bancolombia",
            "Davivienda",
            "Banco de Bogotá",
            "Banco Popular",
            "Banco de Occidente",
            "Banco BBVA Colombia",
            "Banco Agrario de Colombia",
            "Banco AV Villas",
            "Scotiabank Colpatria",
            "Itaú Colombia",
            "Banco Caja Social",
            "Banco GNB Sudameris",
            "Banco W",
            "Banco Pichincha",
            "Bancoomeva",
            "Finandina",
            "Banco Falabella",
            "Banco Santander de Negocios Colombia",
            "Banco Finandina",
            "Banco ProCredit"
        };

        public CuentasBancariasDAO(AppCarrosContext appCarrosContext)
        {
            _appCarrosContext = appCarrosContext;
        }

        public async Task<ResultadoOperacion> PonerCuentaBancaria(BancoRequestDTO bancoRequestDTO, string id)
        {
            try
            {
                bool esValido = false;
                foreach (var banco in bancosColombia)
                {
                    if (banco.Equals(bancoRequestDTO.Banco, StringComparison.OrdinalIgnoreCase))
                    {
                        esValido = true;
                        break;
                    }
                }

                if (!esValido) return new ResultadoOperacion
                {
                    Mensaje = "Banco no valido",
                    Respuesta = false
                };

                if (!bancoRequestDTO.TipoDeCuenta.Equals("ahorros", StringComparison.OrdinalIgnoreCase) && !bancoRequestDTO.TipoDeCuenta.Equals("corriente", StringComparison.OrdinalIgnoreCase))
                    return new ResultadoOperacion { 
                        Mensaje="Tipo de cuenta no valido",
                        Respuesta=false
                    };

                var numeroDeCuenta = await _appCarrosContext.CuentasBancarias.FirstOrDefaultAsync(cb => cb.NumeroDeCuenta.Equals(bancoRequestDTO.NumeroDeCuenta));
                if (bancoRequestDTO.NumeroDeCuenta == null) return new ResultadoOperacion
                {
                    Mensaje="Numero de cuenta ya existente",
                    Respuesta=false
                };

                if (bancoRequestDTO.Cvv.Length < 3 || bancoRequestDTO.Cvv.Length > 4) return new ResultadoOperacion
                {
                    Mensaje="Longitud de CVV invalida",
                    Respuesta=false
                };

                int idInt = int.Parse(id);

                var CuentaBanco = new CuentasBancaria
                {
                    UsuarioId = idInt,
                    Banco = bancoRequestDTO.Banco,
                    TipoDeCuenta = bancoRequestDTO.TipoDeCuenta,
                    NumeroDeCuenta = bancoRequestDTO.NumeroDeCuenta,
                    Cvv = bancoRequestDTO.Cvv,
                    Dinero = bancoRequestDTO.Dinero
                };

                await _appCarrosContext.CuentasBancarias.AddAsync(CuentaBanco);
                await _appCarrosContext.SaveChangesAsync();

                return new ResultadoOperacion
                {
                    Mensaje="Operacion realizada con exito",
                    Respuesta=true
                };
            }
            catch (Exception)
            {
                return new ResultadoOperacion
                {
                    Mensaje="Error al realizar la operacion",
                    Respuesta=false
                };
            }
        }

        public async Task<ResultadoOperacion> AgregarDinero(int idUsuario, string numeroDeCuenta, decimal dinero)
        {
            try
            {
                var cuentaBancaria = await _appCarrosContext.CuentasBancarias.Where(cb => cb.UsuarioId == idUsuario && cb.NumeroDeCuenta.Equals(numeroDeCuenta)).FirstOrDefaultAsync();
                if (cuentaBancaria == null) return new ResultadoOperacion{
                    Mensaje = "Usuario no encontrado",
                    Respuesta = false
                };
                cuentaBancaria.Dinero += dinero;
                await _appCarrosContext.SaveChangesAsync();
                return new ResultadoOperacion
                {
                    Mensaje = "Dinero actualizado con exito",
                    Respuesta = true
                };
            }
            catch (Exception)
            {
                return new ResultadoOperacion
                {
                    Mensaje = "Error al agregar dinero a la cuenta",
                    Respuesta = false
                };
            }
        }

        public async Task<IEnumerable<CuentasBancaria>> ObtenerCuentas(int idUsuario)
        {
            var cuentas = await _appCarrosContext.CuentasBancarias.Where(cb => cb.UsuarioId == idUsuario).ToListAsync();
            if (cuentas.Count == 0) return null;
            return cuentas;
        }
    }
}
