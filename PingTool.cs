using System.Diagnostics;
using Gtk;

namespace KaliDashboard
{
    public class PingTool : ToolPanel
    {
        private TextView? _outputView;
        private Process? _process;
        private readonly string? _host;

        public override int PREFERRED_HEIGHT => 200;
        
        public PingTool(string host) : base($"Ping: {host}")
        {
            Logger.Log("Creating Ping tool...");
            _host = host;
            Start(host);
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

        private void Start(string host)
        {
            Logger.Log("Starting Ping Tool...");

            _process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ping",
                    Arguments = $"-O {host}",
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

            Logger.Log("Overriden stop called...");

            if (_process != null && !_process.HasExited)
            {
                Logger.Log("Stopping Ping Tool...");
                _process.Kill();
            }

            _running = false;
        }

        public override void Dispose()
        {
            Logger.Log($"Disposing Ping tool for {_host}");

            Stop();

            _process?.Dispose();
            _process = null;

            // base.Dispose() calls GC.SuppressFinalize(this)
            GC.SuppressFinalize(this);
            base.Dispose();
        }
    }
}