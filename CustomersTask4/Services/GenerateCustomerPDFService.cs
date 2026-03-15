
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CustomersTask4.Services
{
    public class GenerateCustomerPDFService
        (IEnumerable<CustomerPDFDto> customers
        ,DateOnly from,
        DateOnly to):IDocument
    {
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {

            container
            .Page(page =>
            {
                page.Margin(50);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });


            void ComposeHeader(IContainer container)
            {
                container.Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item()
                            .Text($"Customer Report")
                            .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                        column.Item().Text(text =>
                        {
                            text.Span("From Date: ").SemiBold();
                            text.Span(from.ToString());
                        });

                        column.Item().Text(text =>
                        {
                            text.Span("To Date: ").SemiBold();
                            text.Span(to.ToString());
                        });
                    });
                });
            }

            void ComposeContent(IContainer container)
            {
                container.PaddingVertical(40).Column(column =>
                {
                    column.Item().Element(ComposeTable);
                });
            }

            void ComposeTable(IContainer container)
            {
                if (!customers.Any())
                {
                    container
                        .Height(250)
                        .Background(Colors.Grey.Lighten3)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text($"There is No Customers Created from {from} to {to}").FontSize(16);
                }
                else
                {
                    container.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").Bold();
                            header.Cell().Element(CellStyle).Text("ID").Bold();
                            header.Cell().Element(CellStyle).Text("Name").Bold();
                            header.Cell().Element(CellStyle).Text("Phone Number").Bold();
                        });
                        var number = 0;
                        foreach (var customer in customers)
                        {
                            table.Cell().Element(CellStyle).Text((++number).ToString());
                            table.Cell().Element(CellStyle).Text(customer.Id);
                            table.Cell().Element(CellStyle).Text(customer.Name);
                            table.Cell().Element(CellStyle).Text(customer.Phone);


                        }
                    });
                }
            }
            static IContainer CellStyle(IContainer container)
            {
                return container.BorderBottom(1).AlignCenter().BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
        }
    }
}
