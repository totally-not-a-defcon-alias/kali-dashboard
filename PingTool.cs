using System.Diagnostics;
using Gtk;

namespace KaliDashboard
{
    public class PingTool : ToolPanel
    {
        private TextView? _outputView;
        private Process? _process;

        public PingTool(string host) : base($"Ping: {host}")
        {
            Start(host);
        }

        protected override Widget BuildBody()
        {
            _outputView = new TextView
            {
                Editable = false
            };

            var scroller = new ScrolledWindow();
            scroller.Add(_outputView);
            return scroller;
        }

        private void Start(string host)
        {
            //...??
            Stop();

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

        public void Stop()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
                _process.Dispose();
            }
        }

        public override void Dispose()
        {
            if (_process != null)
            {
                if (_process.HasExited)
                    _process.Kill();

                _process.Dispose();
                _process = null;
            }

            base.Dispose();
        }
    }
}