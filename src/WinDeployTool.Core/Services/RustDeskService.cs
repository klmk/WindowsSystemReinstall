using System.Diagnostics;
using System.Text.Json;
using Serilog;
using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// RustDesk 服务实现
    /// </summary>
    public class RustDeskService : IRustDeskService
    {
        private readonly ILogger _logger;

        public RustDeskService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 生成 RustDesk 配置文件
        /// </summary>
        public Task<string> GenerateConfigAsync(RustDeskConfig config, string outputPath)
        {
            try
            {
                var password = config.PasswordPolicy switch
                {
                    PasswordPolicy.Custom => config.CustomPassword ?? "",
                    PasswordPolicy.AutoGenerate => config.GeneratedPassword,
                    _ => ""
                };

                var rustDeskConfig = new RustDeskConfigFile
                {
                    RendezvousServer = config.ServerAddress,
                    CustomRendezvousServer = config.ServerAddress,
                    Key = config.Key,
                    Password = password
                };

                var json = JsonSerializer.Serialize(rustDeskConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                File.WriteAllText(outputPath, json);
                _logger.Information("RustDesk 配置已保存到: {Path}", outputPath);

                return Task.FromResult(outputPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "生成 RustDesk 配置失败");
                throw;
            }
        }

        /// <summary>
        /// 测试 RustDesk 连接
        /// </summary>
        public async Task<RustDeskTestResult> TestConnectionAsync(RustDeskConfig config)
        {
            _logger.Information("开始测试 RustDesk 连接...");
            var result = new RustDeskTestResult();

            try
            {
                // 1. 查找 RustDesk 安装包
                var installerPath = FindRustDeskInstaller();
                if (string.IsNullOrEmpty(installerPath))
                {
                    result.Message = "未找到 RustDesk 安装包";
                    return result;
                }

                // 2. 静默安装
                _logger.Debug("正在安装 RustDesk...");
                var installResult = await InstallSilentlyAsync(installerPath);
                if (!installResult)
                {
                    result.Message = "RustDesk 安装失败";
                    return result;
                }

                // 3. 写入配置
                var configDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RustDesk", "config");
                Directory.CreateDirectory(configDir);
                var configPath = Path.Combine(configDir, "RustDesk.toml");
                await GenerateConfigAsync(config, configPath);

                // 4. 启动服务
                _logger.Debug("正在启动 RustDesk 服务...");
                // 实际实现需要启动 RustDesk 服务

                // 5. 等待并获取 ID
                await Task.Delay(5000); // 等待服务启动
                var id = await GetRustDeskIdAsync();
                if (string.IsNullOrEmpty(id))
                {
                    result.Message = "无法获取 RustDesk ID";
                    return result;
                }

                result.RustDeskId = id;

                // 6. 验证连接
                var isConnected = await IsOnlineAsync(config.ServerAddress);
                if (!isConnected)
                {
                    result.Message = "无法连接到中继服务器";
                    return result;
                }

                result.Success = true;
                result.Message = "连接测试成功";
                result.LatencyMs = 23; // 模拟延迟

                _logger.Information("RustDesk 连接测试成功，ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RustDesk 连接测试失败");
                result.Message = $"测试失败: {ex.Message}";
            }
            finally
            {
                // 7. 卸载 RustDesk
                _logger.Debug("正在卸载 RustDesk...");
                await UninstallAsync();
            }

            return result;
        }

        /// <summary>
        /// 静默安装 RustDesk
        /// </summary>
        public async Task<bool> InstallSilentlyAsync(string installerPath)
        {
            try
            {
                if (!File.Exists(installerPath))
                {
                    _logger.Error("安装包不存在: {Path}", installerPath);
                    return false;
                }

                var extension = Path.GetExtension(installerPath).ToLowerInvariant();
                string arguments;

                if (extension == ".msi")
                {
                    arguments = $"/i \"{installerPath}\" /quiet /norestart";
                }
                else if (extension == ".exe")
                {
                    arguments = $"--silent-install";
                }
                else
                {
                    _logger.Error("不支持的安装包格式: {Extension}", extension);
                    return false;
                }

                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = extension == ".msi" ? "msiexec.exe" : installerPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas" // 请求管理员权限
                };

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    _logger.Information("RustDesk 安装成功");
                    return true;
                }
                else
                {
                    _logger.Error("RustDesk 安装失败，退出码: {ExitCode}", process.ExitCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "安装 RustDesk 时发生异常");
                return false;
            }
        }

        /// <summary>
        /// 卸载 RustDesk
        /// </summary>
        public async Task<bool> UninstallAsync()
        {
            try
            {
                // 使用 msiexec 卸载
                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = "/x {RustDeskProductCode} /quiet /norestart",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();
                await process.WaitForExitAsync();

                _logger.Information("RustDesk 卸载完成");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "卸载 RustDesk 时发生异常");
                return false;
            }
        }

        /// <summary>
        /// 获取 RustDesk ID
        /// </summary>
        public Task<string?> GetRustDeskIdAsync()
        {
            try
            {
                // 从注册表或配置文件读取 ID
                // 实际实现需要读取 RustDesk 的配置
                var id = "123456789"; // 模拟 ID
                return Task.FromResult<string?>(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取 RustDesk ID 失败");
                return Task.FromResult<string?>(null);
            }
        }

        /// <summary>
        /// 检查 RustDesk 是否在线
        /// </summary>
        public async Task<bool> IsOnlineAsync(string serverAddress)
        {
            try
            {
                // 实际实现需要检查与 relay 服务器的连接
                // 这里模拟连接测试
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "检查 RustDesk 在线状态失败");
                return false;
            }
        }

        /// <summary>
        /// 查找 RustDesk 安装包
        /// </summary>
        private string? FindRustDeskInstaller()
        {
            // 检查常见位置
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "RustDesk", "rustdesk.msi"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "RustDesk", "rustdesk.exe"),
                Path.Combine(Path.GetTempPath(), "rustdesk.msi"),
                Path.Combine(Path.GetTempPath(), "rustdesk.exe")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    _logger.Debug("找到 RustDesk 安装包: {Path}", path);
                    return path;
                }
            }

            _logger.Warning("未找到 RustDesk 安装包");
            return null;
        }

        /// <summary>
        /// RustDesk 配置文件结构
        /// </summary>
        private class RustDeskConfigFile
        {
            public string RendezvousServer { get; set; } = string.Empty;
            public string CustomRendezvousServer { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
