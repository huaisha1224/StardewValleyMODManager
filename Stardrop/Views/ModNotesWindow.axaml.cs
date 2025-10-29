using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Stardrop.Views
{
    public partial class ModNotesWindow : Window
    {
        public ModNotesWindow()
        {
            InitializeComponent();
            InitializeEventHandlers();
        }

        public ModNotesWindow(string notes) : this()
        {
            var notesTextBox = this.FindControl<TextBox>("notesTextBox");
            notesTextBox.Text = notes;
        }

        public string Notes
        {
            get
            {
                var notesTextBox = this.FindControl<TextBox>("notesTextBox");
                return notesTextBox?.Text ?? string.Empty;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitializeEventHandlers()
        {
            var okButton = this.FindControl<Button>("okButton");
            var cancelButton = this.FindControl<Button>("cancelButton");

            okButton.Click += (sender, e) => {
                Close(true);
            };
            
            cancelButton.Click += (sender, e) => {
                Close(false);
            };
        }
    }
}