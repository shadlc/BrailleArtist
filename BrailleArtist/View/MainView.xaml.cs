using BrailleArtist.Common;
using BrailleArtist.View;
using BrailleArtist.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BrailleArtist
{
    public partial class MainView : Window
    {
        MainViewModel Model = new MainViewModel();
        public MainView()
        {
            InitializeComponent();
            DataContext = Model;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            Language_Choose(System.Globalization.CultureInfo.CurrentCulture.Name);
        }
        private void WindowControl(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                {
                    MaximizeButton.Content = "";
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    MaximizeButton.Content = "";
                    WindowState = WindowState.Normal;
                }
            }
        }
        private async void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                GValues.ImgName = null;
                GValues.Image.Dispose();
                Model.ImgSource = null;
                Model.BrailleDraw = null;
                Model.Width = 0;
                Model.Height = 0;
                if (ImportHelpText.Visibility != Visibility.Visible) ImportHelpText.Visibility = Visibility.Visible;
            }
            if (e.Key == Key.Enter)
            {
                BrailleView.Focus();
                Model.LoadingVisibility = "Visible";
                await Task.Run(() => Model.DrawBraille());
                Model.LoadingVisibility = "Collapsed";
            }
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            window.WindowState = WindowState.Minimized;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                MaximizeButton.Content = "";
                WindowState = WindowState.Maximized;
            }
            else
            {
                MaximizeButton.Content = "";
                WindowState = WindowState.Normal;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var story = (Storyboard)Resources["WindowDisappearAnimation"];
            if (story != null)
            {
                story.Completed += delegate { Close(); };
                story.Begin(this);
            }
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            if (AboutView.Visibility == Visibility.Visible) AboutView.Visibility = Visibility.Collapsed;
            else if (AboutView.Visibility == Visibility.Collapsed) AboutView.Visibility = Visibility.Visible;
        }
        private void GithubLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/shadlc/BrailleArtist");
        }
        private void Language_enUS_Click(object sender, RoutedEventArgs e)
        {
            Language_Choose("en-US");
        }
        private void Language_zhCN_Click(object sender, RoutedEventArgs e)
        {
            Language_Choose("zh-CN");
        }
        private void Img_Choose(object sender, MouseButtonEventArgs e)
        {
            var fbd = new System.Windows.Forms.OpenFileDialog() { Title = FindResource("ImgChooseTitle").ToString(), Filter = FindResource("ImgChooseType").ToString() + "(*.bmp;*.png;*.jpg;*.jpeg;*.gif)|*.bmp;*.png;*.jpg;*.jpeg;*.gif" };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportImg(fbd.FileName);
            }
        }
        private void Img_Drop(object sender, System.Windows.DragEventArgs e)
        {
            ImportImg(((System.Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString());
        }
        private void Img_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Link;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }
        private void MiddleBright_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Model.Height * Model.Width <= 250000) Model.DrawBraille();
        }
        private void Size_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Model.IsRatioLock)
            {
                if (Width_TextBox.IsFocused == true && Width_TextBox.Text != "" && Regex.IsMatch(Width_TextBox.Text, @"^\d*[.]?\d*$"))
                {
                    Model.Height = Convert.ToInt32(Convert.ToDouble(Width_TextBox.Text) / GValues.WidthDHeight);
                }
                else if (Height_TextBox.IsFocused == true && Height_TextBox.Text != "" && Regex.IsMatch(Height_TextBox.Text, @"^\d*[.]?\d*$"))
                {
                    Model.Width = Convert.ToInt32(Convert.ToDouble(Height_TextBox.Text) * GValues.WidthDHeight);
                }
            }
        }
        private void ViewScale_Change(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Delta < 0 && Model.ViewFontSize > 1) Model.ViewFontSize -= 1;
                else if (e.Delta > 0) Model.ViewFontSize += 1;
                e.Handled = true;
            }
        }

        private async void ImportImg(string importname)
        {
            try
            {
                Model.LoadingVisibility = "Visible";
                await Task.Run(() =>
                {
                    GValues.ImgName = importname;
                    using (Stream ms = new MemoryStream(File.ReadAllBytes(importname)))
                    {
                        GValues.Image = new Bitmap(ms);
                    }
                    if (GValues.Image.Width > 1000 || GValues.Image.Height > 1000)
                    {
                        Model.Width = GValues.Image.Width / 5;
                        Model.Height = GValues.Image.Height / 5;
                    }
                    else if (GValues.Image.Width > 5000 || GValues.Image.Height > 5000)
                    {
                        Model.Width = GValues.Image.Width / 10;
                        Model.Height = GValues.Image.Height / 10;
                    }
                    else
                    {
                        Model.Width = GValues.Image.Width;
                        Model.Height = GValues.Image.Height;
                    }
                    GValues.WidthDHeight = (float)Model.Width / (float)Model.Height;
                    Model.ShowImg();
                    Model.CountImg();
                    Model.MiddleBright = GValues.PixelAverage;
                    Model.DrawBraille();
                });
                Model.LoadingVisibility = "Collapsed";
                if (ImportHelpText.Visibility != Visibility.Collapsed) ImportHelpText.Visibility = Visibility.Collapsed;
            }
            catch
            {
                GValues.ImgName = null;
                Model.LoadingVisibility = "Collapsed";
                IMessageBox.Show(FindResource("MsgBoxImportErrorText").ToString(), FindResource("MsgBoxImportErrorTitle").ToString(), FindResource("MsgBoxImportErrorOK").ToString());
            }
        }
        private void Language_Choose(string language)
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            string requestedlang = string.Format("Assets/Langs/{0}.xaml", language);
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedlang));
            if (resourceDictionary == null)
            {
                requestedlang = "Assets/Langs/en-US.xaml";
                resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedlang));
            }
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }

    }
}
