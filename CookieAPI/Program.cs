namespace CookieAPI
{
    using System.Net.Http;
    using System.Net;
    using System.Threading.Tasks;
    using static CookieAPI.Program;
    using System.Text.Json;
    using System.Text;

    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private CookieContainer _cookieContainer; // To manage cookies

        public ApiClient(string baseAddress)
        {
            _cookieContainer = new CookieContainer();
            _httpClient = new HttpClient(new HttpClientHandler { CookieContainer = _cookieContainer })
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public async Task<HttpResponseMessage> AuthenticateAsync(string username, string password)
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            // Serialize the login request object to JSON
            var jsonPayload = JsonSerializer.Serialize(loginRequest);

            // Create StringContent with application/json content type
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Explicitly making a POST request to the login endpoint
            var response = await _httpClient.PostAsync("/api/v1/auth/login", content);

            return response;
        }



        // Updated method to fetch data from the specific endpoint
        public async Task<HttpResponseMessage> GetScheduleHierarchyAsync(int scheduleId)
        {
            return await _httpClient.GetAsync($"/api/v1/book-keeping/schedules/{scheduleId}/hierarchy");
        }



    }

    internal class Program
    {
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public static async Task Main(string[] args)
        {
            var apiClient = new ApiClient("http://122.165.178.192:8081");

            // --- Authentication Process ---
            Console.WriteLine("--- Authentication Process ---");
            var authResponse = await AuthenticateUserAsync(apiClient, "admin", "admin123"); // Replace with actual credentials

            if (authResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Authentication successful. Access token cookie should be set.");

                // --- Data Fetching Process ---
                Console.WriteLine("\n--- Data Fetching Process ---");
                await FetchScheduleDataAsync(apiClient, 6); // Fetch data for schedule ID 6
            }
            else
            {
                Console.WriteLine($"Authentication failed: {authResponse.StatusCode} - {await authResponse.Content.ReadAsStringAsync()}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // Separate function for the authentication process
        private static async Task<HttpResponseMessage> AuthenticateUserAsync(ApiClient apiClient, string username, string password)
        {
            return await apiClient.AuthenticateAsync(username, password);
        }

        // Separate function for fetching schedule data (assuming authentication was successful)
        private static async Task FetchScheduleDataAsync(ApiClient apiClient, int scheduleId)
        {
            var dataResponse = await apiClient.GetScheduleHierarchyAsync(scheduleId);
            if (dataResponse.IsSuccessStatusCode)
            {
                var data = await dataResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Data from schedule {scheduleId} hierarchy: {data}");
            }
            else
            {
                Console.WriteLine($"Failed to get data for schedule {scheduleId}: {dataResponse.StatusCode} - {await dataResponse.Content.ReadAsStringAsync()}");
            }
        }

      }
       
    }
