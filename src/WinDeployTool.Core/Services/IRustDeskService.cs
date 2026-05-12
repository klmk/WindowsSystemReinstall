using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// RustDesk服务接口
    /// </summary>
    public interface IRustDeskService
    {
        /// <summary>
        /// 生成RustDesk配置文件
        /// </summary>
        Task<string> GenerateConfigAsync(RustDeskConfig config, string outputPath);

        /// <summary>
        /// 测试RustDesk连接
        /// </summary>
        Task<RustDeskTestResult> TestConnectionAsync(RustDeskConfig config);

        /// <summary>
        /// 静默安装RustDesk
        /// </summary>
        Task<bool> InstallSilentlyAsync(string installerPath);

        /// <summary>
        /// 卸载RustDesk
        /// </summary>
        Task<bool> UninstallAsync();

        /// <summary>
        /// 获取RustDesk ID
        /// </summary>
        Task<string?> GetRustDeskIdAsync();

        /// <summary>
        /// 检查RustDesk是否在线
        /// </summary>
        Task<bool> IsOnlineAsync(string serverAddress);
    }

    /// <summary>
    /// RustDesk测试结果
    /// </summary>
    public class RustDeskTestResult
    {
        public bool Success { get; set; }
        public string? RustDeskId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? LatencyMs { get; set; }
    }
}
