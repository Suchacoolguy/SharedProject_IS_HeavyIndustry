using Avalonia;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

public class ExcelTabViewModel : AvaloniaObject
{
    public string Title { get; } = "프로젝트 정보";
    public string SubTitle { get; } = "프로젝트의 기본 생성 정보를 표시합니다.\n엑셀 원본으로부터 부재 정보를 분석하여 배치를 위한 정보를 구축합니다";

    public ExcelTabViewModel()
    {
        
    }
    
}