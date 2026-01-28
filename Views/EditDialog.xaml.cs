using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AdminUP.Models;
using AdminUP.Views.Controls;

namespace AdminUP.Views
{
    /// <summary>
    /// Логика взаимодействия для EditDialog.xaml
    /// </summary>
    public partial class EditDialog : Window
    {
        private object _item;
        private Type _itemType;
        public EditDialog(object item, string title = "Редактирование")
        {
            InitializeComponent();

            _item = item;
            _itemType = item?.GetType();
            Title = title;

            InitializeEditControl();
        }

        private void InitializeEditControl()
        {
            if (_item == null)
            {
                StatusText.Text = "Ошибка: объект не указан";
                return;
            }

            try
            {
                UserControl editControl = null;

                // Создаем соответствующий UserControl в зависимости от типа
                if (_itemType == typeof(Equipment))
                {
                    editControl = new EquipmentEditControl(_item as Equipment);
                }
                else if (_itemType == typeof(User))
                {
                    editControl = new UserEditControl(_item as User);
                }
                else if (_itemType == typeof(Classroom))
                {
                    editControl = new ClassroomEditControl(_item as Classroom);
                }
                else if (_itemType == typeof(Consumable))
                {
                    editControl = new ConsumableEditControl(_item as Consumable);
                }
                else if (_itemType == typeof(Status))
                {
                    editControl = new StatusEditControl(_item as Status);
                }
                else if (_itemType == typeof(EquipmentType))
                {
                    editControl = new EquipmentTypeEditControl(_item as EquipmentType);
                }
                else if (_itemType == typeof(Model))
                {
                    editControl = new ModelEditControl(_item as Model);
                }
                else if (_itemType == typeof(ConsumableType))
                {
                    editControl = new ConsumableTypeEditControl(_item as ConsumableType);
                }
                else if (_itemType == typeof(Developer))
                {
                    editControl = new DeveloperEditControl(_item as Developer);
                }
                else if (_itemType == typeof(Direction))
                {
                    editControl = new DirectionEditControl(_item as Direction);
                }
                else if (_itemType == typeof(Software))
                {
                    editControl = new SoftwareEditControl(_item as Software);
                }
                else if (_itemType == typeof(Inventory))
                {
                    editControl = new InventoryEditControl(_item as Inventory);
                }
                else if (_itemType == typeof(NetworkSetting))
                {
                    editControl = new NetworkSettingEditControl(_item as NetworkSetting);
                }
                else
                {
                    StatusText.Text = $"Тип {_itemType.Name} не поддерживается для редактирования";
                    return;
                }

                EditContentControl.Content = editControl;
                StatusText.Text = "Готово к редактированию";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка инициализации: {ex.Message}";
            }
        }

        public object GetEditedItem()
        {
            if (EditContentControl.Content is BaseEditControl editControl)
            {
                // Получаем отредактированный объект из UserControl
                // Это нужно реализовать в каждом конкретном UserControl
                return GetItemFromControl(editControl);
            }
            return _item;
        }

        private object GetItemFromControl(BaseEditControl control)
        {
            // Этот метод нужно реализовать в каждом конкретном UserControl
            // Возвращаем GetItem() из соответствующего контрола
            switch (control)
            {
                case EquipmentEditControl equipmentControl:
                    return equipmentControl.GetEquipment();
                case UserEditControl userControl:
                    return userControl.GetUser();
                // Добавить для всех остальных типов
                default:
                    return _item;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем валидацию
            if (EditContentControl.Content is BaseEditControl editControl)
            {
                if (editControl.HasErrors)
                {
                    MessageBox.Show(editControl.ValidationErrorMessage,
                        "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
    }
}
