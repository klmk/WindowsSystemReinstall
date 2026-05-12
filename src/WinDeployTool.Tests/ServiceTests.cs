using WinDeployTool.Core.Helpers;
using WinDeployTool.Core.Models;
using WinDeployTool.Core.Services;

namespace WinDeployTool.Tests
{
    public class ValidationServiceTests
    {
        private readonly ValidationService _validationService = new();

        [Fact]
        public void ValidateDeployConfig_EmptyImagePath_ShouldReturnError()
        {
            var config = new DeployConfig
            {
                ImagePath = "",
                TargetDiskIndex = 0,
                TargetPartitionIndex = 1
            };

            var result = _validationService.ValidateDeployConfig(config);

            Assert.False(result.IsValid);
            Assert.Contains("镜像文件路径不能为空", result.Errors);
        }

        [Fact]
        public void ValidateDeployConfig_InvalidImageIndex_ShouldReturnError()
        {
            var config = new DeployConfig
            {
                ImagePath = "C:\\test.wim",
                ImageIndex = 0,
                TargetDiskIndex = 0,
                TargetPartitionIndex = 1
            };

            var result = _validationService.ValidateDeployConfig(config);

            Assert.False(result.IsValid);
            Assert.Contains("镜像索引必须大于0", result.Errors);
        }

        [Fact]
        public void ValidateDeployConfig_InvalidComputerName_ShouldReturnError()
        {
            var config = new DeployConfig
            {
                ImagePath = "C:\\test.wim",
                ImageIndex = 1,
                TargetDiskIndex = 0,
                TargetPartitionIndex = 1,
                ComputerName = "Invalid@Name#"
            };

            var result = _validationService.ValidateDeployConfig(config);

            Assert.False(result.IsValid);
            Assert.Contains("计算机名格式不正确", result.Errors);
        }

        [Fact]
        public void ValidateDeployConfig_ValidConfig_ShouldReturnValid()
        {
            var config = new DeployConfig
            {
                ImagePath = "C:\\test.wim",
                ImageIndex = 1,
                TargetDiskIndex = 0,
                TargetPartitionIndex = 1,
                ComputerName = "DESKTOP-TEST",
                Username = "Administrator",
                Password = "Password123"
            };

            var result = _validationService.ValidateDeployConfig(config);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateRustDeskConfig_EmptyServer_ShouldReturnError()
        {
            var config = new RustDeskConfig
            {
                ServerAddress = "",
                Port = 21117
            };

            var result = _validationService.ValidateRustDeskConfig(config);

            Assert.False(result.IsValid);
            Assert.Contains("RustDesk 服务器地址不能为空", result.Errors);
        }

        [Fact]
        public void ValidateRustDeskConfig_InvalidPort_ShouldReturnError()
        {
            var config = new RustDeskConfig
            {
                ServerAddress = "relay.example.com",
                Port = 70000
            };

            var result = _validationService.ValidateRustDeskConfig(config);

            Assert.False(result.IsValid);
            Assert.Contains("RustDesk 端口必须在 1-65535 之间", result.Errors);
        }

        [Fact]
        public void ValidateRustDeskConfig_CustomPasswordEmpty_ShouldReturnError()
        {
            var config = new RustDeskConfig
            {
                ServerAddress = "relay.example.com",
                Port = 21117,
                PasswordPolicy = PasswordPolicy.Custom,
                CustomPassword = ""
            };

            var result = _validationService.ValidateRustDeskConfig(config);

            Assert.False(result.IsValid);
            Assert.Contains("自定义密码不能为空", result.Errors);
        }
    }

    public class UnattendGeneratorTests
    {
        private readonly UnattendGenerator _generator = new();

        [Fact]
        public void Generate_ShouldCreateValidXml()
        {
            var config = new DeployConfig
            {
                Language = "zh-CN",
                ImageIndex = 1,
                TargetDiskIndex = 0,
                TargetPartitionIndex = 3,
                ComputerName = "DESKTOP-TEST",
                Username = "Administrator",
                Password = "TestPassword123",
                BackupNetworkConfig = true,
                BackupDrivers = true,
                SelectedDrivers = new List<string> { "driver1", "driver2" }
            };

            var doc = _generator.Generate(config);

            Assert.NotNull(doc);
            Assert.NotNull(doc.Root);
            Assert.Equal("unattend", doc.Root?.Name.LocalName);
        }

