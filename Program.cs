using System;
using System.Net.NetworkInformation;
using Gtk;
using KaliDashboard;

class Program
{
    private const int MAX_COLUMN_HEIGHT = 200; // screen is 1080x600    

    private static LayoutManager? _layoutManager;
    private static Window? _window;

    static void Main(string[] args)
    {
        Logger.Log("Application initialization");
        Application.Init();

        _window = new Window("Kali Dashboard");
        _window.SetDefaultSize(800, 600);
        _window.DeleteEvent += (_, _) => Application.Quit();

        // Make the menus
        var menuBar = CreateMenus();

        // Layout stuff
        _layoutManager = new LayoutManager(_window, menuBar);

        Logger.Log("Init complete.  Starting app...");

        _window.ShowAll();
        Application.Run();
    }

    private static MenuBar CreateMenus()
    {
        if (_window == null) throw new Exception("_window is null");
        //if (_layoutManager == null) throw new Exception("_layoutManager is null");

        var menuBar = new MenuBar();
        var toolsMenuItem = new MenuItem("Tools");
        var toolsSubMenu = new Menu();
        var newPingToolItem = new MenuItem("Ping");

        toolsSubMenu.Append(newPingToolItem);
        toolsMenuItem.Submenu = toolsSubMenu;
        menuBar.Append(toolsMenuItem);

        // Hook menu click
        newPingToolItem.Activated += (_, _) =>
        {
            Logger.Log("Creating new Ping tool");
            var host = DialogHelper.Prompt("New Ping Tool", "Enter host/IP to ping");
            if (host == null) return;

            var tool = new PingTool(host);
            _layoutManager?.AddTool(tool);

            // Close the tool
            tool.CloseRequested += t =>
            {
                Logger.Log("Tool requested to close...");

                var parent = t.Container.Parent as Box;
                parent?.Remove(t.Container);

                t.Dispose();
            };
        };

        return menuBar;
    }
}