namespace DistribuidoraGustavo.Interfaces.Models;

public class PricedProductModel : ProductModel
{
    public IList<ProductPriceModel> Prices { get; set; }
}
