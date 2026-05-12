# Windows 自动部署工具

一款基于 C# WPF 开发的 Windows 系统自动化部署工具，支持 Windows 7/8/10/11 及 Server 2016/2019 的本地一键重装。

## 核心功能

- **多版本支持**：Windows 7/8/10/11、Windows Server 2016/2019
- **原镜像部署**：支持 ISO/WIM/ESD 格式官方镜像
- **数据保留**：仅格式化系统分区，D/E 等数据盘完全保留
- **远程控制**：集成 RustDesk，部署完成后自动连接中继服务器
- **驱动管理**：智能识别第三方驱动，选择性备份与恢复
- **IP 保留**：自动备份和恢复静态 IP 配置
- **自动登录**：部署完成后自动登录桌面，无需手动输入密码
- **完整日志**：全流程日志记录，便于问题排查

## 技术栈

- **开发语言**：C# (.NET 8)
- **UI 框架**：WPF + MaterialDesignInXamlToolkit
- **架构模式**：MVVM (CommunityToolkit.Mvvm)
- **部署环境**：Windows PE (基于 Windows ADK)
- **日志框架**：Serilog

## 项目结构

```
WindowsSystemReinstall/
├── docs/                       # 项目文档
│   ├── design.md              # 技术架构设计
│   ├── ui-design.md           # 界面设计规范
│   ├── implementation.md      # 实现细节
│   ├── references.md          # 参考资料
│   └── troubleshooting.md     # 常见问题排查
├── src/                        # 源代码
│   └── WinDeployTool/         # WPF 应用程序
├── WinPE/                      # WinPE 相关文件
│   ├── boot.wim               # WinPE 镜像（开发时生成）
│   ├── unattend-template.xml  # 应答文件模板
│   └── scripts/               # PE 阶段脚本
└── README.md                   # 本文件
```

## 快速开始

### 开发环境准备

1. 安装 Windows ADK（用于制作 WinPE）
2. 安装 Visual Studio 2022 + .NET 8 SDK
3. 克隆本仓库

### 构建 WinPE

```powershell
# 使用 ADK 制作 WinPE
# 详细步骤见 docs/implementation.md
```

### 运行项目

```bash
dotnet run --project src/WinDeployTool
```

## 使用流程

1. **启动工具**：运行 WinDeployTool.exe
2. **系统检测**：自动检测当前系统信息和兼容性
3. **镜像选择**：选择本地 ISO/WIM/ESD 镜像文件
4. **远程配置**：配置 RustDesk 中继服务器和连接密码
5. **驱动备份**（可选）：选择性备份第三方驱动
6. **部署配置**：选择目标磁盘和分区
7. **执行部署**：确认后开始自动化部署
8. **等待完成**：工具自动完成后续所有步骤

## 文档索引

| 文档 | 说明 |
|------|------|
| [design.md](docs/design.md) | 系统架构设计、技术选型、核心流程 |
| [ui-design.md](docs/ui-design.md) | 界面设计规范、交互逻辑、控件行为 |
| [implementation.md](docs/implementation.md) | 详细实现方案、关键代码说明 |
| [references.md](docs/references.md) | 技术参考资料、官方文档链接 |
| [troubleshooting.md](docs/troubleshooting.md) | 常见问题排查、错误处理 |

## 注意事项

- 部署前请确保重要数据已备份
- 工具仅格式化系统分区，数据分区（D/E 等）会被保留
- 部署过程中请勿断电或强制重启
- 首次启动可能需要几分钟完成初始化

## License

MIT License

## 致谢

感谢以下开源项目提供的参考：
- [ArkDeploy Toolkit](https://github.com/ArkDeployDev/ArkDeployToolkit)
- [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- [RustDesk](https://github.com/rustdesk/rustdesk)
