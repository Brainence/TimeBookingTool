﻿#pragma checksum "..\..\..\..\Views\Authentication\Authentication.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "236AFBF09FBA64908FDD7FAA8B961BAD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Security.Authentication;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TBT.App.Views.Controls;


namespace TBT.App.Views.Authentication {
    
    
    /// <summary>
    /// Authentication
    /// </summary>
    public partial class Authentication : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 2 "..\..\..\..\Views\Authentication\Authentication.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TBT.App.Views.Authentication.Authentication @this;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\..\Views\Authentication\Authentication.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel login;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\..\Views\Authentication\Authentication.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TextBox;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\..\Views\Authentication\Authentication.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox PasswordBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TimeBookingTool;component/views/authentication/authentication.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\Authentication\Authentication.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.@this = ((TBT.App.Views.Authentication.Authentication)(target));
            
            #line 16 "..\..\..\..\Views\Authentication\Authentication.xaml"
            this.@this.KeyDown += new System.Windows.Input.KeyEventHandler(this.authentication_KeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 20 "..\..\..\..\Views\Authentication\Authentication.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CloseButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.login = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            this.TextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 37 "..\..\..\..\Views\Authentication\Authentication.xaml"
            this.TextBox.GotFocus += new System.Windows.RoutedEventHandler(this.TextBox_GotFocus);
            
            #line default
            #line hidden
            
            #line 37 "..\..\..\..\Views\Authentication\Authentication.xaml"
            this.TextBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.TextBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.PasswordBox = ((System.Windows.Controls.PasswordBox)(target));
            
            #line 40 "..\..\..\..\Views\Authentication\Authentication.xaml"
            this.PasswordBox.GotFocus += new System.Windows.RoutedEventHandler(this.PasswordBox_GotFocus);
            
            #line default
            #line hidden
            
            #line 40 "..\..\..\..\Views\Authentication\Authentication.xaml"
            this.PasswordBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.TextBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 46 "..\..\..\..\Views\Authentication\Authentication.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Login_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

