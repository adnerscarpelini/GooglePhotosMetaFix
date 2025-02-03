using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== GooglePhotosMetaFix ===");

        // Ask the user for the Google Takeout root directory
        Console.Write("Enter the Google Takeout export directory path: ");
        string sourceDirectory = Console.ReadLine() ?? string.Empty;

        // Validate if the directory exists
        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine("Error: The specified directory does not exist.");
            return;
        }

        // Ask the user for the destination directory
        Console.Write("Enter the destination directory path: ");
        string destinationDirectory = Console.ReadLine() ?? string.Empty;

        // Validate if the destination directory exists, if not, create it
        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        Console.WriteLine("Scanning for media files...");

        // Supported media file extensions
        string[] mediaExtensions =
         {
            // Images
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".heic", ".heif", 

            // Audio
            ".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg", ".opus", ".amr", ".aiff", ".mid", ".midi",

            // Videos
            ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".3gp", ".3g2"
        };

        // Get all media files recursively
        var mediaFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
            .Where(file => mediaExtensions.Contains(Path.GetExtension(file).ToLower()))
            .ToList();

        Console.WriteLine($"Found {mediaFiles.Count} media files.");

        // Process each media file
        foreach (var mediaFile in mediaFiles)
        {
            Console.WriteLine($"Processing: {Path.GetFileName(mediaFile)}");
            // TODO: Implement metadata restoration logic
        }

        Console.WriteLine("Processing complete.");
    }
}
