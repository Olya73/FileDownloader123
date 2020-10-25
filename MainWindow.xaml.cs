using ClassLibrary3;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WPFDownloader.Models;

namespace WPFDownloader
{
    
    
    public partial class MainWindow : Window
    {
        FileDownloader fileDownloader;

        bool isFirst = true;
        public MainWindow()
        {
            InitializeComponent();            
        }

        private async void OpenFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            Init();
            if (openFileDialog.ShowDialog() == true) {
                urlListGrid.Items.Clear();
                try
                {
                    string line;
                    using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                    {
                        while ((line = await sr.ReadLineAsync()) != null)
                        {
                            urlListGrid.Items.Add(new FileModel(){ Url = line, Start = "start", Progress = 0 }) ;
                        }
                    }

                    chngThreads.Visibility = Visibility.Visible;
                    isFirst = true;
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (DirectoryNotFoundException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };  
        }

        private void Init()
        {
            fileDownloader = new FileDownloader();

            fileDownloader.OnFileProgress += OnProgress;
            fileDownloader.OnFailed += OnFail;
            fileDownloader.OnDownloaded += OnDownload;
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (isFirst)
            {
                var num = (ThreadNumber)this.Resources["threadNum"];

                fileDownloader.SetDegreeOfParallelism(num.Number);

                chngThreads.Visibility = Visibility.Hidden;
                isFirst = false;
            }

            var btn = sender as Button;
            btn.IsEnabled = false;

            var line = urlListGrid.SelectedItem as FileModel;

            var id = urlListGrid.SelectedIndex;
            fileDownloader.AddFileToDownloadingQueue(id.ToString(), line.Url, @"C:\Users\User\Documents\images");

        }


        private void OnProgress(string id, int totalBytes, int downloadedBytes)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                FileModel item = (FileModel)urlListGrid.Items[Int32.Parse(id)];
                item.Progress = downloadedBytes * 100 / totalBytes;
            }));
        }


        public void OnFail(string id, Exception exception)
        {
            File.AppendAllText("log.txt",
                $"{DateTime.Now} \n" +
                $"File {id} is failed with an exception:\n " +
                $"{exception.Message} \n" +
                $"{exception.StackTrace} \n\n");
            Dispatcher.BeginInvoke((Action)(() =>
            {               
                FileModel item = (FileModel)urlListGrid.Items[Int32.Parse(id)];
                item.Start = "failed";
            }));

        }
        public void OnDownload(string id)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                FileModel item = (FileModel)urlListGrid.Items[Int32.Parse(id)];
                item.Start = "completed";
            }));
        }
        private void IncNumbClick(object sender, RoutedEventArgs e)
        {
            ThreadNumber num = (ThreadNumber)this.Resources["threadNum"];
            num.Number++;
        }

        private void DecNumbClick(object sender, RoutedEventArgs e)
        {
            ThreadNumber num = (ThreadNumber)this.Resources["threadNum"];
            if (num.Number >= 1)
                num.Number--;
        }


    }
}
