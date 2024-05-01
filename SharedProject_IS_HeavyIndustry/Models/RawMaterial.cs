using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SkiaSharp;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class RawMaterial
    {
        public int Length;
        public List<Part> parts_inside;

        public RawMaterial(int length)
        {
            this.Length = length;
            this.parts_inside = new List<Part>();
        }

        public bool add_part(Part part)
        {
            if (part.Num > 0)
            {
                parts_inside.Add(part);
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
            if (parts_inside == null)
            {
                return remaining_length;
            }
            else
            {
                foreach (var part in parts_inside)
                {
                    remaining_length -= part.Length;
                }
                return remaining_length;
            }
        }

        public void remove_all_parts()
        {
            parts_inside.Clear();
        }

        public void remove_part(int length)
        {
            var partToRemove = parts_inside.FirstOrDefault(part => part.Length == length);

            if(partToRemove != null)
            {
                parts_inside.Remove(partToRemove);
            }
            else
            {
                System.Console.WriteLine("No part found with given length.");
            }
        }

        public List<Part> get_parts_inside()
        {
            return parts_inside;
        }
        
        public override string ToString()
        {
            string result = "Raw Length: " + Length + "\nParts inside the Raw Material:\n";

            foreach (var part in parts_inside)
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

                foreach (var part in parts_inside)
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