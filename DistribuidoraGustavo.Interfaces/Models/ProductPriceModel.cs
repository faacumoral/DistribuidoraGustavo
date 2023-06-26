namespace DistribuidoraGustavo.Interfaces.Models;

public class ProductPriceModel : BaseModel
{
    public PriceListModel PriceListModel { get; set; }
    public decimal Price { get; set; }
}
