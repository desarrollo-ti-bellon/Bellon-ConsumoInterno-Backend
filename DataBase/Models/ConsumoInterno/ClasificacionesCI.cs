using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("ClasificacionesCI", Schema = "ConsumoInterno")]
public partial class ClasificacionesCI
{
    [Key]
    public int id_clasificacion { get; set; }

    [Required]
    [StringLength(100)]
    public string id_grupo_cont_producto_general { get; set; }

    [Required]
    [StringLength(50)]
    public string codigo_clasificacion { get; set; }

    [Required]
    public string descripcion { get; set; }

    public bool estado { get; set; }

    [InverseProperty("id_clasificacionNavigation")]
    public virtual ICollection<CabeceraSolicitudesCI> CabeceraSolicitudesCI { get; set; } = new List<CabeceraSolicitudesCI>();

    [InverseProperty("id_clasificacionNavigation")]
    public virtual ICollection<ConsumoInterno> ConsumoInterno { get; set; } = new List<ConsumoInterno>();
}
