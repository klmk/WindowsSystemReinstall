using System.Xml.Linq;
using WinDeployTool.Core.Models;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// unattend.xml 生成器
    /// </summary>
    public class UnattendGenerator
    {
        private const string Ns = "urn:schemas-microsoft-com:unattend";
        private const string WcmNs = "http://schemas.microsoft.com/WMIConfig/2002/State";

        /// <summary>
        /// 生成 unattend.xml 文档
        /// </summary>
        public XDocument Generate(DeployConfig config)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement($"{{{Ns}}}unattend",
                    new XAttribute(XNamespace.Xmlns + "wcm", WcmNs),
                    new XAttribute("xmlns", Ns),
                    GenerateWindowsPESettings(config),
                    GenerateSpecializeSettings(config),
                    GenerateOobeSettings(config)
                )
            );

            return doc;
        }

        /// <summary>
        /// 生成 WindowsPE 阶段配置
        /// </summary>
        private XElement GenerateWindowsPESettings(DeployConfig config)
        {
            return new XElement($"{{{Ns}}}settings",
                new XAttribute("pass", "windowsPE"),
                new XElement($"{{{Ns}}}component",
                    new XAttribute("name", "Microsoft-Windows-International-Core-WinPE"),
                    new XAttribute("processorArchitecture", "amd64"),
                    new XAttribute("publicKeyToken", "31bf3856ad364e35"),
                    new XAttribute("language", "neutral"),
                    new XAttribute("versionScope", "nonSxS"),
                    new XElement($"{{{Ns}}}SetupUILanguage",
                        new XElement($"{{{Ns}}}UILanguage", config.Language)
                    ),
                    new XElement($"{{{Ns}}}InputLocale", config.Language),
                    new XElement($"{{{Ns}}}SystemLocale", config.Language),
                    new XElement($"{{{Ns}}}UILanguage", config.Language),
                    new XElement($"{{{Ns}}}UserLocale", config.Language)
                ),
                new XElement($"{{{Ns}}}component",
                    new XAttribute("name", "Microsoft-Windows-Setup"),
                    new XAttribute("processorArchitecture", "amd64"),
                    new XAttribute("publicKeyToken", "31bf3856ad364e35"),
                    new XAttribute("language", "neutral"),
                    new XAttribute("versionScope", "nonSxS"),
                    GenerateDiskConfiguration(config),
                    GenerateImageInstall(config)
                )
            );
        }

        /// <summary>
        /// 生成磁盘配置
        /// </summary>
        private XElement GenerateDiskConfiguration(DeployConfig config)
        {
            return new XElement($"{{{Ns}}}DiskConfiguration",
                new XElement($"{{{Ns}}}Disk",
                    new XAttribute($"{{{WcmNs}}}action", "add"),
                    new XElement($"{{{Ns}}}DiskID", config.TargetDiskIndex),
                    new XElement($"{{{Ns}}}WillWipeDisk", "false"),
                    new XElement($"{{{Ns}}}CreatePartitions",
                        new XElement($"{{{Ns}}}CreatePartition",
                            new XAttribute($"{{{WcmNs}}}action", "add"),
                            new XElement($"{{{Ns}}}Order", "1"),
                            new XElement($"{{{Ns}}}Type", "Primary"),
                            new XElement($"{{{Ns}}}Size", "100") // 100MB for ESP
                        )
                    ),
                    new XElement($"{{{Ns}}}ModifyPartitions",
                        new XElement($"{{{Ns}}}ModifyPartition",
                            new XAttribute($"{{{WcmNs}}}action", "add"),
                            new XElement($"{{{Ns}}}Order", "1"),
                            new XElement($"{{{Ns}}}PartitionID", "1"),
                            new XElement($"{{{Ns}}}Format", "FAT32"),
                            new XElement($"{{{Ns}}}Label", "System")
                        )
                    )
                )
            );
        }

        /// <summary>
        /// 生成镜像安装配置
        /// </summary>
        private XElement GenerateImageInstall(DeployConfig config)
        {
            return new XElement($"{{{Ns}}}ImageInstall",
                new XElement($"{{{Ns}}}OSImage",
                    new XElement($"{{{Ns}}}InstallFrom",
                        new XElement($"{{{Ns}}}MetaData",
                            new XAttribute($"{{{WcmNs}}}action", "add"),
                            new XElement($"{{{Ns}}}Key", "/IMAGE/INDEX"),
                            new XElement($"{{{Ns}}}Value", config.ImageIndex)
                        )
                    ),
                    new XElement($"{{{Ns}}}InstallTo",
                        new XElement($"{{{Ns}}}DiskID", config.TargetDiskIndex),
                        new XElement($"{{{Ns}}}PartitionID", config.TargetPartitionIndex)
                    )
                )
            );
        }

        /// <summary>
        /// 生成 Specialize 阶段配置
        /// </summary>
        private XElement GenerateSpecializeSettings(DeployConfig config)
        {
            return new XElement($"{{{Ns}}}settings",
                new XAttribute("pass", "specialize"),
                new XElement($"{{{Ns}}}component",
                    new XAttribute("name", "Microsoft-Windows-Shell-Setup"),
                    new XAttribute("processorArchitecture", "amd64"),
                    new XAttribute("publicKeyToken", "31bf3856ad364e35"),
                    new XAttribute("language", "neutral"),
                    new XAttribute("versionScope", "nonSxS"),
                    new XElement($"{{{Ns}}}ComputerName", config.ComputerName),
                    new XElement($"{{{Ns}}}TimeZone", "China Standard Time")
                )
            );
        }

        /// <summary>
        /// 生成 OOBE 阶段配置
        /// </summary>
        private XElement GenerateOobeSettings(DeployConfig config)
        {
            return new XElement($"{{{Ns}}}settings",
                new XAttribute("pass", "oobeSystem"),
                new XElement($"{{{Ns}}}component",
                    new XAttribute("name", "Microsoft-Windows-Shell-Setup"),
                    new XAttribute("processorArchitecture", "amd64"),
                    new XAttribute("publicKeyToken", "31bf3856ad364e35"),
                    new XAttribute("language", "neutral"),
                    new XAttribute("versionScope", "nonSxS"),
                    GenerateAutoLogon(config),
                    GenerateFirstLogonCommands(config),
                    new XElement($"{{{Ns}}}OOBE",
                        new XElement($"{{{Ns}}}HideEULAPage", "true"),
                        new XElement($"{{{Ns}}}HideLocalAccountScreen", "true"),
                        new XElement($"{{{Ns}}}HideOEMRegistrationScreen", "true"),
                        new XElement($"{{{Ns}}}HideOnlineAccountScreens", "true"),
                        new XElement($"{{{Ns}}}HideWirelessSetupInOOBE", "true"),
                        new XElement($"{{{Ns}}}NetworkLocation", "Work"),
                        new XElement($"{{{Ns}}}SkipMachineOOBE", "true"),
                        new XElement($"{{{Ns}}}SkipUserOOBE", "true")
                    )
                )
            );
        }

        /// <summary>
        /// 生成自动登录配置
        /// </summary>
        private XElement GenerateAutoLogon(DeployConfig config)
        {
            return new XElement($"{{{Ns}}}AutoLogon",
                new XElement($"{{{Ns}}}Enabled", "true"),
                new XElement($"{{{Ns}}}Username", config.Username),
                new XElement($"{{{Ns}}}Password",
                    new XElement($"{{{Ns}}}Value", config.Password),
                    new XElement($"{{{Ns}}}PlainText", "true")
                ),
                new XElement($"{{{Ns}}}LogonCount", "999999")
            );
        }

        /// <summary>
        /// 生成首次登录命令
        /// </summary>
        private XElement GenerateFirstLogonCommands(DeployConfig config)
        {
            var commands = new XElement($"{{{Ns}}}FirstLogonCommands");
            var order = 1;

            // 1. 安装 RustDesk
            commands.Add(CreateCommand(order++, "Install RustDesk", 
                "msiexec /i C:\\WinDeployTemp\\rustdesk.msi /quiet /norestart"));

            // 2. 恢复 IP 配置
            if (config.BackupNetworkConfig)
            {
                commands.Add(CreateCommand(order++, "Restore Network Config",
                    "netsh -f C:\\WinDeployTemp\\network_backup.txt"));
            }

            // 3. 安装驱动
            if (config.BackupDrivers && config.SelectedDrivers.Any())
            {
                commands.Add(CreateCommand(order++, "Install Drivers",
                    "pnputil /add-driver C:\\WinDeployTemp\\Drivers\\*.inf /subdirs /install"));
            }

            // 4. 执行部署后脚本（如果有）
            if (!string.IsNullOrEmpty(config.PostDeployScript))
            {
                commands.Add(CreateCommand(order++, "Run Post-Deploy Script",
                    config.PostDeployScript));
            }

            // 5. 清理临时文件
            commands.Add(CreateCommand(order++, "Cleanup",
                "cmd /c rd /s /q C:\\WinDeployTemp"));

            return commands;
        }

        /// <summary>
        /// 创建命令元素
        /// </summary>
        private XElement CreateCommand(int order, string description, string commandLine)
        {
            return new XElement($"{{{Ns}}}SynchronousCommand",
                new XAttribute($"{{{WcmNs}}}action", "add"),
                new XElement($"{{{Ns}}}Order", order),
                new XElement($"{{{Ns}}}Description", description),
                new XElement($"{{{Ns}}}CommandLine", commandLine),
                new XElement($"{{{Ns}}}RequiresUserInput", "false")
            );
        }

        /// <summary>
        /// 保存 unattend.xml 到文件
        /// </summary>
        public void SaveToFile(XDocument doc, string filePath)
        {
            // 使用 UTF-8 无 BOM 编码
            var settings = new System.Xml.XmlWriterSettings
            {
                Encoding = new System.Text.UTF8Encoding(false),
                Indent = true,
                IndentChars = "  "
            };

            using var writer = System.Xml.XmlWriter.Create(filePath, settings);
            doc.Save(writer);
        }
    }
}
