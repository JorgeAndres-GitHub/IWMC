using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.Operaciones
{
    public class AutoDAO
    {
        private readonly AppCarrosContext _appCarrosContext;

        public AutoDAO(AppCarrosContext appCarrosContext)
        {
            _appCarrosContext = appCarrosContext;
        }

        public async Task<bool> AgregarAuto(string vehiculo, string versionVehiculo, decimal precio, string tipo)
        {
            try
            {
                Auto auto = new Auto();
                auto.Vehiculo=vehiculo;
                auto.VersionVehiculo=versionVehiculo;
                auto.Precio = precio;
                auto.Tipo=tipo;

                await _appCarrosContext.AddAsync(auto);
                await _appCarrosContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
