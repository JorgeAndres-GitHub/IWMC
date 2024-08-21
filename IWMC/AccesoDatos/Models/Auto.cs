using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class Auto
{
    public int Id { get; set; }

    public string Vehiculo { get; set; } = null!;

    public string VersionVehiculo { get; set; } = null!;

    public decimal Precio { get; set; }

    public string Tipo { get; set; } = null!;

    public virtual ICollection<AutosDeUsuario>? AutosDeUsuarios { get; set; } = new List<AutosDeUsuario>();
}
