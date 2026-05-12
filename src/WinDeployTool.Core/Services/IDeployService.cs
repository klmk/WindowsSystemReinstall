using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 部署服务接口
    /// </summary>
    public interface IDeployService
    {
        /// <summary>
        /// 开始部署
        /// </summary>
        Task<DeployResult> DeployAsync(DeployConfig config, IProgress<DeployProgress>? progress = null);

        /// <summary>
        /// 取消部署
        /// </summary>
        void CancelDeploy();

        /// <summary>
        /// 验证部署配置
        /// </summary>
        Task<ValidationResult> ValidateConfigAsync(DeployConfig config);

        /// <summary>
        /// 准备部署环境（阶段1）
        /// </summary>
        Task<PhaseResult> PrepareAsync(DeployConfig config, IProgress<DeployProgress>? progress = null);

        /// <summary>
        /// 执行WinPE部署（阶段2）
        /// </summary>
        Task<PhaseResult> ExecuteWinPEAsync(DeployConfig config, IProgress<DeployProgress>? progress = null);

        /// <summary>
        /// 验证部署结果（阶段4）
        /// </summary>
        Task<PhaseResult> ValidateAsync(DeployConfig config, IProgress<DeployProgress>? progress = null);
    }

    /// <summary>
    /// 部署进度
    /// </summary>
    public class DeployProgress
    {
        /// <summary>
        /// 当前阶段
        /// </summary>
        public DeployPhase Phase { get; set; }

        /// <summary>
        /// 总体进度百分比
        /// </summary>
        public int OverallPercent { get; set; }

        /// <summary>
        /// 阶段进度百分比
        /// </summary>
        public int PhasePercent { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 详细日志
        /// </summary>
        public string? LogDetail { get; set; }
    }

    /// <summary>
    /// 配置验证结果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
