# Windows 自动部署工具 - 实现细节

## 一、开发环境准备

### 1.1 必需软件

| 软件 | 版本 | 用途 |
|------|------|------|
| Visual Studio 2022 | 17.8+ | 开发 IDE |
| .NET 8 SDK | 8.0.x | 运行时和编译器 |
| Windows ADK | 最新版 | 制作 WinPE |
| Windows PE Add-on | 对应 ADK 版本 | WinPE 组件 |
| Git | 任意 | 版本控制 |

### 1.2 ADK 安装步骤

1. 下载 Windows ADK：[微软官网](https://docs.microsoft.com/zh-cn/windows-hardware/get-started/adk-install)
2. 安装 ADK 核心组件
3. 安装 Windows PE 加载项
4. 验证安装：打开"部署和映像工具环境"命令提示符

### 1.3 WinPE 制作流程

```powershell
# 以管理员身份运行"部署和映像工具环境"

# 1. 创建工作目录
copype amd64 C:\WinPE_amd64

# 2. 挂载 WinPE 镜像（用于添加驱动等）
Dism /Mount-Image /ImageFile:"C:\WinPE_amd64\media\sources\boot.wim" /Index:1 /MountDir:"C:\WinPE_amd64\mount"

# 3. 添加驱动（可选，如需要 RAID 卡驱动）
Dism /Image:C:\WinPE_amd64\mount /Add-Driver /Driver:C:\Drivers\RAID /Recurse

# 4. 添加 PowerShell 支持（可选）
Dism /Image:C:\WinPE_amd64\mount /Add-Package /PackagePath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-WMI.cab"
Dism /Image:C:\WinPE_amd64\mount /Add-Package /PackagePath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-NetFX.cab"
Dism /Image:C:\WinPE_amd64\mount /Add-Package /PackagePath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-Scripting.cab"
Dism /Image:C:\WinPE_amd64\mount /Add-Package /PackagePath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-PowerShell.cab"

# 5. 复制部署脚本到 WinPE
xcopy /s C:\MyScripts\* C:\WinPE_amd64\mount\Scripts\

# 6. 提交更改并卸载镜像
Dism /Unmount-Image /MountDir:C:\WinPE_amd64\mount /Commit

# 7. 将制作好的 boot.wim 复制到项目目录
copy C:\WinPE_amd64\media\sources\boot.wim C:\Projects\WinDeployTool\Resources\WinPE\
```

## 二、核心功能实现

### 2.1 系统信息检测

**检测项**：
- 操作系统版本
- BIOS 模式（UEFI/Legacy）
- 安全启动状态
- TPM 版本
- 磁盘信息

**实现要点**：

```csharp
// 使用 WMI 查询系统信息
public class SystemInfoService
{
    public SystemInfo GetSystemInfo()
    {
        return new SystemInfo
        {
            OsVersion = GetOsVersion(),
            BiosMode = GetBiosMode(),
            SecureBoot = GetSecureBootStatus(),
            TpmVersion = GetTpmVersion(),
            Disks = GetDiskInfo()
        };
    }
    
    private BiosMode GetBiosMode()
    {
        // 通过检查系统分区是否存在 EFI 目录判断
        return Directory.Exists(@"\EFI\Microsoft\Boot") ? BiosMode.Uefi : BiosMode.Legacy;
    }
    
    private SecureBootStatus GetSecureBootStatus()
    {
        try
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
            var value = key?.GetValue("UEFISecureBootEnabled");
            return value?.ToString() == "1" ? SecureBootStatus.Enabled : SecureBootStatus.Disabled;
        }
        catch
        {
            return SecureBootStatus.NotSupported;
        }
    }
}
```

### 2.2 镜像文件处理

**ISO 挂载**：

```csharp
public class ImageService
{
    /// <summary>
    /// 挂载 ISO 文件并提取 install.wim
    /// </summary>
    public async Task<string> ExtractWimFromIso(string isoPath, string tempPath)
    {
        // 使用 PowerShell 挂载 ISO
        var ps = PowerShell.Create();
        ps.AddScript($"Mount-DiskImage -ImagePath '{isoPath}'");
        var result = await ps.InvokeAsync();
        
        // 获取挂载的盘符
        var driveLetter = GetMountedDriveLetter(isoPath);
        
        // 查找 install.wim 或 install.esd
        var wimPath = FindInstallImage($"{driveLetter}:\sources");
        
        // 复制到临时目录
        var destPath = Path.Combine(tempPath, "install.wim");
        File.Copy(wimPath, destPath, true);
        
        // 卸载 ISO
        ps = PowerShell.Create();
        ps.AddScript($"Dismount-DiskImage -ImagePath '{isoPath}'");
        await ps.InvokeAsync();
        
        return destPath;
    }
    
    /// <summary>
    /// 获取 WIM 文件中的镜像列表
    /// </summary>
    public List<ImageInfo> GetImageInfo(string wimPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dism.exe",
                Arguments = $"/Get-WimInfo /WimFile:\"{wimPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        // 解析 DISM 输出
        return ParseImageInfo(output);
    }
}
```

### 2.3 驱动备份与恢复

**驱动识别**：

```csharp
public class DriverService
{
    /// <summary>
    /// 获取第三方驱动列表（排除微软内置驱动）
    /// </summary>
    public List<DriverInfo> GetThirdPartyDrivers()
    {
        var drivers = new List<DriverInfo>();
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dism.exe",
                Arguments = "/online /get-drivers /format:table",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        // 解析输出，过滤掉微软驱动
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("Microsoft") || line.Contains("Windows"))
                continue;
                
            var driver = ParseDriverLine(line);
            if (driver != null)
                drivers.Add(driver);
        }
        
        return drivers;
    }
    
    /// <summary>
    /// 备份选中的驱动
    /// </summary>
    public async Task BackupDrivers(List<DriverInfo> drivers, string backupPath)
    {
        // 使用 DISM 导出驱动
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dism.exe",
                Arguments = $"/online /export-driver /destination:\"{backupPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
    }
}
```

### 2.4 unattend.xml 生成

```csharp
public class UnattendGenerator
{
    public XDocument Generate(DeployConfig config)
    {
        var ns = "urn:schemas-microsoft-com:unattend";
        
        var doc = new XDocument(
            new XElement($"{{{ns}}}unattend",
                new XAttribute("xmlns", ns),
                GenerateWindowsPESettings(config),
                GenerateSpecializeSettings(config),
                GenerateOobeSettings(config)
            )
        );
        
        return doc;
    }
    
    private XElement GenerateOobeSettings(DeployConfig config)
    {
        var ns = "urn:schemas-microsoft-com:unattend";
        
        return new XElement($"{{{ns}}}settings",
            new XAttribute("pass", "oobeSystem"),
            new XElement($"{{{ns}}}component",
                new XAttribute("name", "Microsoft-Windows-Shell-Setup"),
                new XAttribute("processorArchitecture", "amd64"),
                new XAttribute("publicKeyToken", "31bf3856ad364e35"),
                new XAttribute("language", "neutral"),
                new XAttribute("versionScope", "nonSxS"),
                
                // 自动登录配置
                new XElement($"{{{ns}}}AutoLogon",
                    new XElement($"{{{ns}}}Enabled", "true"),
                    new XElement($"{{{ns}}}Username", config.Username),
                    new XElement($"{{{ns}}}Password",
                        new XElement($"{{{ns}}}Value", config.Password),
                        new XElement($"{{{ns}}}PlainText", "true")
                    )
                ),
                
                // 首次登录命令
                new XElement($"{{{ns}}}FirstLogonCommands",
                    GenerateRustDeskInstallCommand(),
                    GenerateDriverRestoreCommand(),
                    GenerateIpRestoreCommand(),
                    GenerateCleanupCommand()
                )
            )
        );
    }
    
    private XElement GenerateRustDeskInstallCommand()
    {
        var ns = "urn:schemas-microsoft-com:unattend";
        
        return new XElement($"{{{ns}}}SynchronousCommand",
            new XElement($"{{{ns}}}Order", "1"),
            new XElement($"{{{ns}}}Description", "Install RustDesk"),
            new XElement($"{{{ns}}}CommandLine", 
                "msiexec /i C:\\WinDeployTemp\\rustdesk.msi /quiet /norestart"),
            new XElement($"{{{ns}}}RequiresUserInput", "false")
        );
    }
}
```

### 2.5 RustDesk 配置

```csharp
public class RustDeskService
{
    /// <summary>
    /// 生成 RustDesk 配置文件
    /// </summary>
    public void GenerateConfig(RustDeskConfig config, string outputPath)
    {
        var json = new
        {
            rendezvous_server = config.ServerAddress,
            nat_type = 1,
            serial = 0,
            custom_rendezvous_server = config.ServerAddress,
            key = config.Key,
            password = config.Password
        };
        
        var jsonString = JsonSerializer.Serialize(json, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(outputPath, jsonString);
    }
    
    /// <summary>
    /// 测试 RustDesk 连接
    /// </summary>
    public async Task<TestResult> TestConnection(RustDeskConfig config)
    {
        try
        {
            // 1. 静默安装 RustDesk
            await InstallSilently();
            
            // 2. 写入配置
            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RustDesk", "config", "RustDesk.toml");
            GenerateConfig(config, configPath);
            
            // 3. 启动 RustDesk 服务
            await StartService();
            
            // 4. 等待并获取 ID
            await Task.Delay(5000);
            var id = await GetRustDeskId();
            
            // 5. 验证连接
            var isConnected = await VerifyConnection(config.ServerAddress);
            
            // 6. 卸载 RustDesk
            await Uninstall();
            
            return new TestResult
            {
                Success = isConnected,
                RustDeskId = id,
                Message = isConnected ? "连接测试成功" : "无法连接到中继服务器"
            };
        }
        catch (Exception ex)
        {
            // 确保卸载
            await Uninstall();
            return new TestResult { Success = false, Message = ex.Message };
        }
    }
}
```

### 2.6 IP 配置备份与恢复

```csharp
public class NetworkService
{
    /// <summary>
    /// 备份网络配置
    /// </summary>
    public async Task BackupNetworkConfig(string outputPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = $"dump > \"{outputPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
    }
    
    /// <summary>
    /// 生成 IP 恢复脚本
    /// </summary>
    public string GenerateIpRestoreScript(NetworkConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@echo off");
        sb.AppendLine("echo Restoring network configuration...");
        sb.AppendLine();
        
        foreach (var adapter in config.Adapters)
        {
            if (adapter.UseDhcp)
            {
                sb.AppendLine($"netsh interface ip set address \"{adapter.Name}\" dhcp");
                sb.AppendLine($"netsh interface ip set dns \"{adapter.Name}\" dhcp");
            }
            else
            {
                sb.AppendLine($"netsh interface ip set address \"{adapter.Name}\" static {adapter.IpAddress} {adapter.SubnetMask} {adapter.Gateway}");
                
                foreach (var dns in adapter.DnsServers)
                {
                    sb.AppendLine($"netsh interface ip add dns \"{adapter.Name}\" {dns}");
                }
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("echo Network configuration restored.");
        
        return sb.ToString();
    }
}
```

### 2.7 磁盘分区操作

```csharp
public class DiskService
{
    /// <summary>
    /// 获取磁盘信息（WinPE 环境）
    /// </summary>
    public List<DiskInfo> GetDisks()
    {
        var disks = new List<DiskInfo>();
        
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
        {
            foreach (ManagementObject disk in searcher.Get())
            {
                disks.Add(new DiskInfo
                {
                    Index = Convert.ToInt32(disk["Index"]),
                    Model = disk["Model"].ToString(),
                    Size = Convert.ToInt64(disk["Size"]),
                    InterfaceType = disk["InterfaceType"].ToString()
                });
            }
        }
        
        return disks;
    }
    
    /// <summary>
    /// 获取磁盘分区（WinPE 环境）
    /// </summary>
    public List<PartitionInfo> GetPartitions(int diskIndex)
    {
        var partitions = new List<PartitionInfo>();
        
        var query = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='\\\\.\\PHYSICALDRIVE{diskIndex}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
        
        using (var searcher = new ManagementObjectSearcher(query))
        {
            foreach (ManagementObject partition in searcher.Get())
            {
                partitions.Add(new PartitionInfo
                {
                    Index = Convert.ToInt32(partition["Index"]),
                    Size = Convert.ToInt64(partition["Size"]),
                    Type = partition["Type"].ToString()
                });
            }
        }
        
        return partitions;
    }
    
    /// <summary>
    /// 创建分区布局（删除系统分区，保留数据分区）
    /// </summary>
    public async Task CreatePartitionLayout(DiskInfo disk, PartitionInfo targetPartition)
    {
        // 使用 diskpart 脚本
        var script = $@"
select disk {disk.Index}
list partition
";
        
        // 识别并删除系统分区（ESP、MSR、原系统分区）
        // 保留数据分区（D、E 等）
        // 创建新的系统分区结构
        
        var scriptPath = Path.Combine(Path.GetTempPath(), "diskpart.txt");
        File.WriteAllText(scriptPath, script);
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "diskpart.exe",
                Arguments = $"/s \"{scriptPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
    }
}
```

### 2.8 BCD 引导配置

```csharp
public class BcdService
{
    /// <summary>
    /// 配置 BCD 引导（WinPE 环境）
    /// </summary>
    public async Task ConfigureBoot(string systemDrive, string windowsPath)
    {
        // 使用 bcdboot 命令
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bcdboot.exe",
                Arguments = $"\"{windowsPath}\" /s {systemDrive} /f {(IsUefi() ? "UEFI" : "BIOS")}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            throw new Exception($"BCD 配置失败，错误码：{process.ExitCode}");
        }
    }
}
```

## 三、部署流程实现

### 3.1 阶段1：准备阶段

```csharp
public class PreparePhase
{
    private readonly ILogger<PreparePhase> _logger;
    private readonly DeployConfig _config;
    
    public async Task<PhaseResult> ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("========== 阶段1：准备阶段开始 ==========");
            
            // 1. 验证配置完整性
            ValidateConfig();
            
            // 2. 生成 unattend.xml
            var unattendPath = GenerateUnattend();
            _logger.LogInformation("unattend.xml 生成完成：{Path}", unattendPath);
            
            // 3. 备份驱动（如果勾选）
            if (_config.BackupDrivers)
            {
                await BackupDriversAsync();
            }
            
            // 4. 准备 RustDesk 安装包和配置
            PrepareRustDesk();
            
            // 5. 备份 IP 配置
            await BackupNetworkAsync();
            
            // 6. 准备 WinPE 引导
            await PrepareWinPEBootAsync();
            
            _logger.LogInformation("========== 阶段1：准备阶段完成 ==========");
            
            return PhaseResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "准备阶段失败");
            return PhaseResult.Failure(ex.Message);
        }
    }
    
    private async Task PrepareWinPEBootAsync()
    {
        // 1. 复制 boot.wim 到目标位置
        // 2. 配置 BCD 添加 WinPE 引导项
        // 3. 设置下次启动进入 WinPE
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bcdedit.exe",
                Arguments = $"/copy {{current}} /d \"WinDeploy WinPE\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        // 解析返回的 GUID
        var guid = ParseGuid(output);
        
        // 设置引导项属性
        await SetBootEntryProperties(guid);
    }
}
```

### 3.2 阶段2：WinPE 部署阶段

```csharp
public class WinPEPhase
{
    private readonly ILogger<WinPEPhase> _logger;
    
    public async Task<PhaseResult> ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("========== 阶段2：WinPE 部署阶段开始 ==========");
            
            // 1. 挂载目标磁盘
            var targetDrive = await MountTargetDiskAsync();
            
            // 2. 分区操作
            await PartitionDiskAsync(targetDrive);
            
            // 3. 应用镜像
            await ApplyImageAsync(targetDrive);
            
            // 4. 注入驱动
            await InjectDriversAsync(targetDrive);
            
            // 5. 复制配置文件
            await CopyConfigFilesAsync(targetDrive);
            
            // 6. 配置引导
            await ConfigureBootAsync(targetDrive);
            
            _logger.LogInformation("========== 阶段2：WinPE 部署阶段完成 ==========");
            
            return PhaseResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WinPE 部署阶段失败");
            return PhaseResult.Failure(ex.Message);
        }
    }
    
    private async Task ApplyImageAsync(string targetDrive)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dism.exe",
                Arguments = $"/Apply-Image /ImageFile:\"{_config.ImagePath}\" /Index:{_config.ImageIndex} /ApplyDir:{targetDrive}:\\",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        // 实时输出进度
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger.LogInformation(e.Data);
                // 解析进度百分比
                var progress = ParseProgress(e.Data);
                if (progress.HasValue)
                {
                    OnProgressChanged(progress.Value);
                }
            }
        };
        
        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            throw new Exception($"镜像应用失败，错误码：{process.ExitCode}");
        }
    }
}
```

### 3.3 阶段3：首次启动阶段

此阶段通过 unattend.xml 的 FirstLogonCommands 自动执行，无需工具干预。

但工具需要在阶段2完成后，等待阶段3完成并验证结果。

```csharp
public class ValidationPhase
{
    private readonly ILogger<ValidationPhase> _logger;
    
    public async Task<PhaseResult> ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("========== 阶段4：验证阶段开始 ==========");
            
            // 等待新系统启动（通过检测 RustDesk 是否在线）
            var rustDeskId = await WaitForRustDeskOnlineAsync(timeout: TimeSpan.FromMinutes(30));
            
            if (string.IsNullOrEmpty(rustDeskId))
            {
                return PhaseResult.Failure("RustDesk 未能在规定时间内上线");
            }
            
            _logger.LogInformation("RustDesk 已上线，ID：{Id}", rustDeskId);
            
            // 验证网络连通性
            var networkStatus = await VerifyNetworkAsync();
            
            // 验证驱动安装
            var driverStatus = await VerifyDriversAsync();
            
            // 生成部署报告
            await GenerateDeploymentReportAsync(rustDeskId, networkStatus, driverStatus);
            
            _logger.LogInformation("========== 阶段4：验证阶段完成 ==========");
            
            return PhaseResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证阶段失败");
            return PhaseResult.Failure(ex.Message);
        }
    }
}
```

## 四、日志系统实现

```csharp
public static class LoggingConfiguration
{
    public static void ConfigureLogging(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(logDirectory, "Deploy-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logDirectory, "Master.log"),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{Phase}] [{Module}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
```

## 五、错误处理

```csharp
public class DeploymentException : Exception
{
    public string ErrorCode { get; }
    public DeploymentPhase Phase { get; }
    
    public DeploymentException(string message, string errorCode, DeploymentPhase phase)
        : base(message)
    {
        ErrorCode = errorCode;
        Phase = phase;
    }
}

public enum DeploymentPhase
{
    Preparation,
    WinPE,
    FirstBoot,
    Validation
}
```

---

*文档版本：v1.0*
*更新日期：2025-05-12*
