namespace WinDeployTool.Core.Models
{
    /// <summary>
    /// 镜像信息模型
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// 镜像索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 镜像名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 镜像描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Windows版本
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 架构
        /// </summary>
        public string Architecture { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        public string BuildNumber { get; set; } = string.Empty;

        /// <summary>
        /// 安装类型
        /// </summary>
        public ImageEdition Edition { get; set; }

        /// <summary>
        /// 镜像大小(字节)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 是否推荐版本
        /// </summary>
        public bool IsRecommended { get; set; }
    }

    public enum ImageEdition
    {
        Unknown,
        Home,
        Pro,
        Enterprise,
        Education,
        HomeSingleLanguage,
        ProForWorkstations,
        ProEducation,
        EnterpriseN,
        IoTEnterprise
    }

    /// <summary>
    /// 镜像文件信息
    /// </summary>
    public class ImageFileInfo
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 镜像格式
        /// </summary>
        public ImageFormat Format { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// SHA256哈希值
        /// </summary>
        public string Sha256Hash { get; set; } = string.Empty;

        /// <summary>
        /// 包含的镜像列表
        /// </summary>
        public List<ImageInfo> Images { get; set; } = new();
    }

    public enum ImageFormat
    {
        Unknown,
        ISO,
        WIM,
        ESD
    }
}
