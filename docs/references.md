# 技术参考资料

本文档收集了与 Windows 自动部署系统相关的技术文章和官方文档，供开发实现时参考。

---

## 一、Windows 无人值守安装

### 官方文档

| 文档名称 | 链接 | 说明 |
|---------|------|------|
| 自动化 Windows 安装 | [Microsoft Learn](https://docs.microsoft.com/zh-cn/windows-hardware/manufacture/desktop/automate-windows-setup) | 微软官方文档，介绍如何使用答案文件自动化 Windows 安装 |
| FirstLogonCommands | [Microsoft Learn](https://learn.microsoft.com/zh-cn/windows-hardware/customize/desktop/unattend/microsoft-windows-shell-setup-firstlogoncommands) | 官方文档，说明首次登录命令的配置方法 |

### 技术文章

| 文章名称 | 链接 | 说明 |
|---------|------|------|
| OEM 无人值守 Windows 部署的工程化方法 | [CSDN](https://blog.csdn.net/wjm2005427/article/details/155662589) | 深入讲解 WinPE、DISM、Sysprep、Unattend 的全流程原理 |
| Autounattend.xml 自动化安装策略文件 | [博客园](https://www.cnblogs.com/suv789/p/18186627) | 详细介绍 unattend.xml 的结构和各阶段配置 |
| Windows 无人值守安装应答文件实战指南 | [PHP中文网](https://m.php.cn/faq/2397975.html) | 实战教程，包含具体的 XML 配置示例 |
| Unattend.xml 应答文件制作 | [CSDN](https://blog.csdn.net/weixin_34238642/article/details/85597513) | 使用 Windows SIM 制作应答文件的教程 |
| 用 Win11 安装盘自带的 WinPE 和 DISM 部署纯净系统 | [CSDN文库](https://wenku.csdn.net/column/o3nn6ajtn8n) | 保姆级流程教程，包含驱动集成和无人值守配置 |

---

## 二、WinPE 制作与驱动注入

### 官方文档

| 文档名称 | 链接 | 说明 |
|---------|------|------|
| WinPE: 创建可启动的介质 | [Microsoft Learn](https://learn.microsoft.com/zh-cn/windows-hardware/manufacture/desktop/winpe-create-usb-bootable-drive?view=windows-11) | 官方教程，如何创建 WinPE 启动 U 盘 |
| 自定义 Windows PE 启动映像 | [Microsoft Learn](https://learn.microsoft.com/zh-cn/windows/deployment/customize-boot-image) | 官方文档，如何向 WinPE 添加驱动程序 |
| DISM 概述 | [Microsoft Learn](https://learn.microsoft.com/zh-cn/windows-hardware/manufacture/desktop/what-is-dism?view=windows-11) | DISM 工具的官方完整文档 |

### 技术文章

| 文章名称 | 链接 | 说明 |
|---------|------|------|
| 使用 ADK 制作 PE | [CSDN](https://blog.csdn.net/weixin_34261739/article/details/93073344) | 详细记录如何使用 Windows ADK 工具制作 PE 系统 |
| WinPE 制作 U 盘启动盘教程 | [PHP中文网](https://m.php.cn/faq/2318406.html) | 完整的 WinPE 制作流程教程 |
| 用 Rufus 和 DISM 制作可启动 PE 镜像 | [CSDN文库](https://wenku.csdn.net/answer/5d44ojick2) | 结合 Rufus 与 DISM 工具的制作流程 |

---

## 三、驱动备份与恢复

### 官方文档

| 文档名称 | 链接 | 说明 |
|---------|------|------|
| DISM 驱动管理 | [Microsoft Learn](https://learn.microsoft.com/zh-cn/windows-hardware/manufacture/desktop/what-is-dism) | DISM 驱动相关命令的官方说明 |

### 技术文章

| 文章名称 | 链接 | 说明 |
|---------|------|------|
| Windows 备份和还原驱动程序 (DISM) | [PHP中文网](https://m.php.cn/faq/1963074.html) | 使用 DISM 导出/导入驱动的详细步骤 |
| DISM 命令行工具详解 | [博客园](https://www.cnblogs.com/suv789/p/18576494) | DISM 常用操作命令汇总，包括驱动管理 |
| 全面掌握 DISM 系统维护 | [CSDN](https://blog.csdn.net/weixin_42433737/article/details/143528168) | DISM 备份、恢复、修复功能的完整教程 |
| 重装 Windows 驱动处理 | [CSDN](https://blog.csdn.net/weixin_38717458/article/details/157400630) | 驱动备份、恢复与深度优化的实战经验 |

---

## 四、RustDesk 远程桌面

### 官方资源

| 资源名称 | 链接 | 说明 |
|---------|------|------|
| RustDesk 命令行接口说明 | [GitHub](https://github.com/xiehan12/rustdesk/blob/main/接口说明.md) | 官方命令行参数文档，包含静默安装、配置等 |
| RustDesk 官方下载 | [GitHub Releases](https://github.com/rustdesk/rustdesk/releases) | 官方发布页面 |

### 技术文章

| 文章名称 | 链接 | 说明 |
|---------|------|------|
| RustDesk 服务端完整安装部署教程 | [CSDN](https://blog.csdn.net/weixin_48503029/article/details/149341976) | 自建中继服务器的完整教程 |
| RustDesk 自建服务端教程 | [掘金](https://juejin.cn/post/7635945097763209262) | 开源远程桌面自建服务器教程 |
| RustDesk 私有服务器搭建实战 | [CSDN](https://blog.csdn.net/StackOverthink/article/details/147691899) | 实战经验分享，包含配置细节 |

---

## 五、IP 配置备份与恢复

### 官方文档

| 文档名称 | 链接 | 说明 |
|---------|------|------|
| netsh dump | [Microsoft Learn](https://learn.microsoft.com/zh-cn/windows-server/administration/windows-commands/netsh-dump) | 官方文档，导出网络配置的命令 |

### 技术文章

| 文章名称 | 链接 | 说明 |
|---------|------|------|
| netsh 命令实战 | [CSDN](https://blog.csdn.net/weixin_30663391/article/details/96638143) | netsh 常用命令，包含备份/恢复网络设置 |
| netsh 网络配置命令行工具 | [百科](https://m.baike.com/wiki/netsh/2432677) | netsh 完整命令参考 |
| Windows netsh 常用命令实战指南 | [阿里云开发者社区](https://developer.aliyun.com/article/1649390) | 覆盖网络配置、防火墙、备份与远程管理 |
| netsh 备份还原 IP 配置 | [51CTO](https://blog.51cto.com/u_16213586/12201883) | 具体的 IP 备份还原命令示例 |

---

## 六、自动登录配置

### 技术文章

| 文章名称 | 链接 | 说明 |
|---------|------|------|
| Autounattend.xml 自动登录问题 | [无忧启动论坛](http://bbs.wuyou.net/forum.php?mod=viewthread&tid=435312) | 自动登录不生效的解决方案 |
| Win10 虚拟机模板制作指南 | [CSDN](https://blog.csdn.net/weixin_38802670/article/details/160578144) | 包含自动登录注册表配置方法 |

---

## 七、开源项目参考

### GitHub 项目

| 项目名称 | 链接 | 说明 |
|---------|------|------|
| ArkDeploy Toolkit | [GitHub](https://github.com/ArkDeployDev/ArkDeployToolkit) | 开源 Windows 部署解决方案，使用 PowerShell 工作流 |
| UnattendTool | [GitHub](https://github.com/dsx42/UnattendTool) | 无人值守安装工具 |
| WinToolKit | [GitHub](https://github.com/DreamPack-Software/WinToolKit) | Windows ISO 定制工具，支持驱动集成 |
| unattended-installation 主题 | [GitHub Topics](https://github.com/topics/unattended-installation) | GitHub 上相关的开源项目集合 |
| autounattend 主题 | [GitHub Topics](https://github.com/topics/autounattend) | 自动应答文件相关的开源项目 |

---

## 八、关键知识点总结

### unattend.xml 核心阶段

| 阶段 | 说明 | 典型用途 |
|------|------|---------|
| windowsPE | Windows PE 阶段 | 磁盘分区、选择安装镜像 |
| offlineServicing | 离线服务阶段 | 注入驱动、更新包 |
| specialize | 特殊化阶段 | 计算机名、时区、产品密钥 |
| oobeSystem | OOBE 阶段 | 用户账户、自动登录、首次登录命令 |

### DISM 常用命令速查

| 功能 | 命令 |
|------|------|
| 列出已安装驱动 | `dism /online /get-drivers /format:table` |
| 导出驱动 | `dism /online /export-driver /destination:D:\Drivers` |
| 添加驱动 | `dism /image:C:\mount /add-driver /driver:D:\Drivers /recurse` |
| 挂载镜像 | `dism /mount-wim /wimfile:install.wim /index:1 /mountdir:C:\mount` |
| 卸载镜像 | `dism /unmount-wim /mountdir:C:\mount /commit` |

### netsh IP 配置命令速查

| 功能 | 命令 |
|------|------|
| 导出网络配置 | `netsh dump > D:\network.txt` |
| 恢复网络配置 | `netsh -f D:\network.txt` |
| 导出 IP 配置 | `netsh interface ip dump > D:\ip.txt` |
| 设置静态 IP | `netsh interface ip set address "本地连接" static 192.168.1.100 255.255.255.0 192.168.1.1` |
| 设置 DHCP | `netsh interface ip set address "本地连接" dhcp` |

---

## 九、注意事项

1. **unattend.xml 编码**：必须使用 UTF-8 无 BOM 编码
2. **文件名**：自动应答文件必须命名为 `Autounattend.xml` 或 `Unattend.xml`
3. **存放位置**：U 盘根目录或 `\sources\` 子目录
4. **AutoLogonCount**：自动登录次数，设置为 0 表示无限次
5. **FirstLogonCommands**：只在首次登录时执行一次
6. **驱动签名**：Win11 要求驱动必须有有效签名

---

*文档更新日期：2025-05-12*
