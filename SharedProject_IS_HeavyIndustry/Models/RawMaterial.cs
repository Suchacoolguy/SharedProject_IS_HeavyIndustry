using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SkiaSharp;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class RawMaterial
    {
        public int Length
        {
            get;
        }

        public List<Part> PartsInside
        {
            get;
        }
        
        public Part insert_part(Part part)
        {
            PartsInside.Add(part);
            return part;
        }

        
        public List<Part> get_parts_inside()
        {
            return PartsInside;
        }
        
        public RawMaterial(int length)
        {
            this.Length = length;
            this.PartsInside = new List<Part>();
        }

        public bool add_part(Part part)
        {
            if (part.Num > 0)
            {
                PartsInside.Add(part);
                return true;
            }
            else
            {
                System.Console.Write("part is not available.");
                return false;
            }
        }

        public int get_remaining_length()
        {
            int remaining_length = Length;
            if (PartsInside == null)
            {
                return remaining_length;
            }
            else
            {
                foreach (var part in PartsInside)
                {
                    remaining_length -= part.Length;
                }
                return remaining_length;
            }
        }

        public void remove_all_parts()
        {
            PartsInside.Clear();
        }

        public void remove_part(int length)
        {
            var partToRemove = PartsInside.FirstOrDefault(part => part.Length == length);

            if(partToRemove != null)
            {
                PartsInside.Remove(partToRemove);
            }
            else
            {
                System.Console.WriteLine("No part found with given length.");
            }
        }
        
        public override string ToString()
        {
            string result = "Raw Length: " + Length + "\nParts inside the Raw Material:\n";

            foreach (var part in PartsInside)
            {
                result += "Part Length: " + part.Length + "\n";
            }

            result += "Remaining Length: " + get_remaining_length() +"\n";

            return result;
        }
        
        public SKBitmap GenerateBarChartImage()
        {
            int width = 480;
            int height = 27;
            SKBitmap image = new SKBitmap(width, height);

            int totalLength = Length;

            using (SKCanvas canvas = new SKCanvas(image))
            {
                canvas.Clear(SKColors.Transparent);
                SKPaint paint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 3,
                    Style = SKPaintStyle.Stroke
                };
                canvas.DrawRect(0, 0, width - 1, height - 1, paint);
            }

            using (SKCanvas canvas = new SKCanvas(image))
            {
                SKPaint paint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 2
                };

                int accumulatedLength = 0;
                int i = 1;
                int lineX = 0;

                foreach (var part in PartsInside)
                {
                    accumulatedLength += part.Length;

                    string label = i.ToString();
                    using (SKPaint textPaint = new SKPaint { TextSize = 10, IsAntialias = true, Color = SKColors.Black })
                    {
                        float x = lineX - textPaint.MeasureText(label) / 2 + 10;
                        float y = height / 2;
                        canvas.DrawText(label, x, y, textPaint);
                    }

                    lineX = (int)Math.Round((double)accumulatedLength / totalLength * (width - 2));
                    canvas.DrawLine(lineX, 0, lineX, height - 1, paint);

                    i++;
                }
            }
            return image;
        }
    }
}