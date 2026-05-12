using WinDeployTool.Core.Models;
using WinDeployTool.Core.Helpers;

namespace WinDeployTool.Tests
{
    public class ModelsTests
    {
        [Fact]
        public void SystemInfo_ShouldInitializeCorrectly()
        {
            var systemInfo = new SystemInfo
            {
                OsVersion = "Windows 10",
                Architecture = "x64",
                BiosMode = BiosMode.Uefi,
                SecureBoot = SecureBootStatus.Enabled
            };

            Assert.Equal("Windows 10", systemInfo.OsVersion);
            Assert.Equal("x64", systemInfo.Architecture);
            Assert.Equal(BiosMode.Uefi, systemInfo.BiosMode);
            Assert.Equal(SecureBootStatus.Enabled, systemInfo.SecureBoot);
        }

        [Fact]
        public void DiskInfo_SizeGB_ShouldCalculateCorrectly()
        {
            var disk = new DiskInfo
            {
                Size = 500L * 1024 * 1024 * 1024 // 500 GB
            };

            Assert.Equal(500.0, disk.SizeGB);
        }

        [Fact]
        public void PartitionInfo_HasUserData_ShouldReturnCorrectValue()
        {
            var systemPartition = new PartitionInfo
            {
                Type = PartitionType.ESP,
                IsSystemPartition = true
            };

            var dataPartition = new PartitionInfo
            {
                Type = PartitionType.Primary,
                IsSystemPartition = false
            };

            Assert.False(systemPartition.HasUserData);
            Assert.True(dataPartition.HasUserData);
        }

        [Fact]
        public void DeployResult_CreateSuccess_ShouldReturnSuccessResult()
        {
            var result = DeployResult.CreateSuccess();

            Assert.True(result.Success);
            Assert.Empty(result.ErrorMessage);
        }

        [Fact]
        public void DeployResult_CreateFailure_ShouldReturnFailureResult()
        {
            var result = DeployResult.CreateFailure("Test error", "ERR001", DeployPhase.Preparation);

            Assert.False(result.Success);
            Assert.Equal("Test error", result.ErrorMessage);
            Assert.Equal("ERR001", result.ErrorCode);
            Assert.Equal(DeployPhase.Preparation, result.FailedPhase);
        }
    }

    public class HelperTests
    {
        [Fact]
        public void PasswordGenerator_Generate_ShouldReturnValidPassword()
        {
            var password = PasswordGenerator.Generate(12);

            Assert.Equal(12, password.Length);
            Assert.True(password.Any(char.IsUpper));
            Assert.True(password.Any(char.IsLower));
            Assert.True(password.Any(char.IsDigit));
        }

        [Fact]
        public void PasswordGenerator_GenerateMemorable_ShouldReturnWordAndNumber()
        {
            var password = PasswordGenerator.GenerateMemorable();

            Assert.True(password.Length >= 7); // 4 letters + 4 digits
            Assert.True(password.Any(char.IsDigit));
            Assert.True(password.Any(char.IsLetter));
        }

        [Fact]
        public void HashHelper_CalculateSha256_ShouldReturnConsistentHash()
        {
            var input = "test string";
            var hash1 = HashHelper.CalculateSha256(input);
            var hash2 = HashHelper.CalculateSha256(input);

            Assert.Equal(hash1, hash2);
            Assert.Equal(64, hash1.Length); // SHA256 is 64 hex characters
        }
    }
}
