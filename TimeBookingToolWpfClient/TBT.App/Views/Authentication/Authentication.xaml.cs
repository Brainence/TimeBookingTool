using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.ViewModels;
using TBT.App.Views.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using TBT.App.Common;
using System;
using TBT.App.ViewModels.Authentication;

namespace TBT.App.Views.Authentication
{

    public partial class Authentication : Window
    {
        public Authentication()
        {
            InitializeComponent();
        }
    }
}
