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
using System.Drawing;
using System.Threading;
using System.IO;
using Path = System.IO.Path;

namespace DataParallelismWithForEach
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            cancellationToken.Cancel();
        }
        private void cmdProcess_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => ProcessFiles());
        }
        private void ProcessFiles()
        {
            ParallelOptions options = new ParallelOptions();
            options.CancellationToken = cancellationToken.Token;
            options.MaxDegreeOfParallelism = System.Environment.ProcessorCount;

            string[] files = Directory.GetFiles(@".\TestPictures", "*.jpg", SearchOption.AllDirectories);
            string newDir = @".\ModifiedPictures";
            Directory.CreateDirectory(newDir);
            try
            {
                Parallel.ForEach(files, options, currentFile =>
                {
                    string filename = Path.GetFileName(currentFile);

                    using (Bitmap bitmap = new Bitmap(currentFile))
                    {
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        bitmap.Save(Path.Combine(newDir, filename));
                        this.Dispatcher.Invoke(() =>
                        {
                            this.Title = $"Processing {filename} on thread {Thread.CurrentThread.ManagedThreadId}";
                        });
                       
                    }
                    
                });
                this.Dispatcher.Invoke(() => this.Title = "Done!");
            }
            catch (OperationCanceledException e)
            {
                this.Dispatcher.Invoke(() => this.Title = e.Message);
                cancellationToken = new CancellationTokenSource();
            }
        }
    }
}
