# Windows 自动部署工具 - 常见问题排查

## 一、部署前问题

### 1.1 工具无法启动

**现象**：双击工具无反应或报错

**排查步骤**：
1. 检查是否以管理员身份运行
2. 检查 .NET 8 运行时是否已安装
3. 检查工具目录是否完整（Resources 文件夹是否存在）
4. 查看 Windows 事件查看器中的应用程序错误日志

**解决方案**：
- 右键点击工具，选择"以管理员身份运行"
- 安装 .NET 8 运行时：[下载链接](https://dotnet.microsoft.com/download/dotnet/8.0)
- 重新解压工具包，确保文件完整

### 1.2 系统检测失败

**现象**：系统信息显示"检测失败"或空白

**可能原因**：
- WMI 服务未运行
- 权限不足

**解决方案**：
```powershell
# 检查 WMI 服务状态
Get-Service winmgmt

# 重启 WMI 服务
Restart-Service winmgmt -Force
```

### 1.3 镜像文件无法识别

**现象**：选择 ISO/WIM/ESD 后提示"无效的镜像文件"

**排查步骤**：
1. 检查文件是否完整（文件大小是否合理）
2. 检查文件是否被占用（是否被其他程序打开）
3. 验证文件 SHA256 校验值

**解决方案**：
- 重新下载镜像文件
- 关闭可能占用该文件的程序（如资源管理器、其他挂载工具）
- 使用官方镜像制作工具重新生成镜像

## 二、部署过程中问题

### 2.1 无法进入 WinPE

**现象**：重启后黑屏、蓝屏或直接进入原系统

**可能原因**：
- BCD 配置失败
- WinPE 镜像损坏
- 磁盘模式不兼容（AHCI/RAID）
- 安全启动阻止

**排查步骤**：
1. 检查日志中的 BCD 配置输出
2. 检查 BIOS 设置中的磁盘模式
3. 检查安全启动是否关闭

**解决方案**：
```cmd
# 手动修复 BCD（在原系统中以管理员运行）
bcdedit /enum

# 查找 WinPE 引导项
# 如果不存在，重新运行工具准备阶段

# 如果存在但无法启动，尝试：
bcdedit /set {guid} device partition=Z:
bcdedit /set {guid} osdevice partition=Z:
bcdedit /set {guid} detecthal on
```

### 2.2 WinPE 中无法识别磁盘

**现象**：进入 WinPE 后磁盘列表为空

**可能原因**：
- 缺少 RAID/SATA 控制器驱动
- 磁盘连接问题

**解决方案**：
1. 在制作 WinPE 时注入对应的存储控制器驱动
2. 检查磁盘数据线和电源线连接
3. 在 BIOS 中切换磁盘模式（AHCI/IDE/RAID）

### 2.3 镜像应用失败

**现象**：DISM 报错，错误码 5、87、112 等

**常见错误码**：

| 错误码 | 含义 | 解决方案 |
|--------|------|---------|
| 5 | 拒绝访问 | 检查目标分区是否被占用，尝试重新格式化 |
| 87 | 参数错误 | 检查镜像索引是否正确，WIM 文件是否损坏 |
| 112 | 磁盘空间不足 | 清理目标分区或选择更大的分区 |
| 1392 | 文件或目录损坏 | 镜像文件损坏，重新下载 |
| 0x80070057 | 参数错误 | 检查 unattend.xml 格式是否正确 |

**排查步骤**：
1. 查看 Phase2_WinPE.log 中的详细错误信息
2. 检查目标分区状态
3. 验证镜像文件完整性

### 2.4 驱动注入失败

**现象**：提示"驱动注入失败"或驱动未生效

**可能原因**：
- 驱动与目标系统不兼容
- 驱动签名问题（Win11）
- 驱动文件不完整

**解决方案**：
- 仅注入确定兼容的驱动
- Win11 系统确保驱动有有效签名
- 检查驱动备份是否完整

## 三、部署后问题

### 3.1 无法启动进入新系统

**现象**：重启后黑屏、蓝屏或提示引导错误

**可能原因**：
- 引导配置错误
- 系统分区未激活
- BCD 损坏

**解决方案**：
```cmd
# 进入 WinPE，打开命令提示符

# 1. 确定系统分区
diskpart
list disk
select disk 0
list partition

# 2. 给系统分区分配盘符
select partition 3  # 系统分区
assign letter=C
exit

# 3. 修复引导
bcdboot C:\Windows /s C: /f ALL

# 4. 修复 BCD
bootrec /fixmbr
bootrec /fixboot
bootrec /rebuildbcd
```

### 3.2 RustDesk 未安装或未连接

**现象**：进入桌面后 RustDesk 未运行，无法远程连接

**排查步骤**：
1. 检查 C:\WinDeployLogs\Phase3_FirstBoot.log 中的 RustDesk 安装日志
2. 检查 RustDesk 服务是否已启动
3. 检查网络连接是否正常

**解决方案**：
```powershell
# 检查 RustDesk 安装状态
Get-Package -Name "RustDesk"

# 手动安装 RustDesk
msiexec /i "C:\WinDeployTemp\rustdesk.msi" /quiet

# 检查服务状态
Get-Service RustDesk
Start-Service RustDesk

# 查看 RustDesk 日志
Get-Content "$env:APPDATA\RustDesk\log\rustdesk.log"
```

### 3.3 网络配置未恢复

**现象**：IP 地址未恢复为静态 IP，仍是 DHCP

**排查步骤**：
1. 检查 Phase3_FirstBoot.log 中的 netsh 执行记录
2. 检查 IP 恢复脚本是否存在

**解决方案**：
```cmd
# 手动执行 IP 恢复脚本
netsh -f "C:\WinDeployTemp\network_backup.txt"

# 或手动设置静态 IP
netsh interface ip set address "以太网" static 192.168.1.100 255.255.255.0 192.168.1.1
netsh interface ip set dns "以太网" static 8.8.8.8
```

### 3.4 驱动未正确安装

**现象**：设备管理器中有黄色感叹号，某些硬件无法使用

**排查步骤**：
1. 打开设备管理器，查看未识别设备
2. 检查 Phase3_FirstBoot.log 中的 pnputil 执行记录

**解决方案**：
```powershell
# 手动安装驱动
pnputil /add-driver "C:\WinDeployTemp\Drivers\*.inf" /subdirs /install

# 或逐个安装
pnputil /add-driver "C:\WinDeployTemp\Drivers\Intel_NIC\e1d68x64.inf" /install
```

## 四、日志分析指南

### 4.1 日志文件位置

```
C:\WinDeployLogs\
├── History\
│   └── 2025-01-12_143022\          # 按时间命名的部署记录
│       ├── Deploy.log               # 主日志
│       ├── Phase1_Prepare.log       # 准备阶段
│       ├── Phase2_WinPE.log         # PE 阶段
│       ├── Phase3_FirstBoot.log     # 首次启动阶段
│       └── Phase4_Validate.log      # 验证阶段
└── Latest\                          # 最新部署（快捷方式）
```

### 4.2 关键日志搜索词

| 问题 | 搜索关键词 | 查看文件 |
|------|-----------|---------|
| 无法进入 PE | "BCD", "boot", "WinPE" | Phase1_Prepare.log |
| 磁盘识别问题 | "disk", "partition", "DiskPart" | Phase2_WinPE.log |
| 镜像应用失败 | "DISM", "Apply-Image", "error" | Phase2_WinPE.log |
| 驱动问题 | "driver", "pnputil", "Add-Driver" | Phase2/Phase3.log |
| RustDesk 问题 | "RustDesk", "msiexec", "connection" | Phase3_FirstBoot.log |
| 网络问题 | "netsh", "IP", "network" | Phase3_FirstBoot.log |
| 引导问题 | "bcdboot", "BCD", "bootrec" | Phase2_WinPE.log |

### 4.3 日志分析示例

**示例1：DISM 错误**
```
[2025-01-12 14:32:15.456] [ERROR] [Phase2] [ImageService] DISM 应用镜像失败
[2025-01-12 14:32:15.789] [ERROR] [Phase2] [ImageService] 错误码：5 (拒绝访问)
[2025-01-12 14:32:16.123] [DEBUG] [Phase2] [ImageService] 目标分区：C:
[2025-01-12 14:32:16.456] [DEBUG] [Phase2] [ImageService] 分区状态：已挂载，可能正在被占用
```

分析：目标分区被占用，可能是未正确格式化或仍有进程访问。

**示例2：RustDesk 连接失败**
```
[2025-01-12 14:36:20.789] [INFO] [Phase3] [FirstBoot] 执行 Command1: RustDesk安装
[2025-01-12 14:36:25.123] [INFO] [Phase3] [FirstBoot] MSI 安装返回码：0 (成功)
[2025-01-12 14:36:30.456] [INFO] [Phase3] [FirstBoot] RustDesk 服务启动成功
[2025-01-12 14:36:35.789] [WARN] [Phase3] [FirstBoot] 连接中继服务器超时
[2025-01-12 14:36:35.890] [DEBUG] [Phase3] [FirstBoot] 服务器地址：relay.example.com:21117
```

分析：RustDesk 安装成功但无法连接中继服务器，可能是网络问题或服务器配置错误。

## 五、常见问题速查

### Q1: 部署过程中断电了怎么办？

**A**: 重新启动计算机，如果无法进入系统，需要重新运行工具进行部署。原系统已无法恢复。

### Q2: 可以中断部署吗？

**A**: 阶段1（准备阶段）可以安全取消。阶段2（PE阶段）开始后不建议中断，可能导致系统无法启动。

### Q3: 部署后 D 盘数据还在吗？

**A**: 是的，工具只格式化系统分区（C盘），D/E 等数据分区完全保留。

### Q4: 支持从 Win7 部署到 Win11 吗？

**A**: 支持，但需要确保硬件满足 Win11 要求（TPM 2.0、Secure Boot）。工具会自动添加绕过配置，但建议尽量满足要求。

### Q5: RustDesk 密码忘记了怎么办？

**A**: 查看部署日志中的 RustDesk 配置，或在目标系统上打开 RustDesk 客户端查看/修改密码。

### Q6: 部署后激活状态如何？

**A**: 工具不处理激活，需要用户自行激活。如果原系统已激活且版本相同，可能自动激活。

### Q7: 可以部署到移动硬盘吗？

**A**: 技术上可以，但不推荐。移动硬盘的驱动和引导可能不稳定。

### Q8: 部署过程中提示"磁盘被写保护"？

**A**: 检查磁盘是否开启了写保护开关（部分U盘/SD卡有物理开关），或使用 diskpart 清除只读属性：
```cmd
diskpart
select disk 0
attributes disk clear readonly
exit
```

## 六、反馈问题

如果以上方案无法解决问题，请收集以下信息反馈：

1. **部署日志**：C:\WinDeployLogs\History\[时间戳]\ 目录下的所有文件
2. **系统信息**：原系统版本、目标系统版本、硬件配置
3. **问题描述**：详细的问题现象、错误提示、发生时机
4. **已尝试的解决方案**：避免重复建议

将以上信息打包后提交，便于快速定位和解决问题。

---

*文档版本：v1.0*
*更新日期：2025-05-12*
