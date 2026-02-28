using AdminUP.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views.Controls
{
    public partial class EquipmentSoftwareEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private EquipmentSoftware _item;

        public ObservableCollection<Equipment> EquipmentList { get; } = new();
        public ObservableCollection<SoftwareEntity> SoftwareList { get; } = new();

        public EquipmentSoftwareEditControl(EquipmentSoftware item = null)
        {
            InitializeComponent();

            _item = item ?? new EquipmentSoftware();

            DataContext = this;

            _ = LoadDataAsync();
        }

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async Task LoadDataAsync()
        {
            try
            {
                var eq = await App.ApiService.GetListAsync<Equipment>("EquipmentController");
                EquipmentList.Clear();
                EquipmentList.Add(new Equipment { id = 0, name = "Не выбрано", inventory_number = "" });
                if (eq != null) foreach (var e in eq) EquipmentList.Add(e);
                RaisePropertyChanged(nameof(EquipmentList));

                var sw = await App.ApiService.GetListAsync<SoftwareEntity>("SoftwareController");
                SoftwareList.Clear();
                SoftwareList.Add(new SoftwareEntity { id = 0, name = "Не выбрано" });
                if (sw != null) foreach (var s in sw) SoftwareList.Add(s);
                RaisePropertyChanged(nameof(SoftwareList));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списков:\n\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== биндинги под XAML =====

        public int EquipmentId
        {
            get => _item.equipment_id;
            set
            {
                _item.equipment_id = value;
                RaisePropertyChanged();
            }
        }

        public int SoftwareId
        {
            get => _item.software_id;
            set
            {
                _item.software_id = value;
                RaisePropertyChanged();
            }
        }

        // EditDialog вызывает Validate()
        public bool Validate()
        {
            if (_item.equipment_id == 0)
            {
                MessageBox.Show("Выберите оборудование", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_item.software_id == 0)
            {
                MessageBox.Show("Выберите ПО", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // чтобы EditDialog.GetEditedItem() работал
        public object GetEditedItem() => _item;

        public EquipmentSoftware GetEquipmentSoftware() => _item;
    }
}