using System.Diagnostics;
using System.IO;

public class PythonRunner : IDisposable
{
    private Process? _pythonProcess;
    private bool _isRunning = false;


    public async Task<string> RunAsync(string task)
    {
        if (_isRunning)
            throw new InvalidOperationException("Python script is already running.");

        string pythonFilePath = GetAgentRunnerPath();
        if (string.IsNullOrEmpty(pythonFilePath))
        {
            throw new FileNotFoundException("agent_runner.py bulunamadı.");
        }
        string arguments = $"\"{pythonFilePath}\" \"{task}\"";

        var tcs = new TaskCompletionSource<string>();

        using (var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            }
        })
        {
            _isRunning = true;
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            _isRunning = false;

            if (!string.IsNullOrEmpty(errorTask.Result))
                throw new Exception($"Python Error: {errorTask.Result}");

            return outputTask.Result;
        }
    }

    private string GetAgentRunnerPath()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        DirectoryInfo directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            if (Directory.GetFiles(directory.FullName, "*.sln").Length > 0)
            {
                return Path.Combine(directory.FullName, "Python", "browser-use", "my_scripts", "agent_runner.py");
            }
            directory = directory.Parent;
        }

        return null;
    }


    public void Dispose()
    {
        if (_pythonProcess != null && !_pythonProcess.HasExited)
        {
            _pythonProcess.Kill();
            _pythonProcess.Dispose();
            _isRunning = false;
        }
    }
}
