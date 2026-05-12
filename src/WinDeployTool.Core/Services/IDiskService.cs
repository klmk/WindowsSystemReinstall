using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 磁盘服务接口
    /// </summary>
    public interface IDiskService
    {
        /// <summary>
        /// 获取所有磁盘信息
        /// </summary>
        Task<List<DiskInfo>> GetDisksAsync();

        /// <summary>
        /// 获取指定磁盘的分区信息
        /// </summary>
        Task<List<PartitionInfo>> GetPartitionsAsync(int diskIndex);

        /// <summary>
        /// 创建分区布局（保留数据分区，重建系统分区）
        /// </summary>
        Task<bool> CreatePartitionLayoutAsync(int diskIndex, int targetPartitionIndex);

        /// <summary>
        /// 格式化分区
        /// </summary>
        Task<bool> FormatPartitionAsync(int diskIndex, int partitionIndex, string fileSystem = "NTFS");

        /// <summary>
        /// 检查分区是否有用户数据
        /// </summary>
        Task<bool> HasUserDataAsync(int diskIndex, int partitionIndex);
    }
}
