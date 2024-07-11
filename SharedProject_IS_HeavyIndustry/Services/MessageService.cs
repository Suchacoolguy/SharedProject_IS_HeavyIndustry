using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Task = DocumentFormat.OpenXml.Office2021.DocumentTasks.Task;

namespace SharedProject_IS_HeavyIndustry.Services;

public static class MessageService
{
    public static void Send(string msg)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("알림", msg, ButtonEnum.Ok);
        box.ShowAsync();
    }

    public static async Task<bool> SendWithAnswer(string msg)
    {
        var result = await sendWithTask(msg);
        return result.ToString().Equals("Yes");
    }

    private static async Task<ButtonResult> sendWithTask(string msg)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("알림", msg, ButtonEnum.YesNo);
        var result = await box.ShowAsync();
    
        return result;
    }
}