using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateFarmerReportAsync(int farmerId)
    {
        var farmer = await _context.Farmers
            .FirstOrDefaultAsync(f => f.FarmerId == farmerId);

        if (farmer == null)
        {
            return Array.Empty<byte>();
        }

        var farms = await _context.Farms
            .Where(f => f.FarmerId == farmerId)
            .ToListAsync();

        var farmIds = farms
            .Select(f => f.FarmId)
            .ToList();

        var cultivations = await _context.Cultivations
            .Where(c => farmIds.Contains(c.FarmId))
            .Include(c => c.Crop)
            .Include(c => c.Farm)
            .ToListAsync();

        var soilTests = await _context.SoilTests
            .Where(s => farmIds.Contains(s.FarmId))
            .Include(s => s.Farm)
            .ToListAsync();

        var diseases = await _context.Diseases
            .Include(d => d.Crop)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.DefaultTextStyle(style =>
                    style.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignCenter()
                            .Text("KRISHIBONDHU")
                            .FontSize(24)
                            .Bold()
                            .FontColor("#176B45");

                        column.Item()
                            .AlignCenter()
                            .Text("Smart Agricultural Assistance Platform")
                            .FontSize(11)
                            .FontColor("#666666");

                        column.Item()
                            .PaddingTop(5)
                            .LineHorizontal(1)
                            .LineColor("#176B45");
                    });

                page.Content()
                    .PaddingTop(20)
                    .Column(column =>
                    {
                        // TITLE
                        column.Item()
                            .AlignCenter()
                            .Text("AGRICULTURAL REPORT")
                            .FontSize(18)
                            .Bold()
                            .FontColor("#244536");

                        column.Item()
                            .PaddingBottom(18)
                            .AlignCenter()
                            .Text($"Generated on: {DateTime.Now:dd MMMM yyyy}")
                            .FontSize(9)
                            .FontColor("#777777");


                        // =========================================
                        // FARMER INFORMATION
                        // =========================================

                        column.Item()
                            .Text("1. Farmer Information")
                            .FontSize(14)
                            .Bold()
                            .FontColor("#176B45");

                        column.Item()
                            .PaddingTop(8)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(130);
                                    columns.RelativeColumn();
                                });

                                AddRow(
                                    table,
                                    "Farmer ID",
                                    farmer.FarmerId.ToString());

                                AddRow(
                                    table,
                                    "Name",
                                    farmer.Name);

                                AddRow(
                                    table,
                                    "Phone",
                                    farmer.Phone);

                                AddRow(
                                    table,
                                    "Email",
                                    farmer.Email);

                                AddRow(
                                    table,
                                    "Address",
                                    farmer.Address);
                            });


                        // =========================================
                        // FARM INFORMATION
                        // =========================================

                        column.Item()
                            .PaddingTop(22)
                            .Text("2. Farm Information")
                            .FontSize(14)
                            .Bold()
                            .FontColor("#176B45");

                        if (farms.Any())
                        {
                            column.Item()
                                .PaddingTop(8)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(55);
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(70);
                                    });

                                    AddHeader(
                                        table,
                                        new[]
                                        {
                                            "ID",
                                            "Farm Name",
                                            "Location",
                                            "Area"
                                        });

                                    foreach (var farm in farms)
                                    {
                                        AddCell(
                                            table,
                                            farm.FarmId.ToString());

                                        AddCell(
                                            table,
                                            farm.FarmName);

                                        AddCell(
                                            table,
                                            farm.Location);

                                        AddCell(
                                            table,
                                            farm.Area.ToString("0.##"));
                                    }
                                });
                        }
                        else
                        {
                            AddEmptyMessage(
                                column,
                                "No farm records available.");
                        }


                        // =========================================
                        // CULTIVATION
                        // =========================================

                        column.Item()
                            .PaddingTop(22)
                            .Text("3. Cultivation Records")
                            .FontSize(14)
                            .Bold()
                            .FontColor("#176B45");

                        if (cultivations.Any())
                        {
                            column.Item()
                                .PaddingTop(8)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(85);
                                        columns.ConstantColumn(85);
                                        columns.ConstantColumn(65);
                                    });

                                    AddHeader(
                                        table,
                                        new[]
                                        {
                                            "Farm",
                                            "Crop",
                                            "Planting",
                                            "Harvest",
                                            "Yield"
                                        });

                                    foreach (var cultivation in cultivations)
                                    {
                                        AddCell(
                                            table,
                                            cultivation.Farm.FarmName);

                                        AddCell(
                                            table,
                                            cultivation.Crop.CropName);

                                        AddCell(
                                            table,
                                            cultivation.PlantingDate
                                                .ToString("dd/MM/yyyy"));

                                        AddCell(
                                            table,
                                            cultivation.HarvestDate?
                                                .ToString("dd/MM/yyyy")
                                                ?? "-");

                                        AddCell(
                                            table,
                                            cultivation.Yield
                                                .ToString("0.##"));
                                    }
                                });
                        }
                        else
                        {
                            AddEmptyMessage(
                                column,
                                "No cultivation records available.");
                        }


                        // =========================================
                        // SOIL TEST
                        // =========================================

                        column.Item()
                            .PaddingTop(22)
                            .Text("4. Soil Test Results")
                            .FontSize(14)
                            .Bold()
                            .FontColor("#176B45");

                        if (soilTests.Any())
                        {
                            column.Item()
                                .PaddingTop(8)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(55);
                                        columns.ConstantColumn(55);
                                        columns.ConstantColumn(55);
                                        columns.ConstantColumn(55);
                                    });

                                    AddHeader(
                                        table,
                                        new[]
                                        {
                                            "Farm",
                                            "Test Date",
                                            "pH",
                                            "N",
                                            "P",
                                            "K"
                                        });

                                    foreach (var soil in soilTests)
                                    {
                                        AddCell(
                                            table,
                                            soil.Farm.FarmName);

                                        AddCell(
                                            table,
                                            soil.TestDate
                                                .ToString("dd/MM/yyyy"));

                                        AddCell(
                                            table,
                                            soil.PH.ToString("0.##"));

                                        AddCell(
                                            table,
                                            soil.Nitrogen
                                                .ToString("0.##"));

                                        AddCell(
                                            table,
                                            soil.Phosphorus
                                                .ToString("0.##"));

                                        AddCell(
                                            table,
                                            soil.Potassium
                                                .ToString("0.##"));
                                    }
                                });
                        }
                        else
                        {
                            AddEmptyMessage(
                                column,
                                "No soil test records available.");
                        }


                        // =========================================
                        // DISEASE
                        // =========================================

                        column.Item()
                            .PaddingTop(22)
                            .Text("5. Crop Disease Information")
                            .FontSize(14)
                            .Bold()
                            .FontColor("#176B45");

                        if (diseases.Any())
                        {
                            column.Item()
                                .PaddingTop(8)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    AddHeader(
                                        table,
                                        new[]
                                        {
                                            "Crop",
                                            "Disease",
                                            "Symptoms",
                                            "Treatment"
                                        });

                                    foreach (var disease in diseases)
                                    {
                                        AddCell(
                                            table,
                                            disease.Crop.CropName);

                                        AddCell(
                                            table,
                                            disease.DiseaseName);

                                        AddCell(
                                            table,
                                            disease.Symptoms);

                                        AddCell(
                                            table,
                                            disease.Treatment);
                                    }
                                });
                        }
                        else
                        {
                            AddEmptyMessage(
                                column,
                                "No disease records available.");
                        }


                        // =========================================
                        // SUMMARY
                        // =========================================

                        column.Item()
                            .PaddingTop(25)
                            .Background("#E9F4ED")
                            .Padding(14)
                            .Column(summary =>
                            {
                                summary.Item()
                                    .Text("Agricultural Summary")
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor("#176B45");

                                summary.Item()
                                    .PaddingTop(7)
                                    .Text(
                                        $"Total Farms: {farms.Count}   |   " +
                                        $"Cultivation Records: {cultivations.Count}   |   " +
                                        $"Soil Tests: {soilTests.Count}   |   " +
                                        $"Disease Records: {diseases.Count}")
                                    .FontSize(10);
                            });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(
                        "KrishiBondhu • Smart Agricultural Assistance Platform")
                    .FontSize(8)
                    .FontColor("#777777");
            });
        });

        return document.GeneratePdf();
    }


    // =========================================
    // HEADER
    // =========================================

    private static void AddHeader(
        TableDescriptor table,
        string[] headers)
    {
        foreach (var header in headers)
        {
            table.Cell()
                .Background("#176B45")
                .Padding(6)
                .Text(text =>
                {
                    text.Span(header)
                        .FontSize(8)
                        .Bold()
                        .FontColor("#FFFFFF");
                });
        }
    }


    // =========================================
    // NORMAL CELL
    // =========================================

    private static void AddCell(
        TableDescriptor table,
        string value)
    {
        table.Cell()
            .BorderBottom(1)
            .BorderColor("#DDE8E1")
            .Padding(6)
            .Text(text =>
            {
                text.Span(value)
                    .FontSize(8);
            });
    }


    // =========================================
    // FARMER INFORMATION ROW
    // =========================================

    private static void AddRow(
        TableDescriptor table,
        string label,
        string value)
    {
        table.Cell()
            .Background("#E9F4ED")
            .Padding(7)
            .Text(text =>
            {
                text.Span(label)
                    .Bold()
                    .FontSize(9);
            });

        table.Cell()
            .BorderBottom(1)
            .BorderColor("#DDE8E1")
            .Padding(7)
            .Text(text =>
            {
                text.Span(value)
                    .FontSize(9);
            });
    }


    // =========================================
    // EMPTY MESSAGE
    // =========================================

    private static void AddEmptyMessage(
        ColumnDescriptor column,
        string message)
    {
        column.Item()
            .PaddingTop(8)
            .Background("#F7F9F8")
            .Padding(12)
            .Text(text =>
            {
                text.Span(message)
                    .FontSize(9)
                    .FontColor("#777777");
            });
    }
}