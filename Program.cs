using System;
using System.Net.NetworkInformation;
using Gtk;

class Program
{
    

    static void Main(string[] args)
    {
        Application.Init();

        var window = new Window("Kali Dashboard");
        window.SetDefaultSize(800, 600);
        window.DeleteEvent += (_, _) => Application.Quit();

        var mainVBox = new Box(Orientation.Vertical, 0);
        window.Add(mainVBox);

        // Menu Bar
        var menuBar = new MenuBar();
        var toolsMenuItem = new MenuItem("Tools");
        var toolsSubMenu = new Menu();
        var newPingToolItem = new MenuItem("Ping");
        
        toolsSubMenu.Appen(newPingToolItem);
        toolsMenuItem.Submenu = toolsSubMenu;
        menuBar.Append(toolsMenuItem);

        mainVBox.PackStart(menuBar, false, false, 0);

        // Scrollable tool area
        var toolContainerVBox = new Box(Orientation.Vertical, 5);
        var scroller = new ScrolledWindow();

        scroller.AddWithViewport(toolContainerVBox);
        scroller.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

        // Hook menu click
        newPintToolItem.Activated += (_, _) => {
            var tool = new PingTool("8.8.8.8");

            toolContainerVBox.PackStart(tool.Container, false, false, 5);
            toolContainerVBox.ShowAll();
        };

        window.ShowAll();        
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