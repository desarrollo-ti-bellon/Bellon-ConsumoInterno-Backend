using Bellon.API.Liquidacion.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace Bellon.API.Liquidacion.DataBase;

public partial class AppDataBase : DbContext
{
    public virtual DbSet<Agentes> Agentes { get; set; }

    public virtual DbSet<CabeceraLiquidaciones> CabeceraLiquidaciones { get; set; }

    public virtual DbSet<CabeceraLlegadas> CabeceraLlegadas { get; set; }

    public virtual DbSet<CabeceraTransito> CabeceraTransito { get; set; }

    public virtual DbSet<CargosAdicionales> CargosAdicionales { get; set; }

    public virtual DbSet<CargosFacturaLiquidacion> CargoFacturaLiquidacion { get; set; }

    public virtual DbSet<CodigoArancelarios> CodigoArancelarios { get; set; }

    public virtual DbSet<HistCabeceraLiquidaciones> HistCabeceraLiquidaciones { get; set; }

    public virtual DbSet<HistCabeceraLlegadas> HistCabeceraLlegadas { get; set; }

    public virtual DbSet<HistCabeceraTransitos> HistCabeceraTransitos { get; set; }

    public virtual DbSet<HistCargosAdicionales> HistCargosAdicionales { get; set; }

    public virtual DbSet<HistLineaLiquidaciones> HistLineaLiquidaciones { get; set; }

    public virtual DbSet<HistLineaLlegadas> HistLineaLlegadas { get; set; }

    public virtual DbSet<HistLineaTransitos> HistLineaTransitos { get; set; }

    public virtual DbSet<LineaLiquidaciones> LineaLiquidaciones { get; set; }

    public virtual DbSet<LineaLlegadas> LineaLlegadas { get; set; }

    public virtual DbSet<LineaTransitos> LineaTransitos { get; set; }

    public virtual DbSet<NoSeries> NoSeries { get; set; }

    public virtual DbSet<Notas> Notas { get; set; }

    public virtual DbSet<TipoContenedores> TipoContenedores { get; set; }

    public virtual DbSet<OrdenesTransferencia> OrdenesTransferencia { get; set; }

    public virtual DbSet<Aplicacion> Aplicacion { get; set; }

    public virtual DbSet<Perfil> Perfil { get; set; }

    public virtual DbSet<UsuarioPerfil> UsuarioPerfil { get; set; }

    public virtual DbSet<RecepcionMercancia> RecepcionMercancia { get; set; }

    //CONSUMO INTERNO
    public virtual DbSet<CabeceraSolicitudes> CabeceraSolicitudes { get; set; }

    public virtual DbSet<EstadosSolicitudes> EstadosSolicitudes { get; set; }

    public virtual DbSet<LineasSolicitudes> LineasSolicitudes { get; set; }

    public virtual DbSet<Clasificaciones> Clasificaciones { get; set; }

