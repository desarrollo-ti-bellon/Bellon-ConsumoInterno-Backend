using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.ConsumoInterno.DataBase.Models;

[Table("UsuarioPerfil", Schema = "Seguridad")]
public class UsuarioPerfil
{
    [Key]
    public long id { get; set; }

    public long perfilId { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string usuario { get; set; } = null!;

    [ForeignKey("perfilId")]
    [InverseProperty("usuarioPerfil")]
    public virtual Perfil perfil { get; set; } = null!;
}
