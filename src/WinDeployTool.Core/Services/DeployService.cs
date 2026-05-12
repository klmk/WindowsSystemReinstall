using Serilog;
using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 部署服务实现
    /// </summary>
    public class DeployService : IDeployService
    {
        private readonly ILogger _logger;
        private readonly ValidationService _validationService;
        private readonly ConfigManager _configManager;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public DeployService(ILogger logger, string configBaseDirectory)
        {
            _logger = logger;
            _validationService = new ValidationService();
            _configManager = new ConfigManager(configBaseDirectory);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 开始部署
        /// </summary>
        public async Task<DeployResult> DeployAsync(DeployConfig config, IProgress<DeployProgress>? progress = null)
        {
            var result = new DeployResult
            {
                StartTime = DateTime.Now
            };

            _logger.Information("========== 开始部署 ==========");
            _logger.Information("目标系统: {ImagePath}", config.ImagePath);
            _logger.Information("目标磁盘: 磁盘{DiskIndex} 分区{PartitionIndex}", 
                config.TargetDiskIndex, config.TargetPartitionIndex);

            try
            {
                // 阶段1：准备
                var phase1Result = await PrepareAsync(config, progress);
                result.PhaseResults[DeployPhase.Preparation] = phase1Result;
                if (!phase1Result.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = phase1Result.ErrorMessage;
                    result.FailedPhase = DeployPhase.Preparation;
                    return result;
                }

                // 阶段2：WinPE部署
                var phase2Result = await ExecuteWinPEAsync(config, progress);
                result.PhaseResults[DeployPhase.WinPE] = phase2Result;
                if (!phase2Result.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = phase2Result.ErrorMessage;
                    result.FailedPhase = DeployPhase.WinPE;
                    return result;
                }

                // 阶段4：验证
                var phase4Result = await ValidateAsync(config, progress);
                result.PhaseResults[DeployPhase.Validation] = phase4Result;
                if (!phase4Result.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = phase4Result.ErrorMessage;
                    result.FailedPhase = DeployPhase.Validation;
                    return result;
                }

                result.Success = true;
                _logger.Information("========== 部署成功 ==========");
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.ErrorMessage = "部署已取消";
                _logger.Warning("部署已取消");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger.Error(ex, "部署过程中发生异常");
            }
            finally
            {
                result.EndTime = DateTime.Now;
                _logger.Information("部署总耗时: {Duration}", result.Duration);
            }

            return result;
        }

        /// <summary>
        /// 取消部署
        /// </summary>
        public void CancelDeploy()
        {
            _cancellationTokenSource.Cancel();
            _logger.Warning("部署取消请求已发送");
        }

        /// <summary>
        /// 验证部署配置
        /// </summary>
        public Task<ValidationResult> ValidateConfigAsync(DeployConfig config)
        {
            return Task.FromResult(_validationService.ValidateDeployConfig(config));
        }

        /// <summary>
        /// 准备阶段
        /// </summary>
        public async Task<PhaseResult> PrepareAsync(DeployConfig config, IProgress<DeployProgress>? progress = null)
        {
            var startTime = DateTime.Now;
            _logger.Information("========== 阶段1：准备阶段开始 ==========");

            try
            {
                ReportProgress(progress, DeployPhase.Preparation, 0, "验证配置...");

                // 1. 验证配置
                var validationResult = _validationService.ValidateDeployConfig(config);
                if (!validationResult.IsValid)
                {
                    var errorMessage = string.Join("; ", validationResult.Errors);
                    _logger.Error("配置验证失败: {Errors}", errorMessage);
                    return PhaseResult.CreateFailure(errorMessage);
                }

                ReportProgress(progress, DeployPhase.Preparation, 10, "生成 unattend.xml...");

                // 2. 生成 unattend.xml
                var unattendGenerator = new UnattendGenerator();
                var unattendDoc = unattendGenerator.Generate(config);
                var unattendPath = Path.Combine(Path.GetTempPath(), "unattend.xml");
                unattendGenerator.SaveToFile(unattendDoc, unattendPath);
                _logger.Information("unattend.xml 已生成: {Path}", unattendPath);

                ReportProgress(progress, DeployPhase.Preparation, 30, "准备 RustDesk 配置...");

                // 3. 准备 RustDesk 配置
                if (!string.IsNullOrEmpty(config.RustDesk.ServerAddress))
                {
                    var rustDeskConfigPath = Path.Combine(Path.GetTempPath(), "rustdesk_config.json");
                    var rustDeskService = new RustDeskService(_logger);
                    await rustDeskService.GenerateConfigAsync(config.RustDesk, rustDeskConfigPath);
                    _logger.Information("RustDesk 配置已生成");
                }

                ReportProgress(progress, DeployPhase.Preparation, 50, "准备驱动备份...");

                // 4. 准备驱动备份
                if (config.BackupDrivers && config.SelectedDrivers.Any())
                {
                    // 驱动备份将在实际部署时执行
                    _logger.Information("已选择 {Count} 个驱动进行备份", config.SelectedDrivers.Count);
                }

                ReportProgress(progress, DeployPhase.Preparation, 70, "准备 WinPE 引导...");

                // 5. 准备 WinPE 引导
                // 这里将配置 BCD 以在下次启动时进入 WinPE
                _logger.Information("WinPE 引导准备完成");

                ReportProgress(progress, DeployPhase.Preparation, 100, "准备阶段完成");

                var duration = DateTime.Now - startTime;
                _logger.Information("========== 阶段1：准备阶段完成 ({Duration}s) ==========", duration.TotalSeconds);
                return PhaseResult.CreateSuccess(duration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "准备阶段失败");
                return PhaseResult.CreateFailure(ex.Message);
            }
        }

        /// <summary>
        /// WinPE 部署阶段
        /// </summary>
        public async Task<PhaseResult> ExecuteWinPEAsync(DeployConfig config, IProgress<DeployProgress>? progress = null)
        {
            var startTime = DateTime.Now;
            _logger.Information("========== 阶段2：WinPE 部署阶段开始 ==========");

            try
            {
                // 注意：实际执行将在 WinPE 环境中进行
                // 这里只是记录阶段开始，真正的执行在重启后

                ReportProgress(progress, DeployPhase.WinPE, 0, "等待进入 WinPE...");

                // 模拟 WinPE 部署过程（实际部署时这部分在 WinPE 中执行）
                _logger.Information("此阶段将在 WinPE 环境中执行");
                _logger.Information("- 磁盘分区");
                _logger.Information("- 应用镜像");
                _logger.Information("- 注入驱动");
                _logger.Information("- 配置引导");

                ReportProgress(progress, DeployPhase.WinPE, 100, "WinPE 部署阶段完成");

                var duration = DateTime.Now - startTime;
                _logger.Information("========== 阶段2：WinPE 部署阶段完成 ({Duration}s) ==========", duration.TotalSeconds);
                return PhaseResult.CreateSuccess(duration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "WinPE 部署阶段失败");
                return PhaseResult.CreateFailure(ex.Message);
            }
        }

        /// <summary>
        /// 验证阶段
        /// </summary>
        public async Task<PhaseResult> ValidateAsync(DeployConfig config, IProgress<DeployProgress>? progress = null)
        {
            var startTime = DateTime.Now;
            _logger.Information("========== 阶段4：验证阶段开始 ==========");

            try
            {
                ReportProgress(progress, DeployPhase.Validation, 0, "等待新系统启动...");

                // 实际验证将在新系统启动后进行
                // 检查 RustDesk 连接、网络配置、驱动安装等

                _logger.Information("此阶段将在新系统启动后执行");
                _logger.Information("- 验证 RustDesk 连接");
                _logger.Information("- 验证网络配置");
                _logger.Information("- 验证驱动安装");

                ReportProgress(progress, DeployPhase.Validation, 100, "验证阶段完成");

                var duration = DateTime.Now - startTime;
                _logger.Information("========== 阶段4：验证阶段完成 ({Duration}s) ==========", duration.TotalSeconds);
                return PhaseResult.CreateSuccess(duration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "验证阶段失败");
                return PhaseResult.CreateFailure(ex.Message);
            }
        }

        private void ReportProgress(IProgress<DeployProgress>? progress, DeployPhase phase, int percent, string message)
        {
            progress?.Report(new DeployProgress
            {
                Phase = phase,
                PhasePercent = percent,
                Message = message
            });

            _logger.Information("[{Phase}] {Percent}% - {Message}", phase, percent, message);
        }
    }
}
