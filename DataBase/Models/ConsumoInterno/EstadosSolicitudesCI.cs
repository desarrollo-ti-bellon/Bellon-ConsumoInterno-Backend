using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("EstadosSolicitudesCI", Schema = "ConsumoInterno")]
public partial class EstadosSolicitudesCI
{
    [Key]
    public int id_estado_solicitud { get; set; }

    [Required]
    [StringLength(50)]
    public string descripcion { get; set; }

    public bool estado { get; set; }

    [InverseProperty("id_estado_solicitudNavigation")]
    public virtual ICollection<CabeceraConsumosInternos> CabeceraConsumosInternos { get; set; } = new List<CabeceraConsumosInternos>();

    [InverseProperty("id_estado_solicitudNavigation")]
    public virtual ICollection<CabeceraSolicitudesCI> CabeceraSolicitudesCI { get; set; } = new List<CabeceraSolicitudesCI>();
}
