using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using ExcelDataReader;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using System.ComponentModel;
using System.Data;
using System.IO;


namespace DistribuidoraGustavo.Core.Services
{
    public class ExcelService : IExcelService
    {
        public IList<ExcelProductModel> ProcessFile(Stream fs)
        {
            var products = new List<ExcelProductModel>();
            using (var reader = ExcelReaderFactory.CreateReader(fs))
            {
                DataSet result = reader.AsDataSet();

                DataTable table = result.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    var excelProduct = new ExcelProductModel
                    {
                        Code = row.ItemArray[0]?.ToString(),
                        Name = row.ItemArray[1]?.ToString(),
                        UnitType = row.ItemArray[2]?.ToString(),
                        Quantity = row.ItemArray[3]?.ToString(),
                        Price = row.ItemArray[4]?.ToString()
                    };
                    products.Add(excelProduct);
                }
            }
            return products;
        }
    }
}
