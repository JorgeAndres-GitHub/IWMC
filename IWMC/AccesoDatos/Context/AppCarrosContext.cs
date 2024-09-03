using System;
using System.Collections.Generic;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Context;

public partial class AppCarrosContext : DbContext
{
    public AppCarrosContext()
    {
    }

    public AppCarrosContext(DbContextOptions<AppCarrosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auto> Autos { get; set; }

    public virtual DbSet<AutosDeUsuario> AutosDeUsuarios { get; set; }

    public virtual DbSet<CuentasBancaria> CuentasBancarias { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Autos__3213E83F0C076FE7");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(15, 4)")
                .HasColumnName("precio");
            entity.Property(e => e.Tipo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipo");
            entity.Property(e => e.Vehiculo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("vehiculo");
            entity.Property(e => e.VersionVehiculo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("versionVehiculo");
        });

        modelBuilder.Entity<AutosDeUsuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AutosDeU__3213E83FD74A92DB");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AutoId).HasColumnName("autoId");
            entity.Property(e => e.UsuarioId).HasColumnName("usuarioId");

            entity.HasOne(d => d.Auto).WithMany(p => p.AutosDeUsuarios)
                .HasForeignKey(d => d.AutoId)
                .HasConstraintName("FK__AutosDeUs__autoI__4222D4EF");

            entity.HasOne(d => d.Usuario).WithMany(p => p.AutosDeUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__AutosDeUs__usuar__412EB0B6");
        });

        modelBuilder.Entity<CuentasBancaria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CuentasB__3213E83F004FF55B");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Banco)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("banco");
            entity.Property(e => e.Cvv)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("cvv");
            entity.Property(e => e.Dinero)
                .HasColumnType("decimal(15, 4)")
                .HasColumnName("dinero");
            entity.Property(e => e.NumeroDeCuenta)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("numeroDeCuenta");
            entity.Property(e => e.TipoDeCuenta)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipoDeCuenta");
            entity.Property(e => e.UsuarioId).HasColumnName("usuarioId");

            entity.HasOne(d => d.Usuario).WithMany(p => p.CuentasBancaria)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__CuentasBa__usuar__49C3F6B7");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3213E83F3BB1F685");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedDate)
                .HasColumnType("datetime")
                .HasColumnName("addedDate");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiryDate");
            entity.Property(e => e.IsRevoked).HasColumnName("isRevoked");
            entity.Property(e => e.IsUsed).HasColumnName("isUsed");
            entity.Property(e => e.JwtId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("jwtId");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("token");
            entity.Property(e => e.UsuarioId).HasColumnName("usuarioId");

            entity.HasOne(d => d.Usuario).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__RefreshTo__usuar__46E78A0C");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83F2D4FFA6C");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidos");
            entity.Property(e => e.Cedula)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("cedula");
            entity.Property(e => e.ConfirmUpdateCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("confirmUpdateCode");
            entity.Property(e => e.ConfirmedEmail)
                .HasDefaultValue(false)
                .HasColumnName("confirmedEmail");
            entity.Property(e => e.Contrasenia)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasenia");
            entity.Property(e => e.Direccion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Usuario")
                .HasColumnName("rol");
            entity.Property(e => e.Telefono)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.VerificationCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("verificationCode");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
