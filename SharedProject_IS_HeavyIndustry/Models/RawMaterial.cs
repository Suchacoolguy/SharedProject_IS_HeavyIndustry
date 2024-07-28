using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using Avalonia.Threading;
using SkiaSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class RawMaterial : INotifyPropertyChanged
    {
        private int _Length; // Field for Length
        public int Length
        {
            get => _Length;
            set
            {
                if (_Length != value)
                {
                    _Length = value;
                    OnPropertyChanged(nameof(Length));
                }
            }
        }

        private int _TotalPartsLength; // Field for TotalPartsLength
        public int TotalPartsLength
        {
            get => _TotalPartsLength;
            set
            {
                if (_TotalPartsLength != value)
                {
                    _TotalPartsLength = value;
                    OnPropertyChanged(nameof(TotalPartsLength));
                }
            }
        }

        public ObservableCollection<Part> PartsInside { get; set; }

        private IBrush _rectangleColor; // Field for RectangleColor
        public IBrush RectangleColor
        {
            get => _rectangleColor;
            set
            {
                if (_rectangleColor != value)
                {
                    _rectangleColor = value;
                    OnPropertyChanged(nameof(RectangleColor));
                }
            }
        }

        private IBrush _backgroundColor; // Field for BackgroundColor
        public IBrush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    OnPropertyChanged(nameof(BackgroundColor));
                }
            }
        }

        private int _RemainingLength; // Field for RemainingLength
        public int RemainingLength
        {
            get => _RemainingLength;
            set
            {
                if (_RemainingLength != value)
                {
                    _RemainingLength = value;
                    UpdateColorsOnRemainingLengthChange(); // Call to update colors
                    OnPropertyChanged(nameof(RemainingLength));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateColorsOnRemainingLengthChange()
        {
            if (_RemainingLength < 0)
            {
                UpdateColorsOnUIThread(new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Pink));
            }
            else
            {
                UpdateColorsOnUIThread(new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Transparent));
            }
        }


        private void UpdateColorsOnUIThread(IBrush rectangleColor, IBrush backgroundColor)
        {
            Dispatcher.UIThread.Post(() =>
            {
                RectangleColor = rectangleColor;
                BackgroundColor = backgroundColor;
                OnPropertyChanged(nameof(RectangleColor));
                OnPropertyChanged(nameof(BackgroundColor));
            });
        }

        public Part insert_part(Part part)
        {
            PartsInside.Add(part);
            RemainingLength -= part.Length;
            TotalPartsLength += part.Length;
            if (PartsInside.Count > 1)
            {
                RemainingLength -= SettingsViewModel.CuttingLoss;
            }

            return part;
        }

        public void removePart(Part part)
        {
            if (PartsInside.Contains(part))
            {
                PartsInside.Remove(part);
                TotalPartsLength -= part.Length;
                RemainingLength += part.Length;
                if (PartsInside.Count >= 1)
                {
                    RemainingLength += SettingsViewModel.CuttingLoss;
                }
            }

            int fitLength = FindShortestLengthPossibleWhenRemovePart(part);
            if (fitLength != -1)
            {
                int lengthNeeded = Length - RemainingLength;
                Length = fitLength;
                RemainingLength = Length - lengthNeeded;
            }
        }

        private int FindShortestLengthPossibleWhenRemovePart(Part part)
        {
            List<int> lengthOptions = SettingsViewModel.GetLengthOption(part.Desc.ToString());

            int chosenLength = -1;
            if (lengthOptions.Any() && lengthOptions.Count > 1)
            {
                // Sorting in Descending order
                lengthOptions.Sort((a, b) => b.CompareTo(a));

                int lengthNeeded = Length - RemainingLength;
                int remaining = int.MaxValue;

                foreach (var len in lengthOptions)
                {
                    if (len >= lengthNeeded && len - lengthNeeded < remaining)
                    {
                        chosenLength = len;
                        remaining = len - lengthNeeded;
                    }
                }
            }

            return chosenLength != -1 ? chosenLength : -1;
        }

        public bool isAddingPossible(Part part)
        {
            return RemainingLength - part.Length - SettingsViewModel.CuttingLoss >= 0;
        }

        public int findPossibleRawLengthToIncrease(Part part)
        {
            // key의 형태가 "SS275,H194*150*6*9" 이런식이라 뒤에꺼만 따로 추출해야 함..
            string key = MainWindowViewModel.SelectedKey;
            key = key.Split(',')[1];

            List<int> lengthSet = SettingsViewModel.GetLengthOption(key);

            // Sorting in Descending order
            lengthSet.Sort((a, b) => b.CompareTo(a));
            int remainingLengthIfPartAdded = RemainingLength - part.Length - SettingsViewModel.CuttingLoss;

            int chosenLength = -1;
            if (lengthSet.Count > 1)
            {
                Console.WriteLine("Count Check Passed");
                foreach (var len in lengthSet)
                {
                    int lengthNeeded = (Length - RemainingLength) + part.Length + SettingsViewModel.CuttingLoss;
                    if (len >= lengthNeeded)
                    {
                        Console.WriteLine("Possible Length Check Passed");
                        chosenLength = len;
                    }
                }
            }

            return chosenLength == -1 ? -1 : chosenLength;
        }

        public bool increaseRawLength(int possibleLength, Part part)
        {
            string key = MainWindowViewModel.SelectedKey;
            key = key.Split(',')[1];

            List<int> lengthSet = SettingsViewModel.GetLengthOption(key);

            int lengthNeeded = (Length - RemainingLength) + part.Length + SettingsViewModel.CuttingLoss;
            int lengthExtended = possibleLength - Length;

            if (lengthSet.Contains(possibleLength))
            {
                Length = possibleLength;
                RemainingLength += lengthExtended;
                Console.WriteLine($"Remaining length after: {RemainingLength}");
                Console.WriteLine("성공?");
                // 추가 로직
            }

            return false;
        }

        public ObservableCollection<Part> get_parts_inside()
        {
            return PartsInside;
        }

        public RawMaterial(int length)
        {
            Length = length;
            PartsInside = new ObservableCollection<Part>();
            RemainingLength = length;
            TotalPartsLength = 0;
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
            int width = 430;
            int height = 15;
            SKBitmap image = new SKBitmap(width, height);

            int totalLength = Length;

            using (SKCanvas canvas = new SKCanvas(image))
            {
                canvas.Clear(SKColors.Transparent);
                SKPaint paint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 1, // 테두리 선의 굵기를 1로 변경
                    Style = SKPaintStyle.Stroke
                };
                canvas.DrawRect(0, 0, width - 1, height - 1, paint);
            }

            using (SKCanvas canvas = new SKCanvas(image))
            {
                SKPaint paint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 2, // 내부 선의 굵기는 1로 유지
                    IsAntialias = true // 안티앨리어싱 활성화
                };

                int accumulatedLength = 0;
                int i = 1;
                int lineX = 0;

                foreach (var part in PartsInside)
                {
                    accumulatedLength += part.Length;

                    // 파트 번호 텍스트 설정
                    string label = i.ToString();
                    SKPaint textPaint = new SKPaint
                    {
                        TextSize = 8, // 텍스트 크기를 작게 조정
                        IsAntialias = true, // 안티앨리어싱 활성화
                        Color = SKColors.Black,
                        TextAlign = SKTextAlign.Left,
                        Typeface = SKTypeface.FromFamilyName("Arial")
                    };

                    float textX = lineX + 4; // 텍스트를 왼쪽으로 조금 이동
                    float textY = height / 2 + textPaint.TextSize / 2; // 중앙에 위치

                    canvas.DrawText(label, textX, textY, textPaint);

                    // 선 그리기
                    lineX = (int)Math.Round((double)accumulatedLength / totalLength * (width - 2));
                    canvas.DrawLine(lineX, 0, lineX, height - 1, paint);

                    i++;
                }
            }
            return image;
        }

    }
}