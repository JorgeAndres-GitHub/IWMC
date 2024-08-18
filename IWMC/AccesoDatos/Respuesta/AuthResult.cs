using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.Respuesta
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefresToken { get; set; }
        public bool Respuesta { get; set; }
        public string Mensaje { get; set; }
    }
}
