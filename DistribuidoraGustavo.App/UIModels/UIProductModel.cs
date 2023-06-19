using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.App.UIModels;

public class UIProductModel : ProductModel
{
    public bool Checked { get; set; } = false;
    public decimal UnitPrice { get; set; }
}
