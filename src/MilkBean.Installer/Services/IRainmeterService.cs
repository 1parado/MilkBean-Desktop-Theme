namespace MilkBean.Installer.Services;

public interface IRainmeterService
{
    bool IsAvailable();
    Task DeploySkinsAsync(string themeRainmeterRoot);
    Task ActivateSkinAsync(string skinRelativePath);
    Task DeactivateSkinAsync(string skinRelativePath);
    Task DeactivateAllMilkBeanSkinsAsync();
    Task<RainmeterStateSnapshot> CaptureStateAsync();
    Task RestoreStateAsync(RainmeterStateSnapshot snapshot);
}

public sealed class RainmeterStateSnapshot
{
    public List<string> ActiveMilkBeanSkins { get; set; } = new();
    public string? SkinsRoot { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
}
