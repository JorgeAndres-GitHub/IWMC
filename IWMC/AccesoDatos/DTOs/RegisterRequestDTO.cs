using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.DTOs
{
    public class RegisterRequestDTO
    {
        public string Nombre { get; set; } = null!;

        public string Apellidos { get; set; } = null!;

        public string Cedula { get; set; } = null!;

        public string Direccion { get; set; } = null!;

        public string Telefono { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Contrasenia { get; set; } = null!;
    }
}
