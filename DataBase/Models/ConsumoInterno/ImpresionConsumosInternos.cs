using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Keyless]
public partial class ImpresionConsumosInternos
{
    [StringLength(100)]
    public string id_producto { get; set; }

    [StringLength(20)]
    public string no_producto { get; set; }

    [StringLength(20)]
    public string no_documento { get; set; }

    public DateTime fecha_creado { get; set; }

    [StringLength(100)]
    public string descripcion { get; set; }

    public int id_clasificacion { get; set; }

    public string clasificacion_descripcion { get; set; }

    [StringLength(100)]
    public string almacen_id { get; set; }

    [StringLength(100)]
    public string almacen_codigo { get; set; }

    public int? cantidad_total { get; set; }

    [Column(TypeName = "decimal(38, 0)")]
    public decimal? precio_unitario_total { get; set; }

    [Column(TypeName = "decimal(38, 0)")]
    public decimal? total { get; set; }
}
