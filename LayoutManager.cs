using Gtk;

namespace KaliDashboard
{
    public class LayoutManager
    {
        public Window Parent { get; set; }
        private Box _mainBox { get; }
        private Box _currentColumn { get; set; }
        private int _currentColumnHeight { get; set; }
        private int _preferredColumnHeight { get; }
        private int _preferredcolumnWidth { get; }

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
            
            // initial column - add to hbox
            _currentColumn = new Box(Orientation.Vertical, 5); // this shouldn't be needed, but the compiler complains if it isn't here.
            AddNewColumn();

            // preferred column height
            _preferredColumnHeight = preferredColumnHeight;

            // Add primary vbox to the parent window
            parent.Add(vbox);

            // Display - should do nothing at this point, but meh!
            Refresh();
        }

        public void AddTool(ToolPanel toolPanel)
        {
            // Test to see if the new tool would overflow the preferred column height
            if (toolPanel.PREFERRED_HEIGHT + _currentColumnHeight > _preferredColumnHeight)
            {
                // new tool would overflow... make a new column
                AddNewColumn();
            }

            // add this tool panel to the current column, track the column size, then refresh
            toolPanel.Container.HeightRequest = toolPanel.PREFERRED_HEIGHT;

            _currentColumn.PackStart(toolPanel, false, false, 5);
            _currentColumnHeight += toolPanel.PREFERRED_HEIGHT;

            Refresh();
        }

        private void AddNewColumn()
        {
            _currentColumn = new Box(Orientation.Vertical, 5)
            {
                WidthRequest = _preferredcolumnWidth
            };

            _mainBox.PackStart(_currentColumn, true, true, 5);
            
            _currentColumnHeight = 0;
        }
    }
}