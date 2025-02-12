using Bellon.API.ConsumoInterno.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.DataBase;

public partial class AppDataBase : DbContext
{

    public virtual DbSet<CabeceraSolicitudes> CabeceraSolicitudes { get; set; }

    public virtual DbSet<Clasificaciones> Clasificaciones { get; set; }

    public virtual DbSet<EstadosSolicitudes> EstadosSolicitudes { get; set; }

    public virtual DbSet<LineasSolicitudes> LineasSolicitudes { get; set; }

    public virtual DbSet<NoSeries> NoSeries { get; set; }

    public virtual DbSet<Notas> Notas { get; set; }

    public virtual DbSet<Aplicacion> Aplicacion { get; set; }

    public virtual DbSet<Perfil> Perfil { get; set; }

    public virtual DbSet<UsuarioPerfil> UsuarioPerfil { get; set; }

    public virtual DbSet<Posiciones> Posiciones { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<HistorialMovimientosSolicitudes> HistorialMovimientosSolicitudes { get; set; }

    private readonly Classes.AppSettings _settings;

    public AppDataBase(IOptions<Classes.AppSettings> settings)
    {
        _settings = settings.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_settings.DataBaseConnection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<CabeceraSolicitudes>(entity =>
         {
             entity.HasKey(e => e.id_cabecera_solicitud).HasName("PK_CabeceraSolicitudesActivas");

             entity.HasOne(d => d.id_clasificacionNavigation).WithMany(p => p.CabeceraSolicitudes)
                 .OnDelete(DeleteBehavior.ClientSetNull)
                 .HasConstraintName("FK_CabeceraSolicitudes_Clasificaciones");

             entity.HasOne(d => d.id_estado_solicitudNavigation).WithMany(p => p.CabeceraSolicitudes)
                 .OnDelete(DeleteBehavior.ClientSetNull)
                 .HasConstraintName("FK_CabeceraSolicitudes_EstadosSolicitudes");

             entity.HasOne(d => d.no_serie).WithMany(p => p.CabeceraSolicitudes)
                 .OnDelete(DeleteBehavior.ClientSetNull)
                 .HasConstraintName("FK_CabeceraSolicitudes_NoSeries");
         });

        modelBuilder.Entity<Clasificaciones>(entity =>
        {
            entity.HasKey(e => e.id_clasificacion).HasName("PK_ClasificacionesConsumoInterno");
        });

        modelBuilder.Entity<EstadosSolicitudes>(entity =>
        {
            entity.Property(e => e.id_estado_solicitud).ValueGeneratedNever();
        });

        modelBuilder.Entity<LineasSolicitudes>(entity =>
        {
            entity.HasKey(e => e.id_linea_solicitud).HasName("PK_LineasSolicitudesActivas");

            entity.HasOne(d => d.cabecera_solicitud).WithMany(p => p.LineasSolicitudes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineasSolicitudes_CabeceraSolicitudes1");
        });

        modelBuilder.Entity<NoSeries>(entity =>
        {
            entity.HasKey(e => e.id_no_serie).HasName("FP_NoSeries_PK");
        });

        modelBuilder.Entity<Perfil>(entity =>
        {
            entity.HasOne(d => d.aplicacion).WithMany(p => p.perfil).HasConstraintName("FK_Perfil_Aplicacion");
        });

        modelBuilder.Entity<Posiciones>(entity =>
        {
            entity.HasKey(e => e.posicion_id).HasName("PK_NewTable");
        });

        modelBuilder.Entity<UsuarioPerfil>(entity =>
        {
            entity.HasOne(d => d.perfil).WithMany(p => p.usuarioPerfil).HasConstraintName("FK_UsuarioPerfil_Perfil");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasOne(d => d.posicion).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Posiciones");
        });

        modelBuilder.Entity<HistorialMovimientosSolicitudes>(entity =>
       {
           entity.HasKey(e => e.id_hist_solicitud).HasName("PK__Historia__D655A71D4DC2C063");
       });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}