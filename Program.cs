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
        
        toolsSubMenu.Append(newPingToolItem);
        toolsMenuItem.Submenu = toolsSubMenu;
        menuBar.Append(toolsMenuItem);

        mainVBox.PackStart(menuBar, false, false, 0);

        // Scrollable tool area
        var toolContainerVBox = new Box(Orientation.Vertical, 5);
        var scroller = new ScrolledWindow
        {
            toolContainerVBox
        };
        scroller.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        mainVBox.PackStart(scroller, true, true, 0);

        // Hook menu click
        newPingToolItem.Activated += (_, _) => {
            //MessageBox(window, "Ping tool clicked");
            var tool = new PingTool("8.8.8.8");

            //Console.WriteLine($"Container type: {tool.Container?.GetType().Name}");

            toolContainerVBox.PackStart(tool.Container, true, true, 5);
            tool.Container?.ShowAll();
            toolContainerVBox.ShowAll();
        };

        // var t = new PingTool("8.8.8.8");
        // t.Container.Show();
        // t.Container.ShowAll();
        // toolContainerVBox.PackStart(t.Container, false, false, 5);
        // toolContainerVBox.ShowAll();

        window.ShowAll();        
        Application.Run();
    }

    private static void MessageBox(Window parent, string msg)
    {
        var dialog = new MessageDialog(
            parent_window: parent,
            DialogFlags.Modal,
            MessageType.Info,
            ButtonsType.Ok,
            msg
        );

        dialog.Run();
        dialog.Destroy();
    }
}