using System.Drawing;
using System.Drawing.Imaging;

namespace badappingStick
{
    public class Program
    {
        static int height;
        static int width;

        static List<string> light = new List<string>();
        static List<string> dark = new List<string>();

        static ParallelOptions po  = new ParallelOptions { MaxDegreeOfParallelism = 8 };

        public static void Main(string[] args)
        {
            Console.Write("Enter the directory with Bad Apple frames: ");

            string jerry = Console.ReadLine() ?? "";

            if (!Directory.Exists(jerry))
            {
                Console.WriteLine("invalid frames directory!");
                return;
            }

            DirectoryInfo appleFrameFolder = new DirectoryInfo(jerry);

            Console.Write("Enter the directory with images to tile: ");

            string koharu = Console.ReadLine() ?? "";

            if (!Directory.Exists(koharu))
            {
                Console.WriteLine("invalid image directory!");
                return;
            }

            DirectoryInfo imagesFolder = new DirectoryInfo(koharu);

            Console.WriteLine("Please wait...\n");

            foreach (FileInfo image in imagesFolder.GetFiles())
            {
                Bitmap img = new Bitmap(Image.FromFile(image.FullName));

                int colorSum = 0;

                int w = img.Width;
                int h = img.Height;

                bool saveNeeded = false;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color jack = img.GetPixel(x, y);

                        if (jack.A < 5)
                        {
                            saveNeeded = true;
                            img.SetPixel(x, y, Color.White);
                            colorSum += 255;
                            continue;
                        }

                        colorSum += (jack.R + jack.B + jack.G) / 3;
                    }
                }

                if (saveNeeded)
                {
                    img.Save(imagesFolder.FullName + @"/" + image.Name + ".bmp");
                    if (colorSum / (w * h) > 127)
                    {
                        light.Add(image.FullName + ".bmp");
                    }
                    else
                    {
                        dark.Add(image.FullName + ".bmp");
                    }
                }
                else
                {
                    if (colorSum / (w * h) > 127)
                    {
                        light.Add(image.FullName);
                    }
                    else
                    {
                        dark.Add(image.FullName);
                    }

                }

                img.Dispose();
            }


            Console.Write("Enter the directory to output Bad Apple frames: ");

            string lita = Console.ReadLine() ?? "";

            if (!Directory.Exists(lita))
            {
                Console.WriteLine("invalid output directory!");
                return;
            }

            DirectoryInfo outputFolder = new DirectoryInfo(lita);

            Console.Write("Enter the number of images to tile vertically: ");

            Bitmap thing = new Bitmap(appleFrameFolder.GetFiles().First().FullName);

            double height2;
            bool result = double.TryParse(Console.ReadLine(), out height2);

            if (!result)
            {
                Console.WriteLine("Invalid height!");
                return;
            }

            double ratio = height2 / thing.Height;
            width = (int)Math.Round(thing.Width * ratio);

            string[] paths = appleFrameFolder.GetFiles().Select(a => a.FullName).ToArray();

            thing.Dispose();


            List<string[]> paths2 = new List<string[]>();

            height = (int)height2;

            Console.WriteLine($"Running the big thing [{width} by {height}]");

            /*
            int times = (int)Math.Ceiling(paths.Length / 5.0);

            for (int i = 0; i < times; i++)
            {
                paths2.Add(paths.Skip(i * 5).Take(5).ToArray());
            }

            foreach (string[] e in paths2)
            {

            }
            */

            Parallel.ForEach(paths, po, path => generateFrame(path, outputFolder));
        }

        static void generateFrame(string path, DirectoryInfo outputFolder)
        {
            string name = path.Split("\\").Last();

            Image i = Image.FromFile(path);
            Bitmap inputFrame = new Bitmap(i, width, height);
            i.Dispose();

            Bitmap output = new Bitmap(width * 32, height * 32);

            Console.WriteLine("Initialized " + name);

            Graphics g = Graphics.FromImage(output);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = inputFrame.GetPixel(x, y);
                    Random random = new Random();

                    if ((c.R + c.G + c.B) / 3 > 127)
                    {
                        Bitmap image = new Bitmap(Image.FromFile(light[random.Next(0, light.Count)]), 32, 32);

                        g.DrawImage(image, x * 32, y * 32);

                        image.Dispose();
                    }
                    else
                    {
                        Bitmap image = new Bitmap(Image.FromFile(dark[random.Next(0, dark.Count)]), 32, 32);

                        g.DrawImage(image, x * 32, y * 32);

                        image.Dispose();
                    }
                }
            }

            inputFrame.Dispose();
            g.Dispose();

            Console.WriteLine("Finished " + name);

            output.Save(outputFolder.FullName + @"/" + name);

            output.Dispose();
        }
    }
}
