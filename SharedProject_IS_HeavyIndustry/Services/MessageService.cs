using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace SharedProject_IS_HeavyIndustry.Services;

public class MessageService
{
    public static void Send(string msg)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("알림", msg, ButtonEnum.Ok);
        box.ShowAsync();
    }
}