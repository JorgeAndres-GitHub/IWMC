using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.DTOs
{
    public class AutoRequestDTO
    {
        public string Vehiculo { get; set; }
        public string VersionVehiculo { get; set; }
        public decimal Precio { get; set; }
        public string Tipo { get; set; }
    }
}
