using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Cedula { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Contrasenia { get; set; } = null!;

    public bool? ConfirmedEmail { get; set; }

    public string? VerificationCode { get; set; }

    public string? Rol { get; set; }

    public string? ConfirmUpdateCode { get; set; }

    public virtual ICollection<AutosDeUsuario> AutosDeUsuarios { get; set; } = new List<AutosDeUsuario>();

    public virtual ICollection<CuentasBancaria> CuentasBancaria { get; set; } = new List<CuentasBancaria>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
