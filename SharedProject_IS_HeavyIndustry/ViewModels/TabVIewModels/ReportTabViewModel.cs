using Avalonia;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabViewModels
{
    public class ReportTabViewModel : AvaloniaObject
    {
        public string Title { get; } = "레포트 출력";
        public string SubTitle { get; } = "입력 데이터 필요";

        private static int _width;
        public static int Width 
        { 
            get => _width; 
            set
            {
                if (_width != value && IsTextAllowed(value.ToString()))
                {
                    _width = value;
                    Console.WriteLine($"Width changed to: {_width}");
                    OnStaticPropertyChanged(nameof(Width));
                }
            }
        }

        private static int _height;
        public static int Height 
        { 
            get => _height; 
            set
            {
                if (_height != value && IsTextAllowed(value.ToString()))
                {
                    _height = value;
                    Console.WriteLine($"Height changed to: {_height}");
                    OnStaticPropertyChanged(nameof(Height));
                }
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); // 정수만 허용
            return !regex.IsMatch(text);
        }

        public ReportTabViewModel()
        {
            InitializeDimensions();
        }

        private void InitializeDimensions()
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(appDirectory, "Assets", "imageSize.txt");

            if (!Directory.Exists(Path.Combine(appDirectory, "Assets")))
            {
                Directory.CreateDirectory(Path.Combine(appDirectory, "Assets"));
            }

            if (!File.Exists(filePath))
            {
                Width = 430;
                Height = 320;

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"Width={Width}");
                    writer.WriteLine($"Height={Height}");
                }
            }
            else
            {
                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            if (parts[0] == "Width" && int.TryParse(parts[1], out var width))
                            {
                                Width = width;
                            }
                            else if (parts[0] == "Height" && int.TryParse(parts[1], out var height))
                            {
                                Height = height;
                            }
                        }
                    }
                }
            }
        }
    }
}
