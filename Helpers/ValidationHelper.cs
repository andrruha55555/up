using AdminUP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdminUP.Helpers
{
    public static class ValidationHelper
    {
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // телефон не обязателен

            return Regex.IsMatch(phone, @"^[\d\s\-\+\(\)]+$");
        }

        public static bool ValidateDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, out result) && result >= 0;
        }

        public static bool ValidateInteger(string value, out int result)
        {
            return int.TryParse(value, out result) && result >= 0;
        }

        public static bool ValidateDate(string value, out DateTime result)
        {
            return DateTime.TryParse(value, out result);
        }

        public static bool ValidateIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            return Regex.IsMatch(ip, @"^(\d{1,3}\.){3}\d{1,3}$");
        }

        public static List<string> ValidateEquipment(Equipment equipment)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(equipment.Name))
                errors.Add("Название оборудования обязательно");

            if (string.IsNullOrWhiteSpace(equipment.InventoryNumber))
                errors.Add("Инвентарный номер обязателен");

            if (equipment.Cost.HasValue && equipment.Cost < 0)
                errors.Add("Стоимость не может быть отрицательной");

            if (equipment.StatusId <= 0)
                errors.Add("Статус обязателен");

            return errors;
        }

        public static List<string> ValidateUser(User user)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(user.Login))
                errors.Add("Логин обязателен");
            else if (user.Login.Length < 3)
                errors.Add("Логин должен содержать минимум 3 символа");

            if (string.IsNullOrWhiteSpace(user.LastName))
                errors.Add("Фамилия обязательна");

            if (string.IsNullOrWhiteSpace(user.FirstName))
                errors.Add("Имя обязательно");

            if (!ValidateEmail(user.Email))
                errors.Add("Некорректный email");

            if (!string.IsNullOrWhiteSpace(user.Phone) && !ValidatePhone(user.Phone))
                errors.Add("Некорректный телефон");

            if (string.IsNullOrWhiteSpace(user.Role))
                errors.Add("Роль обязательна");

            return errors;
        }

        public static List<string> ValidateConsumable(Consumable consumable)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(consumable.Name))
                errors.Add("Название расходника обязательно");

            if (consumable.Quantity < 0)
                errors.Add("Количество не может быть отрицательным");

            if (consumable.ArrivalDate > DateTime.Now)
                errors.Add("Дата поступления не может быть в будущем");

            if (consumable.ConsumableTypeId <= 0)
                errors.Add("Тип расходника обязателен");

            return errors;
        }

        public static List<string> ValidateClassroom(Classroom classroom)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(classroom.Name))
                errors.Add("Название аудитории обязательно");

            if (string.IsNullOrWhiteSpace(classroom.ShortName))
                errors.Add("Сокращенное название обязательно");

            return errors;
        }

        public static List<string> ValidateNetworkSettings(NetworkSetting networkSetting)
        {
            var errors = new List<string>();

            if (!ValidateIpAddress(networkSetting.IpAddress))
                errors.Add("Некорректный IP адрес");

            if (!ValidateIpAddress(networkSetting.SubnetMask))
                errors.Add("Некорректная маска подсети");

            if (!ValidateIpAddress(networkSetting.Gateway))
                errors.Add("Некорректный адрес шлюза");

            if (!string.IsNullOrWhiteSpace(networkSetting.Dns1) && !ValidateIpAddress(networkSetting.Dns1))
                errors.Add("Некорректный DNS1");

            if (!string.IsNullOrWhiteSpace(networkSetting.Dns2) && !ValidateIpAddress(networkSetting.Dns2))
                errors.Add("Некорректный DNS2");

            return errors;
        }

        public static string FormatValidationErrors(List<string> errors)
        {
            if (errors == null || errors.Count == 0)
                return string.Empty;

            return "Обнаружены ошибки:\n" + string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));
        }
    }
}