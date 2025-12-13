using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

class AsciiArt
{

    static readonly string Ramp = "@%`#$&?!*+=~/_-'.";

    static void Main(string[] args)
    { 
        if (args.Length == 0)
        {   Console.WriteLine("-------------------------------------------------------------------------------------------");
            Console.WriteLine("dotnet run [путь/к/файлу. (в кавычках)] [Ширина] [true/false] [Название получаемого файла.]");
            Console.WriteLine("-------------------------------------------------------------------------------------------");
            return;
        }

        string path = args[0];
        int targetWidth = args.Length >= 2 && int.TryParse(args[1], out var w) ? Math.Max(70, w) : 120;

        bool invert = args.Length >= 3 && bool.TryParse(args[2], out var iv) ? iv : false;
        string ?outFile = args.Length >= 4 ? args[3] : null;

        if (!File.Exists(path))
        {
            Console.WriteLine("File not found: " + path);
            return;
        }

        try
        {
            using (var image = Image.Load<Rgba32>(path))
            {   
                // Аспект: символы выше чем ширина, поэтому масштабируем по высоте меньше — корректируем ratio;
                double charAspect = 0.4; // символы примерно в 0.5 раз ниже ширины (или же в 2 раза выше);
                int targetHeight = (int)(image.Height * (targetWidth / (double)image.Width) * charAspect);

                image.Mutate(x => x.Resize(targetWidth, Math.Max(1, targetHeight)));

                var sb = new StringBuilder();

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Rgba32 px = image[x, y];
                        // Перевод в яркость: стандартная формула perceived luminance
                        double luminance = (0.2126 * px.R + 0.7152 * px.G + 0.0722 * px.B) / 255.0;
                        if (invert) luminance = 1.0 - luminance;
                        int idx = (int)Math.Round(luminance * (Ramp.Length - 1));
                        sb.Append(Ramp[idx]);
                    }
                    sb.AppendLine();
                }

                string result = sb.ToString();

                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine(result);

                if (!string.IsNullOrEmpty(outFile))
                {
                    File.WriteAllText(outFile, result, Encoding.UTF8);
                    Console.WriteLine("Saved to " + outFile);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}