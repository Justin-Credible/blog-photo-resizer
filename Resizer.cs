using System;
using System.IO;
using SixLabors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.Primitives;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Collections.Generic;
using System.Text;

namespace JustinCredible.BlogPhotoResizer
{
    class Resizer
    {
        private const int FULL_LANDSCAPE_WIDTH = 2048;
        private const int FULL_LANDSCAPE_HEIGHT = 1536;

        private const int THUMBNAIL_LANDSCAPE_WIDTH = 160;
        private const int THUMBNAIL_LANDSCAPE_HEIGHT = 120;

        private const int FULL_PORTRAIT_WIDTH = 1536;
        private const int FULL_PORTRAIT_HEIGHT = 2048;

        private const int THUMBNAIL_PORTRAIT_WIDTH = 120;
        private const int THUMBNAIL_PORTRAIT_HEIGHT = 160;

        public static void Resize(string inputPath, string outputPath, bool forceOutput = false, string galleryName = "new-gallery", bool generateHtmlOnly = false)
        {
            // Ensure input directory exists.
            if (!Directory.Exists(inputPath))
                throw new ArgumentException("Could not locate the input path: " + inputPath);

            // Ensure output directory is ready.
            if (!generateHtmlOnly)
            {
                if (forceOutput)
                {
                    if (Directory.Exists(outputPath))
                    {
                        Directory.Delete(outputPath, true);
                    }
                }
                else
                {
                    if (Directory.Exists(outputPath))
                        throw new ArgumentException("The output path already exists. To overwrite, use the --force option.");
                }
            }

            // Create the output directory.
            Directory.CreateDirectory(outputPath);

            if (!generateHtmlOnly)
                Directory.CreateDirectory(Path.Combine(outputPath, "thumbnails"));

            // Get a list of all the jpg images at the top level only.
            var imageFilePaths = new List<string>(Directory.GetFiles(inputPath, "*.jpg", SearchOption.TopDirectoryOnly));

            imageFilePaths.Sort();

            // Sanity check.
            if (imageFilePaths.Count == 0)
                throw new ArgumentException("No *.jpg files found in the directory: " + inputPath);

            var totalCount = imageFilePaths.Count;

            Console.WriteLine($"Found {totalCount} images...");

            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<div class=\"photo-gallery\">");

            for (var index = 0; index < totalCount; index++)
            {
                var imageFilePath = imageFilePaths[index];
                var fileName = (new FileInfo(imageFilePath)).Name;

                Console.WriteLine($"Processing {index + 1} of {totalCount}: {fileName}");

                using (var image = Image.Load(imageFilePath))
                {
                    // Assume landscape
                    var fullHeight = FULL_LANDSCAPE_HEIGHT;
                    var fullWidth = FULL_LANDSCAPE_WIDTH;
                    var thumbnailHeight = THUMBNAIL_LANDSCAPE_HEIGHT;
                    var thumbnailWidth = THUMBNAIL_LANDSCAPE_WIDTH;

                    // Detect portrait
                    if (image.Width < image.Height)
                    {
                        fullHeight = FULL_PORTRAIT_HEIGHT;
                        fullWidth = FULL_PORTRAIT_WIDTH;
                        thumbnailHeight = THUMBNAIL_PORTRAIT_HEIGHT;
                        thumbnailWidth = THUMBNAIL_PORTRAIT_WIDTH;
                    }

                    if (!generateHtmlOnly)
                    {
                        var encoder = new JpegEncoder();

                        // Downscale the original image down to the target "full sized" image.

                        image.Mutate(x => x
                            .Resize(fullWidth, fullHeight));

                        encoder.Quality = 70;

                        var newFullSizedImagePath = Path.Combine(outputPath, fileName);

                        using (var outputStream = new FileStream(newFullSizedImagePath, FileMode.CreateNew))
                        {
                            image.SaveAsJpeg(outputStream, encoder);
                        }

                        // Downscale the original image down to the target thumbnail image.

                        image.Mutate(x => x
                            .Resize(thumbnailWidth, thumbnailHeight));

                        encoder.Quality = 80;

                        var newThumbnailImagePath = Path.Combine(outputPath, "thumbnails", fileName);

                        using (var outputStream = new FileStream(newThumbnailImagePath, FileMode.CreateNew))
                        {
                            image.SaveAsJpeg(outputStream, encoder);
                        }
                    }

                    // Build the HTML snippet for the thumbnail.
                    htmlBuilder.AppendLine($"    <a href=\"/content/images/galleries/{galleryName}/{fileName}\" title=\"\" data-width=\"{fullWidth}\" data-height=\"{fullHeight}\">");
                    htmlBuilder.AppendLine($"        <img src=\"/content/images/galleries/{galleryName}/thumbnails/{fileName}\" alt=\"\"/>");
                    htmlBuilder.AppendLine($"    </a>");
                }
            }

            // Finalize and write out the HTML.

            htmlBuilder.AppendLine("</div>");

            var htmlFilePath = Path.Combine(outputPath, "markup.html");
            File.WriteAllText(htmlFilePath, htmlBuilder.ToString());
        }
    }
}
