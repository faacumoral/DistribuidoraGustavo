namespace DistribuidoraGustavo.Interfaces.Models;

public class ProductModel : BaseModel
{
    public int ProductId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal BasePrice { get; set; }
    public int QuantityPerBox { get; set; }
}
