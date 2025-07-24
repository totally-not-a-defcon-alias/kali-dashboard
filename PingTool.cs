using System.Diagnostics;
using Gtk;

namespace KaliDashboard
{
    public class PingTool : ToolPanel
    {
        //protected readonly string? _host;
        protected override string? Host { get; set; }

        public override int PREFERRED_HEIGHT => 200;

        public PingTool(string host) : base($"Ping: {host}", host)
        {
            Logger.Log("Creating Ping tool...");
            Host = host;
            Start();
        }

        protected override Widget BuildBody()
        {
            Logger.Log("Building Ping Tool body...");
            _outputView = new TextView
            {
                Editable = false
            };

            var scroller = new ScrolledWindow
            {
                _outputView
            };
            return scroller;
        }

        protected override void Start()
        {
            Logger.Log("Starting Ping Tool...");

            _process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ping",
                    Arguments = $"{Host}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (x, e) =>
            {
                if (e.Data != null && _outputView != null)
                    Application.Invoke(delegate { AddText(_outputView, e.Data); });

            };

            _process.ErrorDataReceived += (x, e) =>
            {
                if (e.Data != null && _outputView != null)
                    Application.Invoke(delegate { AddText(_outputView, $"[err] {e.Data}"); });
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            _running = true;
        }

        private static void AddText(TextView tv, string text)
        {
            var end = tv.Buffer.EndIter;
            tv.Buffer.Insert(ref end, $"{text}\n");

            GLib.Idle.Add(() =>
            {
                tv.ScrollToIter(tv.Buffer.EndIter, 0, false, 0, 0);
                return false;
            });
        }

        public override void Stop()
        {
            if (!_running) return;

            if (_process != null && !_process.HasExited)
            {
                Logger.Log("Stopping Ping Tool...");
                _process.Kill();
            }

            _running = false;
        }

        public override void Dispose()
        {
            Logger.Log($"Disposing Ping tool for {Host}");

            Stop();

            _process?.Dispose();
            _process = null;

            // base.Dispose() calls GC.SuppressFinalize(this)
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        public static PingTool? Create()
        {
            Logger.Log("Creating new Ping tool");
            var host = DialogHelper.Prompt("New Ping Tool", "Enter host/IP to ping");
            if (host == null) return null;

            var tool = new PingTool(host);

            // Close the tool
            tool.CloseRequested += t =>
            {
                Logger.Log("Tool requested to close...");

                var parent = t.Container.Parent as Box;
                parent?.Remove(t.Container);

                t.Dispose();
            };

            return tool;
        }
    }
}