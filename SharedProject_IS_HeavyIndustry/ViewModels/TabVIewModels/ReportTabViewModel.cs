using Avalonia;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

public class ReportTabViewModel : AvaloniaObject
{
    public string Title { get; } = "레포트 출력";
    public string SubTitle { get; } = "입력 데이터 필요";

    public ReportTabViewModel()
    {
        
    }

}