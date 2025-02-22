using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("CabeceraSolicitudesCI", Schema = "ConsumoInterno")]
public partial class CabeceraSolicitudesCI
{
    [Key]
    public int id_cabecera_solicitud { get; set; }

    public int? no_serie_id { get; set; }

    [Required]
    [StringLength(20)]
    public string? no_documento { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime fecha_creado { get; set; }

    [Required]
    [StringLength(100)]
    public string creado_por { get; set; }

    [Required]
    [StringLength(100)]
    public string usuario_responsable { get; set; }

    [Required]
    [StringLength(100)]
    public string? usuario_despacho { get; set; }

    [Required]
    [StringLength(100)]
    public string id_departamento { get; set; }

    public int id_estado_solicitud { get; set; }

    public int id_clasificacion { get; set; }

    [Required]
    [StringLength(100)]
    public string id_sucursal { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? fecha_modificado { get; set; }

    [StringLength(50)]
    public string? modificado_por { get; set; }

    [Required]
    public string comentario { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal total { get; set; }

    public int id_usuario_responsable { get; set; }

    public int? id_usuario_despacho { get; set; }

    public string? nombre_creado_por { get; set; }

    [InverseProperty("cabecera_solicitud")]
    public virtual ICollection<LineasSolicitudesCI> LineasSolicitudesCI { get; set; } = new List<LineasSolicitudesCI>();

    [ForeignKey("id_clasificacion")]
    [InverseProperty("CabeceraSolicitudesCI")]
    public virtual ClasificacionesCI id_clasificacionNavigation { get; set; }

    [ForeignKey("id_estado_solicitud")]
    [InverseProperty("CabeceraSolicitudesCI")]
    public virtual EstadosSolicitudesCI id_estado_solicitudNavigation { get; set; }

    [ForeignKey("id_usuario_despacho")]
    [InverseProperty("CabeceraSolicitudesCIid_usuario_despachoNavigation")]
    public virtual UsuariosCI id_usuario_despachoNavigation { get; set; }

    [ForeignKey("id_usuario_responsable")]
    [InverseProperty("CabeceraSolicitudesCIid_usuario_responsableNavigation")]
    public virtual UsuariosCI id_usuario_responsableNavigation { get; set; }

    [ForeignKey("no_serie_id")]
    [InverseProperty("CabeceraSolicitudesCI")]
    public virtual NoSeries no_serie { get; set; }
}
