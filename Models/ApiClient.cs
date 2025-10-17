using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace tpfred2.Models
{
    public class ApiClient : IDisposable
    {
        private readonly string _urlBaseApi;
        private readonly HttpClient _httpClient;

        public string? LastBody { get; private set; } 
        public ApiClient(string urlBaseApi)
        {
            // S'assurer qu'il y a TOUJOURS un slash final
            if (!urlBaseApi.EndsWith('/'))
                urlBaseApi += '/';
            _urlBaseApi = urlBaseApi;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }


        public void SetHttpRequestHeader(string header, string val)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(header))
                _httpClient.DefaultRequestHeaders.Remove(header);
            _httpClient.DefaultRequestHeaders.Add(header, val);
        }

        public async Task<string> RequeteGetAsync(string endpoint)
        {
            using var hrm = await _httpClient.GetAsync(_urlBaseApi + endpoint);
            LastBody = await hrm.Content.ReadAsStringAsync();
            hrm.EnsureSuccessStatusCode();
            return LastBody!;
        }

        public async Task<string> RequetePostJsonAsync(string endpoint, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var hrm = await _httpClient.PostAsync(_urlBaseApi + endpoint, content);
            LastBody = await hrm.Content.ReadAsStringAsync();
            hrm.EnsureSuccessStatusCode();
            return LastBody!;
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
