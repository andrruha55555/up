using System.Linq;
using System.Reflection;
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

            _control = control;
            Title = title;

            EditContentControl.Content = _control;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!TryValidateControl(_control))
                return;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public object GetEditedItem()
        {
            if (_control == null) return null;

            // 1) Если контрол умеет GetEditedItem() — используем его (как в Users)
            var mi = _control.GetType().GetMethod("GetEditedItem", BindingFlags.Instance | BindingFlags.Public);
            if (mi != null && mi.GetParameters().Length == 0)
                return mi.Invoke(_control, null);

            // 2) Иначе ищем любой public GetXxx() без параметров, который возвращает НЕ void
            var getters = _control.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name.StartsWith("Get") &&
                            m.GetParameters().Length == 0 &&
                            m.ReturnType != typeof(void))
                .ToList();

            // предпочтём GetClassroom/GetUser/... если есть
            var preferred = getters.FirstOrDefault(m => m.Name != "GetType");
            if (preferred != null)
                return preferred.Invoke(_control, null);

            return null;
        }

        private static bool TryValidateControl(UserControl control)
        {
            var mi = control.GetType().GetMethod("Validate", BindingFlags.Instance | BindingFlags.Public);
            if (mi == null) return true;

            if (mi.ReturnType != typeof(bool) || mi.GetParameters().Length != 0)
                return true;

            var result = (bool)mi.Invoke(control, null);
            if (!result)
            {
                MessageBox.Show("Проверьте введённые данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }
    }
}