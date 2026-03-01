using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PdfService : IDocument
{
    private readonly BankStatement _data;

    public PdfService(BankStatement data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Element(Header);
            page.Content().Element(Content);
            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Generated on ");
                x.Span(DateTime.Now.ToString("dd-MM-yyyy"));
            });
        });
    }

    void Header(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text(_data.BankName).Bold().FontSize(14);
            col.Item().Text("CUSTOMER STATEMENT").Bold();
            col.Item().Text(_data.CustomerName);
            col.Item().Text($"Account No: {_data.AccountNumber}");
            col.Item().Text($"Account Type: {_data.AccountType} | Currency: {_data.Currency}");
            col.Item().Text($"Period: {_data.PeriodFrom:dd MMM yyyy} - {_data.PeriodTo:dd MMM yyyy}");
            col.Item().LineHorizontal(1);
        });
    }

    void Content(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60); // Date
                columns.RelativeColumn(3);  // Description
                columns.RelativeColumn(1);  // Debit
                columns.RelativeColumn(1);  // Credit
                columns.RelativeColumn(1);  // Balance
            });

            table.Header(header =>
            {
                header.Cell().Text("Date").Bold();
                header.Cell().Text("Description").Bold();
                header.Cell().Text("Debit").Bold();
                header.Cell().Text("Credit").Bold();
                header.Cell().Text("Balance").Bold();
            });

            foreach (var t in _data.Transactions)
            {
                table.Cell().Text(t.Date.ToString("dd/MM/yy"));
                table.Cell().Text(t.Description);
                table.Cell().Text(t.Debit == 0 ? "" : t.Debit.ToString("N2"));
                table.Cell().Text(t.Credit == 0 ? "" : t.Credit.ToString("N2"));
                table.Cell().Text(t.Balance.ToString("N2"));
            }
        });
    }

    public byte[] GeneratePdfBytes()
    {
        try
        {
            // Step 1: Create the PDF document using the Compose method
            var document = Document.Create(container =>
            {
                Compose(container);
            });

            // Step 2: Generate the PDF byte array
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            // Handle any exceptions and log if necessary
            throw new InvalidOperationException("Failed to generate PDF.", ex);
        }
    }
}