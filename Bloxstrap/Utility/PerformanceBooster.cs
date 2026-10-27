namespace Bloxstrap.Utility
{
    /// <summary>
    /// Optional pre-launch optimizations. Every step is opt-in via Settings and
    /// failures are logged and ignored, so they can never stop Roblox from launching.
    /// </summary>
    public static class PerformanceBooster
    {
        private const string LOG_IDENT = "PerformanceBooster";

        // never killed, even if the user types them into the list
        private static readonly HashSet<string> Protected = new(StringComparer.OrdinalIgnoreCase)
        {
            "explorer", "winlogon", "csrss", "wininit", "services", "lsass", "svchost",
            "dwm", "smss", "system", "taskmgr", "RobloxPlayerBeta", "RobloxStudioBeta"
        };

        /// <summary>Runs the steps that must happen before Roblox is started.</summary>
        public static void RunBeforeLaunch()
        {
            var s = App.Settings.Prop;

            if (s.BoostCleanLogs)
                Try(CleanLogs, "CleanLogs");

            if (s.BoostHighPerformancePlan)
                Try(SetHighPerformancePlan, "SetHighPerformancePlan");

            if (s.BoostCloseApps)
                Try(CloseApps, "CloseApps");
        }

        /// <summary>
        /// Raises Roblox's priority. Called with the Process object returned by Process.Start,
        /// so no extra handle to the game process is opened.
        /// </summary>
        public static void ApplyPriority(Process process)
        {
            if (!App.Settings.Prop.BoostHighPriority)
                return;

            Try(() =>
            {
                process.PriorityClass = ProcessPriorityClass.High;
                App.Logger.WriteLine(LOG_IDENT, "Set Roblox priority to High");
            }, "ApplyPriority");
        }

        private static void CleanLogs()
        {
            string logDir = Path.Combine(Paths.LocalAppData, "Roblox", "logs");

            if (!Directory.Exists(logDir))
                return;

            long freed = 0;

            foreach (string file in Directory.EnumerateFiles(logDir))
            {
                try
                {
                    var info = new FileInfo(file);
                    long size = info.Length;
                    info.Delete();
                    freed += size;
                }
                catch
                {
                    // in use by another Roblox instance, skip it
                }
            }

            App.Logger.WriteLine(LOG_IDENT, $"Cleaned Roblox logs ({freed / 1024 / 1024.0:0.0} MB)");
        }

        private static void SetHighPerformancePlan()
        {
            // SCHEME_MIN is the built-in alias for the High performance plan
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = "/setactive SCHEME_MIN",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            process?.WaitForExit(5000);
            App.Logger.WriteLine(LOG_IDENT, $"powercfg exited with code {process?.ExitCode}");
        }

        private static void CloseApps()
        {
            int ownPid = Environment.ProcessId;

            var names = App.Settings.Prop.BoostAppsToClose
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? x[..^4] : x)
                .Where(x => x.Length > 0 && !Protected.Contains(x))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (string name in names)
            {
                foreach (var process in Process.GetProcessesByName(name))
                {
                    using (process)
                    {
                        if (process.Id == ownPid)
                            continue;

                        try
                        {
                            process.Kill();
                            App.Logger.WriteLine(LOG_IDENT, $"Closed {name} (PID {process.Id})");
                        }
                        catch (Exception ex)
                        {
                            App.Logger.WriteLine(LOG_IDENT, $"Could not close {name}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static void Try(Action action, string step)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"{step} failed: {ex.Message}");
            }
        }
    }
}
