using System;
using System.Collections.Generic;

namespace AccesoDatos.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Token { get; set; } = null!;

    public string JwtId { get; set; } = null!;

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime AddedDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
