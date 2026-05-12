namespace WinDeployTool.Core.Models
{
    /// <summary>
    /// 系统信息模型
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OsVersion { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统版本号
        /// </summary>
        public Version OsVersionNumber { get; set; } = new Version();

        /// <summary>
        /// 系统架构
        /// </summary>
        public string Architecture { get; set; } = "x64";

        /// <summary>
        /// BIOS模式
        /// </summary>
        public BiosMode BiosMode { get; set; }

        /// <summary>
        /// 安全启动状态
        /// </summary>
        public SecureBootStatus SecureBoot { get; set; }

        /// <summary>
        /// TPM版本
        /// </summary>
        public string TpmVersion { get; set; } = "None";

        /// <summary>
        /// 计算机名
        /// </summary>
        public string ComputerName { get; set; } = string.Empty;

        /// <summary>
        /// 总物理内存(GB)
        /// </summary>
        public double TotalPhysicalMemory { get; set; }

        /// <summary>
        /// 磁盘列表
        /// </summary>
        public List<DiskInfo> Disks { get; set; } = new();
    }

    public enum BiosMode
    {
        Unknown,
        Legacy,
        Uefi
    }

    public enum SecureBootStatus
    {
        Unknown,
        NotSupported,
        Disabled,
        Enabled
    }
}
