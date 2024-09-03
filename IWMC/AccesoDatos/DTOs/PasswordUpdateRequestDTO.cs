using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.DTOs
{
    public class PasswordUpdateRequestDTO
    {
        public string Correo { get; set; }
        public string Password { get; set; }
        public string CodigoConfirmacion { get; set; }
    }
}
