using Gtk;

public static class DialogHelper
{
    public static string? Prompt(string title, string prompt)
    {
        var dialog = new Dialog(title, null, DialogFlags.Modal);
        dialog.AddButton("Cancel", ResponseType.Cancel);
        dialog.AddButton("OK", ResponseType.Ok);

        var label = new Label(prompt);
        var entry = new Entry();
        var content = dialog.ContentArea;
        content.Spacing = 10;
        content.BorderWidth = 10;
        content.PackStart(label, false, false, 0);
        content.PackStart(entry, false, false, 0);

        dialog.ShowAll();

        string? result = null;
        if (dialog.Run() == (int)ResponseType.Ok && !string.IsNullOrWhiteSpace(entry.Text))
        {
            result = entry.Text.Trim();
            dialog.Destroy();
        }

        return result;
    }

   public static void MessageBox(Window parent, string msg)
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