        [Fact]
        public void Generate_ShouldContainAutoLogon()
        {
            var config = new DeployConfig
            {
                Username = "TestUser",
                Password = "TestPass123"
            };

            var doc = _generator.Generate(config);
            var xml = doc.ToString();

            Assert.Contains("AutoLogon", xml);
            Assert.Contains("TestUser", xml);
            Assert.Contains("TestPass123", xml);
        }

        [Fact]
        public void Generate_ShouldContainFirstLogonCommands()
        {
            var config = new DeployConfig
            {
                BackupNetworkConfig = true,
                BackupDrivers = true,
                SelectedDrivers = new List<string> { "driver1" }
            };

            var doc = _generator.Generate(config);
            var xml = doc.ToString();

            Assert.Contains("FirstLogonCommands", xml);
            Assert.Contains("Install RustDesk", xml);
            Assert.Contains("Restore Network Config", xml);
            Assert.Contains("Install Drivers", xml);
        }
    }

    public class ConfigManagerTests
    {
        private readonly string _testDir;
        private readonly ConfigManager _configManager;

        public ConfigManagerTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"WinDeployTest_{Guid.NewGuid()}");
            _configManager = new ConfigManager(_testDir);
        }

        [Fact]
        public async Task SaveAndLoadDeployConfig_ShouldWork()
        {
            var config = new DeployConfig
            {
                ComputerName = "TEST-PC",
                Username = "TestUser",
                Language = "en-US"
            };

            await _configManager.SaveDeployConfigAsync(config, "testconfig");
            var loaded = await _configManager.LoadDeployConfigAsync("testconfig");

            Assert.NotNull(loaded);
            Assert.Equal("TEST-PC", loaded.ComputerName);
            Assert.Equal("TestUser", loaded.Username);
            Assert.Equal("en-US", loaded.Language);
        }

        [Fact]
        public async Task LoadNonExistentConfig_ShouldReturnNull()
        {
            var loaded = await _configManager.LoadDeployConfigAsync("nonexistent");
            Assert.Null(loaded);
        }

        [Fact]
        public async Task GetSavedConfigNames_ShouldReturnSavedConfigs()
        {
            await _configManager.SaveDeployConfigAsync(new DeployConfig(), "config1");
            await _configManager.SaveDeployConfigAsync(new DeployConfig(), "config2");

            var names = _configManager.GetSavedConfigNames().ToList();

            Assert.Contains("config1", names);
            Assert.Contains("config2", names);
        }
    }

    public class PasswordGeneratorTests
    {
        [Fact]
        public void Generate_DefaultLength_ShouldReturn12Chars()
        {
            var password = PasswordGenerator.Generate();

            Assert.Equal(12, password.Length);
        }

        [Fact]
        public void Generate_CustomLength_ShouldReturnCorrectLength()
        {
            var password = PasswordGenerator.Generate(16);

            Assert.Equal(16, password.Length);
        }

        [Fact]
        public void Generate_ShouldContainUpperCase()
        {
            var password = PasswordGenerator.Generate();

            Assert.True(password.Any(char.IsUpper));
        }

        [Fact]
        public void Generate_ShouldContainLowerCase()
        {
            var password = PasswordGenerator.Generate();

            Assert.True(password.Any(char.IsLower));
        }

        [Fact]
        public void Generate_ShouldContainDigit()
        {
            var password = PasswordGenerator.Generate();

            Assert.True(password.Any(char.IsDigit));
        }

        [Fact]
        public void GenerateMemorable_ShouldContainWordAndNumber()
        {
            var password = PasswordGenerator.GenerateMemorable();

            Assert.True(password.Any(char.IsLetter));
            Assert.True(password.Any(char.IsDigit));
            Assert.True(password.Length >= 7);
        }
    }

    public class HashHelperTests
    {
        [Fact]
        public void CalculateSha256_SameInput_ShouldReturnSameHash()
        {
            var input = "test input";
            var hash1 = HashHelper.CalculateSha256(input);
            var hash2 = HashHelper.CalculateSha256(input);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void CalculateSha256_DifferentInput_ShouldReturnDifferentHash()
        {
            var hash1 = HashHelper.CalculateSha256("input1");
            var hash2 = HashHelper.CalculateSha256("input2");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void CalculateSha256_ShouldReturn64Chars()
        {
            var hash = HashHelper.CalculateSha256("test");

            Assert.Equal(64, hash.Length);
        }
    }
}
