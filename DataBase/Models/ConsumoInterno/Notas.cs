using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("Notas", Schema = "ConsumoInterno")]
public partial class Notas
{
    [Key]
    public int id_nota { get; set; }

    public int id_documento { get; set; }

    [Required]
    [StringLength(5)]
    public string tipo_documento { get; set; }

    [Required]
    [StringLength(20)]
    public string no_documento { get; set; }

    [StringLength(100)]
    public string usuario_destino { get; set; }

    [Required]
    [StringLength(150)]
    public string descripcion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime fecha_creado { get; set; }

    [Required]
    [StringLength(100)]
    public string creado_por { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? fecha_modificado { get; set; }

    [StringLength(100)]
    public string modificado_por { get; set; }
}