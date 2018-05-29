using System;
using Microsoft.Extensions.CommandLineUtils;
using System.IO;

namespace JustinCredible.BlogPhotoResizer
{
    class Program
    {
        private static void Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.Name = "bpr";
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            app.Command("resize", (command) =>
            {
                command.Description = "Resizes images in the given directory, creates thumbnails, and emits HTML markup.";
                command.HelpOption("-?|-h|--help");

                var inputPathArg = command.Argument("[path]", "Directory containing images to resize; a directory named 'output' will be created as a subdirectory here as well.");

                var forceOption = command.Option("-f|--force", "Overwrites the output directory if it already exists.", CommandOptionType.NoValue);
                var galleryNameOption = command.Option("-gn|--gallery-name", "The name of the gallery; used for the name of the absolute path in the markup.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var inputPath = inputPathArg.Value;

                    if (String.IsNullOrEmpty(inputPath))
                    {
                        ShowError("An input path must be provided.", app, "resize");
                        return 1;
                    }

                    var outputPath = Path.Combine(inputPath, "output");
                    var force = forceOption.HasValue();
                    var galleryName = galleryNameOption.HasValue() ? galleryNameOption.Value() : "new-gallery";

                    Console.WriteLine("Performing image resize...");
                    Console.WriteLine("Input directory: " + inputPath);
                    Console.WriteLine("Output directory: " + outputPath);

                    try
                    {
                        Resizer.Resize(inputPath, outputPath, force, galleryName);
                    }
                    catch (Exception exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("An unhandled exception occurred!");
                        Console.WriteLine(exception.Message);
                        Console.WriteLine(exception.ToString());
                        return 1;
                    }

                    Console.WriteLine("Operation completed.");
                    return 0;
                });
            });

            app.Execute(args);
        }

        private static void ShowError(string message, CommandLineApplication app = null, string command = null)
        {
            var originalForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {message}");

            Console.ForegroundColor = originalForegroundColor;

            if (app != null && !String.IsNullOrEmpty(command))
                app.ShowHelp("resize");
        }
    }
}
