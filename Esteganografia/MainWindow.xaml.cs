using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;

namespace Esteganografia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string rutaInEste1 = "";
        string rutaInEste2 = "";
        string rutaOutEste = "";
        public MainWindow()
        {
            InitializeComponent();
            btnDesesteganografiar.IsEnabled = false;
            btnDesesteganografiar.Visibility = Visibility.Hidden;
        }

        private string ObtenirImatge(System.Windows.Controls.Image img)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png;*.jpg;*.jpeg;";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                img.Source = bitmapImage;
                return openFileDialog.FileName;
            }
            return null;
        }
        private void btnImg1Esteganografiar_Click(object sender, RoutedEventArgs e)
        {
            rutaInEste1 = ObtenirImatge(imgIn1);
            if (string.IsNullOrEmpty(rutaInEste1))
            {
                MessageBox.Show("Ruta no seleccionada");
            }
            else
            {
                btnImg1Esteganografiar.Visibility = Visibility.Hidden;

                btnImg1Esteganografiar.IsEnabled = false;
            }
        }
        private void btnImg2Esteganografiar_Click(object sender, RoutedEventArgs e)
        {
            rutaInEste2 = ObtenirImatge(imgIn2);
            if (string.IsNullOrEmpty(rutaInEste2))
            {
                MessageBox.Show("Ruta no seleccionada");
            }
            else
            {
                btnImg2Esteganografiar.Visibility = Visibility.Hidden;

                btnImg2Esteganografiar.IsEnabled = false;
            }
        }
        private void btnEsteganografiar_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bmsImatgeVisible = new BitmapImage(new Uri(rutaInEste1));
            BitmapSource bmsImatgeAmagada = new BitmapImage(new Uri(rutaInEste2));
            BitmapSource imatgeEsteganografiada = Esteganografiar(bmsImatgeVisible, bmsImatgeAmagada);
            imgFinal.Source = imatgeEsteganografiada;

            btnDesesteganografiar.IsEnabled = true;
            btnDesesteganografiar.Visibility = Visibility.Visible;
        }
        public static BitmapImage CarregarImatge(string imagePath)
        {
            Uri imageUri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            BitmapImage bitmapImage = new BitmapImage(imageUri);
            return bitmapImage;
        }
        private BitmapSource Esteganografiar(BitmapSource imatge1, BitmapSource imatge2)
        {
            Bitmap bitmap1 = BitmapImage2Bitmap(imatge1);
            Bitmap bitmap2 = BitmapImage2Bitmap(imatge2);

            Bitmap steganografiarBitmap = Esteganografia(bitmap1, bitmap2);

            BitmapSource steganografiarImatge = Bitmap2BitmapSource(steganografiarBitmap);

            return steganografiarImatge;
        }

        private Bitmap Esteganografia(Bitmap sourceBitmap1, Bitmap sourceBitmap2)
        {
            if (sourceBitmap1.Width != sourceBitmap2.Width || sourceBitmap1.Height != sourceBitmap2.Height)
            {
                throw new ArgumentException("Han de tenir les mateixes dimensions.");
            }

            var esteganografiarBitmap = new Bitmap(sourceBitmap1.Width, sourceBitmap1.Height);

            for (int y = 0; y < sourceBitmap1.Height; y++)
            {
                for (int x = 0; x < sourceBitmap1.Width; x++)
                {
                    System.Drawing.Color color1 = sourceBitmap1.GetPixel(x, y);
                    System.Drawing.Color color2 = sourceBitmap2.GetPixel(x, y);

                    System.Drawing.Color esteganografiarColor = System.Drawing.Color.FromArgb(color1.R, color2.G, color1.B);

                    esteganografiarBitmap.SetPixel(x, y, esteganografiarColor);
                }
            }

            return esteganografiarBitmap;
        }
        private Bitmap BitmapImage2Bitmap(BitmapSource bitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
                bitmap = new Bitmap(stream);
            }
            return bitmap;
        }

        private BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private void btnDesesteganografiar_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource sourceImage = (BitmapSource)imgFinal.Source;

            (BitmapSource image1, BitmapSource image2) = SepararImatges(sourceImage);

            imgDeses1.Source = image1;
            imgDeses2.Source = image2;

        }

        private (BitmapSource, BitmapSource) SepararImatges(BitmapSource sourceImage)
        {
            Bitmap bitmap = BitmapImage2Bitmap(sourceImage);

            int width = bitmap.Width;
            int height = bitmap.Height;

            Bitmap image1Bitmap = new Bitmap(width, height);
            Bitmap image2Bitmap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    System.Drawing.Color color = bitmap.GetPixel(x, y);

                    System.Drawing.Color image1Color = System.Drawing.Color.FromArgb(color.R, 0, 0);
                    System.Drawing.Color image2Color = System.Drawing.Color.FromArgb(0, color.G, color.B);

                    image1Bitmap.SetPixel(x, y, image1Color);
                    image2Bitmap.SetPixel(x, y, image2Color);
                }
            }

            BitmapSource image1 = Bitmap2BitmapSource(image1Bitmap);
            BitmapSource image2 = Bitmap2BitmapSource(image2Bitmap);

            return (image1, image2);
        }
    }
}
