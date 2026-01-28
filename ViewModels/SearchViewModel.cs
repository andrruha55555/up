using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace AdminUP.ViewModels
{
    public class SearchViewModel
    {
        private string _searchText;
        private string _selectedProperty;
        private ObservableCollection<string> _availableProperties;
        private CollectionViewSource _filteredCollection;
        private bool _isSearchEnabled;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        public string SelectedProperty
        {
            get => _selectedProperty;
            set
            {
                if (_selectedProperty != value)
                {
                    _selectedProperty = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        public ObservableCollection<string> AvailableProperties
        {
            get => _availableProperties;
            set
            {
                _availableProperties = value;
                OnPropertyChanged();
            }
        }

        public CollectionViewSource FilteredCollection
        {
            get => _filteredCollection;
            set
            {
                _filteredCollection = value;
                OnPropertyChanged();
            }
        }

        public bool IsSearchEnabled
        {
            get => _isSearchEnabled;
            set
            {
                _isSearchEnabled = value;
                OnPropertyChanged();
            }
        }
        public SearchViewModel()
        {
            FilteredCollection = new CollectionViewSource();
            AvailableProperties = new ObservableCollection<string>();
            IsSearchEnabled = true;
        }

        public void Initialize<T>(ObservableCollection<T> sourceCollection)
        {
            FilteredCollection.Source = sourceCollection;
            FilteredCollection.Filter += FilteredCollection_Filter;
            UpdateAvailableProperties<T>();
            ApplyFilter();
        }

        private void FilteredCollection_Filter(object sender, FilterEventArgs e)
        {
            if (!IsSearchEnabled || string.IsNullOrWhiteSpace(SearchText))
            {
                e.Accepted = true;
                return;
            }

            var item = e.Item;
            var searchText = SearchText.ToLower();

            if (string.IsNullOrEmpty(SelectedProperty) || SelectedProperty == "Все поля")
            {
                // Поиск по всем строковым свойствам
                var stringProperties = item.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(string));

                foreach (var property in stringProperties)
                {
                    var value = property.GetValue(item) as string;
                    if (value != null && value.ToLower().Contains(searchText))
                    {
                        e.Accepted = true;
                        return;
                    }
                }

                // Также проверяем числовые поля
                var numericProperties = item.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(int) ||
                               p.PropertyType == typeof(decimal) ||
                               p.PropertyType == typeof(int?) ||
                               p.PropertyType == typeof(decimal?));

                foreach (var property in numericProperties)
                {
                    var value = property.GetValue(item);
                    if (value != null && value.ToString().Contains(searchText))
                    {
                        e.Accepted = true;
                        return;
                    }
                }

                e.Accepted = false;
            }
            else
            {
                // Поиск по выбранному свойству
                var property = item.GetType().GetProperty(SelectedProperty);
                if (property != null)
                {
                    var value = property.GetValue(item)?.ToString()?.ToLower();
                    if (value != null && value.Contains(searchText))
                    {
                        e.Accepted = true;
                        return;
                    }
                }
                e.Accepted = false;
            }
        }

        private void UpdateAvailableProperties<T>()
        {
            AvailableProperties.Clear();
            AvailableProperties.Add("Все поля");

            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) ||
                           p.PropertyType == typeof(int) ||
                           p.PropertyType == typeof(decimal) ||
                           p.PropertyType == typeof(int?) ||
                           p.PropertyType == typeof(decimal?))
                .Select(p => p.Name);

            foreach (var property in properties)
            {
                AvailableProperties.Add(property);
            }

            SelectedProperty = AvailableProperties.FirstOrDefault();
        }

        public void ApplyFilter()
        {
            if (FilteredCollection?.View != null)
            {
                FilteredCollection.View.Refresh();
            }
        }

        public void ClearSearch()
        {
            SearchText = string.Empty;
            SelectedProperty = AvailableProperties.FirstOrDefault();
            ApplyFilter();
        }

        public IEnumerable<T> GetFilteredItems<T>()
        {
            return FilteredCollection?.View?.OfType<T>() ?? Enumerable.Empty<T>();
        }

        public int GetFilteredCount<T>()
        {
            return GetFilteredItems<T>().Count();
        }

        public void EnableSearch()
        {
            IsSearchEnabled = true;
            ApplyFilter();
        }

        public void DisableSearch()
        {
            IsSearchEnabled = false;
            ApplyFilter();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}