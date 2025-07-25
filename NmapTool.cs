using System.Collections.Generic;
using System.Diagnostics;
using Gtk;
using System.Text.RegularExpressions;

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
        private readonly NmapMode _mode = NmapMode.Sweep;

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

        public override int PREFERRED_HEIGHT => 250;

        protected override Widget BuildBody()
        {
            Logger.Log("Building Nmap tool body...");

            _outputView = new TextView()
            {
                Editable = false
            };
            _outputView.Buffer.TagTable.Add(_ipTag);

            var scroller = new ScrolledWindow()
            {
                _outputView
            };

            _outputView.ButtonPressEvent += OnTextViewClicked;

            return scroller;
        }

        private void OnTextViewClicked(object o, ButtonPressEventArgs args)
        {
            if (args.Event.Type != Gdk.EventType.ButtonPress || args.Event.Button != 1)
                return; // abort condition

            var tv = o as TextView ?? throw new Exception("o is not a TextView");

            tv.WindowToBufferCoords(TextWindowType.Text, (int)args.Event.X, (int)args.Event.Y, out int bufX, out int bufY);

            if (!tv.GetIterAtLocation(out TextIter iter, bufX, bufY))
                return; // abort condition

            var buffer = tv.Buffer;
            var ipTag = buffer.TagTable.Lookup("ip-link");
            if (ipTag == null) return; // abort condition
            if (!iter.HasTag(ipTag)) return; // abort condition

            var ipStart = iter;
            var ipEnd = iter;

            while (ipStart.BackwardChar() && ipStart.HasTag(ipTag)) { }
            while (ipEnd.ForwardChar() && ipEnd.HasTag(ipTag)) { }

            string ipText = tv.Buffer.GetText(ipStart, ipEnd, false);

            Console.WriteLine($"clicked IP: {ipText}");
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
                    NmapMode.Syn => $"nmap -sS {Host} -oG -",
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
        private void AddText(TextView tv, string text)
        {
            if (text.StartsWith('#')) return;

            // var end = tv.Buffer.EndIter;
            // tv.Buffer.Insert(ref end, $"{text}\n");

            var buffer = tv.Buffer;
            var startIter = buffer.EndIter;
            int startOffset = startIter.Offset;

            buffer.Insert(ref startIter, text + "\n");

            var endIter = buffer.EndIter;
            var insertedStart = buffer.GetIterAtOffset(startOffset);
            var insertedText = buffer.GetText(insertedStart, endIter, false);

            foreach (Match match in IP_REGEX.Matches(insertedText))
            {
                var ipStart = buffer.GetIterAtOffset(startOffset + match.Index);
                var ipEnd = buffer.GetIterAtOffset(startOffset + match.Index + match.Length);
                buffer.ApplyTag(buffer.TagTable.Lookup("ip-link"), ipStart, ipEnd);
            }

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