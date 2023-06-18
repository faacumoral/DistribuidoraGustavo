namespace DistribuidoraGustavo.Interfaces.Models;

public class ProductInvoiceModel : ProductModel
{
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
