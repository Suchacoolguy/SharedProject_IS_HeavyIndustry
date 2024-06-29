using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using SkiaSharp;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class RawMaterial : INotifyPropertyChanged
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
        
        public IBrush RectangleColor
        {
            get;
            set;
        }

        public int _RemainingLength
        {
            get;
            set;
        }
        
        public int RemainingLength
        {
            get { return _RemainingLength; }
            set
            {
                if (_RemainingLength != value)
                {
                    _RemainingLength = value;
                    if (_RemainingLength < 0)
                        RectangleColor = new SolidColorBrush(Colors.Red);
                    else
                        RectangleColor = new SolidColorBrush(Colors.Black);
                    OnPropertyChanged(nameof(RectangleColor)); // Notify that RectangleColor has changed
                    OnPropertyChanged(nameof(RemainingLength)); // Notify that RemainingLength has changed
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public Part insert_part(Part part)
        {
            PartsInside.Add(part);
            RemainingLength -= part.Length;
            return part;
        }
        
        // public Part remove_part(Part part)
        // {
        //     PartsInside.Remove(part);
        //     RemainingLength += part.Length;
        //     return part;
        // }
        
        public void remove_part_at(int part_index)
        {
            if (part_index >= 0 && part_index < PartsInside.Count)
            {
                int length = PartsInside[part_index].Length;
                PartsInside.RemoveAt(part_index);
                RemainingLength += length;   
            }
            else
            {
                Console.WriteLine("파트 인덱스 문제많다.");
                Console.WriteLine("파트 인덱스: " + part_index);
            }
        }
        
        public int GetTotalLengthOfPartsInside()
        {
            return PartsInside.Sum(part => part.Length);
        }
        
        
        public ObservableCollection<Part> get_parts_inside()
        {
            return PartsInside;
        }
        
        public RawMaterial(int length)
        {
            this.Length = length;
            this.PartsInside = new ObservableCollection<Part>();
            this.RemainingLength = length;
        }
        
        public override string ToString()
        {
            string result = "Raw Length: " + Length + "\nParts inside the Raw Material:\n";

            foreach (var part in PartsInside)
            {
                result += "Part Length: " + part.Length + "\n";
            }

            result += "Remaining Length: " + RemainingLength +"\n";

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