using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class EditDialog : Window
    {
        private readonly UserControl _control;

        public EditDialog(UserControl control, string title)
        {
            InitializeComponent();
            Title = title;
            _control = control;
            EditContentControl.Content = _control;
        }
        public object GetEditedItem()
        {
            var method = _control.GetType().GetMethod("GetUser")
                      ?? _control.GetType().GetMethod("GetStatus")
                      ?? _control.GetType().GetMethod("GetItem")
                      ?? _control.GetType().GetMethod("GetEditedItem");

            return method?.Invoke(_control, null);
        }
    }
}
