# GooglePhotosMetaFix

GooglePhotosMetaFix is a console application written in C# with .NET Core that restores metadata for media files exported from Google Photos via Google Takeout. When exporting backups, Google Photos removes metadata such as capture date. This tool reads the corresponding `.json` supplemental metadata files and applies the correct timestamps to images and videos.

## Features
- Scans all subfolders within the Google Takeout export directory.
- Identifies media files (jpg, jpeg, gif, png, mp3, wave, etc.).
- Finds the corresponding JSON metadata file.
- Extracts and applies the original photo/video capture date.
- Maintains the folder structure when exporting corrected files.
- Generates a report summarizing successful and failed operations.

## Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR-USERNAME/GooglePhotosMetaFix.git
   cd GooglePhotosMetaFix
   ```
2. Ensure you have .NET Core installed.
3. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

## Usage
1. Run the application and provide the path to your Google Takeout export directory.
2. Specify the output directory where fixed files will be saved.
3. The tool will process files and generate a log report.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

