using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class AutosDeUsuario
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int AutoId { get; set; }

    public virtual Auto Auto { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
