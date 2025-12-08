namespace ActivityProvider.Services
{
    public class AuthService
    {
        public int GetUserIdByApiKey(string apiKey)
        {
            // Simula autenticação via API Key do revisor.
            return Random.Shared.Next();
        }
        public int GetUserIdByExternalUserId(string userId)
        {
            // Simula autenticação via API Key do revisor.
            return Random.Shared.Next();
        }
    }
}
