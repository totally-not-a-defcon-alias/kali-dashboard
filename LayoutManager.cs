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

        private void Refresh() => _mainBox.ShowAll();

        public LayoutManager(Window parent, int preferredColumnHeight = 550)
        {
            Parent = parent;

            // base horizontal box
            _mainBox = new Box(Orientation.Horizontal, 5);
            parent.Add(_mainBox);
            
            // initial column - add to hbox
            _currentColumn = new Box(Orientation.Vertical, 5); // this shouldn't be needed, but the compiler complains if it isn't here.
            AddNewColumn();

            // preferred column height
            _preferredColumnHeight = preferredColumnHeight;

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
            _currentColumn.PackStart(toolPanel, false, false, 5);
            _currentColumnHeight += toolPanel.PREFERRED_HEIGHT;

            Refresh();
        }

        private void AddNewColumn()
        {
            _currentColumn = new Box(Orientation.Vertical, 5);
            _mainBox.PackStart(_currentColumn, false, false, 5);
            
            _currentColumnHeight = 0;
        }
    }
}