using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 系统信息服务接口
    /// </summary>
    public interface ISystemInfoService
    {
        /// <summary>
        /// 获取当前系统信息
        /// </summary>
        Task<SystemInfo> GetSystemInfoAsync();

        /// <summary>
        /// 检测BIOS模式
        /// </summary>
        BiosMode DetectBiosMode();

        /// <summary>
        /// 检测安全启动状态
        /// </summary>
        SecureBootStatus DetectSecureBootStatus();

        /// <summary>
        /// 检测TPM版本
        /// </summary>
        string DetectTpmVersion();

        /// <summary>
        /// 检查目标系统兼容性
        /// </summary>
        CompatibilityResult CheckCompatibility(string targetWindowsVersion);
    }

    /// <summary>
    /// 兼容性检查结果
    /// </summary>
    public class CompatibilityResult
    {
        /// <summary>
        /// 是否兼容
        /// </summary>
        public bool IsCompatible { get; set; }

        /// <summary>
        /// 警告信息列表
        /// </summary>
        public List<CompatibilityWarning> Warnings { get; set; } = new();

        /// <summary>
        /// 错误信息列表
        /// </summary>
        public List<CompatibilityError> Errors { get; set; } = new();
    }

    public class CompatibilityWarning
    {
        public string Message { get; set; } = string.Empty;
        public string? Solution { get; set; }
    }

    public class CompatibilityError
    {
        public string Message { get; set; } = string.Empty;
        public string? Solution { get; set; }
    }
}
