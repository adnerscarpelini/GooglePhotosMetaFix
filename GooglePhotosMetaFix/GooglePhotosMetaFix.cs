using Newtonsoft.Json.Linq;
using System.Text;

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

        int successCount = 0;
        int failureCount = 0;
        string reportPath = Path.Combine(destinationDirectory, "GooglePhotosMetaFix_Report.csv");

        using (StreamWriter report = new StreamWriter(reportPath, false, Encoding.UTF8))
        {
            // Write CSV header
            report.WriteLine("File Name;Status;Error Message");

            // Process each media file
            foreach (var mediaFile in mediaFiles)
            {
                bool success = ProcessMediaFile(mediaFile, sourceDirectory, destinationDirectory, report);
                if (success) successCount++;
                else failureCount++;
            }

            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine($"Total files processed: {mediaFiles.Count}");
            Console.WriteLine($"✅ Successfully processed: {successCount}");
            Console.WriteLine($"❌ Failed to process: {failureCount}");
            Console.WriteLine("Processing complete. Report saved at: " + reportPath);
        }
    }

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

    private static bool ProcessMediaFile(string mediaFile, string sourceRoot, string destinationRoot, StreamWriter report)
    {
        Console.WriteLine($"Processing: {Path.GetFileName(mediaFile)}");

        string jsonFile = FindMetadataFile(mediaFile);
        if (jsonFile == null)
        {
            Console.WriteLine("No metadata file found.");
            report.WriteLine($"{Path.GetFileName(mediaFile)};Failure;No metadata file found");
            return false;
        }

        Console.WriteLine($"Found metadata file: {Path.GetFileName(jsonFile)}");

        DateTime? photoTakenDate = ExtractPhotoTakenDate(jsonFile);
        if (photoTakenDate == null)
        {
            Console.WriteLine("Could not extract capture date.");
            report.WriteLine($"{Path.GetFileName(mediaFile)};Failure;Could not extract capture date");
            return false;
        }

        Console.WriteLine($"Applying capture date: {photoTakenDate}");

        // Apply metadata to file timestamps
        ApplyMetadataToFile(mediaFile, photoTakenDate.Value);

        // Copy file to destination while preserving folder structure
        string relativePath = Path.GetRelativePath(sourceRoot, mediaFile);
        string destinationFile = Path.Combine(destinationRoot, relativePath);

        try
        {
            // Ensure the destination directory exists
            string? destinationFolder = Path.GetDirectoryName(destinationFile);
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Copy the file
            File.Copy(mediaFile, destinationFile, true);
            Console.WriteLine("File copied successfully!");
            report.WriteLine($"{Path.GetFileName(mediaFile)};Success;");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying file: {ex.Message}");
            report.WriteLine($"{Path.GetFileName(mediaFile)};Failure;{ex.Message}");
            return false;
        }
    }

    private static string? FindMetadataFile(string mediaFile)
    {
        string directory = Path.GetDirectoryName(mediaFile)!;
        string fileNameWithoutExtension = Path.GetFileName(mediaFile);

        return Directory.GetFiles(directory, $"{fileNameWithoutExtension}*.json").FirstOrDefault();
    }

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
            Console.WriteLine($"Error reading metadata file: {ex.Message}");
            return null;
        }
    }

    private static void ApplyMetadataToFile(string mediaFile, DateTime captureDate)
    {
        try
        {
            File.SetCreationTime(mediaFile, captureDate);
            File.SetLastWriteTime(mediaFile, captureDate);
            Console.WriteLine("Metadata applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying metadata: {ex.Message}");
        }
    }
}
