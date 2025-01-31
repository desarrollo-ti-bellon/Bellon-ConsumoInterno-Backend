using System.Text.Json.Serialization;

namespace Bellon.API.ConsumoInterno.Classes;

[Serializable]
public class LSCentralTransferencia
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("no")]
    public string No { get; set; }

    [JsonPropertyName("transferFromCode")]
    public string TransferFromCode { get; set; }

    [JsonPropertyName("transferToCode")]
    public string TransferToCode { get; set; }

    [JsonPropertyName("postingDate")]
    public string PostingDate { get; set; }

    [JsonPropertyName("shipmentDate")]
    public string ShipmentDate { get; set; }

    [JsonPropertyName("receiptDate")]
    public string ReceiptDate { get; set; }

    [JsonPropertyName("noSeries")]
    public string NoSeries { get; set; }

    [JsonPropertyName("transferFromContact")]
    public string TransferFromContact { get; set; }

    [JsonPropertyName("lscBuyerID")]
    public string LscBuyerId { get; set; }

    [JsonPropertyName("assignedUserID")]
    public string AssignedUserID { get; set; }

    [JsonPropertyName("inTransitCode")]
    public string InTransitCode { get; set; }

    [JsonPropertyName("transferLines")]
    public LSCentralTransferenciaLinea[] TransferLines { get; set; }
}
