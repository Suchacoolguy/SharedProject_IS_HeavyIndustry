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
        public int Length
        {
            get { return _Length; }
            set
            {
                if (_Length != value)
                {
                    _Length = value;
                }

                OnPropertyChanged(nameof(Length));
            }
        }


        private int _TotalPartsLength { get; set; }

        public int TotalPartsLength
        {
            get { return _TotalPartsLength; }
            set
            {
                if (_TotalPartsLength != value)
                {
                    _TotalPartsLength = value;
                }

                OnPropertyChanged(nameof(TotalPartsLength));
            }
        }

        public ObservableCollection<Part> PartsInside { get; set; }

        public IBrush RectangleColor { get; set; }

        public IBrush BackgroundColor { get; set; }

        private int _RemainingLength;

        public int RemainingLength
        {
            get { return _RemainingLength; }
            set
            {
                if (_RemainingLength != value)
                {
                    _RemainingLength = value;
                    if (_RemainingLength < 0)
                    {
                        RectangleColor = new SolidColorBrush(Colors.Red);
                        BackgroundColor = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        RectangleColor = new SolidColorBrush(Colors.Black);
                        BackgroundColor = new SolidColorBrush(Colors.Transparent);
                    }

                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(RectangleColor)); // Notify that RectangleColor has changed
                    OnPropertyChanged(nameof(RemainingLength)); // Notify that RemainingLength has changed
                }
            }
        }

        private int _Length { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Part insert_part(Part part)
        {
            PartsInside.Add(part);
            RemainingLength -= part.Length;
            TotalPartsLength += part.Length;
            if (PartsInside.Count > 1)
            {
                RemainingLength -= SettingsViewModel.CuttingLoss;
                part.LengthForUI += SettingsViewModel.CuttingLoss; // Part개수가 2개 이상이 추가된 파트 길이에 컷팅로스 추가
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

                part.LengthForUI = part.Length; // 파트 제외할 떄 UI용 Length 다시 초기화
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
                int remaining = Int32.MaxValue;

                foreach (var len in lengthOptions)
                {
                    if (len >= lengthNeeded && len - lengthNeeded < remaining)
                    {
                        chosenLength = len;
                        remaining = len - lengthNeeded;
                    }
                }
            }

            if (chosenLength != -1)
                return chosenLength;

            return -1;
        }

        public bool isAddingPossible(Part part)
        {
            if (RemainingLength - part.Length - SettingsViewModel.CuttingLoss >= 0)
                return true;

            return false;
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

            if (chosenLength == -1)
                return -1;

            return chosenLength;
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
            this.Length = length;
            this.PartsInside = new ObservableCollection<Part>();
            this.RemainingLength = length;
            this.TotalPartsLength = 0;
        }

        public override string ToString()
        {
            string result = "Raw Length: " + Length + "\nParts inside the Raw Material:\n";

            foreach (var part in PartsInside)
            {
                result += "Part Length: " + part.Length + "\n";
            }

            result += "Remaining Length: " + RemainingLength + "\n";

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
                SKPaint blackPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 2, // 내부 선의 굵기는 1로 유지
                    IsAntialias = true // 안티앨리어싱 활성화
                };

                SKPaint redPaint = new SKPaint
                {
                    Color = SKColors.Red,
                    StrokeWidth = 2, // 초과된 부분의 선 굵기
                    IsAntialias = true // 안티앨리어싱 활성화
                };

                int accumulatedLength = 0;
                int i = 1;
                int previousLineX = 0;

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

                    float textX = previousLineX + 4; // 텍스트를 왼쪽으로 조금 이동
                    float textY = height / 2 + textPaint.TextSize / 2; // 중앙에 위치

                    canvas.DrawText(label, textX, textY, textPaint);

                    // 선 그리기
                    int lineX = (int)Math.Round((double)accumulatedLength / totalLength * (width - 2));

                    if (lineX > width - 2) // 바깥 막대의 길이를 초과할 경우
                    {
                        // 초과하지 않은 부분은 검은색으로 그리기
                        if (previousLineX < width - 2)
                        {
                            canvas.DrawLine(previousLineX, 0, width - 2, height - 1, blackPaint);
                        }
                        // 초과된 부분은 빨간색으로 그리기
                        canvas.DrawLine(width - 2, 0, lineX, height - 1, redPaint);
                    }
                    else
                    {
                        canvas.DrawLine(lineX, 0, lineX, height - 1, blackPaint);
                    }

                    previousLineX = lineX;
                    i++;
                }
            }
            return image;
        }

    }
}