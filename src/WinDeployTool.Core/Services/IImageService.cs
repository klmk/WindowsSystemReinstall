using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 镜像服务接口
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// 获取镜像文件信息
        /// </summary>
        Task<ImageFileInfo> GetImageInfoAsync(string imagePath);

        /// <summary>
        /// 从ISO提取WIM文件
        /// </summary>
        Task<string> ExtractWimFromIsoAsync(string isoPath, string outputDirectory);

        /// <summary>
        /// 验证镜像文件完整性
        /// </summary>
        Task<bool> VerifyImageAsync(string imagePath);

        /// <summary>
        /// 计算文件SHA256哈希
        /// </summary>
        Task<string> CalculateSha256Async(string filePath);

        /// <summary>
        /// 应用镜像到目标分区
        /// </summary>
        Task<bool> ApplyImageAsync(string imagePath, int imageIndex, string targetDrive);

        /// <summary>
        /// 获取镜像中的应用列表
        /// </summary>
        Task<List<ImageInfo>> GetImageEditionsAsync(string imagePath);
    }
}
