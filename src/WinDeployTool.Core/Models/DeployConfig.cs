namespace WinDeployTool.Core.Models
{
    /// <summary>
    /// 部署配置模型
    /// </summary>
    public class DeployConfig
    {
        /// <summary>
        /// 目标系统语言
        /// </summary>
        public string Language { get; set; } = "zh-CN";

        /// <summary>
        /// 镜像文件路径
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// 镜像索引
        /// </summary>
        public int ImageIndex { get; set; } = 1;

        /// <summary>
        /// 目标磁盘索引
        /// </summary>
        public int TargetDiskIndex { get; set; }

        /// <summary>
        /// 目标分区索引
        /// </summary>
        public int TargetPartitionIndex { get; set; }

        /// <summary>
        /// 计算机名
        /// </summary>
        public string ComputerName { get; set; } = "DESKTOP-PC";

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = "Administrator";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 是否备份驱动
        /// </summary>
        public bool BackupDrivers { get; set; }

        /// <summary>
        /// 选中的驱动列表
        /// </summary>
        public List<string> SelectedDrivers { get; set; } = new();

        /// <summary>
        /// RustDesk配置
        /// </summary>
        public RustDeskConfig RustDesk { get; set; } = new();

        /// <summary>
        /// 是否备份IP配置
        /// </summary>
        public bool BackupNetworkConfig { get; set; } = true;

        /// <summary>
        /// 部署前执行的脚本路径
        /// </summary>
        public string? PreDeployScript { get; set; }

        /// <summary>
        /// 部署中执行的脚本路径
        /// </summary>
        public string? MidDeployScript { get; set; }

        /// <summary>
        /// 部署后执行的脚本路径
        /// </summary>
        public string? PostDeployScript { get; set; }
    }

    /// <summary>
    /// RustDesk配置
    /// </summary>
    public class RustDeskConfig
    {
        /// <summary>
        /// 中继服务器地址
        /// </summary>
        public string ServerAddress { get; set; } = string.Empty;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 21117;

        /// <summary>
        /// 密钥
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 密码策略
        /// </summary>
        public PasswordPolicy PasswordPolicy { get; set; } = PasswordPolicy.AutoGenerate;

        /// <summary>
        /// 自定义密码
        /// </summary>
        public string? CustomPassword { get; set; }

        /// <summary>
        /// 生成的密码
        /// </summary>
        public string GeneratedPassword { get; set; } = string.Empty;
    }

    public enum PasswordPolicy
    {
        Custom,
        AutoGenerate,
        None
    }
}
