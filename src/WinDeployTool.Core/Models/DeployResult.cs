namespace WinDeployTool.Core.Models
{
    /// <summary>
    /// 部署结果模型
    /// </summary>
    public class DeployResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 错误码
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// 失败的阶段
        /// </summary>
        public DeployPhase? FailedPhase { get; set; }

        /// <summary>
        /// 部署开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 部署结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 总耗时
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// 各阶段结果
        /// </summary>
        public Dictionary<DeployPhase, PhaseResult> PhaseResults { get; set; } = new();

        /// <summary>
        /// RustDesk连接信息
        /// </summary>
        public RustDeskConnectionInfo? RustDeskInfo { get; set; }

        /// <summary>
        /// 网络配置恢复状态
        /// </summary>
        public bool NetworkConfigRestored { get; set; }

        /// <summary>
        /// 驱动安装数量
        /// </summary>
        public int DriversInstalled { get; set; }

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string LogPath { get; set; } = string.Empty;

        public static DeployResult CreateSuccess()
        {
            return new DeployResult { Success = true };
        }

        public static DeployResult CreateFailure(string errorMessage, string errorCode, DeployPhase phase)
        {
            return new DeployResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                FailedPhase = phase
            };
        }
    }

    /// <summary>
    /// 阶段结果
    /// </summary>
    public class PhaseResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 阶段耗时
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 阶段开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 阶段结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        public static PhaseResult CreateSuccess(TimeSpan duration)
        {
            return new PhaseResult { Success = true, Duration = duration };
        }

        public static PhaseResult CreateFailure(string errorMessage)
        {
            return new PhaseResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    /// <summary>
    /// RustDesk连接信息
    /// </summary>
    public class RustDeskConnectionInfo
    {
        /// <summary>
        /// RustDesk ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 连接密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServerAddress { get; set; } = string.Empty;
    }

    public enum DeployPhase
    {
        Preparation,
        WinPE,
        FirstBoot,
        Validation
    }
}
