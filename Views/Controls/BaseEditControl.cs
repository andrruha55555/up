using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public class BaseEditControl : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();

        public ObservableCollection<string> ValidationErrors
        {
            get => _validationErrors;
            set
            {
                _validationErrors = value;
                OnPropertyChanged();
            }
        }

        public bool HasErrors => ValidationErrors.Count > 0;

        protected void AddValidationError(string error)
        {
            if (!ValidationErrors.Contains(error))
                ValidationErrors.Add(error);
        }

        protected void ClearValidationErrors()
        {
            ValidationErrors.Clear();
        }

        protected virtual bool ValidateData()
        {
            ClearValidationErrors();
            return !HasErrors;
        }

        protected virtual void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                Window.GetWindow(this).DialogResult = true;
                Window.GetWindow(this).Close();
            }
            else
            {
                MessageBox.Show(string.Join("\n", ValidationErrors),
                    "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = false;
            Window.GetWindow(this).Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
