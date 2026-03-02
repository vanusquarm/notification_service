using GTBStatementService.Models;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;


public class StatementPdfGenerator
{
    private readonly string _downloadPath;
    private readonly string _logoPath;

    public StatementPdfGenerator()
    {
        //_downloadPath = Path.Combine(Utility.MapPath("~/Downloads"), "Temp");
        //Directory.CreateDirectory(_downloadPath);

        //_logoPath = Path.Combine(HostingEnvironment.MapPath("~/Resources"), "glogo.jpg");
    }

    public async Task<string> GenerateAsync(
        IEnumerable<BankStatement> accounts,
        DateTime startDate,
        DateTime endDate,
        string cifId)
    {
        string month = endDate.ToString("MMM", CultureInfo.CreateSpecificCulture("en-GB"));
        string outputFile = Path.Combine(
            _downloadPath,
            $"AC_{month}_{cifId}_Stmt_{Guid.NewGuid()}.pdf");

        using var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new PdfWriter(fs);
        using var pdfDoc = new PdfDocument(writer);
        using var document = new Document(pdfDoc);

        pdfDoc.GetDocumentInfo().SetAuthor("Appdev-GTBank GH");
        pdfDoc.GetDocumentInfo().SetSubject("Customer Statement");

        //pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomFooterEvent());

        bool firstAccount = true;

        foreach (var ctx in accounts)
        {
            if (!firstAccount)
            {
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));  // Adds new page
            }

            AddLogo(document);
            AddHeader(document, ctx, startDate, endDate);
            AddStatementTable(document, ctx);

            firstAccount = false;
        }

        document.Close();
        return outputFile;
    }

    private void AddLogo(Document document)
    {
        if (!File.Exists(_logoPath)) return;

        var logo = new Image(ImageDataFactory.Create(_logoPath));
        logo.ScaleToFit(50f, 50f);
        logo.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
        logo.SetFixedPosition(450, 800);  // Adjust positioning as needed
        document.Add(logo);
    }

    private void AddHeader(Document document, BankStatement ctx, DateTime startDate, DateTime endDate)
    {
        var boldFont = PdfFontFactory.CreateFont("Helvetica-Bold");
        string printDate = DateTime.UtcNow.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

        var period = new Paragraph($"Statement Period: {startDate:dd-MMM-yyyy} to {endDate:dd-MMM-yyyy}")
            .SetFont(PdfFontFactory.CreateFont("HELVETICA"))
            .SetFontSize(8)
            .SetFont(boldFont);
            //.SetMarginLeft(45)
            //.SetMarginBottom(5);

        var title = new Paragraph("CUSTOMER STATEMENT")
            .SetFont(PdfFontFactory.CreateFont("HELVETICA"))
            .SetFontSize(12)
            .SetFont(boldFont);
            //.SetTextAlignment(TextAlignment.RIGHT)
            //.SetMarginRight(45);

        var name = new Paragraph(ctx.AccountName.ToUpper())
            .SetFont(PdfFontFactory.CreateFont("HELVETICA"))
            .SetFontSize(14)
            .SetFont(boldFont);
            //.SetTextColor(255, 69, 0)
            //.SetTextAlignment(TextAlignment.RIGHT)
            //.SetMarginRight(45)
            //.SetMarginBottom(10);

        var header = new Table(2)
            .SetWidth(100);
        header.AddCell("Print Date").AddCell(printDate);
        header.AddCell("Branch Name").AddCell(ctx.BranchName);
        header.AddCell("Account No.").AddCell(ctx.AccountNumber);
        header.AddCell("Address").AddCell(ctx.Address);
        header.AddCell("Account Type").AddCell(ctx.AccountType);
        header.AddCell("Currency").AddCell(ctx.Currency);
        header.AddCell("Opening Balance").AddCell(ctx.OpeningBalance.ToString("N2"));
        header.AddCell("Closing Balance").AddCell(ctx.ClosingBalance.ToString("N2"));

        document.Add(period);
        document.Add(header);
        document.Add(title);
        document.Add(name);
    }

    private void AddStatementTable(Document document, BankStatement ctx)
    {
        var table = new Table(6)
            .SetWidth(100)
            .SetMarginTop(10);

        table.AddHeaderCell("Trans. Date");
        table.AddHeaderCell("Value Date");
        table.AddHeaderCell("Debits");
        table.AddHeaderCell("Credits");
        table.AddHeaderCell("Balance");
        table.AddHeaderCell("Remarks");

        if (ctx.Transactions == null || ctx.Transactions.Count == 0)
        {
            //table.AddCell("No transactions found").SetHorizontalAlignment(TextAlignment.CENTER);
        }
        else
        {
            foreach (StatementTransaction row in ctx.Transactions)
            {
                table.AddCell(row.TransactionDate.ToString("dd-MMM-yyyy"));
                table.AddCell(row.TransactionValueDate.ToString("dd-MMM-yyyy"));
                table.AddCell(row.Debit.ToString("N2"));
                table.AddCell(row.Credit.ToString("N2"));
                table.AddCell(row.Balance.ToString("N2"));
                table.AddCell(row.Remarks);
            }
        }

        document.Add(table);
    }
}