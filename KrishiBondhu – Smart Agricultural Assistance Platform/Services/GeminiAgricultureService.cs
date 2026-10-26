
using Google.GenAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace KrishiBondhu.Services
{
    public class GeminiAgricultureService
    {
        private readonly IConfiguration _configuration;

        public GeminiAgricultureService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> AskAgricultureQuestionAsync(string question)
        {
            string apiKey = _configuration["Gemini:ApiKey"];
            string model = _configuration["Gemini:Model"]
                           ?? "gemini-2.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is missing from User Secrets.");
            }

            using var client = new Client(apiKey: apiKey);

            string prompt = $@"
You are KrishiBondhu, a helpful agricultural assistant for Bangladesh.

Answer the farmer in simple Bangla if the question is in Bangla.
Give practical guidance about crops, cultivation, soil, irrigation,
fertilizers and plant diseases.

Do not invent facts. If you are unsure, say so.
For pesticide or chemical treatment, advise following the product
label and consulting a local agricultural officer when necessary.

Farmer's question:
{question}
";

            var response = await client.Models.GenerateContentAsync(
                model: model,
                contents: prompt
            );

            return response.Candidates?[0].Content?.Parts?[0].Text
                   ?? "দুঃখিত, এই মুহূর্তে উত্তর পাওয়া যায়নি। আবার চেষ্টা করুন।";
        }
    }
}
