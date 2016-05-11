// Copyright (c) The Perspex Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Perspex;
using Perspex.Controls;
using Perspex.Diagnostics;
using Perspex.Markup.Xaml;
using XamlTestApplication.ViewModels;

namespace XamlTestApplication.Views
{
    public class MainWindow : Window
    {
        private MenuItem _exitMenu;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            DevTools.Attach(this);
        }

        private void InitializeComponent()
        {
            PerspexXamlLoader.Load(this);
            _exitMenu = this.FindControl<MenuItem>("exitMenu");
            _exitMenu.Click += (s, e) => Application.Current.Exit();

            var vadd = this.FindControl<Button>("vadd");
            var vsp = this.FindControl<VirtualizingStackPanel>("vsp");
            var ivp = (IVirtualizingPanel)vsp;
            var index = 0;

            vadd.Click += (s, e) =>
            {
                vsp.Children.Add(new TextBlock { Text = "Hello " + ++index });
                vadd.IsEnabled = !ivp.IsFull;
            };
        }
    }
}