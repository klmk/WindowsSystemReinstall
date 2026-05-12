using System.Text.RegularExpressions;
using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 验证服务
    /// </summary>
    public class ValidationService
    {
        private readonly List<ValidationRule> _rules = new();

        public ValidationService()
        {
            InitializeRules();
        }

        /// <summary>
        /// 验证部署配置
        /// </summary>
        public ValidationResult ValidateDeployConfig(DeployConfig config)
        {
            var result = new ValidationResult { IsValid = true };

            foreach (var rule in _rules)
            {
                var error = rule.Validate(config);
                if (error != null)
                {
                    result.IsValid = false;
                    result.Errors.Add(error);
                }
            }

            // 添加警告
            AddWarnings(config, result);

            return result;
        }

        /// <summary>
        /// 验证镜像文件
        /// </summary>
        public async Task<ValidationResult> ValidateImageAsync(string imagePath)
        {
            var result = new ValidationResult { IsValid = true };

            // 检查文件是否存在
            if (!File.Exists(imagePath))
            {
                result.IsValid = false;
                result.Errors.Add("镜像文件不存在");
                return result;
            }

            // 检查文件扩展名
            var extension = Path.GetExtension(imagePath).ToLowerInvariant();
            if (!new[] { ".iso", ".wim", ".esd" }.Contains(extension))
            {
                result.IsValid = false;
                result.Errors.Add("不支持的镜像格式，仅支持 ISO、WIM、ESD");
                return result;
            }

            // 检查文件大小
            var fileInfo = new FileInfo(imagePath);
            if (fileInfo.Length < 1024 * 1024 * 100) // 小于 100MB
            {
                result.IsValid = false;
                result.Errors.Add("镜像文件过小，可能已损坏");
                return result;
            }

            // 检查文件是否可读
            try
            {
                using var stream = File.OpenRead(imagePath);
                var buffer = new byte[1024];
                await stream.ReadAsync(buffer, 0, 1024);
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"无法读取镜像文件: {ex.Message}");
                return result;
            }

            return result;
        }

        /// <summary>
        /// 验证 RustDesk 配置
        /// </summary>
        public ValidationResult ValidateRustDeskConfig(RustDeskConfig config)
        {
            var result = new ValidationResult { IsValid = true };

            // 验证服务器地址
            if (string.IsNullOrWhiteSpace(config.ServerAddress))
            {
                result.IsValid = false;
                result.Errors.Add("RustDesk 服务器地址不能为空");
            }

            // 验证端口
            if (config.Port <= 0 || config.Port > 65535)
            {
                result.IsValid = false;
                result.Errors.Add("RustDesk 端口必须在 1-65535 之间");
            }

            // 验证密码策略
            switch (config.PasswordPolicy)
            {
                case PasswordPolicy.Custom:
                    if (string.IsNullOrWhiteSpace(config.CustomPassword))
                    {
                        result.IsValid = false;
                        result.Errors.Add("自定义密码不能为空");
                    }
                    else if (config.CustomPassword.Length < 6)
                    {
                        result.Warnings.Add("自定义密码长度建议不少于6位");
                    }
                    break;

                case PasswordPolicy.AutoGenerate:
                    if (string.IsNullOrWhiteSpace(config.GeneratedPassword))
                    {
                        result.Warnings.Add("未生成密码，将在部署时自动生成");
                    }
                    break;

                case PasswordPolicy.None:
                    result.Warnings.Add("未设置密码，将使用 RustDesk 自动生成的临时密码");
                    break;
            }

            return result;
        }

        /// <summary>
        /// 验证磁盘和分区选择
        /// </summary>
        public ValidationResult ValidateDiskSelection(DiskInfo disk, PartitionInfo partition)
        {
            var result = new ValidationResult { IsValid = true };

            if (disk == null)
            {
                result.IsValid = false;
                result.Errors.Add("未选择目标磁盘");
                return result;
            }

            if (partition == null)
            {
                result.IsValid = false;
                result.Errors.Add("未选择目标分区");
                return result;
            }

            // 检查分区大小
            if (partition.SizeGB < 20)
            {
                result.IsValid = false;
                result.Errors.Add("目标分区空间不足，至少需要 20GB");
            }
            else if (partition.SizeGB < 50)
            {
                result.Warnings.Add("目标分区空间较小，建议至少 50GB");
            }

            // 检查是否为系统分区
            if (partition.IsSystemPartition && partition.Type != PartitionType.Primary)
            {
                result.Warnings.Add("选择的分区可能是系统保留分区，请确认");
            }

            return result;
        }

        /// <summary>
        /// 验证账户配置
        /// </summary>
        public ValidationResult ValidateAccountConfig(string computerName, string username, string password)
        {
            var result = new ValidationResult { IsValid = true };

            // 验证计算机名
            if (string.IsNullOrWhiteSpace(computerName))
            {
                result.IsValid = false;
                result.Errors.Add("计算机名不能为空");
            }
            else if (!Regex.IsMatch(computerName, @"^[a-zA-Z0-9\-]{1,15}$"))
            {
                result.IsValid = false;
                result.Errors.Add("计算机名只能包含字母、数字和连字符，长度不超过15位");
            }

            // 验证用户名
            if (string.IsNullOrWhiteSpace(username))
            {
                result.IsValid = false;
                result.Errors.Add("用户名不能为空");
            }

            // 验证密码
            if (string.IsNullOrWhiteSpace(password))
            {
                result.Warnings.Add("密码为空，将使用空密码自动登录");
            }
            else if (password.Length < 6)
            {
                result.Warnings.Add("密码长度较短，建议至少6位");
            }

            return result;
        }

        private void InitializeRules()
        {
            // 镜像路径规则
            _rules.Add(new ValidationRule
            {
                Name = "ImagePath",
                Validate = config =>
                {
                    if (string.IsNullOrWhiteSpace(config.ImagePath))
                        return "镜像文件路径不能为空";
                    // 文件存在性检查移到单独的 ValidateImageAsync 方法中
                    return null;
                }
            });

            // 镜像索引规则
            _rules.Add(new ValidationRule
            {
                Name = "ImageIndex",
                Validate = config =>
                {
                    if (config.ImageIndex < 1)
                        return "镜像索引必须大于0";
                    return null;
                }
            });

            // 目标磁盘规则
            _rules.Add(new ValidationRule
            {
                Name = "TargetDisk",
                Validate = config =>
                {
                    if (config.TargetDiskIndex < 0)
                        return "未选择目标磁盘";
                    return null;
                }
            });

            // 目标分区规则
            _rules.Add(new ValidationRule
            {
                Name = "TargetPartition",
                Validate = config =>
                {
                    if (config.TargetPartitionIndex < 0)
                        return "未选择目标分区";
                    return null;
                }
            });

            // 计算机名规则
            _rules.Add(new ValidationRule
            {
                Name = "ComputerName",
                Validate = config =>
                {
                    if (string.IsNullOrWhiteSpace(config.ComputerName))
                        return "计算机名不能为空";
                    if (!Regex.IsMatch(config.ComputerName, @"^[a-zA-Z0-9\-]{1,15}$"))
                        return "计算机名格式不正确";
                    return null;
                }
            });

            // 用户名规则
            _rules.Add(new ValidationRule
            {
                Name = "Username",
                Validate = config =>
                {
                    if (string.IsNullOrWhiteSpace(config.Username))
                        return "用户名不能为空";
                    return null;
                }
            });
        }

        private void AddWarnings(DeployConfig config, ValidationResult result)
        {
            // 驱动备份警告
            if (config.BackupDrivers && !config.SelectedDrivers.Any())
            {
                result.Warnings.Add("已启用驱动备份但未选择任何驱动");
            }

            // 自定义脚本警告
            if (!string.IsNullOrEmpty(config.PreDeployScript) && !File.Exists(config.PreDeployScript))
            {
                result.Warnings.Add("部署前脚本文件不存在");
            }
            if (!string.IsNullOrEmpty(config.MidDeployScript) && !File.Exists(config.MidDeployScript))
            {
                result.Warnings.Add("部署中脚本文件不存在");
            }
            if (!string.IsNullOrEmpty(config.PostDeployScript) && !File.Exists(config.PostDeployScript))
            {
                result.Warnings.Add("部署后脚本文件不存在");
            }
        }
    }

    /// <summary>
    /// 验证规则
    /// </summary>
    public class ValidationRule
    {
        public string Name { get; set; } = string.Empty;
        public Func<DeployConfig, string?> Validate { get; set; } = _ => null;
    }
}
