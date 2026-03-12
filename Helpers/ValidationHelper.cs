using AdminUP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdminUP.Helpers
{
    /// <summary>
    /// Статический класс валидации данных.
    /// п. 1.1 ТЗ: инв. номер, стоимость; п. 1.9: пользователи; п. 1.10: IP-адреса; п. 1.11: расходники.
    /// </summary>
    public static class ValidationHelper
    {
        // ─── Email ───────────────────────────────────────────────────────────

        /// <summary>Проверяет формат email (опциональное поле).</summary>
        public static bool ValidateEmail(string? email)
        {
            // Email необязателен по ТЗ — пустое значение допустимо
            if (string.IsNullOrWhiteSpace(email))
                return true;

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

        // ─── Телефон ─────────────────────────────────────────────────────────

        /// <summary>Проверяет формат телефона (опциональное поле).</summary>
        public static bool ValidatePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            return Regex.IsMatch(phone, @"^[\d\s\-\+\(\)]+$");
        }

        // ─── Числа ───────────────────────────────────────────────────────────

        /// <summary>Проверяет, что строка является неотрицательным decimal.</summary>
        public static bool ValidateDecimal(string? value, out decimal result)
        {
            return decimal.TryParse(value, out result) && result >= 0;
        }

        /// <summary>Проверяет, что строка является неотрицательным целым числом.</summary>
        public static bool ValidateInteger(string? value, out int result)
        {
            return int.TryParse(value, out result) && result >= 0;
        }

        /// <summary>Проверяет, что строка является целым числом (любым).</summary>
        public static bool ValidatePositiveInteger(string? value, out int result)
        {
            return int.TryParse(value, out result) && result > 0;
        }

        // ─── Дата ────────────────────────────────────────────────────────────

        /// <summary>
        /// Проверяет формат даты ДД.ММ.ГГГГ (п. 1.11 ТЗ).
        /// </summary>
        public static bool ValidateDate(string? value, out DateTime result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = DateTime.MinValue;
                return false;
            }

            // Пробуем приоритетный формат ДД.ММ.ГГГГ
            if (DateTime.TryParseExact(value, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out result))
                return true;

            // Фолбэк — любой понятный DateTime
            return DateTime.TryParse(value, out result);
        }

        // ─── IP-адрес ────────────────────────────────────────────────────────

        /// <summary>
        /// Проверяет формат IP-адреса и диапазон каждого октета 0–255.
        /// п. 1.10 ТЗ: "маска XXX.XXX.XXX.XXX, максимальное значение от 0 до 255".
        /// </summary>
        public static bool ValidateIpAddress(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            ip = ip.Trim();

            // Проверяем общий формат (1-3 цифры через точки)
            if (!Regex.IsMatch(ip, @"^(\d{1,3}\.){3}\d{1,3}$"))
                return false;

            // ✅ ИСПРАВЛЕНО: проверяем каждый октет на диапазон 0-255
            var parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int octet) || octet < 0 || octet > 255)
                    return false;
            }

            return true;
        }

        // ─── Инвентарный номер ───────────────────────────────────────────────

        /// <summary>
        /// Проверяет, что инвентарный номер содержит только цифры.
        /// п. 1.1 ТЗ: "в поле инвентарный номер нельзя ввести буквы".
        /// </summary>
        public static bool ValidateInventoryNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Regex.IsMatch(value.Trim(), @"^\d+$");
        }

        // ─── Комплексные валидаторы моделей ──────────────────────────────────

        /// <summary>Валидация оборудования (п. 1.1 ТЗ).</summary>
        public static List<string> ValidateEquipment(Equipment equipment)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(equipment.name))
                errors.Add("Название оборудования обязательно");

            if (string.IsNullOrWhiteSpace(equipment.inventory_number))
                errors.Add("Инвентарный номер обязателен");
            else if (!ValidateInventoryNumber(equipment.inventory_number))
                errors.Add("Инвентарный номер должен содержать только цифры");

            if (equipment.cost.HasValue && equipment.cost < 0)
                errors.Add("Стоимость не может быть отрицательной");

            if (equipment.status_id <= 0)
                errors.Add("Статус обязателен");

            return errors;
        }

        /// <summary>
        /// Валидация пользователя (п. 1.9 ТЗ).
        /// Обязательные поля: логин, пароль (при создании), фамилия.
        /// Email — необязателен (исправлено по ТЗ).
        /// </summary>
        public static List<string> ValidateUser(User user)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(user.login))
                errors.Add("Логин обязателен");
            else if (user.login.Length < 3)
                errors.Add("Логин должен содержать минимум 3 символа");

            if (string.IsNullOrWhiteSpace(user.last_name))
                errors.Add("Фамилия обязательна");

            // ✅ ИСПРАВЛЕНО: email НЕ обязателен по ТЗ, только формат проверяем если указан
            if (!string.IsNullOrWhiteSpace(user.email) && !ValidateEmail(user.email))
                errors.Add("Некорректный формат email");

            if (!string.IsNullOrWhiteSpace(user.phone) && !ValidatePhone(user.phone))
                errors.Add("Некорректный телефон");

            if (string.IsNullOrWhiteSpace(user.role))
                errors.Add("Роль обязательна");

            return errors;
        }

        /// <summary>Валидация расходного материала (п. 1.11 ТЗ).</summary>
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

        /// <summary>Валидация аудитории (п. 1.2 ТЗ).</summary>
        public static List<string> ValidateClassroom(Classroom classroom)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(classroom.name))
                errors.Add("Название аудитории обязательно");

            return errors;
        }

        /// <summary>Валидация сетевых настроек (п. 1.10 ТЗ).</summary>
        public static List<string> ValidateNetworkSettings(NetworkSetting networkSetting)
        {
            var errors = new List<string>();

            if (!ValidateIpAddress(networkSetting.ip_address))
                errors.Add("Некорректный IP-адрес (формат: 0-255.0-255.0-255.0-255)");

            if (!string.IsNullOrWhiteSpace(networkSetting.subnet_mask) &&
                !ValidateIpAddress(networkSetting.subnet_mask))
                errors.Add("Некорректная маска подсети");

            if (!string.IsNullOrWhiteSpace(networkSetting.gateway) &&
                !ValidateIpAddress(networkSetting.gateway))
                errors.Add("Некорректный адрес шлюза");

            if (!string.IsNullOrWhiteSpace(networkSetting.dns1) &&
                !ValidateIpAddress(networkSetting.dns1))
                errors.Add("Некорректный DNS1");

            if (!string.IsNullOrWhiteSpace(networkSetting.dns2) &&
                !ValidateIpAddress(networkSetting.dns2))
                errors.Add("Некорректный DNS2");

            return errors;
        }

        // ─── Форматирование ошибок ────────────────────────────────────────────

        /// <summary>Форматирует список ошибок в строку для MessageBox.</summary>
        public static string FormatValidationErrors(List<string> errors)
        {
            if (errors == null || errors.Count == 0)
                return string.Empty;

            return "Обнаружены ошибки:\n" +
                   string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));
        }
    }
}