    public virtual DbSet<Posiciones> Posiciones { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var connectionString = configuration.GetConnectionString("Database");
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecepcionMercancia>(eb =>
        {
            eb.HasNoKey();
        });

        modelBuilder.Entity<Agentes>(entity =>
        {
            entity.Property(e => e.estado).HasDefaultValue(true);
        });

        modelBuilder.Entity<CabeceraLiquidaciones>(entity =>
        {
            entity
                .HasOne(d => d.agente)
                .WithMany(p => p.CabeceraLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraLiquidaciones_Agentes");

            entity
                .HasOne(d => d.no_serie)
                .WithMany(p => p.CabeceraLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraLiquidaciones_NoSeries");
        });

        modelBuilder.Entity<CabeceraLlegadas>(entity =>
        {
            entity
                .HasOne(d => d.agente)
                .WithMany(p => p.CabeceraLlegadas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraLlegadas_Agentes");

            entity
                .HasOne(d => d.no_serie)
                .WithMany(p => p.CabeceraLlegadas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraLlegadas_NoSeries");
        });

        modelBuilder.Entity<CabeceraTransito>(entity =>
        {
            entity
                .HasOne(d => d.no_serie)
                .WithMany(p => p.CabeceraTransito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraTransito_NoSeries");

            entity
                .HasOne(d => d.tipo_contenedor)
                .WithMany(p => p.CabeceraTransito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CabeceraTransito_TipoContenedores");
        });

        modelBuilder.Entity<CargosFacturaLiquidacion>(entity =>
        {
            entity.Property(e => e.no_cargo_producto)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.no_proveedor)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.nombre_proveedor)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<CargosAdicionales>(entity =>
        {
            entity
                .HasOne(d => d.cabecera_liquidacion)
                .WithMany(p => p.CargosAdicionales)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CargosAdicionales_CabeceraLiquidaciones");
        });

        modelBuilder.Entity<HistCabeceraLiquidaciones>(entity =>
        {
            entity.Property(e => e.id_hist_cabecera_liquidacion).ValueGeneratedNever();

            entity
                .HasOne(d => d.agente)
                .WithMany(p => p.HistCabeceraLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistCabeceraLiquidaciones_Agentes");

            entity
                .HasOne(d => d.no_serie)
                .WithMany(p => p.HistCabeceraLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistCabeceraLiquidaciones_NoSeries");
        });

        modelBuilder.Entity<HistCabeceraLlegadas>(entity =>
        {
            entity.Property(e => e.id_hist_cabecera_llegada).ValueGeneratedNever();

            entity
                .HasOne(d => d.agente)
                .WithMany(p => p.HistCabeceraLlegadas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistCabeceraLlegadas_Agentes");

            entity
                .HasOne(d => d.no_serie)
                .WithMany(p => p.HistCabeceraLlegadas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistCabeceraLlegadas_NoSeries");
        });

        modelBuilder.Entity<HistCabeceraTransitos>(entity =>
        {
            entity.Property(e => e.id_hist_cabecera_transito).ValueGeneratedNever();

            entity
                .HasOne(d => d.no_serie)
                .WithMany(p => p.HistCabeceraTransitos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistCabeceraTransitos_NoSeries");

            entity
                .HasOne(d => d.tipo_contenedor)
                .WithMany(p => p.HistCabeceraTransitos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistCabeceraTransitos_TipoContenedores");
        });

        modelBuilder.Entity<HistCargosAdicionales>(entity =>
        {
            entity.Property(e => e.id_hist_cargo_adicional).ValueGeneratedNever();

            entity
                .HasOne(d => d.hist_cabecera_liquidacion)
                .WithMany(p => p.HistCargosAdicionales)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistCargosAdicionales_HistCabeceraLiquidaciones");
        });

        modelBuilder.Entity<HistLineaLiquidaciones>(entity =>
        {
            entity.Property(e => e.id_hist_linea_liquidacion).ValueGeneratedNever();

            entity
                .HasOne(d => d.codigo_arancelario)
                .WithMany(p => p.HistLineaLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistLineaLiquidaciones_CodigoArancelarios");

            entity
                .HasOne(d => d.hist_cabecera_liquidacion)
                .WithMany(p => p.HistLineaLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistLineaLiquidaciones_HistCabeceraLiquidaciones");

            entity
                .HasOne(d => d.hist_cabecera_transito)
                .WithMany(p => p.HistLineaLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistLineaLiquidaciones_HistCabeceraTransitos");
        });

        modelBuilder.Entity<HistLineaLlegadas>(entity =>
        {
            entity.Property(e => e.id_hist_linea_llegada).ValueGeneratedNever();

            entity
                .HasOne(d => d.hist_cabecera_llegada)
                .WithMany(p => p.HistLineaLlegadas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistLineaLlegadas_HistCabeceraLlegadas");
        });

        modelBuilder.Entity<HistLineaTransitos>(entity =>
        {
            entity.Property(e => e.id_hist_linea_transito).ValueGeneratedNever();

            entity
                .HasOne(d => d.hist_cabecera_llegada)
                .WithMany(p => p.HistLineaTransitos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistLineaTransitos_HistCabeceraLlegadas");

            entity
                .HasOne(d => d.hist_cabecera_transito)
                .WithMany(p => p.HistLineaTransitos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HistLineaTransitos_HistCabeceraTransitos");
        });

        modelBuilder.Entity<LineaLiquidaciones>(entity =>
        {
            entity
                .HasOne(d => d.cabecera_liquidacion)
                .WithMany(p => p.LineaLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineaLiquidaciones_CabeceraLiquidaciones");

            entity
                .HasOne(d => d.codigo_arancelario)
                .WithMany(p => p.LineaLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineaLiquidaciones_CodigoArancelarios");

            entity
                .HasOne(d => d.hist_cabecera_transito)
                .WithMany(p => p.LineaLiquidaciones)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineaLiquidaciones_HistCabeceraTransitos");
        });

        modelBuilder.Entity<LineaLlegadas>(entity =>
        {
            entity
                .HasOne(d => d.cabecera_llegada)
                .WithMany(p => p.LineaLlegadas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineaLlegadas_CabeceraLlegadas");
        });

        modelBuilder.Entity<LineaTransitos>(entity =>
        {
            entity
                .HasOne(d => d.cabecera_transito)
                .WithMany(p => p.LineaTransitos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineaTransitos_CabeceraTransito");

            entity
                .HasOne(d => d.hist_cabecera_llegada)
                .WithMany(p => p.LineaTransitos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineaTransitos_HistCabeceraLlegadas");
        });

        modelBuilder.Entity<OrdenesTransferencia>(entity =>
        {
            entity.Property(e => e.id_orden_transferencia).ValueGeneratedNever();

            entity
                .HasOne(d => d.hist_cabecera_transito)
                .WithMany(p => p.OrdenesTransferencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenesTransferencia_HistCabeceraTransitos");
        });

        modelBuilder.Entity<Perfil>(entity =>
        {
            entity
                .HasOne(d => d.aplicacion)
                .WithMany(p => p.perfil)
                .HasConstraintName("FK_Perfil_Aplicacion");
        });

        modelBuilder.Entity<TipoContenedores>(entity =>
        {
            entity.Property(e => e.estado).HasDefaultValue(true);
        });

        modelBuilder.Entity<UsuarioPerfil>(entity =>
        {
            entity
                .HasOne(d => d.perfil)
                .WithMany(p => p.usuarioPerfil)
                .HasConstraintName("FK_UsuarioPerfil_Perfil");
        });

        modelBuilder.Entity<Notas>(entity =>
        {
            entity.HasKey(e => e.id_nota).HasName("PK__Notas__26991D8CD3BDF6BB");
        });

        // CONSUMO  INTERNO
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

        modelBuilder.Entity<Posiciones>(entity =>
        {
            entity.HasKey(e => e.posicion_id).HasName("PK_NewTable");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.Property(e => e.codigo_departamento).IsFixedLength();
            entity.Property(e => e.codigo_sucursal).IsFixedLength();
            entity.Property(e => e.id_departamento).IsFixedLength();
            entity.Property(e => e.id_sucursal).IsFixedLength();

            // entity.HasOne(d => d.posicion).WithMany(p => p.Usuarios)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("FK_Usuarios_Posiciones");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
