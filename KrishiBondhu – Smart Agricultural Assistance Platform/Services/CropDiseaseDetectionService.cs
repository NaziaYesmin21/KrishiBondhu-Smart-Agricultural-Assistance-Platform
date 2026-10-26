
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;

namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Services
{
    public class CropDiseaseDetectionService
    {
        private readonly IConfiguration _configuration;

        public CropDiseaseDetectionService(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> AnalyzeCropImageAsync(
            byte[] imageBytes,
            string contentType,
            string? cropName = null,
            string? question = null)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return "অনুগ্রহ করে গাছের পাতার একটি ছবি upload করুন।";

            if (imageBytes.Length > 10 * 1024 * 1024)
                return "ছবির size 10 MB-এর কম হতে হবে।";

            contentType = contentType.ToLowerInvariant();

            if (contentType != "image/jpeg" &&
                contentType != "image/png" &&
                contentType != "image/webp")
            {
                return "JPG, PNG অথবা WebP format-এর ছবি upload করুন।";
            }

            string apiKey =
                _configuration["Gemini:ApiKey"] ?? "";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Gemini API key পাওয়া যায়নি। " +
                       "Visual Studio User Secrets configuration পরীক্ষা করুন।";
            }

            string model = "gemini-3.8-flash";

            string selectedCrop =
                string.IsNullOrWhiteSpace(cropName)
                ? "Not specified"
                : cropName.Trim();

            string farmerQuestion =
                string.IsNullOrWhiteSpace(question)
                ? "No additional question"
                : question.Trim();

            string prompt = $"""
                You are KrishiBondhu, an agricultural assistant
                helping farmers in Bangladesh.

                Analyze the attached plant leaf image.

                Selected crop: {selectedCrop}
                Farmer's question: {farmerQuestion}

                Answer in simple, natural Bangla.

                Include these sections where relevant:
                1. সম্ভাব্য রোগ বা অবস্থা
                2. ছবিতে দেখা লক্ষণ
                3. সম্ভাব্য কারণ
                4. করণীয়
                5. প্রতিরোধের উপায়
                6. সতর্কতা

                Rules:
                - This is an estimate, not a confirmed diagnosis.
                - If the image is unclear, explain that.
                - If the image is not a plant image, say so.
                - Never invent confidence percentages.
                - Do not invent pesticide doses.
                - Recommend a local agricultural officer if uncertain.
                - Answer the farmer's question when provided.
                """;

            var contents = new Content
            {
                Role = "user",
                Parts = new List<Part>
                {
                    Part.FromText(prompt),
                    Part.FromBytes(imageBytes, contentType)
                }
            };

            // Maximum 3 attempts, only for likely temporary errors.
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var client = new Client(apiKey: apiKey);

                    var response =
                        await client.Models.GenerateContentAsync(
                            model: model,
                            contents: contents);

                    string? answer = response.Candidates?
                        .FirstOrDefault()?
                        .Content?
                        .Parts?
                        .FirstOrDefault(
                            p => !string.IsNullOrWhiteSpace(p.Text))?
                        .Text;

                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        return "AI কোনো উত্তর দেয়নি। " +
                               "অন্য একটি পরিষ্কার ছবি দিয়ে চেষ্টা করুন।";
                    }

                    return answer;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Crop image attempt {attempt}/{maxAttempts}: {ex}");

                    string error = ex.ToString().ToLowerInvariant();

                    bool temporaryError =
                        error.Contains("high demand") ||
                        error.Contains("429") ||
                        error.Contains("503") ||
                        error.Contains("500") ||
                        error.Contains("temporarily") ||
                        error.Contains("overloaded") ||
                        error.Contains("unavailable") ||
                        error.Contains("resource_exhausted") ||
                        error.Contains("timeout");

                    if (!temporaryError)
                    {
                        return "Gemini-এর সঙ্গে সংযোগ বা configuration-এ " +
                               "সমস্যা হয়েছে। Visual Studio Output window-তে " +
                               "Crop image attempt error দেখুন।";
                    }

                    if (attempt < maxAttempts)
                    {
                        // Wait 2 seconds, then 4 seconds.
                        await Task.Delay(
                            TimeSpan.FromSeconds(attempt * 2));
                    }
                }
            }

            return "Gemini server এখন ব্যস্ত অথবা সাময়িকভাবে unavailable। " +
                   "কিছুক্ষণ পরে আবার চেষ্টা করুন। " +
                   "আপনার upload করা ছবিটি আবার select করতে হতে পারে।";
        }
    }
}
