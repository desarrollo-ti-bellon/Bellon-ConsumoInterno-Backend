using Bellon.API.ConsumoInterno.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bellon.API.ConsumoInterno.DataBase;

public partial class AppDataBase : DbContext
{

    public virtual DbSet<Aplicacion> Aplicacion { get; set; }

    public virtual DbSet<CabeceraSolicitudesCI> CabeceraSolicitudesCI { get; set; }

    public virtual DbSet<ClasificacionesCI> ClasificacionesCI { get; set; }

    public virtual DbSet<ConsumoInterno> ConsumoInterno { get; set; }

    public virtual DbSet<EstadosSolicitudesCI> EstadosSolicitudesCI { get; set; }

    public virtual DbSet<HistorialMovimientosSolicitudesCI> HistorialMovimientosSolicitudesCI { get; set; }

    public virtual DbSet<LineasConsumoInterno> LineasConsumoInterno { get; set; }

    public virtual DbSet<LineasMovimientosSolicitudesCI> LineasMovimientosSolicitudesCI { get; set; }

    public virtual DbSet<LineasSolicitudesCI> LineasSolicitudesCI { get; set; }

    public virtual DbSet<NoSeries> NoSeries { get; set; }

    public virtual DbSet<Notas> Notas { get; set; }

    public virtual DbSet<Perfil> Perfil { get; set; }

    public virtual DbSet<PosicionesUsuariosCI> PosicionesUsuariosCI { get; set; }

    public virtual DbSet<UsuarioPerfil> UsuarioPerfil { get; set; }

    public virtual DbSet<UsuariosCI> UsuariosCI { get; set; }
    
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
        modelBuilder.Entity<CabeceraSolicitudesCI>(entity =>
        {
            entity.HasKey(e => e.id_cabecera_solicitud).HasName("PK_CabeceraSolicitudesActivas");

            entity.HasOne(d => d.id_clasificacionNavigation).WithMany(p => p.CabeceraSolicitudesCI)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraSolicitudes_Clasificaciones");

            entity.HasOne(d => d.id_estado_solicitudNavigation).WithMany(p => p.CabeceraSolicitudesCI)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraSolicitudes_EstadosSolicitudes");

            entity.HasOne(d => d.id_usuario_despachoNavigation).WithMany(p => p.CabeceraSolicitudesCIid_usuario_despachoNavigation).HasConstraintName("FK__CabeceraS__id_us__6A1BB7B0");

            entity.HasOne(d => d.id_usuario_responsableNavigation).WithMany(p => p.CabeceraSolicitudesCIid_usuario_responsableNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CabeceraS__id_us__69279377");

            entity.HasOne(d => d.no_serie).WithMany(p => p.CabeceraSolicitudesCI)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraSolicitudes_NoSeries");
        }
        );

        modelBuilder.Entity<ClasificacionesCI>(entity =>
        {
            entity.HasKey(e => e.id_clasificacion).HasName("PK_ClasificacionesConsumoInterno");
        });

        modelBuilder.Entity<ConsumoInterno>(entity =>
        {
            entity.HasKey(e => e.id_cabecera_consumo_interno).HasName("PK__Cabecera__79B996641EAD6DEE");

            entity.HasOne(d => d.id_clasificacionNavigation).WithMany(p => p.ConsumoInterno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CabeceraS__id_cl__2EC5E7B8");

            entity.HasOne(d => d.id_estado_solicitudNavigation).WithMany(p => p.ConsumoInterno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CabeceraS__id_es__2DD1C37F");

            entity.HasOne(d => d.id_usuario_despachoNavigation).WithMany(p => p.ConsumoInternoid_usuario_despachoNavigation).HasConstraintName("FK__CabeceraS__id_us__30AE302A");

            entity.HasOne(d => d.id_usuario_responsableNavigation).WithMany(p => p.ConsumoInternoid_usuario_responsableNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CabeceraS__id_us__2FBA0BF1");

            entity.HasOne(d => d.no_serie).WithMany(p => p.ConsumoInterno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CabeceraS__no_se__2CDD9F46");
        });

        modelBuilder.Entity<EstadosSolicitudesCI>(entity =>
        {
            entity.HasKey(e => e.id_estado_solicitud).HasName("PK_EstadosSolicitudes");

            entity.Property(e => e.id_estado_solicitud).ValueGeneratedNever();
        });

        modelBuilder.Entity<HistorialMovimientosSolicitudesCI>(entity =>
        {
            entity.HasKey(e => e.id_hist_mov_solicitud).HasName("PK__Historia__D655A71D4DC2C063");
        });

        modelBuilder.Entity<LineasConsumoInterno>(entity =>
        {
            entity.HasKey(e => e.id_linea_consumo_interno).HasName("PK__LineasSo__A0E9C7BCE52F5EAC");

            entity.HasOne(d => d.cabecera_consumo_interno).WithMany(p => p.LineasConsumoInterno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LineasCon__cabec__3EFC4F81");
        });

        modelBuilder.Entity<LineasMovimientosSolicitudesCI>(entity =>
        {
            entity.HasKey(e => e.id_linea_hist_mov_solicitud).HasName("PK__LineasSo__A0E9C7BC331C8C50");

            entity.HasOne(d => d.cabecera_hist_mov_solicitud).WithMany(p => p.LineasMovimientosSolicitudesCI)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LineasMov__cabec__2818EA29");
        });

        modelBuilder.Entity<LineasSolicitudesCI>(entity =>
        {
            entity.HasKey(e => e.id_linea_solicitud).HasName("PK_LineasSolicitudesActivas");

            entity.HasOne(d => d.cabecera_solicitud).WithMany(p => p.LineasSolicitudesCI)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineasSolicitudes_CabeceraSolicitudes1");
        });

        modelBuilder.Entity<NoSeries>(entity =>
        {
            entity.HasKey(e => e.id_no_serie).HasName("FP_NoSeries_PK");
        });

        modelBuilder.Entity<PosicionesUsuariosCI>(entity =>
        {
            entity.HasKey(e => e.posicion_id).HasName("PK_NewTable");
        });

        modelBuilder.Entity<UsuariosCI>(entity =>
        {
            entity.HasKey(e => e.id_usuario_ci).HasName("PK_Usuarios");

            entity.HasOne(d => d.posicion).WithMany(p => p.UsuariosCI)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Posiciones");
        });

        modelBuilder.Entity<UsuarioPerfil>(entity =>
        {
            entity.HasOne(d => d.perfil).WithMany(p => p.usuarioPerfil).HasConstraintName("FK_UsuarioPerfil_Perfil");
        });

        modelBuilder.Entity<Perfil>(entity =>
        {
            entity.HasOne(d => d.aplicacion).WithMany(p => p.perfil).HasConstraintName("FK_Perfil_Aplicacion");
        });

        modelBuilder.Entity<NoSeries>(entity =>
        {
            entity.HasKey(e => e.id_no_serie).HasName("FP_NoSeries_PK");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}