using Gtk;

namespace KaliDashboard
{
    public class LayoutManager
    {
        public Window Parent { get; set; }
        private Box _mainBox { get; }
        //private Box _currentColumn { get; set; }
        //private int _currentColumnHeight { get; set; }
        private int _preferredColumnHeight { get; }
        private int _preferredcolumnWidth { get; }

        private static int GetColumnHeight(Box col) => col.Children.Cast<Box>().Sum(b => b.AllocatedHeight);

        private void Refresh() => _mainBox.ShowAll();

        public LayoutManager(Window parent, MenuBar menuBar, int preferredColumnHeight = 550, int preferredcolumnWidth = 300)
        {
            Parent = parent;

            // base vertical box to contain the menu and the main horizontal box
            var vbox = new Box(Orientation.Vertical, 5);

            // Add the menus
            vbox.PackStart(menuBar, false, false, 0);

            // base horizontal box
            _mainBox = new Box(Orientation.Horizontal, 5);
            vbox.PackStart(_mainBox, true, true, 5);

            // This is removed due to dynamic adding/removing of columns as needed
            // initial column - add to hbox
            //_currentColumn = new Box(Orientation.Vertical, 5); // this shouldn't be needed, but the compiler complains if it isn't here.
            //AddNewColumn();

            // preferred column height
            _preferredColumnHeight = preferredColumnHeight;

            // Add primary vbox to the parent window
            parent.Add(vbox);

            // Display - should do nothing at this point, but meh!
            Refresh();
        }

        public void AddTool(ToolPanel toolPanel)
        {
            // get the first column that has room - make a new one if necessary
            Box? colToUse = null;
            foreach (var col in _mainBox.Children.Cast<Box>())
            {
                if (toolPanel.PREFERRED_HEIGHT + GetColumnHeight(col) <= _preferredColumnHeight)
                {
                    colToUse = col;
                    break;
                }
            }
            colToUse ??= AddNewColumn();

            if (toolPanel == null) throw new Exception("Need to specify a tool to add");

            toolPanel.CloseRequested += ToolClosed;

            // add this tool panel to the current column, track the column size, then refresh
            toolPanel.Container.HeightRequest = toolPanel.PREFERRED_HEIGHT;

            colToUse.PackStart(toolPanel, false, false, 5);

            Refresh();
        }

        private void ToolClosed(ToolPanel tool)
        {
            //if (tool == null) Console.WriteLine("tool is null");
            if (tool.Container.Parent == null) // in case the last column has been removed
            {
                tool.Dispose();
                return;
            }

            // get the tool's column
            if (tool.Container.Parent is not Box col) throw new Exception($"What type is col? {tool.Container.Parent.GetType()}");

            // if there's nothing left in the column, remove it
            if (col.Children.Length == 1) // this is the last tool
            {
                if (col.Parent is not Box parent) throw new Exception($"What type is parent? {col.Parent.GetType()}");
                parent.Remove(col);
                col.Dispose();
                Console.WriteLine($"Removed col. Current cols left: {parent.Children.Length}");
            }

            // let the tool finish closing itself
            tool.Dispose();
        }

        private Box AddNewColumn()
        {
            var newCol = new Box(Orientation.Vertical, 5)
            {
                WidthRequest = _preferredcolumnWidth
            };

            _mainBox.PackStart(newCol, true, true, 5);
            Console.WriteLine($"New column added.  Current count: {_mainBox.Children.Length}?");

            return newCol;
        }
    }
}