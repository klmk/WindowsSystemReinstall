namespace WinDeployTool.Core.Models
{
    /// <summary>
    /// 磁盘信息模型
    /// </summary>
    public class DiskInfo
    {
        /// <summary>
        /// 磁盘索引号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 磁盘型号
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// 磁盘大小(字节)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 磁盘大小(GB)
        /// </summary>
        public double SizeGB => Math.Round(Size / (1024.0 * 1024.0 * 1024.0), 2);

        /// <summary>
        /// 接口类型
        /// </summary>
        public string InterfaceType { get; set; } = string.Empty;

        /// <summary>
        /// 分区表类型
        /// </summary>
        public PartitionStyle PartitionStyle { get; set; }

        /// <summary>
        /// 分区列表
        /// </summary>
        public List<PartitionInfo> Partitions { get; set; } = new();

        /// <summary>
        /// 是否系统磁盘
        /// </summary>
        public bool IsSystemDisk { get; set; }
    }

    /// <summary>
    /// 分区信息模型
    /// </summary>
    public class PartitionInfo
    {
        /// <summary>
        /// 分区索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 分区类型
        /// </summary>
        public PartitionType Type { get; set; }

        /// <summary>
        /// 文件系统
        /// </summary>
        public string FileSystem { get; set; } = string.Empty;

        /// <summary>
        /// 分区大小(字节)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 分区大小(GB)
        /// </summary>
        public double SizeGB => Math.Round(Size / (1024.0 * 1024.0 * 1024.0), 2);

        /// <summary>
        /// 盘符
        /// </summary>
        public string DriveLetter { get; set; } = string.Empty;

        /// <summary>
        /// 是否系统分区
        /// </summary>
        public bool IsSystemPartition { get; set; }

        /// <summary>
        /// 是否包含用户数据
        /// </summary>
        public bool HasUserData => !IsSystemPartition && Type == PartitionType.Primary;

        /// <summary>
        /// 分区标签
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }

    public enum PartitionStyle
    {
        Unknown,
        MBR,
        GPT
    }

    public enum PartitionType
    {
        Unknown,
        Primary,
        Extended,
        Logical,
        ESP,        // EFI System Partition
        MSR,        // Microsoft Reserved
        Recovery
    }
}
