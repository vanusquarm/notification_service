using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

using iTextSharp.text;
using iTextSharp.text.pdf;

public class StatementPdfGenerator : IStatementPdfGenerator
{
    private readonly string _downloadPath;
    private readonly string _logoPath;

    public StatementPdfGenerator()
    {
        _downloadPath = Path.Combine(Utility.MapPath("~/Downloads"), "Temp");
        Directory.CreateDirectory(_downloadPath);

        _logoPath = Path.Combine(HostingEnvironment.MapPath("~/Resources"), "glogo.jpg");
    }

    public async Task<string> GenerateAsync(
        IEnumerable<AccountStatementContext> accounts,
        DateTime startDate,
        DateTime endDate,
        string cifId)
    {
        string month = endDate.ToString("MMM", CultureInfo.CreateSpecificCulture("en-GB"));
        string outputFile = Path.Combine(
            _downloadPath,
            $"AC_{month}_{cifId}_Stmt_{Guid.NewGuid()}.pdf");

        using var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None);
        using var document = new Document(PageSize.A4, 10, 10, 10, 40);

        PdfWriter writer = PdfWriter.GetInstance(document, fs);
        writer.PageEvent = new CustomFooterEvent();

        document.AddAuthor("Appdev-GTBank GH");
        document.AddSubject("Customer Statement");
        document.Open();

        bool firstAccount = true;

        foreach (var ctx in accounts)
        {
            if (!firstAccount)
            {
                document.NewPage();
            }

            AddLogo(document);
            AddHeader(document, ctx, startDate, endDate);
            AddStatementTable(document, ctx);

            firstAccount = false;
        }

        document.Close();
        return outputFile;
    }

    // ----------------- Helpers -----------------

    private void AddLogo(Document document)
    {
        if (!File.Exists(_logoPath)) return;

        Image logo = Image.GetInstance(_logoPath);
        logo.ScaleToFit(50f, 50f);
        logo.Alignment = Element.ALIGN_RIGHT;
        logo.IndentationRight = 30;
        logo.SpacingAfter = 5f;

        document.Add(logo);
    }

    private void AddHeader(
        Document document,
        AccountStatementContext ctx,
        DateTime startDate,
        DateTime endDate)
    {
        string printDate = DateTime.UtcNow.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

        Paragraph period = new Paragraph(
            $"Statement Period: {startDate:dd-MMM-yyyy} to {endDate:dd-MMM-yyyy}",
            FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.BOLD))
        {
            IndentationLeft = 45,
            SpacingAfter = 5f
        };

        Paragraph title = new Paragraph(
            "CUSTOMER STATEMENT",
            FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.BOLD))
        {
            Alignment = Element.ALIGN_RIGHT,
            IndentationRight = 45
        };

        Paragraph name = new Paragraph(
            ctx.Account.AccountName.ToUpper(),
            FontFactory.GetFont(FontFactory.HELVETICA, 14, Font.BOLD, new BaseColor(255, 69, 0)))
        {
            Alignment = Element.ALIGN_RIGHT,
            IndentationRight = 45,
            SpacingAfter = 10f
        };

        PdfPTable header = new PdfPTable(2)
        {
            WidthPercentage = 40,
            HorizontalAlignment = Element.ALIGN_LEFT
        };
        header.SetWidths(new[] { 1f, 2f });

        AddHeaderRow(header, "Print Date", printDate);
        AddHeaderRow(header, "Branch Name", ctx.BranchName);
        AddHeaderRow(header, "Account No.", ctx.Account.AccountNumber);
        AddHeaderRow(header, "Address", ctx.Address);
        AddHeaderRow(header, "Account Type", ctx.ProductType);
        AddHeaderRow(header, "Currency", ctx.Currency);
        AddHeaderRow(header, "Opening Balance", ctx.OpeningBalance.ToString("N2"));
        AddHeaderRow(header, "Closing Balance", ctx.ClosingBalance.ToString("N2"));

        document.Add(period);
        document.Add(header);
        document.Add(title);
        document.Add(name);
    }

    private void AddHeaderRow(PdfPTable table, string label, string value)
    {
        Font font = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.BOLD);

        table.AddCell(new PdfPCell(new Phrase(label, font))
        {
            BackgroundColor = new BaseColor(245, 245, 245),
            Padding = 5
        });

        table.AddCell(new PdfPCell(new Phrase(value?.ToUpper() ?? string.Empty, font))
        {
            BackgroundColor = new BaseColor(245, 245, 245),
            Padding = 5
        });
    }

    private void AddStatementTable(Document document, AccountStatementContext ctx)
    {
        PdfPTable table = new PdfPTable(6)
        {
            WidthPercentage = 100,
            SpacingBefore = 10f
        };

        table.SetWidths(new[] { 12f, 12f, 10f, 10f, 10f, 16f });

        AddTableHeaders(table);

        if (ctx.Statement == null || ctx.Statement.Rows.Count == 0)
        {
            table.AddCell(new PdfPCell(new Phrase("No transactions found"))
            {
                Colspan = 6,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 10
            });
        }
        else
        {
            foreach (DataRow row in ctx.Statement.Rows)
            {
                table.AddCell(GetCell(row["TransactionDate"]));
                table.AddCell(GetCell(row["TransactionValueDate"]));
                table.AddCell(GetAmountCell(row["Debit"]));
                table.AddCell(GetAmountCell(row["Credit"]));
                table.AddCell(GetAmountCell(row["Balance"]));
                table.AddCell(GetCell(row["Remarks"]));
            }
        }

        document.Add(table);
    }

    private void AddTableHeaders(PdfPTable table)
    {
        string[] headers = { "Trans. Date", "Value Date", "Debits", "Credits", "Balance", "Remarks" };
        Font font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);

        foreach (var text in headers)
        {
            table.AddCell(new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = BaseColor.BLACK,
                Border = Rectangle.NO_BORDER,
                Padding = 5
            });
        }
    }

    private PdfPCell GetCell(object value)
    {
        string text = value == DBNull.Value ? "" : value.ToString();
        return new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA, 8)))
        {
            Padding = 5
        };
    }

    private PdfPCell GetAmountCell(object value)
    {
        decimal amount = value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        return GetCell(amount > 0 ? amount.ToString("N2") : "");
    }
}