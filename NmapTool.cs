using System.Collections.Generic;
using System.Diagnostics;
using Gtk;

namespace KaliDashboard
{
    public class NmapTool : ToolPanel
    {
        public enum NmapMode
        {
            Sweep, // "SWEEP Mode", -sn, fast, stealthy (host detection)
            Syn, // Stealthy, fast, accurate (open port detection)
            Tcp, // Full TCP handshake, logged by target, slow
            Udp, // Slow, noisy, hosts often don't respond
            Ack, // Firewall scan
            Ver, // Detect software version(s), combine with Syn or Tcp (service detection)
            OS, // Detect OS 
        }
        private NmapMode _mode = NmapMode.Sweep;

        // private static Dictionary<NmapMode, string> NmapModeMap()
        // => new()
        // {
        //     { NmapMode.Sweep, "sn" },
        //     { NmapMode.Syn, "sS" },
        //     { NmapMode.Tcp, "sT" },
        //     { NmapMode.Udp, "sU" },
        //     { NmapMode.Ack, "sA" },
        //     { NmapMode.Ver, "sV" },
        //     { NmapMode.OS, "O" }
        // };

        protected override string? Host { get; set; }

        public NmapTool(string host, NmapMode mode) : base($"Nmap: {host}", host)
        {
            Logger.Log("Constructing Nmap tool...");
            Host = host;
            _mode = mode;
            Start();
        }

        public override int PREFERRED_HEIGHT => 200;

        protected override Widget BuildBody()
        {
            Logger.Log("Building Nmap tool body...");

            _outputView = new TextView()
            {
                Editable = false
            };

            var scroller = new ScrolledWindow()
            {
                _outputView
            };

            return scroller;
        }

        protected override void Start()
        {
            Logger.Log("Starting Nmap tool...");

            var startInfo = new ProcessStartInfo
            {
                FileName = "sudo",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = _mode switch
                {
                    NmapMode.Sweep => $"nmap -sn {Host} -oG -",
                    _ => throw new Exception($"Nmap mode not implemented: {_mode}"),
                }
            };

            _process = new Process()
            {
                StartInfo = startInfo,
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
            if (text.StartsWith('#')) return;

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
                Logger.Log("Stopping Nmap Tool...");
                _process.Kill();
            }

            _running = false;
        }

        public override void Dispose()
        {
            Logger.Log($"Disposing Nmap tool for {Host}");

            Stop();

            _process?.Dispose();
            _process = null;

            // base.Dispose() calls GC.SuppressFinalize(this)
            GC.SuppressFinalize(this);
            base.Dispose();
        }


        public static NmapTool? Create(NmapMode mode)
        {
            Logger.Log("Creating new Nmap tool");
            var host = DialogHelper.Prompt("New Nmap Tool", $"Enter host/IP/subnet to scan in ${mode} mode.");
            if (host == null) return null;

            var tool = new NmapTool(host, mode);

            // Close the tool
            tool.CloseRequested += t =>
            {
                Logger.Log("Nmap requested to close...");

                var parent = t.Container.Parent as Box;
                parent?.Remove(t.Container);

                t.Dispose();
            };

            return tool;
        }
    }
}