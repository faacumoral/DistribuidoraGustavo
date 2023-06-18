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

        public PriceListService(DistribuidoraGustavoContext context)
        {
            _context = context;
        }

        public async Task<IList<PriceListModel>> GetAll()
        {
            var priceListDb = await _context.PriceLists.ToListAsync();
            return priceListDb.Select(CastEfToModel.ToModel).ToList();
        }
    }
}
