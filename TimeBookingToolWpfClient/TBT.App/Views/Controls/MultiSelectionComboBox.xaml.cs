using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Controls
{
    public partial class MultiSelectionComboBox : ComboBox
    {
        public MultiSelectionComboBox()
        {
            InitializeComponent();
            if (!_companyId.HasValue) { Task.Run(() => RefreshEvents.RefreshCurrentUser(null)).Wait(); }
        }

        public static void CurrentUserChanged(object sender, User value)
        {
            _companyId = !_companyId.HasValue && value?.Company?.Id > 0 ? value.Company?.Id : _companyId;
        }

        private static int? _companyId;

        public string PropertyPath { get; set; }

        public IEnumerable ItemsSourceMultiple
        {
            get { return (IEnumerable)GetValue(ItemsSourceMultipleProperty); }
            set { SetValue(ItemsSourceMultipleProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceMultipleProperty = DependencyProperty
            .Register(
                "ItemsSourceMultiple",
                typeof(IEnumerable),
                typeof(MultiSelectionComboBox),
                new PropertyMetadata(ItemsSourceMultiple_PropertyChangedCallback)
            );

        public string SelectionBoxText
        {
            get { return (string)GetValue(SelectionBoxTextProperty); }
            set { SetValue(SelectionBoxTextProperty, value); }
        }
        public static readonly DependencyProperty SelectionBoxTextProperty = DependencyProperty
            .Register(
                "SelectionBoxText",
                typeof(string),
                typeof(MultiSelectionComboBox),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty UserProperty = DependencyProperty
            .Register(nameof(User), typeof(User), typeof(MultiSelectionComboBox));

        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public IEnumerable<object> SelectedItems
        {
            get
            {
                return ItemsSource?.Cast<CheckableObject>()
                    .Where(x => x.IsChecked)
                    .Select(y => y.Obj);
            }
            set
            {
                if (value == null) return;
                var list = ItemsSource.Cast<CheckableObject>().ToList();
                foreach (var item in list)
                {
                    if (value.Contains(item.Obj))
                    {
                        item.IsChecked = value.Contains(item.Obj);
                    }
                }
                ItemsSource = list;
            }
        }

        public static async void ItemsSourceMultiple_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var comboBox = (MultiSelectionComboBox)d;
            var list = new List<object>();

            var data = await App.CommunicationService.GetAsJson($"Project/GetByCompany/{_companyId ?? 0}");
            if (data != null)
            {
                var allProjects = JsonConvert.DeserializeObject<List<Project>>(data);
                var userProjects = (ObservableCollection<Project>)args.NewValue;
                foreach (var elem in allProjects)
                {
                    var item = new CheckableObject(comboBox.PropertyPath)
                    {
                        Obj = elem,
                        IsChecked = userProjects?.Select(t => t.Id).Contains(elem.Id) ?? false
                    };
                    item.IsCheckedPropertyChanged += comboBox.ItemChecked;
                    list.Add(item);
                }
                comboBox.ItemsSource = list;
            }
        }

        public event Action Checked;
        protected void ItemChecked()
        {
            Checked?.Invoke();
            try
            {
                SelectionBoxText = SelectedItems.Any()
                    ? string.Join(", ", SelectedItems.Select(x => x.GetType().GetProperty(PropertyPath).GetValue(x, null).ToString()))
                    : Text;
            }
            catch (Exception)
            {
                SelectionBoxText = Text;
            }
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var project = ((sender as CheckBox).DataContext as CheckableObject).Obj as Project;
            var data = await App.CommunicationService.GetAsJson($"User/{User.Id}");
            if (data != null)
            {
                var user = JsonConvert.DeserializeObject<User>(data);

                if (project != null && user != null)
                {
                    await App.CommunicationService.PostAsJson($"UserProject/Add/{user.Id}/{project.Id}", null);
                }
            }
        }

        private async void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var project = ((sender as CheckBox).DataContext as CheckableObject).Obj as Project;
            var data = await App.CommunicationService.GetAsJson($"User/{User.Id}");
            if (data != null)
            {
                var user = JsonConvert.DeserializeObject<User>(data);
                if (project != null && user != null)
                {
                    await App.CommunicationService.Delete($"UserProject/Remove/{user.Id}/{project.Id}");
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            comboBox.SelectedItem = null;
        }
    }

    public class CheckableObject : INotifyPropertyChanged
    {
        private string _propertyPathName;
        private bool _checked;
        private object _obj;
        public CheckableObject(string propname)
        {
            _propertyPathName = propname;
        }
        public bool IsChecked
        {
            get { return _checked; }
            set
            {
                _checked = value;
                OnPropertyChanged();
                OnIsCheckedPropertyChanged();
            }
        }

        public string TextProperty
        {
            get
            {
                try
                {
                    var result = Obj.GetType().GetProperty(_propertyPathName).GetValue(Obj, null);
                    return result.ToString();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public object Obj
        {
            get { return _obj; }
            set
            {
                _obj = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action IsCheckedPropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void OnIsCheckedPropertyChanged()
        {
            IsCheckedPropertyChanged?.Invoke();
        }
    }
}
