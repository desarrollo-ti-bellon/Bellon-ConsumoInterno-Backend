using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase.Models;

[Table("Aplicacion", Schema = "Seguridad")]
public class Aplicacion
{
    [Key]
    public long id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? nombre { get; set; }

    [InverseProperty("aplicacion")]
    public virtual ICollection<Perfil> perfil { get; set; } = new List<Perfil>();
}
