using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFDownloader.Models
{
    public class FileModel : INotifyPropertyChanged
    {
        private string url;
        private string start;
        private int progress;
        public string Start
        {
            get { return start; }
            set
            {
                start = value;
                OnPropertyChanged(nameof(Start));
            }
        }
        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                OnPropertyChanged(nameof(Url));
            }
        }
        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
