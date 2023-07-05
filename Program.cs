using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats.Jpeg;
using Telegram.Bot;
using Telegram.Bot.Types;

class Program
{
    // Telegram Bot API token
    private const string ApiToken = "6397873297:AAGFSnyG4fyb_92XZ5sGdq8jlaCd1-4LPQQ";

    // Pexels API key
    private const string PexelsApiKey = "U6srRnBlRpJTjKLpyE3dN5Ozp4C1MzOxHC25bCTF2D08saISmsbU8X11";

    // Pexels API endpoint
    private const string PexelsApiUrl = "https://api.pexels.com/v1/search?size=original";

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
        "islamic", "Mecca", "Madina", "Haj", "Quran",
        "Fajr", "Dhuhr", "Asr", "Maghrib",
        "Alhamdulillah", "Allahu Akbar", "Subhanalloh", "MashaAlloh",
        "nature", "muslim", "muslimah",
        "islam", "Aqso", "mosque", "nature",
        "landscape", "buildings", "tower", "sky", "mountain",
        "cat", "water", "ocean", "sea", "lake", "river",
        "flower", "trees", "tree", "tarawih", "solat",
        "sunnah", "cloud", "kitten", "panda", "islamic",
        "galaxy", "space", "earth", "moon", "star",
        "twilight", "moonrise", "sunrise", "sunset", "moonlight",
        "waterfall", "forest"
    };
    private const int desiredWidth = 1000;
    private const int desiredHeight = 1000;

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

                // Upload and set the new profile photo
                using (var photoStream = await httpClient.GetStreamAsync(photoUrl))
                {
                    var resizedPhotoStream = ResizePhoto(photoStream, desiredWidth, desiredHeight);
                    var inputFile = new InputFileStream(resizedPhotoStream, "resized_photo.jpg");
                    await botClient.SetChatPhotoAsync(groupId, inputFile);
                }

                Console.WriteLine($"Profile photo changed successfully in theme {query}!");

                var updates = await botClient.GetUpdatesAsync();
                var lastUpdate = updates?.Length > 0 ? updates[updates.Length - 1] : null;
                if (lastUpdate?.Message != null)
                {
                    var messageId = lastUpdate.Message.MessageId;
                    await botClient.DeleteMessageAsync(groupId, messageId);
                }
            }
            else
            {
                Console.WriteLine("Failed to fetch a photo from Pexels API.");
            }

            await Task.Delay(TimeSpan.FromSeconds(100));
        }
    }

    // Resize the photo stream to the specified width and height
    public static Stream ResizePhoto(Stream photoStream, int width, int height)
    {
        using var image = Image.Load(photoStream);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Max
        }));

        var resizedPhotoStream = new MemoryStream();
        image.Save(resizedPhotoStream, new JpegEncoder());
        resizedPhotoStream.Position = 0;

        return resizedPhotoStream;
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