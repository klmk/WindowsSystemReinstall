using System.Text.Json;
using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configDirectory;
        private readonly string _historyDirectory;

        public ConfigManager(string baseDirectory)
        {
            _configDirectory = Path.Combine(baseDirectory, "Config");
            _historyDirectory = Path.Combine(baseDirectory, "History");
            
            Directory.CreateDirectory(_configDirectory);
            Directory.CreateDirectory(_historyDirectory);
        }

        /// <summary>
        /// 保存部署配置
        /// </summary>
        public async Task SaveDeployConfigAsync(DeployConfig config, string configName)
        {
            var filePath = Path.Combine(_configDirectory, $"{configName}.json");
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// 加载部署配置
        /// </summary>
        public async Task<DeployConfig?> LoadDeployConfigAsync(string configName)
        {
            var filePath = Path.Combine(_configDirectory, $"{configName}.json");
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<DeployConfig>(json);
        }

        /// <summary>
        /// 获取所有保存的配置名称
        /// </summary>
        public IEnumerable<string> GetSavedConfigNames()
        {
            var files = Directory.GetFiles(_configDirectory, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f));
        }

        /// <summary>
        /// 删除配置
        /// </summary>
        public void DeleteConfig(string configName)
        {
            var filePath = Path.Combine(_configDirectory, $"{configName}.json");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// 创建部署历史记录
        /// </summary>
        public async Task<string> CreateDeploymentHistoryAsync(DeployConfig config, DeployResult result)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            var historyDir = Path.Combine(_historyDirectory, timestamp);
            Directory.CreateDirectory(historyDir);

            // 保存配置
            var configPath = Path.Combine(historyDir, "config.json");
            var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configPath, configJson);

            // 保存结果
            var resultPath = Path.Combine(historyDir, "result.json");
            var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(resultPath, resultJson);

            return historyDir;
        }

        /// <summary>
        /// 获取所有部署历史
        /// </summary>
        public IEnumerable<DeploymentHistory> GetDeploymentHistory()
        {
            var dirs = Directory.GetDirectories(_historyDirectory);
            foreach (var dir in dirs.OrderByDescending(d => d))
            {
                var resultPath = Path.Combine(dir, "result.json");
                if (File.Exists(resultPath))
                {
                    var json = File.ReadAllText(resultPath);
                    var result = JsonSerializer.Deserialize<DeployResult>(json);
                    if (result != null)
                    {
                        yield return new DeploymentHistory
                        {
                            Timestamp = Directory.GetCreationTime(dir),
                            Directory = dir,
                            Success = result.Success,
                            Duration = result.Duration,
                            TargetSystem = result.RustDeskInfo?.Id ?? "Unknown"
                        };
                    }
                }
            }
        }

        /// <summary>
        /// 加载历史部署配置
        /// </summary>
        public async Task<DeployConfig?> LoadHistoryConfigAsync(string historyDir)
        {
            var configPath = Path.Combine(historyDir, "config.json");
            if (!File.Exists(configPath))
                return null;

            var json = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<DeployConfig>(json);
        }
    }

    /// <summary>
    /// 部署历史记录
    /// </summary>
    public class DeploymentHistory
    {
        public DateTime Timestamp { get; set; }
        public string Directory { get; set; } = string.Empty;
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
        public string TargetSystem { get; set; } = string.Empty;
    }
}
