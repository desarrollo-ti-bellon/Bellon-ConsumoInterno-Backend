using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase;

[Table("PosicionesUsuariosCI", Schema = "ConsumoInterno")]
public partial class PosicionesUsuariosCI
{
    [Key]
    public int posicion_id { get; set; }

    [Required]
    [StringLength(50)]
    public string descripcion { get; set; }

    public bool? crear_solicitud { get; set; }

    public bool? enviar_solicitud { get; set; }

    public bool? aprobar_solicitud { get; set; }

    public bool? rechazar_solicitud { get; set; }

    public bool? confirmar_solicitud { get; set; }

    public bool? entregar_solicitud { get; set; }

    public bool? cancelar_solicitud { get; set; }

    [InverseProperty("posicion")]
    public virtual ICollection<UsuariosCI> UsuariosCI { get; set; } = new List<UsuariosCI>();
}
