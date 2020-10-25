using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary3.interfaces
{
    interface IFileDownloader
    {
        void SetDegreeOfParallelism(int degreeOfParallelism);
        void AddFileToDownloadingQueue(string fileId, string url, string pathToSave);

        event Action<string> OnDownloaded;
        event Action<string, Exception> OnFailed;
        event Action<string, int, int> OnFileProgress;

    }
}
