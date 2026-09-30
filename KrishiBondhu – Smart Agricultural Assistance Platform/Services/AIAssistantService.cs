using Microsoft.EntityFrameworkCore;
using Google.GenAI;

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
                return "Please enter an agricultural question.";
            }

            try
            {
                // 1. Get Crop Information
                var crops = await _context.Crops
                    .AsNoTracking()
                    .Select(c => new
                    {
                        c.CropName,
                        c.Season,
                        c.Description
                    })
                    .ToListAsync();

                // 2. Get Soil Test Information
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

                // 3. Get Disease Information
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

                // 4. Get Cultivation Information
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

                // 5. Prepare Database Context
                var cropContext = crops.Any()
                    ? string.Join("\n",
                        crops.Select(c =>
                            $"- Crop: {c.CropName}, " +
                            $"Season: {c.Season}, " +
                            $"Description: {c.Description}"))
                    : "No crop records available.";

                var soilContext = soilTests.Any()
                    ? string.Join("\n",
                        soilTests.Select(s =>
                            $"- Farm ID: {s.FarmId}, " +
                            $"Test Date: {s.TestDate:dd MMM yyyy}, " +
                            $"pH: {s.PH}, " +
                            $"Nitrogen: {s.Nitrogen}, " +
                            $"Phosphorus: {s.Phosphorus}, " +
                            $"Potassium: {s.Potassium}"))
                    : "No soil test records available.";

                var diseaseContext = diseases.Any()
                    ? string.Join("\n",
                        diseases.Select(d =>
                            $"- Disease: {d.DiseaseName}, " +
                            $"Crop ID: {d.CropId}, " +
                            $"Symptoms: {d.Symptoms}, " +
                            $"Treatment: {d.Treatment}"))
                    : "No disease records available.";

                var cultivationContext = cultivations.Any()
                    ? string.Join("\n",
                        cultivations.Select(c =>
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

                // 6. Create AI Prompt
                var prompt = $"""
                    You are the AI Agricultural Assistant of
                    KrishiBondhu – Smart Agricultural Assistance Platform.

                    Your job is to provide simple, practical and
                    understandable agricultural guidance.

                    The system contains real agricultural records.
                    Use the following database information as context
                    when it is relevant to the farmer's question.

                    ==============================
                    CROP INFORMATION
                    ==============================

                    {cropContext}

                    ==============================
                    SOIL TEST INFORMATION
                    ==============================

                    {soilContext}

                    ==============================
                    DISEASE INFORMATION
                    ==============================

                    {diseaseContext}

                    ==============================
                    CULTIVATION INFORMATION
                    ==============================

                    {cultivationContext}

                    ==============================
                    FARMER QUESTION
                    ==============================

                    {question}

                    ==============================
                    RESPONSE GUIDELINES
                    ==============================

                    1. Answer the farmer's actual question directly.
                    2. Use the KrishiBondhu database information
                       when it is relevant.
                    3. If database information is not sufficient,
                       clearly say that the available records are
                       not enough and provide general guidance.
                    4. Use simple language.
                    5. If the question is in Bangla, answer in Bangla.
                    6. If the question is in English, answer in English.
                    7. For disease-related questions, mention possible
                       symptoms and appropriate management steps.
                    8. For soil-related questions, consider pH,
                       Nitrogen, Phosphorus and Potassium values
                       when relevant.
                    9. Do not invent database records.
                    10. Do not claim that a disease is definitely
                        present based only on a description or symptom.
                    11. For pesticides or chemicals, advise the farmer
                        to follow the product label and consult a
                        qualified local agricultural professional
                        when necessary.

                    Give a concise but useful answer.
                    """;

                // 7. Gemini request with retry
                using var cts =
                    new CancellationTokenSource(
                        TimeSpan.FromSeconds(90));

                Exception? lastException = null;

                int[] retryDelays =
                {
                    2000,
                    5000,
                    10000
                };

                for (int attempt = 0; attempt <= retryDelays.Length; attempt++)
                {
                    try
                    {
                        var response =
                            await _client.Models.GenerateContentAsync(
                                model: "gemini-3.7-flash",
                                contents: prompt,
                                cancellationToken: cts.Token);

                        // 8. Extract AI Response
                        var answer =
                            response.Candidates?
                                .FirstOrDefault()?
                                .Content?
                                .Parts?
                                .FirstOrDefault()?
                                .Text;

                        if (!string.IsNullOrWhiteSpace(answer))
                        {
                            return answer;
                        }

                        return "Sorry, I could not generate an answer right now.";
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;

                        var errorMessage =
                            ex.Message.ToLowerInvariant();

                        bool temporaryError =
                            errorMessage.Contains("high demand") ||
                            errorMessage.Contains("503") ||
                            errorMessage.Contains("unavailable") ||
                            errorMessage.Contains("429") ||
                            errorMessage.Contains("resource exhausted");

                        if (!temporaryError ||
                            attempt >= retryDelays.Length)
                        {
                            break;
                        }

                        await Task.Delay(
                            retryDelays[attempt],
                            cts.Token);
                    }
                }

                if (lastException != null)
                {
                    var errorMessage =
                        lastException.Message.ToLowerInvariant();

                    if (errorMessage.Contains("high demand") ||
                        errorMessage.Contains("503") ||
                        errorMessage.Contains("unavailable"))
                    {
                        return "The AI service is temporarily busy. Please try again in a few moments.";
                    }

                    if (errorMessage.Contains("429") ||
                        errorMessage.Contains("resource exhausted"))
                    {
                        return "The AI service has temporarily reached its request limit. Please try again later.";
                    }

                    return "Sorry, the AI assistant is temporarily unavailable.";
                }

                return "Sorry, I could not generate an answer right now.";
            }
            catch (OperationCanceledException)
            {
                return "The AI request took too long. Please try again.";
            }
            catch (Exception)
            {
                return "Sorry, the AI assistant is temporarily unavailable.";
            }
        }
    }
}