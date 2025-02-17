using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("UsuariosCI", Schema = "ConsumoInterno")]
public partial class UsuariosCI
{
    [Key]
    public int? id_usuario_ci { get; set; }

    [Required]
    [StringLength(100)]
    public string id_usuario { get; set; }

    [Required]
    [StringLength(50)]
    public string nombre_usuario { get; set; }

    [Required]
    [StringLength(50)]
    public string correo { get; set; }

    [Required]
    [StringLength(100)]
    public string? codigo_sucursal { get; set; }

    [Required]
    [StringLength(100)]
    public string? id_sucursal { get; set; }

    [Required]
    [StringLength(100)]
    public string codigo_departamento { get; set; }

    [Required]
    [StringLength(100)]
    public string id_departamento { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal limite { get; set; }

    public int posicion_id { get; set; }

    public bool estado { get; set; }

    [StringLength(100)]
    public string? id_almacen { get; set; }

    [StringLength(100)]
    public string? codigo_almacen { get; set; }

    [InverseProperty("id_usuario_despachoNavigation")]
    public virtual ICollection<CabeceraSolicitudesCI> CabeceraSolicitudesCIid_usuario_despachoNavigation { get; set; } = new List<CabeceraSolicitudesCI>();

    [InverseProperty("id_usuario_responsableNavigation")]
    public virtual ICollection<CabeceraSolicitudesCI> CabeceraSolicitudesCIid_usuario_responsableNavigation { get; set; } = new List<CabeceraSolicitudesCI>();

    [InverseProperty("id_usuario_despachoNavigation")]
    public virtual ICollection<ConsumoInterno> ConsumoInternoid_usuario_despachoNavigation { get; set; } = new List<ConsumoInterno>();

    [InverseProperty("id_usuario_responsableNavigation")]
    public virtual ICollection<ConsumoInterno> ConsumoInternoid_usuario_responsableNavigation { get; set; } = new List<ConsumoInterno>();

    [ForeignKey("posicion_id")]
    [InverseProperty("UsuariosCI")]
    public virtual PosicionesUsuariosCI posicion { get; set; }
}
