namespace KrishiBondhu___Smart_Agricultural_Assistance_Platform.Services
{
    public interface IAIAssistantService
    {
        Task<string> AskAsync(string question);
    }
}