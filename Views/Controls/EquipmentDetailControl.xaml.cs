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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdminUP.Models;

namespace AdminUP.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для EquipmentDetailControl.xaml
    /// </summary>
    public partial class EquipmentDetailControl : UserControl
    {
        public EquipmentDetailControl()
        {
            InitializeComponent();
        }
        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var equipment = DataContext as Equipment;
            if (equipment != null)
            {
                var text = $"Оборудование: {equipment.name}\n" +
                          $"Инвентарный номер: {equipment.inventory_number}\n" +
                          $"Стоимость: {equipment.cost:N2} руб.\n" +
                          $"Статус: {equipment.status_id}\n" +
                          $"Комментарий: {equipment.comment}";

                Clipboard.SetText(text);
                MessageBox.Show("Информация скопирована в буфер обмена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
