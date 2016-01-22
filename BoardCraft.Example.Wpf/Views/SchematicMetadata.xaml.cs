namespace BoardCraft.Example.Wpf.Views
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;

    /// <summary>
    ///     Interaction logic for SchematicMetadata.xaml
    /// </summary>
    public partial class SchematicMetadata : UserControl
    {
        public SchematicMetadata()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this is hack too :)
            var canvas = Canvas.Canvas.NativeCanvas;
            var b = Canvas.ComponentPlacement;
            var s = b.GetRenderSize();

            var cWidth = (int)canvas.RenderSize.Width;
            var cHeight = (int)canvas.RenderSize.Height;

            var bWidth = (int)(s.Width);
            var bHeight = (int)(s.Height);

            var rtb = new RenderTargetBitmap(
                cWidth,
                cHeight, 
                500d, 
                500d,
                PixelFormats.Default
            );
            rtb.Render(canvas);

            var crop = new CroppedBitmap(rtb, new Int32Rect(0, cHeight-bHeight, bWidth, bHeight));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            var fod = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                OverwritePrompt = true               
            };

            if (fod.ShowDialog() ?? false)
            {
                using (var fs = System.IO.File.OpenWrite(fod.FileName))
                {
                    pngEncoder.Save(fs);
                }
            }
        }
    }
}