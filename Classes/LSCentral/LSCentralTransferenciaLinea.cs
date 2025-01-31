using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralTransferenciaLinea
{
    [JsonPropertyName("lineNo")]
    public long LineNo { get; set; }

    [JsonPropertyName("itemNo")]
    public string ItemNo { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("qtyToShip")]
    public decimal QtyToShip { get; set; }

    [JsonPropertyName("unitOfMeasureCode")]
    public string UnitOfMeasureCode { get; set; }

    [JsonPropertyName("shipmentDate")]
    public string ShipmentDate { get; set; }

    [JsonPropertyName("receiptDate")]
    public string ReceiptDate { get; set; }
}
