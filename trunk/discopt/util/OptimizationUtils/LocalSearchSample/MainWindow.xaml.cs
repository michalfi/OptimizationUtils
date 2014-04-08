using LocalSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace LocalSearchSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SearchEngine<SampleSolution> Engine { get; set; }
        private CancellationTokenSource CancelTokenSrc { get; set; }
        private TextBlock[] WorkerStatusFields { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelTokenSrc != null)
                return;
            int workerCount = 4;
            WorkerStatusFields = GenerateWorkerStatusFields(workerCount);

            CancelTokenSrc = new CancellationTokenSource();
            Engine = new SearchEngine<SampleSolution>(new SampleProblem(), new RandomWalkStrategyFactory(), TaskScheduler.Default) { WorkerProcessCount = workerCount };
            Engine.BestSolutionImproved += (s, ea) => { Dispatcher.Invoke((Action)delegate() { ResultLabel.Text = ea.Value.ToString(SolutionValueFormat); }); };
            Engine.WorkerSolutionUpdate += (s, ea) => { Dispatcher.Invoke((Action)delegate() { WorkerStatusFields[ea.WorkerId.Value].Text = ea.Value.ToString(SolutionValueFormat); }); };

            StopButton.IsEnabled = true;
            StartButton.IsEnabled = false;
            Task<SampleSolution> t = Engine.SearchAsync(CancelTokenSrc);
            await t;

            ResultLabel.Text = t.Result.ObjectiveValue().ToString(SolutionValueFormat);
            StopButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            CancelTokenSrc = null;
        }

        private const string SolutionValueFormat = "F2";

        private TextBlock[] GenerateWorkerStatusFields(int workerCount)
        {
            TextBlock[] fields = new TextBlock[workerCount];
            for (int i = 0; i < workerCount; i++)
            {
                var panel = new StackPanel() { Orientation = Orientation.Horizontal};
                panel.Children.Add(new TextBlock() { Text = string.Format("Worker {0}:", i), Width = 100, VerticalAlignment = VerticalAlignment.Center });
                fields[i] = new TextBlock() { Text = "-", VerticalAlignment = VerticalAlignment.Center, Width = 100, TextAlignment = TextAlignment.Right };
                panel.Children.Add(fields[i]);
                WorkerResultsPanel.Children.Add(panel);
            }
            return fields;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelTokenSrc != null)
                CancelTokenSrc.Cancel();
        }
    }
}
