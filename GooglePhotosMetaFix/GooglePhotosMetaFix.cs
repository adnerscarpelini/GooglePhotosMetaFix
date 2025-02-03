using Newtonsoft.Json.Linq;

class GooglePhotosMetaFix
{
    static void Main()
    {
        Console.WriteLine("=== GooglePhotosMetaFix ===");

        // Get user directories
        string sourceDirectory = GetDirectory("Enter the Google Takeout export directory path: ");
        string destinationDirectory = GetDirectory("Enter the destination directory path: ", createIfNotExists: true);

        Console.WriteLine("Scanning for media files...");

        var mediaFiles = GetMediaFiles(sourceDirectory);
        Console.WriteLine($"Found {mediaFiles.Count} media files.");

        // Process each media file
        foreach (var mediaFile in mediaFiles)
        {
            ProcessMediaFile(mediaFile);
        }

        Console.WriteLine("Processing complete.");
    }

    /// <summary>
    /// Asks the user for a directory path and validates it.
    /// </summary>
    private static string GetDirectory(string message, bool createIfNotExists = false)
    {
        Console.Write(message);
        string directory = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            if (createIfNotExists)
            {
                Directory.CreateDirectory(directory);
                return directory;
            }

            Console.WriteLine("Error: The specified directory does not exist.");
            Environment.Exit(1);
        }

        return directory;
    }

    /// <summary>
    /// Retrieves a list of media files from the source directory.
    /// </summary>
    private static List<string> GetMediaFiles(string sourceDirectory)
    {
        string[] mediaExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".heic", ".heif",
            ".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg", ".opus", ".amr", ".aiff", ".mid", ".midi",
            ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp", ".3g2"
        };

        return Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
            .Where(file => mediaExtensions.Contains(Path.GetExtension(file).ToLower()))
            .ToList();
    }

    /// <summary>
    /// Processes a media file by finding and applying metadata from its corresponding JSON file.
    /// </summary>
    private static void ProcessMediaFile(string mediaFile)
    {
        Console.WriteLine($"Processing: {Path.GetFileName(mediaFile)}");

        string jsonFile = FindMetadataFile(mediaFile);
        if (jsonFile == null)
        {
            Console.WriteLine("❌ No metadata file found.");
            return;
        }

        Console.WriteLine($"✅ Found metadata file: {Path.GetFileName(jsonFile)}");

        DateTime? photoTakenDate = ExtractPhotoTakenDate(jsonFile);
        if (photoTakenDate == null)
        {
            Console.WriteLine("⚠️ Could not extract capture date.");
            return;
        }

        Console.WriteLine($"📅 Applying capture date: {photoTakenDate}");

        // Apply metadata to file timestamps
        ApplyMetadataToFile(mediaFile, photoTakenDate.Value);
    }

    /// <summary>
    /// Finds the JSON metadata file corresponding to a media file.
    /// </summary>
    private static string? FindMetadataFile(string mediaFile)
    {
        string directory = Path.GetDirectoryName(mediaFile)!;
        string fileNameWithoutExtension = Path.GetFileName(mediaFile);

        return Directory.GetFiles(directory, $"{fileNameWithoutExtension}*.json").FirstOrDefault();
    }

    /// <summary>
    /// Extracts the photo taken date from the JSON metadata file.
    /// </summary>
    private static DateTime? ExtractPhotoTakenDate(string jsonFile)
    {
        try
        {
            string jsonContent = File.ReadAllText(jsonFile);
            JObject json = JObject.Parse(jsonContent);

            string? timestamp = json["photoTakenTime"]?["timestamp"]?.ToString();
            return timestamp != null
                ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).UtcDateTime
                : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error reading metadata file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Applies extracted metadata (capture date) to the media file.
    /// </summary>
    private static void ApplyMetadataToFile(string mediaFile, DateTime captureDate)
    {
        try
        {
            File.SetCreationTime(mediaFile, captureDate);
            File.SetLastWriteTime(mediaFile, captureDate);
            Console.WriteLine("✅ Metadata applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error applying metadata: {ex.Message}");
        }
    }
}
