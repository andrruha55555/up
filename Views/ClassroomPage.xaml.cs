using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class ClassroomPage : Page
    {
        private ClassroomPageViewModel _viewModel;

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
            var newClassroom = new Classroom();
            ShowEditDialog(newClassroom, "Добавление аудитории");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedClassroom != null)
            {
                ShowEditDialog(_viewModel.SelectedClassroom, "Редактирование аудитории");
            }
            else
            {
                MessageBox.Show("Выберите аудиторию для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedClassroom != null)
            {
                await _viewModel.DeleteClassroomAsync(_viewModel.SelectedClassroom.Id);
            }
            else
            {
                MessageBox.Show("Выберите аудиторию для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterClassrooms();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(Classroom classroom, string title)
        {
            var editDialog = new EditDialog(classroom, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedClassroom = editDialog.GetEditedItem() as Classroom;
                if (editedClassroom != null)
                {
                    if (editedClassroom.Id == 0)
                        await _viewModel.AddClassroomAsync(editedClassroom);
                    else
                        await _viewModel.UpdateClassroomAsync(editedClassroom.Id, editedClassroom);
                }
            }
        }
    }
}