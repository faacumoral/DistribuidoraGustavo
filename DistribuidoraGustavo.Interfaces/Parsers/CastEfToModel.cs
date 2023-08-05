using DistribuidoraGustavo.Data.EfModels;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Shared;

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

        public static ClientModel ToModel(this Client client)
            => new()
            {
                Name = client.Name,
                ClientId = client.ClientId,
                DefaultPriceList = client.DefaultPriceList?.ToModel(),
                InvoicePrefix = client.InvoicePrefix,
                ActualBalance = client.ActualBalance,
            };

        public static ProductModel ToModel(this Product product)
            => new()
            {
                Code = product.Code,
                Description = product.Description,
                Name = product.Name,
                ProductId = product.ProductId,
                BasePrice = product.BasePrice,
                QuantityPerBox = product.QuantityPerBox
            };

        public static ProductInvoiceModel ToModel(this InvoicesProduct invoicesProducts)
            => new()
            {
                Code = invoicesProducts.Product?.Code,
                Description = invoicesProducts.Product?.Description,
                Name = invoicesProducts.Product?.Name,
                ProductId = invoicesProducts.Product?.ProductId ?? 0,
                Amount = invoicesProducts.Amount,
                Quantity = invoicesProducts.Quantity,
                UnitPrice = invoicesProducts.UnitPrice
            };

        public static InvoiceModel ToModel(this Invoice invoice)
            => new()
            {
                Client = invoice.Client?.ToModel(),
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                PriceList = invoice.PriceList?.ToModel(),
                Products = invoice.InvoicesProducts?.Select(ToModel).ToList(),
                CreatedDate = invoice.CreatedDate.DateToString(),
                Description = invoice.Description
            };


        public static TransactionModel ToModel(this Transaction transaction)
            => new()
            {
                Amount = transaction.Amount,
                Date = transaction.Date.DateToString(),
                TransactionId = transaction.TransactionId,
                Client = transaction.Client?.ToModel(),
                Description = transaction.Description,
                Type = transaction.Type,
            };
    }
}
