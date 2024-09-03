using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.DTOs
{
    public class BancoRequestDTO
    {
        public string Banco { get; set; }
        public string TipoDeCuenta { get; set; }
        public string NumeroDeCuenta { get; set; }
        public string Cvv { get; set; }
        public decimal Dinero { get; set; }
    }
}
