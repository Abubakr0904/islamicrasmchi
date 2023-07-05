using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

class Program
{
    // Telegram Bot API token
    private const string ApiToken = "6397873297:AAGFSnyG4fyb_92XZ5sGdq8jlaCd1-4LPQQ";

    // Pexels API key
    private const string PexelsApiKey = "U6srRnBlRpJTjKLpyE3dN5Ozp4C1MzOxHC25bCTF2D08saISmsbU8X11";

    // Pexels API endpoint
    private const string PexelsApiUrl = "https://api.pexels.com/v1/search?per_page=1&size=original";

    // Search query for Pexels
    private const string PexelsQuery = "nature";

    private const long groupId = -920967227;

    private static readonly string[] pexelsQueries = new string[]
    {
        "nature", "city", "food", "business", "technology", "office", "people", "travel", "landscape", "building",
        "beach", "car", "music", "fitness", "animal", "cat", "dog", "water", "forest", "mountain",
        "coffee", "book", "art", "flower", "family", "sky", "summer", "winter", "spring", "autumn",
        "sunset", "street", "home", "birthday", "party", "love", "friend", "work", "school", "vacation",
        "holiday", "wedding", "baby", "sport", "game", "movie", "musician", "artist", "fashion", "shopping",
        "health", "finance", "money", "sun", "moon", "star", "cloud", "rain", "storm", "fire",
        "light", "dark", "color", "black", "white", "red", "blue", "green", "yellow", "orange",
        "purple", "pink", "gray", "vintage", "retro", "urban", "modern", "abstract", "minimal", "concept",
        "happy", "sad", "funny", "serious", "creative", "inspiration", "success", "failure", "challenge", "relaxation"
    };

    private static readonly string[] islamicQueries = new string[]
    {
        "islamic", "Mecca", "Haj", "Koran", "Quran",
        "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha", "Ishaa",
        "Alhamdulillah", "Alhamdulilah", "Allahumma", "Allahu Akbar",
        "Zikr", "Subhanalloh", "MashaAlloh", "nature", "muslim", "muslimah",
        "islam", "islamia", "islamiya", "islamia", "islamia",
        "islamic", "islamic", "islamic", "islamic", "islamic", "islamic",
        "islamic", "islamic", "islamic", "islamic", "islamic", "islamic",
        "islamic love", "Aqso", "beatiful mosques",
        "mosque", "nature", "madinah", "islamic"
    };

    static async Task Main(string[] args)
    {
        while (true)
        {
            // Create a new HttpClient with the Pexels API key in the Authorization header
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", PexelsApiKey);

            // Send a GET request to the Pexels API with the search query
            var random = new Random().Next(islamicQueries.Length);
            var query = islamicQueries[random];
            var response = await httpClient.GetAsync($"{PexelsApiUrl}&query={query}");
            // var response = await httpClient.GetAsync($"{PexelsApiUrl}?query=islamic");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON response from Pexels API
            var responseData = JsonConvert.DeserializeObject<PexelsResponse>(responseContent);
            var count = responseData?.Photos?.Count ?? 0;
            if (count <= 0)
                continue;
            var randomPhoto = responseData?.Photos?[new Random().Next(count)];
            var photoUrl = randomPhoto?.Src?.Original;

            if (!string.IsNullOrEmpty(photoUrl))
            {
                // Create a new TelegramBotClient instance with the Telegram Bot API token
                var botClient = new TelegramBotClient(ApiToken);
                // Get the updates for the bot

                // Get the updates for the bot
                var updates = await botClient.GetUpdatesAsync();

                // Upload and set the new profile photo
                using (var photoStream = await httpClient.GetStreamAsync(photoUrl))
                {
                    var inputFile = new InputFileStream(photoStream);
                    await botClient.SetChatPhotoAsync(groupId, inputFile);
                }

                Console.WriteLine($"Profile photo changed successfully in theme {query}!");
            }
            else
            {
                Console.WriteLine("Failed to fetch a photo from Pexels API.");
            }

            await Task.Delay(TimeSpan.FromSeconds(180));
        }
    }

    // Classes for deserializing Pexels API response
    public class PexelsResponse
    {
        [JsonProperty("photos")]
        public List<PexelsPhoto> Photos { get; set; }
    }

    public class PexelsPhoto
    {
        [JsonProperty("src")]
        public PexelsPhotoSource Src { get; set; }
    }

    public class PexelsPhotoSource
{
    [JsonProperty("large")]
    public string Large { get; set; }

    [JsonProperty("original")]
    public string Original { get; set; }
}
}