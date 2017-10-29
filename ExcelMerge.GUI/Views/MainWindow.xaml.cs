using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExcelMerge.GUI.Shell;

namespace ExcelMerge.GUI.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private GridLength previousConsoleHeight = new GridLength(0);

        public MainWindow()
        {
            InitializeComponent();

            var host = new PowerShellHost();
            Console.PowerShellHost = host;
            host.Open();
        }

        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
                return;

            var binding = menuItem.GetBindingExpression(MenuItem.IsEnabledProperty);
            if (binding == null)
                return;

            binding.UpdateTarget();
        }

        private void ConsoleVisibilityChanged(object sender, RoutedEventArgs e)
        {
            if (Console.Visibility == Visibility.Collapsed)
                ShowConsole();
            else
                HideConsole();
        }

        private void ShowConsole()
        {
            Console.Visibility = Visibility.Visible;
            ConsoleGridSplitter.Visibility = Visibility.Visible;

            if (previousConsoleHeight.Value > 0)
            {
                MainGrid.RowDefinitions[3].Height = previousConsoleHeight;
            }
            else
            {
                MainGrid.RowDefinitions[3].Height = new GridLength(Height / 2d);
                previousConsoleHeight = MainGrid.RowDefinitions[3].Height;
            }
        }

        private void HideConsole()
        {
            Console.Visibility = Visibility.Collapsed;
            ConsoleGridSplitter.Visibility = Visibility.Collapsed;
            previousConsoleHeight = new GridLength(MainGrid.RowDefinitions[3].ActualHeight);
            MainGrid.RowDefinitions[3].Height = new GridLength(0);

            UpdateLayout();
        }

        public void WriteToConsole(string message)
        {
            ConsoleVisibilityMenuItem.IsChecked = true;
            ShowConsole();

            Console.Write(message);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (Console.Visibility == Visibility.Collapsed)
                            {
                                ShowConsole();
                                Console.Focus();
                            }
                            else
                            {
                                HideConsole();
                            }

                            e.Handled = true;
                        }
                    }
                    break;
            }
        }
    }
}
