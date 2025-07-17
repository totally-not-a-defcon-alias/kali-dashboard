using System;
using System.Net.NetworkInformation;
using Gtk;

class Program
{
    private static Window? _window;

    static void Main(string[] args)
    {
        Application.Init();

        _window = new Window("Tool Dashboard");
        _window.SetDefaultSize(600, 400);
        _window.DeleteEvent += (o, e) => Application.Quit();

        //var mainBox = new VBox(false, 5);
        var mainBox = new Box(Orientation.Vertical, 5);
        var pingTool = new PingTool("8.8.8.8");

        mainBox.PackStart(pingTool.Container, true, true, 0);

        _window.Add(mainBox);
        _window.ShowAll();
        
        Application.Run();
    }

    private static void MessageBox(string msg)
    {
        var dialog = new MessageDialog(
            parent_window: _window,
            DialogFlags.Modal,
            MessageType.Info,
            ButtonsType.Ok,
            msg
        );

        dialog.Run();
        dialog.Destroy();
    }
}