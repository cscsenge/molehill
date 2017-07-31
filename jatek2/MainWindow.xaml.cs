using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace jatek2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel VM;
        public MainWindow()
        {
            InitializeComponent();
            VM = new ViewModel();
            this.DataContext = VM;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right: VM.BL.Mozog(1, 0); break;
                case Key.Left: VM.BL.Mozog(-1, 0); break;
                case Key.Up: VM.BL.Mozog(0, -1); break;
                case Key.Down: VM.BL.Mozog(0, 1); break;
            }
            if (e.Key == Key.Escape && (!VM.IsInMenu && !VM.IsInLevelChooser && !VM.IsInHelp))
                VM.BL.Pause();
            else if (e.Key == Key.Escape)
                VM.BL.UnPause();
        }

        private void LevelSelect_FromMenu(object sender, MouseButtonEventArgs e)
        {
            VM.IsInMenu = false;
            VM.IsInLevelChooser = true;
        }

        private void Help_FromMenu(object sender, MouseButtonEventArgs e)
        {
            VM.IsInHelp = true;
            VM.IsInMenu = false;
        }

        private void Menu_FromHelp(object sender, MouseButtonEventArgs e)
        {
            VM.IsInHelp = false;
            VM.IsInMenu = true;
        }

        private void Menu_FromLevelSelect(object sender, MouseButtonEventArgs e)
        {
            VM.IsInLevelChooser = false;
            VM.IsInMenu = true;
        }

        private void ElsoPalya(object sender, MouseButtonEventArgs e)
        {
            VM.IsInLevelChooser = false;
            VM.BL.PalyaBetolt(1,30);
        }

        private void MasodikPalya(object sender, MouseButtonEventArgs e)
        {
            VM.IsInLevelChooser = false;
            VM.BL.PalyaBetolt(2,40);
        }

        private void HarmadikPalya(object sender, MouseButtonEventArgs e)
        {
            VM.IsInLevelChooser = false;
            VM.BL.PalyaBetolt(3,50);
        }

        private void NegyedikPalya(object sender, MouseButtonEventArgs e)
        {
            VM.IsInLevelChooser = false;
            VM.BL.PalyaBetolt(4,60);
        }

        private void Exit(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult mbr = MessageBox.Show("Tényleg be akarod zárni a játékot?","Kilépés",MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.Yes)
                this.Close();
        }
    }
}
