using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FastWpfGridSyncTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LeftGrid.Model = new GridModel();
            RightGrid.Model = new GridModel();

            FastWpfGrid.FastGridControl.RegisterSyncScrollGrid("DataGrid", LeftGrid);
            FastWpfGrid.FastGridControl.RegisterSyncScrollGrid("DataGrid", RightGrid);
        }
    }
}
