using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using AdminUP.Models;

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
        private void InitializeEditControl()
        {
            if (_item == null)
            {
                StatusText.Text = "Ошибка: объект не указан";
                return;
            }

            try
            {
                BaseEditControl editControl = null;

                switch (_item)
                {
                    case Equipment equipment:
                        editControl = new EquipmentEditControl(equipment);
                        break;
                    case User user:
                        editControl = new UserEditControl(user);
                        break;
                    case Classroom classroom:
                        editControl = new ClassroomEditControl(classroom);
                        break;
                    case Consumable consumable:
                        editControl = new ConsumableEditControl(consumable);
                        break;
                    case Status status:
                        editControl = new StatusEditControl(status);
                        break;
                    case EquipmentType equipmentType:
                        editControl = new EquipmentTypeEditControl(equipmentType);
                        break;
                    case Model model:
                        editControl = new ModelEditControl(model);
                        break;
                    case ConsumableType consumableType:
                        editControl = new ConsumableTypeEditControl(consumableType);
                        break;
                    case ConsumableCharacteristic characteristic:
                        editControl = new ConsumableCharacteristicEditControl(characteristic);
                        break;
                    case Developer developer:
                        editControl = new DeveloperEditControl(developer);
                        break;
                    case Direction direction:
                        editControl = new DirectionEditControl(direction);
                        break;
                    case Software software:
                        editControl = new SoftwareEditControl(software);
                        break;
                    case Inventory inventory:
                        editControl = new InventoryEditControl(inventory);
                        break;
                    case InventoryItem inventoryItem:
                        editControl = new InventoryItemEditControl(inventoryItem);
                        break;
                    case NetworkSetting networkSetting:
                        editControl = new NetworkSettingEditControl(networkSetting);
                        break;
                    case EquipmentHistory equipmentHistory:
                        editControl = new EquipmentHistoryEditControl(equipmentHistory);
                        break;
                    case EquipmentSoftware equipmentSoftware:
                        editControl = new EquipmentSoftwareEditControl(equipmentSoftware);
                        break;
                    default:
                        StatusText.Text = $"Тип {_item.GetType().Name} не поддерживается для редактирования";
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

        private object GetItemFromControl(BaseEditControl control)
        {
            return control switch
            {
                EquipmentEditControl equipmentControl => equipmentControl.GetEquipment(),
                UserEditControl userControl => userControl.GetUser(),
                ClassroomEditControl classroomControl => classroomControl.GetClassroom(),
                ConsumableEditControl consumableControl => consumableControl.GetConsumable(),
                StatusEditControl statusControl => statusControl.GetStatus(),
                EquipmentTypeEditControl equipmentTypeControl => equipmentTypeControl.GetEquipmentType(),
                ModelEditControl modelControl => modelControl.GetModel(),
                ConsumableTypeEditControl consumableTypeControl => consumableTypeControl.GetConsumableType(),
                ConsumableCharacteristicEditControl characteristicControl => characteristicControl.GetCharacteristic(),
                DeveloperEditControl developerControl => developerControl.GetDeveloper(),
                DirectionEditControl directionControl => directionControl.GetDirection(),
                SoftwareEditControl softwareControl => softwareControl.GetSoftware(),
                InventoryEditControl inventoryControl => inventoryControl.GetInventory(),
                InventoryItemEditControl inventoryItemControl => inventoryItemControl.GetInventoryItem(),
                NetworkSettingEditControl networkSettingControl => networkSettingControl.GetNetworkSetting(),
                EquipmentHistoryEditControl equipmentHistoryControl => equipmentHistoryControl.GetEquipmentHistory(),
                EquipmentSoftwareEditControl equipmentSoftwareControl => equipmentSoftwareControl.GetEquipmentSoftware(),
                _ => _item
            };
        }
    }
}
