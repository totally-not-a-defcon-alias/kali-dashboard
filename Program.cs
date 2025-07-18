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

        // Layout stuff
        _layoutManager = new LayoutManager(_window);

        // Menu Bar
        CreateMenus();

        // Scrollable tool area
        // var toolContainerVBox = new Box(Orientation.Vertical, 5);
        // var scroller = new ScrolledWindow
        // {
        //     toolContainerVBox
        // };
        // scroller.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        // mainVBox.PackStart(scroller, true, true, 0);

        Logger.Log("Init complete.  Starting app...");

        _window.ShowAll();
        Application.Run();
    }

    private static void CreateMenus()
    {
        if (_window == null) throw new Exception("_window is null");
        if (_layoutManager == null) throw new Exception("_layoutManager is null");

        var primaryVBox = new Box(Orientation.Vertical, 5);

        var menuBar = new MenuBar();
        var toolsMenuItem = new MenuItem("Tools");
        var toolsSubMenu = new Menu();
        var newPingToolItem = new MenuItem("Ping");

        toolsSubMenu.Append(newPingToolItem);
        toolsMenuItem.Submenu = toolsSubMenu;
        menuBar.Append(toolsMenuItem);

        primaryVBox.PackStart(menuBar, false, false, 5);
        _window.Add(primaryVBox);

                // Hook menu click
        newPingToolItem.Activated += (_, _) =>
        {
            Logger.Log("Creating new Ping tool");
            var host = DialogHelper.Prompt("New Ping Tool", "Enter host/IP to ping");
            if (host == null) return;

            var tool = new PingTool(host);
            _layoutManager.AddTool(tool);

            // // Column wrap logic
            // if (currentColumnHeight + PingTool.PREFERRED_HEIGHT > MAX_COLUMN_HEIGHT)
            // {
            //     Logger.Log("Added new column");

            //     currentColumn = new Box(Orientation.Vertical, 5);
            //     columnList.Add(currentColumn);

            //     toolColumns.Add(currentColumn);
            //     toolColumns.PackStart(currentColumn, false, false, 5);
            //     currentColumnHeight = 0;
            // }
            // else
            // {
            //     Logger.Log($"Did not add new column. currentColumnHeight at the time was {currentColumnHeight}");
            // }

            // var tool = new PingTool(host);
            // tool.Container.HeightRequest = PingTool.PREFERRED_HEIGHT;
            // currentColumnHeight += PingTool.PREFERRED_HEIGHT;

            // Close the tool
            tool.CloseRequested += t =>
            {
                Logger.Log("Tool requested to close...");

                var parent = t.Container.Parent as Box;
                parent?.Remove(t.Container);

                t.Dispose();
            };

            // Start the tool
            //Logger.Log("starting the tool...");
            //toolContainerVBox.PackStart(tool.Container, true, true, 5);
            //currentColumn.PackStart(tool.Container, true, true, 5);
            //tool.Container?.ShowAll();
            //toolContainerVBox.ShowAll();
            //currentColumn.ShowAll();
            //toolColumns.ShowAll();
        };

    }
}