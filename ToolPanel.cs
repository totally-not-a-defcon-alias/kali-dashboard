using Gtk;

namespace KaliDashboard
{
    public abstract class ToolPanel : IDisposable
    {
        private readonly Box _outerBox;
        private readonly Button _closeButton;
        private readonly Label _titleLabel;

        public Widget Container => _outerBox;

        protected ToolPanel(string title)
        {
            //_outerBox = new VBox(false, 2);
            _outerBox = new Box(Orientation.Vertical, 2);
            //var headerBox = new HBox(false, 5);
            var headerBox = new Box(Orientation.Horizontal, 5);

            _titleLabel = new Label(title);
            _closeButton = new Button("X");
            _closeButton.WidthRequest = 24;
            _closeButton.Clicked += (_, _) => OnCloseRequested();

            headerBox.PackStart(_titleLabel, true, true, 0);
            headerBox.PackEnd(_closeButton, false, false, 0);
            _outerBox.PackStart(headerBox, false, false, 0);

            var body = BuildBody();
            _outerBox.PackStart(body, true, true, 0);
        }

        protected abstract Widget BuildBody();

        public event Action<ToolPanel>? CloseRequested;

        private void OnCloseRequested()
        {
            CloseRequested?.Invoke(this);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}