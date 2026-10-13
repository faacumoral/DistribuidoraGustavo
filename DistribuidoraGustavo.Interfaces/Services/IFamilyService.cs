using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Results;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IFamilyService : IBaseService
{
    Task<IList<FamilyModel>> GetAll(string filter = "");
    Task<FamilyModel> GetById(int familyId);
    Task<DTOResult<FamilyModel>> Upsert(FamilyModel model);
    Task<BoolResult> Delete(int familyId);
}
