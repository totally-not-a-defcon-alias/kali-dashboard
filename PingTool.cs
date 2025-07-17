using System;
using System.Diagnostics;
using Atk;
using Gtk;
using Microsoft.VisualBasic;

public class PingTool : IDisposable
{
    private Process? _process;
    private readonly TextView _outputView;
    private readonly Box _container;
    public Widget Container => _container;

    //public event Action<string>? OutputLine;

    public PingTool(string host)
    {
        _outputView = new()
        {
            Editable = false
        };

        var scroller = new ScrolledWindow();
        scroller.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroller.Add(_outputView);

        var startButton = new Button("Start Ping");
        startButton.Clicked += (_, _) => Start(host);

        //_container = new VBox(false, 5);
        _container = new Box(Orientation.Vertical, 5);

        _container.PackStart(startButton, false, false, 0);
        _container.PackStart(scroller, true, true, 0);

    }
    public void Start(string host)
    {
        Stop(); // clean up if already running
        _process = new Process
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

        _process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                Application.Invoke(delegate { AddText(_outputView, e.Data); });
            }
        };

        _process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                Application.Invoke(delegate { AddText(_outputView, $"[err] {e.Data}"); });
            }
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    private static void AddText(TextView buffer, string text)
    {
        var end = buffer.Buffer.EndIter;
        buffer.Buffer.Insert(ref end, $"{text}\n");

        GLib.Idle.Add(() =>
        {
            buffer.ScrollToIter(buffer.Buffer.EndIter, 0, false, 0, 0);
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

    public void Dispose()
    {
        if (_process != null)
        {
            if (!_process.HasExited)
                _process.Kill();

            _process.Dispose();
            _process = null;
        }

        GC.SuppressFinalize(this);
    }
}