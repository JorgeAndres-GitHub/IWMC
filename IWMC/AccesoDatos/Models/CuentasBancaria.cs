using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class CuentasBancaria
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Banco { get; set; } = null!;

    public string TipoDeCuenta { get; set; } = null!;

    public string NumeroDeCuenta { get; set; } = null!;

    public string Cvv { get; set; } = null!;

    public decimal Dinero { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
