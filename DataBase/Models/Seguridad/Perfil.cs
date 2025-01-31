using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase.Models;

[Table("Perfil", Schema = "Seguridad")]
public class Perfil
{
    [Key]
    public long id { get; set; }

    public long aplicacionId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string nombre { get; set; } = null!;

    [ForeignKey("aplicacionId")]
    [InverseProperty("perfil")]
    public virtual Aplicacion aplicacion { get; set; } = null!;

    [InverseProperty("perfil")]
    public virtual ICollection<UsuarioPerfil> usuarioPerfil { get; set; } =
        new List<UsuarioPerfil>();
}
