using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaskManager;
using WpfApp15.ViewModel.UserControl;

namespace WpfApp15.ViewModel
{

    /// <summary>
    /// Логика взаимодействия для MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
                InitializeComponent();
                HideTabControl();
                TaskManager.ViewModel viewModel = new TaskManager.ViewModel(this);
                this.DataContext = viewModel;
                Task.Delay(50);
        }
        private void AppWindow_Deactivated(object sender, EventArgs e)
        {
            (DataContext as MainViewModel).DimmableOverlayVisible = true;
        }
        private void AppWindow_Activated(object sender, EventArgs e)
        {
            (DataContext as MainViewModel).DimmableOverlayVisible = false;
        }
        private void HideTabControl()
        {
            foreach (var item in tabControl1.Items)
            {
                (item as TabItem).Visibility = Visibility.Collapsed;
            }
        }
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu2.Visibility = Visibility.Collapsed;
            ButtonOpenMenu3.Visibility = Visibility.Collapsed;
            ButtonOpenMenu4.Visibility = Visibility.Collapsed;
            ButtonOpenMenu5.Visibility = Visibility.Collapsed;
            ButtonOpenMenu6.Visibility = Visibility.Collapsed;
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonCloseMenu2.Visibility = Visibility.Visible;
            ButtonCloseMenu3.Visibility = Visibility.Visible;
            ButtonCloseMenu4.Visibility = Visibility.Visible; 
            ButtonCloseMenu5.Visibility = Visibility.Visible;
            ButtonCloseMenu6.Visibility = Visibility.Visible;
        }
        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu2.Visibility = Visibility.Visible;
            ButtonOpenMenu3.Visibility = Visibility.Visible;
            ButtonOpenMenu4.Visibility = Visibility.Visible;
            ButtonOpenMenu5.Visibility = Visibility.Visible;
            ButtonOpenMenu6.Visibility = Visibility.Visible;
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonCloseMenu2.Visibility = Visibility.Collapsed;
            ButtonCloseMenu3.Visibility = Visibility.Collapsed;
            ButtonCloseMenu4.Visibility = Visibility.Collapsed;
            ButtonCloseMenu5.Visibility = Visibility.Collapsed;
            ButtonCloseMenu6.Visibility = Visibility.Collapsed;
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tabControl1.SelectedIndex = listview1.SelectedIndex;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
                if (sender is TextBox textBox && textBox.LineCount > 0)
                {
                    textBox.ScrollToLine(textBox.LineCount - 1);
                }           
        }
    }
}
