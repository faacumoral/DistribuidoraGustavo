using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Parsers;
using DistribuidoraGustavo.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Core.Services
{
    public class PriceListService : IPriceListService
    {
        private readonly DistribuidoraGustavoContext _context;
        private readonly IExcelService _excelService;

        public PriceListService(DistribuidoraGustavoContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        public async Task<IList<PriceListModel>> GetAll()
        {
            var priceListDb = await _context.PriceLists.ToListAsync();
            return priceListDb.Select(CastEfToModel.ToModel).ToList();
        }

        public async Task SaveImportedFile(string fileName, Stream stream)
        {
            var todayImportedFile = await _context.ImportedFiles.FirstOrDefaultAsync(f => f.DateTime.Date == DateTime.UtcNow.Date);

            if (todayImportedFile != null) {
                _context.ImportedFiles.Remove(todayImportedFile);
            }

            using MemoryStream memoryStream = new();
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                memoryStream.Write(buffer, 0, bytesRead);

            var fileContent =  memoryStream.ToArray();

            var base64File = Convert.ToBase64String(fileContent);

            var importedFile = new ImportedFile
            {
                FileName = fileName,
                DateTime = DateTime.UtcNow,
                FileContent = base64File
            };

            _context.ImportedFiles.Add(importedFile);

            await _context.SaveChangesAsync();

            if (_context.ImportedFiles.Count() > 5)
            { 
                var oldestFile = await _context.ImportedFiles.OrderBy(file => file.DateTime.Date).FirstAsync();
                _context.ImportedFiles.Remove(oldestFile);
            }
        }

        public async Task<IList<string>> ProcessFile(string fileName, Stream stream)
        {
            var errors = new List<string>();

            await SaveImportedFile(fileName, stream);

            var excelContent = _excelService.ProcessFile(stream);

            var codes = excelContent.Select(e => e.Code).ToList();

            var products = await _context
                .Products
                .Where(p => codes.Contains(p.Code))
                .Include(p => p.ProductsPriceLists).ToListAsync();

            var pricesLists = await _context.PriceLists.ToListAsync();

            foreach (var excelProduct in excelContent)
            {
                var product = products.FirstOrDefault(p => p.Code == excelProduct.Code);

                var priceCasted = decimal.TryParse(excelProduct.Price, out decimal basePrice);

                if (product == null)
                {
                    product = new Product
                    {
                        Active = true,
                        Code = excelProduct.Code,
                        Description = excelProduct.Name,
                        Name = excelProduct.Name,
                        BasePrice = basePrice
                    };
                    _context.Products.Add(product);
                }
                else
                {
                    product.Active = true;
                    product.Name = excelProduct.Name;
                    product.Description = excelProduct.Name;
                    product.BasePrice = basePrice;
                    _context.Products.Update(product);
                }

                if (priceCasted)
                {
                    CalculatePrices(product, pricesLists);
                }
                else
                {
                    errors.Add($"'{excelProduct.Price}' no es un precio válido");
                }
            }

            await _context.SaveChangesAsync();

            return errors;
        }


        public void CalculatePrices(Product product, List<PriceList> pricesLists)
        {
            foreach (var priceList in pricesLists)
            {
                var newPrice = product.BasePrice + (product.BasePrice * priceList.Percent / 100);

                var productPrice = product.ProductsPriceLists.FirstOrDefault(ppl => ppl.PriceListId == priceList.PriceListId);

                if (productPrice == null)
                {
                    productPrice = new ProductsPriceList
                    {
                        PriceListId = priceList.PriceListId,
                        Price = newPrice
                    };
                    product.ProductsPriceLists.Add(productPrice);
                }
                else
                {
                    productPrice.Price = newPrice;
                    _context.ProductsPriceLists.Update(productPrice);
                }
            }

        }
    }
}
