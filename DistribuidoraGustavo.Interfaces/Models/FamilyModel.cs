namespace DistribuidoraGustavo.Interfaces.Models;

public class FamilyModel : BaseModel
{
    public int FamilyId { get; set; }
    public string Name { get; set; }
    public string CodeSuffix { get; set; }
    public int ProductsCount { get; set; }
}
