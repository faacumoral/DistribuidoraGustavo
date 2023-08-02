namespace DistribuidoraGustavo.Interfaces.Models;

public class ClientModel : BaseModel
{
    public int ClientId { get; set; }
    public PriceListModel DefaultPriceList { get; set; }
    public string Name { get; set; }
    public string InvoicePrefix { get; set; }
    public decimal ActualBalance { get; set; }

}
