using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("LineasConsumosInternos", Schema = "ConsumoInterno")]
public partial class LineasConsumosInternos
{
    [Key]
    public int id_linea_consumo_interno { get; set; }

    public int cabecera_consumo_interno_id { get; set; }

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
    public string almacen_id { get; set; }

    [StringLength(100)]
    public string almacen_codigo { get; set; }

    public string nota { get; set; }
    public decimal total { get; set; }

    [ForeignKey("cabecera_consumo_interno_id")]
    [InverseProperty("LineasConsumosInternos")]
    public virtual CabeceraConsumosInternos cabecera_consumo_interno { get; set; }

}
