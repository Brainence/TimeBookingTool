using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Controls
{
    public partial class MultiSelectionComboBox : ComboBox
    {
        public MultiSelectionComboBox()
        {
            InitializeComponent();
        }

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
                return ItemsSource == null ? null : ItemsSource.Cast<CheckableObject>()
                    .Where(x => x.IsChecked)
                    .Select(y => y.Obj);
            }
            set
            {
                if (value == null) return;

                var temp = ItemsSource.Cast<CheckableObject>();
                foreach (var x in temp)
                {
                    if (value.Contains(x.Obj))
                        x.IsChecked = value.Contains(x.Obj);
                }
                ItemsSource = temp;
            }
        }

        public async static void ItemsSourceMultiple_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var x = (MultiSelectionComboBox)d;

            var list = new List<object>();

            var allProjects = JsonConvert.DeserializeObject<List<Project>>(await App.CommunicationService.GetAsJson("Project"));
            var userProjects = (ObservableCollection<Project>)e.NewValue;

            foreach (var elem in allProjects)
            {
                var item = new CheckableObject(x.PropertyPath);
                item.Obj = elem;
                item.IsChecked = userProjects == null ? false : userProjects.Select(t => t.Id).Contains(elem.Id);
                item.IsCheckedPropertyChanged += () =>
                {
                    x.ItemChecked();
                };
                list.Add(item);
            }
            x.ItemsSource = list;
        }

        public event Action Checked;
        protected void ItemChecked()
        {
            if (Checked != null)
            {
                Checked();
            }
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
            try
            {

                var project = ((sender as CheckBox).DataContext as CheckableObject).Obj as Project;

                var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User/{User.Id}"));

                if (project != null && user != null)
                {
                    await App.CommunicationService.PostAsJson($"UserProject/Add/{user.Id}/{project.Id}", null);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error occurred while assigning user to project.");
            }
        }

        private async void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var project = ((sender as CheckBox).DataContext as CheckableObject).Obj as Project;

                var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User/{User.Id}"));

                if (project != null && user != null)
                {
                    await App.CommunicationService.Delete($"UserProject/Remove/{user.Id}/{project.Id}");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error occurred while dissociating user from project.");
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected void OnIsCheckedPropertyChanged()
        {
            if (IsCheckedPropertyChanged != null)
            {
                IsCheckedPropertyChanged();
            }
        }
    }
}
