namespace WinDeployTool.Core.Models
{
    /// <summary>
    /// 驱动信息模型
    /// </summary>
    public class DriverInfo
    {
        /// <summary>
        /// 驱动名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 驱动版本
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 厂商
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// 驱动类
        /// </summary>
        public string DriverClass { get; set; } = string.Empty;

        /// <summary>
        /// 发布日期
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// INF文件路径
        /// </summary>
        public string InfPath { get; set; } = string.Empty;

        /// <summary>
        /// 是否已选中备份
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否为微软内置驱动
        /// </summary>
        public bool IsMicrosoftDriver => 
            Manufacturer.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) ||
            InfPath.Contains("windows", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 设备描述
        /// </summary>
        public string DeviceDescription { get; set; } = string.Empty;
    }
}
