namespace Bellon.API.ConsumoInterno.Classes;

public class AppSettings
{
    public int AplicacionId { get; set; }
    public int DocumentoLLegadaNoSerieId { get; set; }
    public int DocumentoTransitoNoSerieId { get; set; }
    public int DocumentoLiquidacionNoSerieId { get; set; }
    public int DocumentoConsumoInternoNoSerieId { get; set; }
    public int CantidadDigitosDocumento { get; set; }
    public string LSCentralTokenUrl { get; set; }
    public string LSCentralTokenClientSecret { get; set; }
    public string LSCentralAPIsComunes { get; set; }
    public string LSCentralAPIsLiquidacion { get; set; }
    public string DataBaseConnection { get; set; }
}
