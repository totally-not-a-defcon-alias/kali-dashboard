using System;
using System.Net.NetworkInformation;
using Gtk;
using KaliDashboard;

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
            var host = DialogHelper.Prompt("New Ping Tool", "Enter host/IP to ping");
            if (host == null) return;
            
            var tool = new PingTool(host);

            tool.CloseRequested += t =>
            {
                toolContainerVBox.Remove(t.Container);
                t.Dispose();
            };
            toolContainerVBox.PackStart(tool.Container, true, true, 5);
            tool.Container?.ShowAll();
            toolContainerVBox.ShowAll();
        };

        window.ShowAll();        
        Application.Run();
    }
}