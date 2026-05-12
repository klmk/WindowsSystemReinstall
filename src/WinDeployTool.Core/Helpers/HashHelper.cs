using System.Security.Cryptography;

namespace WinDeployTool.Core.Helpers
{
    /// <summary>
    /// 哈希计算助手
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// 计算文件SHA256哈希
        /// </summary>
        public static async Task<string> CalculateSha256Async(string filePath)
        {
            using var sha256 = SHA256.Create();
            await using var stream = File.OpenRead(filePath);
            var hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 计算文件MD5哈希
        /// </summary>
        public static async Task<string> CalculateMd5Async(string filePath)
        {
            using var md5 = MD5.Create();
            await using var stream = File.OpenRead(filePath);
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 计算字符串SHA256
        /// </summary>
        public static string CalculateSha256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
