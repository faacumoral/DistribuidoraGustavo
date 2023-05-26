using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Parsers
{
    public static class CastEfToModel
    {
        public static PriceListModel ToModel(this PriceList priceList)
            => new()
            {
                Name = priceList.Name,
                PriceListId = priceList.PriceListId,
                Percent = priceList.Percent
            };

        public static UserModel ToModel(this User user)
            => new()
            {
                Active = user.Active,
                Name = user.Name,
                UserId = user.UserId,
                Username  = user.Username,
            };
    }
}
