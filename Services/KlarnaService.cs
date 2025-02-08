using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using E_handel.Payment.Config;
using E_handel.Payment.Interfaces;
using E_handel.Payment.Models;
using Microsoft.Extensions.Options;

namespace E_handel.Payment.Services
{
    public class KlarnaService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly KlarnaConfig _config;

        public KlarnaService(HttpClient httpClient, IOptions<KlarnaConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"))
            );
        }

        public async Task<string> CreateOrderAsync(OrderRequest order)
        {
            var klarnaOrder = new
            {
                purchase_country = "SE",
                purchase_currency = "SEK",
                locale = "sv-SE",
                order_amount = 10000,
                order_tax_amount = 2000,
                order_lines = new[]
                {
                    new
                    {
                        type = "physical",
                        reference = "test-1",
                        name = "Test product",
                        quantity = 1,
                        unit_price = 10000,
                        tax_rate = 2500,
                        total_amount = 10000,
                        total_tax_amount = 2000
                    }
                },
                merchant_urls = new
                {
                    terms = "https://special-mildly-bug.ngrok-free.app/terms",
                    checkout = "https://special-mildly-bug.ngrok-free.app/checkout",
                    confirmation = "https://special-mildly-bug.ngrok-free.app/confirmation",
                    push = "https://special-mildly-bug.ngrok-free.app/push"
                    // terms = "http://localhost:3000/terms",
                    // checkout = "http://localhost:3000/checkout",
                    // confirmation = "http://localhost:3000/confirmation",
                    // push = "http://localhost:3000/push"
                }
            };

            var response = await _httpClient.PostAsJsonAsync("checkout/v3/orders", klarnaOrder);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Klarna API error: {error}");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
            var orderId = responseContent.GetProperty("order_id").GetString();
            var htmlSnippet = responseContent.GetProperty("html_snippet").GetString();

            return JsonSerializer.Serialize(new
            {
                order_id = orderId,
                html_snippet = htmlSnippet
            });
        }
    }
}