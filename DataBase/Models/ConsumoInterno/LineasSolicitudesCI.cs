using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("LineasSolicitudesCI", Schema = "ConsumoInterno")]
public partial class LineasSolicitudesCI
{
    [Key]
    public int id_linea_solicitud { get; set; }

    public int cabecera_solicitud_id { get; set; }

    [Required]
    [StringLength(100)]
    public string id_producto { get; set; }

    [Required]
    [StringLength(20)]
    public string no_producto { get; set; }

    [Required]
    [StringLength(100)]
    public string descripcion { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal precio_unitario { get; set; }

    public int cantidad { get; set; }

    [Required]
    [StringLength(100)]
    public string id_unidad_medida { get; set; }

    [Required]
    [StringLength(20)]
    public string codigo_unidad_medida { get; set; }

    [StringLength(100)]
    public string? almacen_id { get; set; }

    [StringLength(100)]
    public string? almacen_codigo { get; set; }

    public string? nota { get; set; }
    public decimal total { get; set; }

    [ForeignKey("cabecera_solicitud_id")]
    [InverseProperty("LineasSolicitudesCI")]
    public virtual CabeceraSolicitudesCI cabecera_solicitud { get; set; }
}
