using System.Text.Json;
using MaterialSkin.Controls;

namespace ToolsForOffice.DailyTasks.Forms
{
    public partial class HomePageForm : MaterialForm
    {
        private static readonly HttpClient client = new();

        public HomePageForm()
        {
            InitializeComponent();
            string apiKey = "21d566ff98366dd488938f59cf6d1376";
            string city = "Budapest";
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                string json = response.Content.ReadAsStringAsync().Result;

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                double temperature = root.GetProperty("main").GetProperty("temp").GetDouble();
                string conditions = root.GetProperty("weather")[0].GetProperty("main").GetString()!;
                ConditionsLabel.Text = conditions;
                CelsiusLabel.Text = $"{temperature}°C";
            }
            catch (HttpRequestException)
            {
                // Handle the situation when the request fails
                ConditionsLabel.Text = "Weather data is not available";
                CelsiusLabel.Text = "";
            }
            catch (JsonException)
            {
                // Handle the situation when the JSON data is invalid
                ConditionsLabel.Text = "Weather data is not available";
                CelsiusLabel.Text = "";
            }
        }
    }
}
