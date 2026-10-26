
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.GenAI;
using Microsoft.EntityFrameworkCore;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Services
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly Client _client;
        private readonly ApplicationDbContext _context;

        public AIAssistantService(
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is not configured.");
            }

            _client = new Client(apiKey: apiKey);
            _context = context;
        }

        public async Task<string> AskAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return "অনুগ্রহ করে কৃষি সম্পর্কিত একটি প্রশ্ন লিখুন।";
            }

            try
            {
                System.Diagnostics.Debug.WriteLine(
                    "KrishiBondhu: Loading crop information...");

                var crops = await _context.Crops
                    .AsNoTracking()
                    .Select(c => new
                    {
                        c.CropName,
                        c.Season,
                        c.Description
                    })
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine(
                    "KrishiBondhu: Loading soil test information...");

                var soilTests = await _context.SoilTests
                    .AsNoTracking()
                    .Select(s => new
                    {
                        s.FarmId,
                        s.TestDate,
                        s.PH,
                        s.Nitrogen,
                        s.Phosphorus,
                        s.Potassium
                    })
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine(
                    "KrishiBondhu: Loading disease information...");

                var diseases = await _context.Diseases
                    .AsNoTracking()
                    .Select(d => new
                    {
                        d.DiseaseName,
                        d.Symptoms,
                        d.Treatment,
                        d.CropId
                    })
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine(
                    "KrishiBondhu: Loading cultivation information...");

                var cultivations = await _context.Cultivations
                    .AsNoTracking()
                    .Include(c => c.Crop)
                    .Select(c => new
                    {
                        c.CultivationId,
                        c.FarmId,
                        CropName = c.Crop.CropName,
                        c.PlantingDate,
                        c.HarvestDate,
                        c.Yield
                    })
                    .ToListAsync();

                var cropContext = crops.Any()
                    ? string.Join("\n", crops.Select(c =>
                        $"- Crop: {c.CropName}, " +
                        $"Season: {c.Season}, " +
                        $"Description: {c.Description}"))
                    : "No crop records available.";

                var soilContext = soilTests.Any()
                    ? string.Join("\n", soilTests.Select(s =>
                        $"- Farm ID: {s.FarmId}, " +
                        $"Test Date: {s.TestDate:dd MMM yyyy}, " +
                        $"pH: {s.PH}, " +
                        $"Nitrogen: {s.Nitrogen}, " +
                        $"Phosphorus: {s.Phosphorus}, " +
                        $"Potassium: {s.Potassium}"))
                    : "No soil test records available.";

                var diseaseContext = diseases.Any()
                    ? string.Join("\n", diseases.Select(d =>
                        $"- Disease: {d.DiseaseName}, " +
                        $"Crop ID: {d.CropId}, " +
                        $"Symptoms: {d.Symptoms}, " +
                        $"Treatment: {d.Treatment}"))
                    : "No disease records available.";

                var cultivationContext = cultivations.Any()
                    ? string.Join("\n", cultivations.Select(c =>
                        $"- Cultivation ID: {c.CultivationId}, " +
                        $"Farm ID: {c.FarmId}, " +
                        $"Crop: {c.CropName}, " +
                        $"Planting Date: {c.PlantingDate:dd MMM yyyy}, " +
                        $"Harvest Date: " +
                        $"{(c.HarvestDate.HasValue
                            ? c.HarvestDate.Value.ToString("dd MMM yyyy")
                            : "Not specified")}, " +
                        $"Yield: {c.Yield}"))
                    : "No cultivation records available.";

                var prompt = $"""
                    You are the AI Agricultural Assistant of
                    KrishiBondhu – Smart Agricultural Assistance Platform.

                    Help farmers in Bangladesh with practical,
                    understandable and responsible agricultural guidance.

                    Use the database information when relevant.
                    Never invent database records or values.

                    CROP INFORMATION:
                    {cropContext}

                    SOIL TEST INFORMATION:
                    {soilContext}

                    DISEASE INFORMATION:
                    {diseaseContext}

                    CULTIVATION INFORMATION:
                    {cultivationContext}

                    FARMER QUESTION:
                    {question}

                    RESPONSE RULES:
                    1. Answer the actual question directly.
                    2. If the question is in Bangla, answer in simple Bangla.
                    3. If the question is in English, answer in simple English.
                    4. Return Markdown, not HTML.
                    5. Use clear headings and separate bullet points.
                    6. Keep paragraphs short and readable.
                    7. Use database records when relevant.
                    8. If information is missing, say so honestly.
                    9. Do not claim a disease is confirmed without evidence.
                    10. Never invent soil values or treatment results.
                    11. Do not guess pesticide doses.
                    12. For serious problems, recommend a local agricultural
                        officer and advise following approved product labels.
                    13. Give a concise but useful answer.
                    """;

                System.Diagnostics.Debug.WriteLine(
                    "KrishiBondhu: Sending text request to Gemini...");

                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(45));

                Exception? lastException = null;

                // One initial attempt plus up to two retries.
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"KrishiBondhu: Gemini attempt {attempt}/3.");

                        var response =
                            await _client.Models.GenerateContentAsync(
                                model: "gemini-3.7-flash",
                                contents: prompt,
                                cancellationToken: cts.Token);

                        System.Diagnostics.Debug.WriteLine(
                            "KrishiBondhu: Gemini text response received.");

                        var answer = response.Candidates?
                            .FirstOrDefault()?
                            .Content?
                            .Parts?
                            .FirstOrDefault(p =>
                                !string.IsNullOrWhiteSpace(p.Text))?
                            .Text;

                        if (!string.IsNullOrWhiteSpace(answer))
                        {
                            return answer;
                        }

                        return "AI কোনো উত্তর দেয়নি। কিছুক্ষণ পর আবার চেষ্টা করুন।";
                    }
                    catch (OperationCanceledException)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "KrishiBondhu: Gemini request timed out.");

                        return "AI উত্তর দিতে বেশি সময় নিচ্ছে। " +
                               "কিছুক্ষণ পর আবার চেষ্টা করুন।";
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;

                        // Do not print the API key or configuration values.
                        System.Diagnostics.Debug.WriteLine(
                            $"KrishiBondhu: Gemini attempt {attempt} failed: " +
                            $"{ex.GetType().Name}: {ex.Message}");

                        var error = ex.ToString().ToLowerInvariant();

                        bool temporaryError =
                            error.Contains("high demand") ||
                            error.Contains("503") ||
                            error.Contains("429") ||
                            error.Contains("500") ||
                            error.Contains("unavailable") ||
                            error.Contains("overloaded") ||
                            error.Contains("resource_exhausted") ||
                            error.Contains("timeout");

                        if (!temporaryError || attempt == 3)
                        {
                            break;
                        }

                        await Task.Delay(
                            TimeSpan.FromSeconds(attempt * 2));
                    }
                }

                if (lastException != null)
                {
                    var error = lastException.ToString().ToLowerInvariant();

                    if (error.Contains("429") ||
                        error.Contains("resource_exhausted"))
                    {
                        return "Gemini API-এর request limit বা quota " +
                               "সংক্রান্ত সমস্যা হয়েছে। AI Studio-তে " +
                               "usage ও limits পরীক্ষা করুন।";
                    }

                    if (error.Contains("high demand") ||
                        error.Contains("503") ||
                        error.Contains("overloaded") ||
                        error.Contains("unavailable"))
                    {
                        return "Gemini server এখন ব্যস্ত বা unavailable। " +
                               "কিছুক্ষণ পরে আবার চেষ্টা করুন।";
                    }

                    if (error.Contains("api key") ||
                        error.Contains("permission denied") ||
                        error.Contains("unauthenticated") ||
                        error.Contains("401") ||
                        error.Contains("403"))
                    {
                        return "Gemini API key বা permission-এ সমস্যা হয়েছে। " +
                               "Google AI Studio ও User Secrets configuration পরীক্ষা করুন।";
                    }

                    return "AI request ব্যর্থ হয়েছে (" +
                           lastException.GetType().Name +
                           ")। বিস্তারিত জানতে Visual Studio Output window দেখুন।";
                }

                return "এই মুহূর্তে AI উত্তর দিতে পারেনি। পরে আবার চেষ্টা করুন।";
            }
            catch (OperationCanceledException)
            {
                return "Database বা AI operation-এ বেশি সময় লাগছে। আবার চেষ্টা করুন।";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "KrishiBondhu AIAssistantService Error: " +
                    ex.GetType().Name + ": " + ex.Message);

                return "AI Assistant-এর আগে database বা service-এ সমস্যা হয়েছে। " +
                       "Visual Studio Output window পরীক্ষা করুন।";
            }
        }
    }
}
