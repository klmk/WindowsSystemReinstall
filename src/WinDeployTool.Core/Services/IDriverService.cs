using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 驱动服务接口
    /// </summary>
    public interface IDriverService
    {
        /// <summary>
        /// 获取所有第三方驱动列表
        /// </summary>
        Task<List<DriverInfo>> GetThirdPartyDriversAsync();

        /// <summary>
        /// 备份选中的驱动
        /// </summary>
        Task<bool> BackupDriversAsync(List<string> driverNames, string backupPath);

        /// <summary>
        /// 将驱动注入到离线系统
        /// </summary>
        Task<bool> InjectDriversAsync(string offlineWindowsPath, string driversPath);

        /// <summary>
        /// 在在线系统中安装驱动
        /// </summary>
        Task<bool> InstallDriversAsync(string driversPath);

        /// <summary>
        /// 检查驱动兼容性
        /// </summary>
        Task<DriverCompatibilityResult> CheckCompatibilityAsync(string driverPath, string targetWindowsVersion);
    }

    /// <summary>
    /// 驱动兼容性检查结果
    /// </summary>
    public class DriverCompatibilityResult
    {
        public bool IsCompatible { get; set; }
        public string? WarningMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
