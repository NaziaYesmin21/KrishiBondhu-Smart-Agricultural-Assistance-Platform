using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ReportService(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<byte[]> GenerateFarmerReportAsync(int farmerId)
    {
        // =====================================================
        // FARMER
        // =====================================================

        var farmer = await _context.Farmers
            .FirstOrDefaultAsync(f => f.FarmerId == farmerId);

        if (farmer == null)
        {
            return Array.Empty<byte>();
        }


        // =====================================================
        // FARMER PHOTO
        // =====================================================

        byte[]? farmerPhoto = null;

        if (!string.IsNullOrWhiteSpace(farmer.PhotoPath))
        {
            string photoFileName =
                Path.GetFileName(farmer.PhotoPath);

            string photoFullPath =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "farmers",
                    photoFileName);

            if (System.IO.File.Exists(photoFullPath))
            {
                farmerPhoto =
                    await System.IO.File.ReadAllBytesAsync(photoFullPath);
            }
        }


        // =====================================================
        // FARMS
        // =====================================================

        var farms = await _context.Farms
            .Where(f => f.FarmerId == farmerId)
            .ToListAsync();


        // =====================================================
        // CULTIVATIONS
        // =====================================================

        var cultivations = await _context.Cultivations
            .Where(c => c.Farm.FarmerId == farmerId)
            .Include(c => c.Crop)
            .Include(c => c.Farm)
            .OrderByDescending(c => c.PlantingDate)
            .ToListAsync();


        // =====================================================
        // SOIL TESTS
        // =====================================================

        var soilTests = await _context.SoilTests
            .Where(s => s.Farm.FarmerId == farmerId)
            .Include(s => s.Farm)
            .OrderByDescending(s => s.TestDate)
            .ToListAsync();


        // =====================================================
        // DISEASES
        // ONLY DISEASES RELATED TO THIS FARMER'S CROPS
        // =====================================================

        var cropIds = cultivations
            .Select(c => c.CropId)
            .Distinct()
            .ToList();

        var diseases = await _context.Diseases
            .Where(d => cropIds.Contains(d.CropId))
            .Include(d => d.Crop)
            .ToListAsync();


        // =====================================================
        // STATISTICS
        // =====================================================

        int totalFarms = farms.Count;

        double totalLandArea = farms.Sum(f => f.Area);

        int totalCropTypes = cultivations
            .Select(c => c.CropId)
            .Distinct()
            .Count();

        int totalCultivations = cultivations.Count;

        int activeCultivations = 0;

        foreach (var cultivation in cultivations)
        {
            if (!cultivation.HarvestDate.HasValue)
            {
                activeCultivations++;
            }
            else if (
                cultivation.HarvestDate.GetValueOrDefault()
                >= DateTime.Today)
            {
                activeCultivations++;
            }
        }

        int completedCultivations =
            totalCultivations - activeCultivations;

        double totalYield =
            cultivations.Sum(c => c.Yield);

        int totalSoilTests =
            soilTests.Count;

        int totalDiseases =
            diseases.Count;

        var latestSoilTest =
            soilTests.FirstOrDefault();


        // =====================================================
        // PDF DOCUMENT
        // =====================================================

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // -------------------------------------------------
                // PAGE SETTINGS
                // -------------------------------------------------

                page.Size(PageSizes.A4);
                page.Margin(35);

                page.DefaultTextStyle(style =>
                    style.FontSize(9)
                        .FontColor("#263238"));


                // -------------------------------------------------
                // HEADER
                // -------------------------------------------------

                page.Header()
                    .PaddingBottom(15)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Column(column =>
                            {
                                column.Item()
                                    .Text("KRISHIBONDHU")
                                    .FontSize(22)
                                    .Bold()
                                    .FontColor("#1B5E20");

                                column.Item()
                                    .PaddingTop(3)
                                    .Text(
                                        "Smart Agricultural & Farm Management System")
                                    .FontSize(8)
                                    .FontColor("#607D66");
                            });

                        row.ConstantItem(130)
                            .AlignRight()
                            .Column(column =>
                            {
                                column.Item()
                                    .Text("FARMER REPORT")
                                    .FontSize(10)
                                    .Bold()
                                    .FontColor("#2E7D32");

                                column.Item()
                                    .PaddingTop(3)
                                    .Text(
                                        "Report ID: FR-" +
                                        farmerId.ToString("D4"))
                                    .FontSize(8)
                                    .FontColor("#78909C");
                            });
                    });


                // -------------------------------------------------
                // CONTENT
                // -------------------------------------------------

                page.Content()
                    .Column(main =>
                    {
                        // =================================================
                        // TITLE
                        // =================================================

                        main.Item()
                            .Background("#E8F5E9")
                            .Border(1)
                            .BorderColor("#C8E6C9")
                            .Padding(18)
                            .Column(column =>
                            {
                                column.Item()
                                    .Text("Farmer Agricultural Report")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor("#1B5E20");

                                column.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        "Farm management overview for " +
                                        Safe(farmer.Name))
                                    .FontSize(9)
                                    .FontColor("#546E5A");

                                column.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        "Generated: " +
                                        DateTime.Now.ToString(
                                            "dd MMM yyyy, hh:mm tt"))
                                    .FontSize(8)
                                    .FontColor("#78909C");
                            });


                        // =================================================
                        // FARMER PROFILE
                        // =================================================

                        main.Item()
                            .PaddingTop(18)
                            .Column(column =>
                            {
                                AddSectionTitle(
                                    column,
                                    "Farmer Profile");

                                column.Item()
                                    .PaddingTop(8)
                                    .Background("#FAFFFA")
                                    .Border(1)
                                    .BorderColor("#DDEBDD")
                                    .Padding(14)
                                    .Column(profile =>
                                    {
                                        // -----------------------------
                                        // FARMER PHOTO
                                        // -----------------------------

                                        if (farmerPhoto != null)
                                        {
                                            profile.Item()
                                                .AlignCenter()
                                                .Width(90)
                                                .Height(90)
                                                .Image(farmerPhoto)
                                                .FitArea();

                                            profile.Item()
                                                .PaddingBottom(8);
                                        }


                                        // -----------------------------
                                        // FARMER INFORMATION
                                        // -----------------------------

                                        AddProfileRow(
                                            profile,
                                            "Farmer ID",
                                            farmer.FarmerId.ToString());

                                        AddProfileDivider(profile);

                                        AddProfileRow(
                                            profile,
                                            "Full Name",
                                            Safe(farmer.Name));

                                        AddProfileDivider(profile);

                                        AddProfileRow(
                                            profile,
                                            "Phone",
                                            Safe(farmer.Phone));

                                        AddProfileDivider(profile);

                                        AddProfileRow(
                                            profile,
                                            "Email",
                                            Safe(farmer.Email));

                                        AddProfileDivider(profile);

                                        AddProfileRow(
                                            profile,
                                            "Address",
                                            Safe(farmer.Address));
                                    });
                            });


                        // =================================================
                        // PERFORMANCE SUMMARY
                        // =================================================

                        main.Item()
                            .PaddingTop(18)
                            .Column(column =>
                            {
                                AddSectionTitle(
                                    column,
                                    "Farm Performance Summary");

                                column.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });

                                        AddKpiCell(
                                            table.Cell(),
                                            "Farms",
                                            totalFarms.ToString());

                                        AddKpiCell(
                                            table.Cell(),
                                            "Land Area",
                                            totalLandArea.ToString("0.##"));

                                        AddKpiCell(
                                            table.Cell(),
                                            "Crop Types",
                                            totalCropTypes.ToString());

                                        AddKpiCell(
                                            table.Cell(),
                                            "Cultivations",
                                            totalCultivations.ToString());

                                        AddKpiCell(
                                            table.Cell(),
                                            "Active",
                                            activeCultivations.ToString());

                                        AddKpiCell(
                                            table.Cell(),
                                            "Completed",
                                            completedCultivations.ToString());

                                        AddKpiCell(
                                            table.Cell(),
                                            "Total Yield",
                                            totalYield.ToString("0.##"));

                                        AddKpiCell(
                                            table.Cell(),
                                            "Diseases",
                                            totalDiseases.ToString());
                                    });
                            });


                        // =================================================
                        // FARM PORTFOLIO
                        // =================================================

                        main.Item()
                            .PaddingTop(18)
                            .Column(column =>
                            {
                                AddSectionTitle(
                                    column,
                                    "Farm Portfolio");

                                column.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(50);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                        });

                                        table.Header(header =>
                                        {
                                            AddTableCell(
                                                header.Cell(),
                                                "ID",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Farm Name",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Location",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Area",
                                                true);
                                        });

                                        if (farms.Count == 0)
                                        {
                                            table.Cell()
                                                .ColumnSpan(4)
                                                .Padding(10)
                                                .Text(
                                                    "No farm records available.")
                                                .Italic()
                                                .FontColor("#78909C");
                                        }
                                        else
                                        {
                                            foreach (var farm in farms)
                                            {
                                                AddTableCell(
                                                    table.Cell(),
                                                    farm.FarmId.ToString());

                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(farm.FarmName));

                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(farm.Location));

                                                AddTableCell(
                                                    table.Cell(),
                                                    farm.Area.ToString("0.##"));
                                            }
                                        }
                                    });
                            });


                        // =================================================
                        // CULTIVATION
                        // =================================================

                        main.Item()
                            .PaddingTop(18)
                            .Column(column =>
                            {
                                AddSectionTitle(
                                    column,
                                    "Crop & Cultivation Monitoring");

                                column.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.3f);
                                            columns.RelativeColumn(1.3f);
                                            columns.RelativeColumn(1);
                                        });

                                        table.Header(header =>
                                        {
                                            AddTableCell(
                                                header.Cell(),
                                                "Crop",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Farm",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Planting",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Harvest",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Yield",
                                                true);
                                        });

                                        if (cultivations.Count == 0)
                                        {
                                            table.Cell()
                                                .ColumnSpan(5)
                                                .Padding(10)
                                                .Text(
                                                    "No cultivation records available.")
                                                .Italic()
                                                .FontColor("#78909C");
                                        }
                                        else
                                        {
                                            foreach (var cultivation in cultivations)
                                            {
                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(
                                                        cultivation.Crop?.CropName));

                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(
                                                        cultivation.Farm?.FarmName));

                                                AddTableCell(
                                                    table.Cell(),
                                                    cultivation.PlantingDate
                                                        .ToString(
                                                            "dd MMM yyyy"));

                                                string harvestDate;

                                                if (cultivation.HarvestDate.HasValue)
                                                {
                                                    harvestDate =
                                                        cultivation.HarvestDate
                                                            .GetValueOrDefault()
                                                            .ToString(
                                                                "dd MMM yyyy");
                                                }
                                                else
                                                {
                                                    harvestDate = "Ongoing";
                                                }

                                                AddTableCell(
                                                    table.Cell(),
                                                    harvestDate);

                                                AddTableCell(
                                                    table.Cell(),
                                                    cultivation.Yield
                                                        .ToString("0.##"));
                                            }
                                        }
                                    });
                            });


                        // =================================================
                        // SOIL HEALTH
                        // =================================================

                        main.Item()
                            .PaddingTop(18)
                            .Column(column =>
                            {
                                AddSectionTitle(
                                    column,
                                    "Soil Health Monitoring");

                                column.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.4f);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                        });

                                        table.Header(header =>
                                        {
                                            AddTableCell(
                                                header.Cell(),
                                                "Farm",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Test Date",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "pH",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Nitrogen",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Phosphorus",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Potassium",
                                                true);
                                        });

                                        if (soilTests.Count == 0)
                                        {
                                            table.Cell()
                                                .ColumnSpan(6)
                                                .Padding(10)
                                                .Text(
                                                    "No soil test records available.")
                                                .Italic()
                                                .FontColor("#78909C");
                                        }
                                        else
                                        {
                                            foreach (var soil in soilTests)
                                            {
                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(
                                                        soil.Farm?.FarmName));

                                                AddTableCell(
                                                    table.Cell(),
                                                    soil.TestDate
                                                        .ToString(
                                                            "dd MMM yyyy"));

                                                AddTableCell(
                                                    table.Cell(),
                                                    soil.PH.ToString("0.##"));

                                                AddTableCell(
                                                    table.Cell(),
                                                    soil.Nitrogen
                                                        .ToString("0.##"));

                                                AddTableCell(
                                                    table.Cell(),
                                                    soil.Phosphorus
                                                        .ToString("0.##"));

                                                AddTableCell(
                                                    table.Cell(),
                                                    soil.Potassium
                                                        .ToString("0.##"));
                                            }
                                        }
                                    });
                            });


                        // =================================================
                        // LATEST SOIL SNAPSHOT
                        // =================================================

                        if (latestSoilTest != null)
                        {
                            main.Item()
                                .PaddingTop(12)
                                .Background("#F1F8E9")
                                .Border(1)
                                .BorderColor("#DCEDC8")
                                .Padding(14)
                                .Column(column =>
                                {
                                    column.Item()
                                        .Text(
                                            "Latest Soil Test Snapshot")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor("#33691E");

                                    column.Item()
                                        .PaddingTop(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text(
                                                    "pH: " +
                                                    latestSoilTest.PH
                                                        .ToString("0.##"));

                                            row.RelativeItem()
                                                .Text(
                                                    "N: " +
                                                    latestSoilTest.Nitrogen
                                                        .ToString("0.##"));

                                            row.RelativeItem()
                                                .Text(
                                                    "P: " +
                                                    latestSoilTest.Phosphorus
                                                        .ToString("0.##"));

                                            row.RelativeItem()
                                                .Text(
                                                    "K: " +
                                                    latestSoilTest.Potassium
                                                        .ToString("0.##"));
                                        });
                                });
                        }


                        // =================================================
                        // DISEASE MONITORING
                        // =================================================

                        main.Item()
                            .PaddingTop(18)
                            .Column(column =>
                            {
                                AddSectionTitle(
                                    column,
                                    "Crop Disease Monitoring");

                                column.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(3);
                                        });

                                        table.Header(header =>
                                        {
                                            AddTableCell(
                                                header.Cell(),
                                                "Crop",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Disease",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Symptoms",
                                                true);

                                            AddTableCell(
                                                header.Cell(),
                                                "Treatment",
                                                true);
                                        });

                                        if (diseases.Count == 0)
                                        {
                                            table.Cell()
                                                .ColumnSpan(4)
                                                .Padding(10)
                                                .Text(
                                                    "No disease records found for this farmer's cultivated crops.")
                                                .Italic()
                                                .FontColor("#78909C");
                                        }
                                        else
                                        {
                                            foreach (var disease in diseases)
                                            {
                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(
                                                        disease.Crop?.CropName));

                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(
                                                        disease.DiseaseName));

                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(
                                                        disease.Symptoms));

                                                AddTableCell(
                                                    table.Cell(),
                                                    Safe(
                                                        disease.Treatment));
                                            }
                                        }
                                    });
                            });


                        // =================================================
                        // AGRICULTURAL SUMMARY
                        // =================================================

                        main.Item()
                            .PaddingTop(18)
                            .Column(column =>
                            {
                                AddSectionTitle(
                                    column,
                                    "Agricultural Summary");

                                column.Item()
                                    .PaddingTop(8)
                                    .Background("#FAFAFA")
                                    .Border(1)
                                    .BorderColor("#E0E0E0")
                                    .Padding(14)
                                    .Column(summary =>
                                    {
                                        summary.Item()
                                            .Text(
                                                "Farmer " +
                                                Safe(farmer.Name) +
                                                " currently manages " +
                                                totalFarms.ToString() +
                                                " farm(s) covering approximately " +
                                                totalLandArea.ToString("0.##") +
                                                " area units.");

                                        summary.Item()
                                            .PaddingTop(5)
                                            .Text(
                                                "The agricultural records contain " +
                                                totalCropTypes.ToString() +
                                                " crop type(s) across " +
                                                totalCultivations.ToString() +
                                                " cultivation record(s), with " +
                                                activeCultivations.ToString() +
                                                " active cultivation(s) and " +
                                                completedCultivations.ToString() +
                                                " completed cultivation(s).");

                                        summary.Item()
                                            .PaddingTop(5)
                                            .Text(
                                                "A total of " +
                                                totalSoilTests.ToString() +
                                                " soil test record(s) and " +
                                                totalDiseases.ToString() +
                                                " disease record(s) are currently " +
                                                "available for this farmer's " +
                                                "agricultural activities.");
                                    });
                            });
                    });


                // =====================================================
                // FOOTER
                // =====================================================

                page.Footer()
                    .PaddingTop(10)
                    .BorderTop(1)
                    .BorderColor("#E0E0E0")
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text(
                                "KrishiBondhu • Smart Agricultural Management")
                            .FontSize(7)
                            .FontColor("#78909C");

                        row.ConstantItem(80)
                            .AlignRight()
                            .Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
            });
        });


        return document.GeneratePdf();
    }


    // =============================================================
    // SECTION TITLE
    // =============================================================

    private void AddSectionTitle(
        ColumnDescriptor column,
        string title)
    {
        column.Item()
            .Row(row =>
            {
                row.ConstantItem(5)
                    .Height(18)
                    .Background("#2E7D32");

                row.RelativeItem()
                    .PaddingLeft(8)
                    .AlignMiddle()
                    .Text(title)
                    .FontSize(13)
                    .Bold()
                    .FontColor("#1B5E20");
            });
    }


    // =============================================================
    // KPI CELL
    // =============================================================

    private void AddKpiCell(
        IContainer container,
        string label,
        string value)
    {
        container
            .Background("#F8FBF8")
            .Border(1)
            .BorderColor("#DDEBDD")
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text(value)
                    .FontSize(18)
                    .Bold()
                    .FontColor("#2E7D32");

                column.Item()
                    .PaddingTop(3)
                    .Text(label)
                    .FontSize(8)
                    .FontColor("#607D66");
            });
    }


    // =============================================================
    // PROFILE ROW
    // =============================================================

    private void AddProfileRow(
        ColumnDescriptor column,
        string label,
        string value)
    {
        column.Item()
            .Row(row =>
            {
                row.ConstantItem(90)
                    .Text(label)
                    .FontSize(8)
                    .Bold()
                    .FontColor("#607D66");

                row.RelativeItem()
                    .Text(value)
                    .FontSize(9)
                    .FontColor("#263238");
            });
    }


    // =============================================================
    // PROFILE DIVIDER
    // =============================================================

    private void AddProfileDivider(
        ColumnDescriptor column)
    {
        column.Item()
            .PaddingVertical(5)
            .LineHorizontal(1)
            .LineColor("#E8EFE8");
    }


    // =============================================================
    // TABLE CELL
    // =============================================================

    private void AddTableCell(
        IContainer container,
        string text,
        bool header = false)
    {
        if (header)
        {
            container
                .Background("#E8F5E9")
                .BorderBottom(1)
                .BorderColor("#C8E6C9")
                .PaddingVertical(6)
                .PaddingHorizontal(5)
                .Text(text)
                .Bold()
                .FontSize(8)
                .FontColor("#1B5E20");
        }
        else
        {
            container
                .BorderBottom(1)
                .BorderColor("#E5EAE5")
                .PaddingVertical(6)
                .PaddingHorizontal(5)
                .Text(text)
                .FontSize(7.5f)
                .FontColor("#37474F");
        }
    }


    // =============================================================
    // SAFE TEXT
    // =============================================================

    private string Safe(object value)
    {
        if (value == null)
        {
            return "N/A";
        }

        string result = value.ToString();

        if (string.IsNullOrWhiteSpace(result))
        {
            return "N/A";
        }

        return result;
    }
}