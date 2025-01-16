// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

namespace Bellon.API.Liquidacion.Classes;

[Serializable]
public partial class CabeceraLiquidacion
{
    [JsonPropertyName("id_cabecera_liquidacion")]
    public int? IdCabeceraLiquidacion { get; set; }

    [JsonPropertyName("no_serie_id")]
    public int? NoSerieId { get; set; }

    [JsonPropertyName("no_documento")]
    public string? NoDocumento { get; set; } = null!;

    [JsonPropertyName("fecha_documento")]
    public DateOnly FechaDocumento { get; set; }

    [JsonPropertyName("fecha_registro")]
    public DateOnly FechaRegistro { get; set; }

    [JsonPropertyName("nombre_proveedor")]
    public string NombreProveedor { get; set; } = null!;

    [JsonPropertyName("no_dua")]
    public string NoDua { get; set; } = null!;

    [JsonPropertyName("fecha_dua")]
    public DateOnly FechaDua { get; set; }

    [JsonPropertyName("dga_liquidacion")]
    public string DgaLiquidacion { get; set; } = null!;

    [JsonPropertyName("agente_id")]
    public int AgenteId { get; set; }

    [JsonPropertyName("detalle_mercancia")]
    public string DetalleMercancia { get; set; } = null!;

    [JsonPropertyName("no_conocimiento_embarque")]
    public string NoConocimientoEmbarque { get; set; } = null!;

    [JsonPropertyName("tasa_dolar")]
    public decimal TasaDolar { get; set; }

    [JsonPropertyName("total_gasto_manejo")]
    public decimal TotalGastoManejo { get; set; }

    [JsonPropertyName("monto_seguro")]
    public decimal MontoSeguro { get; set; }

    [JsonPropertyName("monto_flete")]
    public decimal MontoFlete { get; set; }

    [JsonPropertyName("monto_otros_gastos")]
    public decimal MontoOtrosGastos { get; set; }

    [JsonPropertyName("tasa_aduana")]
    public decimal TasaAduana { get; set; }

    [JsonPropertyName("monto_multa")]
    public decimal MontoMulta { get; set; }

    [JsonPropertyName("monto_articulo_52")]
    public decimal MontoArticulo52 { get; set; }

    [JsonPropertyName("monto_impuesto")]
    public decimal MontoImpuesto { get; set; }

    [JsonPropertyName("total_cif_general")]
    public decimal TotalCifGeneral { get; set; }

    [JsonPropertyName("total_gravamen_general")]
    public decimal TotalGravamenGeneral { get; set; }

    [JsonPropertyName("total_selectivo_general")]
    public decimal TotalSelectivoGeneral { get; set; }

    [JsonPropertyName("total_itbis_general")]
    public decimal TotalItbisGeneral { get; set; }

    [JsonPropertyName("fecha_creado")]
    public DateTime? FechaCreado { get; set; }

    [JsonPropertyName("creado_por")]
    public string? CreadoPor { get; set; } = null!;

    [JsonPropertyName("fecha_modificado")]
    public DateTime? FechaModificado { get; set; }

    [JsonPropertyName("modificado_por")]
    public string? ModificadoPor { get; set; }

    [JsonPropertyName("lineas")]
    public List<LineaLiquidacion>? Lineas { get; set; }

    [JsonPropertyName("cargos")]
    public List<CargoAdicional>? Cargos { get; set; }
}
