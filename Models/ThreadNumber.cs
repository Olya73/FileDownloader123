using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFDownloader.Models
{
    public class ThreadNumber : INotifyPropertyChanged
    {
        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged(nameof(Number));
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
