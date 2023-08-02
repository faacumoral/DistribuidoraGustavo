using DistribuidoraGustavo.Interfaces.Shared;
using DistribuidoraGustavo.Interfaces.Models;
using DistribuidoraGustavo.Interfaces.Services;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;

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

            htmlTemplate = htmlTemplate
                .ReplaceToken("InvoiceNumber", invoice.InvoiceNumber)
                .ReplaceToken("ClientName", invoice.Client.Name)
                .ReplaceToken("TotalAmount", invoice.TotalAmount.ToString())
                .ReplaceToken("InvoiceProducts", productsHtml);

            return GeneratePdfFromHtml(htmlTemplate);
        }

    }
}
