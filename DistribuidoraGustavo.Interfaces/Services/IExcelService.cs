using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Services;

public interface IExcelService : IBaseService
{
    IList<ExcelProductModel> ProcessFile(Stream fs);
}
