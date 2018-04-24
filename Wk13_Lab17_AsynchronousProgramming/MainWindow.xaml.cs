using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Wk13_Lab17_AsynchronousProgramming;

namespace Wk13_Lab17_AsynchronousProgramming
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();
            Wk13_Lab17_DbUtils.Db.InitDb(new List<string>() { "Ford", "Honda" });
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = e.Source as FrameworkElement;
            int result;
            switch (fe.Name)
            {
                //UI can be updated if the operation is short
                case "btnNoDelay":
                    lblStatus.Content = "Executing query with no delay";
                    result = Wk13_Lab17_DbUtils.Db.Get("Ford");
                    lblStatus.Content = "Ford: " + result;
                    break;

                //UI hangs because the operation is lengthy
                case "btnDelayIncorrect":
                    lblStatus.Content = "Executing query with delay";
                    result = Wk13_Lab17_DbUtils.Db.GetWithDelay("Ford");
                    lblStatus.Content = "Ford: " + result;
                    break;

                //This is the recomended way to update the UI
                case "btnDelaycorrect":
                    lblStatus.Content = "Executing query correctly";
                    Func<Task<int>> func = async () =>
                    {
                        result = await Wk13_Lab17_DbUtils.Db.GetAsync("Ford");
                        return result;
                    };
                    result = await func();
                    lblStatus.Content = "Ford: " + result;
                    break;

                //This call to Db.GetAsync can be cancelled
                case "btnWithCancellation":
                    Button button = fe as Button;
                    try
                    {
                        Func<Task<int>> func1 = async () =>
                        {
                            int res = await Wk13_Lab17_DbUtils.Db.GetAsync("Ford", cts.Token); //the CancellationTokenSource must be declared in From scope
                            return res;
                        };

                        if (button.Content.ToString() == "Long Delay (With Cancel)")
                        {
                            button.Content = "Cancel Call";         //change the button text to prepare for cancellation
                            lblStatus.Content = "Executing no delay query ...";
                            result = await func1();
                            lblStatus.Content = "Ford: " + result;
                            button.Content = "Long Delay (With Cancel)";//reset the button text
                        }
                        else //user want to cancel the call
                        {
                            button.Content = "Long Delay (With Cancel)";//reset the button text
                            cts.Cancel();//cancels the current task
                            lblStatus.Content = "Cancelled... Ford Lookup";
                            cts = new CancellationTokenSource(); //resets the token
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        button.Content = "Long Delay (With Cancel)";//reset the button text
                        lblStatus.Content = "Cancel Operation Cleaned Up Successfully";
                    }

                    break;

                //This call to Db.GetAsync facilates progress reporting
                case "btnWithProgress":
                    lblStatus.Content = "Executing query with progress update";
                    progressBar.Value = 0;
                    Progress<int> progressIndicator = new Progress<int>(ReportProgress);//you will need to code the method ReportProgress
                    Func<Task<int>> func2 = async () =>
                    {
                        int res = await Wk13_Lab17_DbUtils.Db.GetAsync("Ford", progressIndicator);
                        return res;
                    };

                    result = await func2();
                    lblStatus.Content = "Ford: " + result;
                    break;
            }
        }

        void ReportProgress(int value)
        {
            progressBar.Value = value;
        }
    }
}
