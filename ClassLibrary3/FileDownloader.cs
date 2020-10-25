using ClassLibrary3.interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary3
{
    public class FileDownloader : IFileDownloader
    {
        static HttpClient _httpClient;

        SemaphoreSlim semaphore;
        int _maxDegree = 4;
        bool _isWorking;
        List<Task> tasks;


        public event Action<string> OnDownloaded = delegate { };
        public event Action<string, Exception> OnFailed = delegate { };
        public event Action<string, int, int> OnFileProgress = delegate { };

        public FileDownloader()
        {
            _httpClient = new HttpClient();
            _isWorking = false;
            tasks = new List<Task>();
        }
        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {
            if (semaphore == null)
                semaphore = new SemaphoreSlim(_maxDegree, _maxDegree);
            if (!Directory.Exists(pathToSave))
                throw new DirectoryNotFoundException();
            Task task = Task.Run(() => ProccessUrlAsync(fileId, url, pathToSave));
            tasks.Add(task);
        }
        private async Task ProccessUrlAsync(string id, string url, string pathToSave)
        {
            await semaphore.WaitAsync();
            try
            {
                var response = await _httpClient.GetAsync(url);
                var totalBytes = response.Content.Headers.ContentLength;
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                    await ProcessContentStream(id, totalBytes, contentStream, Path.Combine(pathToSave, Path.GetFileName(url)));
                OnDownloaded(id);
            }
            catch (Exception ex)
            {
                OnFailed(id.ToString(), ex);
            }
            finally
            {
                semaphore.Release();
            }
        }
        private async Task ProcessContentStream(string id, long? totalDownloadSize, Stream contentStream, string destinationFilePath)
        {

            var downloadedBytes = 0;
            var readCount = 0;
            var buffer = new byte[8192];
            var isMoreToRead = true;
            using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                do
                {
                    var bytes = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytes == 0)
                    {
                        isMoreToRead = false;
                        OnFileProgress(id, (int)totalDownloadSize, downloadedBytes);
                        continue;
                    }

                    await fileStream.WriteAsync(buffer, 0, bytes);

                    downloadedBytes += bytes;
                    readCount++;

                    if (readCount % 100 == 0)
                       OnFileProgress(id, (int)totalDownloadSize, downloadedBytes);
                }
                while (isMoreToRead);
            }
        }

        public async Task OnAllAsync() => await Task.WhenAll(tasks.ToArray());
        public void SetDegreeOfParallelism(int degreeOfParallelism)
        {
            if (_isWorking)
                throw new InvalidOperationException();
            if (degreeOfParallelism <= 0)
                throw new ArgumentOutOfRangeException();
            _maxDegree = degreeOfParallelism;
            _isWorking = true;
        }
        public int GetDegreeOfParallelism() => _maxDegree;
    }

}
