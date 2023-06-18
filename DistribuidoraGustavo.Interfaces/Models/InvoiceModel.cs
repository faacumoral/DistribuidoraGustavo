namespace DistribuidoraGustavo.Interfaces.Models;

public class InvoiceModel : BaseModel
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public ClientModel Client { get; set; }
    public PriceListModel PriceList { get; set; }
    public IList<ProductInvoiceModel> Products { get; set; }
    public string Description { get; set; }
    public string CreatedDate { get; set; }

    public decimal TotalAmount => Products?.Sum(p => p.Amount) ?? 0;
}
