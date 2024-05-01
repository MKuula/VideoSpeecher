using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Threading;

namespace VideoSpeecher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel vModel;
        Timer placementTimer;

        public MainWindow()
        {
            InitializeComponent();
            ViewModel viewModel = new ViewModel();
            DataContext = vModel = viewModel;
            viewModel.Dispatcher = Dispatcher;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (((Control)sender).IsMouseCaptureWithin)
                ((ViewModel)DataContext).UpdatePosition((long)e.NewValue);
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.CheckFileExists = true;
            if (openDialog.ShowDialog().Value)
            {
                Cursor = Cursors.Wait;
                vModel.AudioFilename = openDialog.FileName;
                vModel.SubtitleFilename = openDialog.FileName.Replace(".mp3", ".fin.srt");
                vModel.LoadFiles();
                Cursor = Cursors.Arrow;
            }
        }

        private void TextBox_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.CheckFileExists = true;
            if (openDialog.ShowDialog().Value)
            {
                Cursor = Cursors.Wait;
                vModel.SubtitleFilename = openDialog.FileName;
                vModel.LoadSubtitle();
                Cursor = Cursors.Arrow;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            vModel.Exit();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
            {
                ControlPanel.Visibility = Visibility.Visible;
                Top -= ControlPanel.ActualHeight;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            new Timer(o =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (WindowState != WindowState.Minimized)
                    {
                        Top += ControlPanel.ActualHeight;
                        ControlPanel.Visibility = Visibility.Collapsed;
                    }
                });
            }, null, 200, Timeout.Infinite);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (Top < 0)
                Top = 0;
            else if (Top > SystemParameters.PrimaryScreenHeight - SystemParameters.WindowCaptionHeight)
                Top = SystemParameters.PrimaryScreenHeight - SystemParameters.WindowCaptionHeight;
        }
    }
}
