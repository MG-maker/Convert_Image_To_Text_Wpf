using IronOcr;
using Microsoft.Win32;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using static IronSoftware.Drawing.AnyBitmap;

namespace Convert_Image_To_Text_Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string GetImageFilter()
        {
            return
                "All Pictures (*.emf;*.wmf;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.bmp;*.dib;*.rle;*.gif;*.emz;*.wmz;*.tif;*.tiff;*.svg;*.ico)" +
                    "|*.emf;*.wmf;*.jpg;*.jpeg;*.jfif;*.jpe;*.png;*.bmp;*.dib;*.rle;*.gif;*.emz;*.wmz;*.tif;*.tiff;*.svg;*.ico";
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ImageDrop_DropDown(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    imageTF.Source = new BitmapImage(new Uri(files[0]));
                    txtB_1.Text = System.IO.Path.GetFullPath(files[0]);
                }
                catch 
                {
                    MessageBox.Show("Your file is not image!!. Try to paste right image file!!", "Image File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                    
            }
        }

        private void EnterClicked(object sender, KeyEventArgs e)
        {
            Uri uriResult;
            if (txtbLink.Text != "")
            if (e.Key == Key.Enter)
                    {
                    bool result = Uri.TryCreate(txtbLink.Text, UriKind.Absolute, out uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                    if (result == true)
                        {
                            CreateImage(sender, e);
                            e.Handled = true;
                        }
                    else
                    {
                        MessageBox.Show("Your image link is not right!!. Try to paste right image link!!", "Image Link Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
        }

        private void Button_AddImgFromFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = GetImageFilter();
            openFileDialog.FilterIndex = 2;

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(openFileDialog.FileName);
                    bitmapImage.EndInit();
                    imageTF.Source = bitmapImage;

                    txtB_1.Text = System.IO.Path.GetFullPath(openFileDialog.FileName);// путь к файлу
                }

                catch
                {
                    MessageBox.Show("Your file is not image!!. Try to open right image file!!", "Image File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_ClearImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (imageTF.Source != null)
                    imageTF.Source = null;

                if (txtB_1.Text != "")
                    txtB_1.Text = "";

                if (txtbLink.Text != "")
                    txtbLink.Text = "";  
                
                if (txtBTF.Text != "")
                    txtBTF.Text = "";
            }
            catch
            {
                MessageBox.Show("No objects to clean up", "Clear Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_GetTextFromImg_Click(object sender, RoutedEventArgs e)
        {
            if (imageTF.Source != null)
            {
             //   try
               // {
                    string temp = imageTF.Source.ToString();
                    if (temp.Contains("file:///") == true)
                        temp = Regex.Replace(temp, "file:///", "");

                    IronTesseract ocr = new IronTesseract();

                    ocr.Language = OcrLanguage.RussianBest;
                    ocr.AddSecondaryLanguage(OcrLanguage.EnglishBest);
                    ocr.AddSecondaryLanguage(OcrLanguage.GermanBest);
                
                // ... add languages what you want but don't forget add a language library to the nuget package

                //   ocr.AddSecondaryLanguage(OcrLanguage.UkrainianBest);
                //   ocr.AddSecondaryLanguage(OcrLanguage.ChineseTraditionalBest);
                //   ocr.AddSecondaryLanguage(OcrLanguage.JapaneseBest);
                

                IronOcr.Installation.LogFilePath = "Default.log";
                IronOcr.Installation.LoggingMode = IronOcr.Installation.LoggingModes.All;

                using (OcrInput input = new OcrInput(temp))
                    {
                        input.Deskew();
                        OcrResult result = ocr.Read(input);
                        txtBTF.Text = result.Text;
                    }
            //}
            //    catch
            //    {
            //    MessageBox.Show("Your path image file is not correct!!. Check your image file path!!", "Image File Path Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

        }
            else
            {
                MessageBox.Show("Image is not added","Image File Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CreateImage(object sender, RoutedEventArgs e)
        {
            string picPath = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf("bin")) + @"TempImage\" + "4200020789506" + ".png";
            try
            {
                if (Clipboard.ContainsImage() && txtbLink.Text == "")
                {
                    using (var fileStream = new FileStream(picPath, FileMode.Create))
                    {
                        var pngBitmapEncoder = new PngBitmapEncoder();
                        pngBitmapEncoder.Frames.Add(BitmapFrame.Create(Clipboard.GetImage()));
                        pngBitmapEncoder.Save(fileStream);
                    }
                }
                if(Clipboard.ContainsData(DataFormats.Text) && txtbLink.Text != "")
                {
                    using (var fileStream = new FileStream(picPath, FileMode.Create))
                    {
                        var c = new WebClient { Encoding = Encoding.UTF8 }; 
                        c.Headers["User-Agent"] = "Mozilla/5.0";
                        var bytes = c.DownloadData(txtbLink.Text);
                        var ms = new MemoryStream(bytes);
                        var bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = ms;
                        bi.EndInit();
                        var pngBitmapEncoder = new PngBitmapEncoder();
                        pngBitmapEncoder.Frames.Add(BitmapFrame.Create(bi));
                        pngBitmapEncoder.Save(fileStream);
                    }
                }

                var bitmap = new BitmapImage(new Uri(picPath)).Clone();
                bitmap.Freeze();
                Dispatcher.BeginInvoke(
              new ThreadStart(
                  delegate
                  {
                      imageTF.Source = bitmap;
                      txtB_1.Text = System.IO.Path.GetFullPath(picPath);
                  }
                  )
              );
            }
            catch
            {
                MessageBox.Show("Your image source file is not correct!!. Try to add correct image source file!!", "Image Source Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickPaste(object sender, RoutedEventArgs e)
        {
            txtbLink.Text = "";
            CreateImage(sender, e);
            
        }

        private void StackPanel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ClickPaste(sender,e);
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            GridPan.Focus();
        }

        private void stkPanFocus_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridPan.Focus();
            e.Handled = true;
        }

    }
}

