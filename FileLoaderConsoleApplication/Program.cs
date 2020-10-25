using ClassLibrary3;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileLoaderConsoleApp
{
    class Program
    {
        public static ConcurrentDictionary<string, bool> files = new ConcurrentDictionary<string, bool>();
        public static event Func<Task> OnEnd;

        static async Task Main(string[] args)
        {

            FileDownloader fileDownloader = new FileDownloader();
            fileDownloader.SetDegreeOfParallelism(1);
            
            fileDownloader.OnDownloaded += OnDownload;
            fileDownloader.OnFailed += OnFail;
            OnEnd += fileDownloader.OnAllAsync;

            string fileName = $@"{System.IO.Directory.GetCurrentDirectory()}\url.txt";
            int id = 0;
            try
            {
                string line;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        fileDownloader.AddFileToDownloadingQueue(id.ToString(), line, @"C:\Users\User\Documents\images"); 
                        files.GetOrAdd(id.ToString(), false);
                        id++;
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (OnEnd != null)
                await OnEnd().ContinueWith(delegate {
                    Console.WriteLine($"{files.Values.Where(v => v == true).Count()} downloaded successfully");
                    Console.WriteLine($"{files.Values.Where(v => v == false).Count()} was failed");
                });           
            Console.ReadKey();
        }
        public static void OnDownload(string id)
        {           
            files[id] = true;
            int count = files.Values.Where(v => v == true).Count();
            Console.WriteLine($"File {id} is downloaded. Downloaded { count * 100 / files.Count}% outta 100%");
        }
        public static void OnFail(string id, Exception exception)
        {
            File.AppendAllText("log.txt",
                $"{DateTime.Now} \n" +
                $"File {id} is failed with an exception:\n " +
                $"{exception.Message} \n" +
                $"{exception.StackTrace} \n\n");
        }
        
        
    }
}
