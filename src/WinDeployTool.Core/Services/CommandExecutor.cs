using System.Diagnostics;
using Serilog;

namespace WinDeployTool.Core.Services
{
    /// <summary>
    /// 命令执行器
    /// </summary>
    public class CommandExecutor
    {
        private readonly ILogger _logger;
        private readonly int _defaultTimeout;

        public CommandExecutor(ILogger logger, int defaultTimeoutSeconds = 300)
        {
            _logger = logger;
            _defaultTimeout = defaultTimeoutSeconds * 1000;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public async Task<CommandResult> ExecuteAsync(string fileName, string arguments, 
            string? workingDirectory = null, 
            int? timeoutMs = null,
            IProgress<string>? progress = null)
        {
            _logger.Debug("Executing command: {FileName} {Arguments}", fileName, arguments);

            var result = new CommandResult();
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            // 处理输出
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                    progress?.Report(e.Data);
                    _logger.Debug("[STDOUT] {Data}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    _logger.Warning("[STDERR] {Data}", e.Data);
                }
            };

            try
            {
                var startTime = DateTime.Now;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var timeout = timeoutMs ?? _defaultTimeout;
                var completed = await Task.Run(() => process.WaitForExit(timeout));

                if (!completed)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }

                    result.ExitCode = -1;
                    result.ErrorMessage = $"命令执行超时（{timeout}ms）";
                    _logger.Error("Command timeout: {FileName} {Arguments}", fileName, arguments);
                    return result;
                }

                result.ExitCode = process.ExitCode;
                result.StandardOutput = outputBuilder.ToString();
                result.StandardError = errorBuilder.ToString();
                result.ExecutionTime = DateTime.Now - startTime;

                if (process.ExitCode != 0)
                {
                    result.ErrorMessage = $"命令执行失败，退出码：{process.ExitCode}";
                    if (!string.IsNullOrEmpty(result.StandardError))
                    {
                        result.ErrorMessage += $"\n错误输出：{result.StandardError}";
                    }
                    _logger.Error("Command failed with exit code {ExitCode}: {FileName} {Arguments}", 
                        process.ExitCode, fileName, arguments);
                }
                else
                {
                    _logger.Debug("Command completed successfully in {Duration}ms", 
                        result.ExecutionTime.TotalMilliseconds);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.ErrorMessage = $"执行命令时发生异常：{ex.Message}";
                _logger.Error(ex, "Exception executing command: {FileName} {Arguments}", fileName, arguments);
                return result;
            }
        }

        /// <summary>
        /// 执行 PowerShell 脚本
        /// </summary>
        public async Task<CommandResult> ExecutePowerShellAsync(string script, 
            IProgress<string>? progress = null)
        {
            var arguments = $"-ExecutionPolicy Bypass -Command \"{script.Replace("\"", "\"\"")}\"";
            return await ExecuteAsync("powershell.exe", arguments, progress: progress);
        }

        /// <summary>
        /// 执行 DISM 命令
        /// </summary>
        public async Task<CommandResult> ExecuteDismAsync(string arguments, 
            IProgress<string>? progress = null)
        {
            return await ExecuteAsync("dism.exe", arguments, progress: progress);
        }

        /// <summary>
        /// 执行 diskpart 脚本
        /// </summary>
        public async Task<CommandResult> ExecuteDiskPartAsync(string script)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(tempFile, script);
                return await ExecuteAsync("diskpart.exe", $"/s \"{tempFile}\"");
            }
            finally
            {
                try { File.Delete(tempFile); } catch { }
            }
        }
    }

    /// <summary>
    /// 命令执行结果
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// 退出码
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// 标准输出
        /// </summary>
        public string StandardOutput { get; set; } = string.Empty;

        /// <summary>
        /// 标准错误
        /// </summary>
        public string StandardError { get; set; } = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 执行时间
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => ExitCode == 0;
    }
}
