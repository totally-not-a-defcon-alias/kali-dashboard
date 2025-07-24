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
        var newNmapSweepToolItem = new MenuItem("Nmap (sweep)");
        var newNmapPokeToolItem = new MenuItem("Nmap (poke)");

        toolsSubMenu.Append(newPingToolItem);
        toolsSubMenu.Append(newNmapSweepToolItem);
        toolsSubMenu.Append(newNmapPokeToolItem);
        
        toolsMenuItem.Submenu = toolsSubMenu;
        menuBar.Append(toolsMenuItem);
        
        // Hook menu clicks
        newPingToolItem.Activated += (_, _) =>
        {
            var tool = PingTool.Create();
            if (tool == null) return;

            _layoutManager?.AddTool(tool);
        };

        newNmapSweepToolItem.Activated += (_, _) =>
        {
            var tool = NmapTool.Create(NmapTool.NmapMode.Sweep);
            if (tool == null) return;

            _layoutManager?.AddTool(tool);
        };

        return menuBar;
    }
}