using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class ClassroomPage : Page
    {
        private readonly ClassroomPageViewModel _viewModel;

        public ClassroomPage()
        {
            InitializeComponent();
            _viewModel = new ClassroomPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadClassroomsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEditDialog(new Classroom(), "Добавление аудитории");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedClassroom == null)
            {
                MessageBox.Show("Выберите аудиторию для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ShowEditDialog(_viewModel.SelectedClassroom, "Редактирование аудитории");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedClassroom == null)
            {
                MessageBox.Show("Выберите аудиторию для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await _viewModel.DeleteClassroomAsync(_viewModel.SelectedClassroom.id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterClassrooms();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
            _viewModel.FilterClassrooms();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(Classroom classroom, string title)
        {
            var control = new ClassroomEditControl(classroom);

            var dialog = new EditDialog(control, title);
            dialog.Owner = Window.GetWindow(this);

            if (dialog.ShowDialog() == true)
            {
                if (dialog.GetEditedItem() is Classroom editedClassroom)
                {
                    if (editedClassroom.id == 0)
                        await _viewModel.AddClassroomAsync(editedClassroom);
                    else
                        await _viewModel.UpdateClassroomAsync(editedClassroom.id, editedClassroom);
                }
            }
        }
    }
}