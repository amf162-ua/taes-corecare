using System.Windows;

namespace CoreCare.Views
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percent, string status)
        {
            Dispatcher.Invoke(() =>
            {
                if (percent <= 0)
                {
                    ProgressBar.IsIndeterminate = true;
                    PercentText.Text = "";
                }
                else
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = percent;
                    PercentText.Text = percent + "%";
                }
                StatusText.Text = status;
            });
        }
    }
}