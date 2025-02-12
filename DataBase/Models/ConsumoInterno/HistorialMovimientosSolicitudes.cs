using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("HistorialMovimientosSolicitudes", Schema = "ConsumoInterno")]
public partial class HistorialMovimientosSolicitudes
{
    [Key]
    public int id_hist_solicitud { get; set; }

    public int? id_cabecera_solicitud { get; set; }

    public int? no_serie_id { get; set; }

    [Required]
    [StringLength(20)]
    public string no_documento { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime fecha_creado { get; set; }

    [Required]
    [StringLength(100)]
    public string creado_por { get; set; }

    [Required]
    [StringLength(100)]
    public string usuario_responsable { get; set; }

    [StringLength(100)]
    public string usuario_despacho { get; set; }

    [StringLength(100)]
    public string usuario_asistente_inventario { get; set; }

    [StringLength(100)]
    public string usuario_asistente_contabilidad { get; set; }

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
    public string modificado_por { get; set; }

    public string comentario { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal total { get; set; }

    public int id_usuario_responsable { get; set; }

    public int? id_usuario_despacho { get; set; }

    public int? id_usuario_asistente_inventario { get; set; }

    public int? id_usuario_asistente_contabilidad { get; set; }
}
