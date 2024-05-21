using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            set;
        }

        public ObservableCollection<Part> PartsInside
        {
            get;
            set;
        }

        public int remaining_length
        {
            get;
            set;
        }
        
        public Part insert_part(Part part)
        {
            PartsInside.Add(part);
            remaining_length -= part.Length;
            return part;
        }
        
        
        public ObservableCollection<Part> get_parts_inside()
        {
            return PartsInside;
        }
        
        public RawMaterial(int length)
        {
            this.Length = length;
            this.PartsInside = new ObservableCollection<Part>();
            this.remaining_length = length;
        }

        

        public void remove_all_parts()
        {
            PartsInside.Clear();
        }
        
        public override string ToString()
        {
            string result = "Raw Length: " + Length + "\nParts inside the Raw Material:\n";

            foreach (var part in PartsInside)
            {
                result += "Part Length: " + part.Length + "\n";
            }

            result += "Remaining Length: " + remaining_length +"\n";

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