using DistribuidoraGustavo.Interfaces.Shared;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using System.Globalization;

namespace DistribuidoraGustavo.Core.Services
{
    public class PdfService : IPdfService
    {

        private const string INVOICE_HTML = @"HTML/InvoiceTemplate.html";

        private static byte[] GeneratePdfFromHtml(string html)
        {
            using MemoryStream ms = new();
            HtmlConverter.ConvertToPdf(html, ms);
            return ms.ToArray();
        }

        public async Task<byte[]> GenerateInvoicePdf(InvoiceModel invoice)
        {
            var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, INVOICE_HTML);
            var htmlTemplate = await File.ReadAllTextAsync(htmlPath);

            var productsHtml = "";

            foreach (var product in invoice.Products)
            {
                productsHtml += $@"
                    <tr>
                        <td>{product.Code}</td>
                        <td>{product.Name.ToShortString()}</td>
                        <td>{product.Quantity}</td>
                        <td>${product.UnitPrice}</td>
                        <td>${product.Amount}</td>
                    </tr>";
            }

            var argCulture = new CultureInfo("es-AR");

            htmlTemplate = htmlTemplate
                .ReplaceToken("InvoiceNumber", invoice.InvoiceNumber)
                .ReplaceToken("ClientName", invoice.Client.Name)
                .ReplaceToken("OldBalance", (invoice.Client.ActualBalance - invoice.TotalAmount).ToString("N0", argCulture))
                .ReplaceToken("TotalAmount", invoice.TotalAmount.ToString("N0", argCulture))
                .ReplaceToken("InvoiceProducts", productsHtml)
                .ReplaceToken("Date", DateTime.UtcNow.DateToString())
                .ReplaceToken("ActualBalance", invoice.Client.ActualBalance.ToString("N0", argCulture));

            return GeneratePdfFromHtml(htmlTemplate);
        }

    }
}
