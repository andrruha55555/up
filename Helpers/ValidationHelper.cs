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
                return true; 

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

            if (string.IsNullOrWhiteSpace(equipment.name))
                errors.Add("Название оборудования обязательно");

            if (string.IsNullOrWhiteSpace(equipment.inventory_number))
                errors.Add("Инвентарный номер обязателен");

            if (equipment.cost.HasValue && equipment.cost < 0)
                errors.Add("Стоимость не может быть отрицательной");

            if (equipment.status_id <= 0)
                errors.Add("Статус обязателен");

            return errors;
        }

        public static List<string> ValidateUser(User user)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(user.login))
                errors.Add("Логин обязателен");
            else if (user.login.Length < 3)
                errors.Add("Логин должен содержать минимум 3 символа");

            if (string.IsNullOrWhiteSpace(user.last_name))
                errors.Add("Фамилия обязательна");

            if (string.IsNullOrWhiteSpace(user.first_name))
                errors.Add("Имя обязательно");

            if (!ValidateEmail(user.email))
                errors.Add("Некорректный email");

            if (!string.IsNullOrWhiteSpace(user.phone) && !ValidatePhone(user.phone))
                errors.Add("Некорректный телефон");

            if (string.IsNullOrWhiteSpace(user.role))
                errors.Add("Роль обязательна");

            return errors;
        }

        public static List<string> ValidateConsumable(Consumable consumable)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(consumable.name))
                errors.Add("Название расходника обязательно");

            if (consumable.quantity < 0)
                errors.Add("Количество не может быть отрицательным");

            if (consumable.arrival_date > DateTime.Now)
                errors.Add("Дата поступления не может быть в будущем");

            if (consumable.consumable_type_id <= 0)
                errors.Add("Тип расходника обязателен");

            return errors;
        }

        public static List<string> ValidateClassroom(Classroom classroom)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(classroom.name))
                errors.Add("Название аудитории обязательно");

            if (string.IsNullOrWhiteSpace(classroom.short_name))
                errors.Add("Сокращенное название обязательно");

            return errors;
        }

        public static List<string> ValidateNetworkSettings(NetworkSetting networkSetting)
        {
            var errors = new List<string>();

            if (!ValidateIpAddress(networkSetting.ip_address))
                errors.Add("Некорректный IP адрес");

            if (!ValidateIpAddress(networkSetting.subnet_mask))
                errors.Add("Некорректная маска подсети");

            if (!ValidateIpAddress(networkSetting.gateway))
                errors.Add("Некорректный адрес шлюза");

            if (!string.IsNullOrWhiteSpace(networkSetting.dns1) && !ValidateIpAddress(networkSetting.dns1))
                errors.Add("Некорректный DNS1");

            if (!string.IsNullOrWhiteSpace(networkSetting.dns2) && !ValidateIpAddress(networkSetting.dns2))
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