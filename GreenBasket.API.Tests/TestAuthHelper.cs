using System.Net.Http;
using System.Net.Http.Headers;

namespace GreenBasket.API.Tests
{
    public static class TestAuthHelper
    {
        public static void AddBearerToken(this HttpClient client, string role = "Customer", string userId = "test-user-id")
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{role}|{userId}");
        }
    }
}