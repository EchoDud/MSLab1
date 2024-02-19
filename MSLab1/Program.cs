using MSLab1;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;


internal class Program
{
    static void Main(string[] args)
    {
        var model = new PlotModel 
        { 
            Title = "Результаты эксперимента по подбрасыванию монеты" 
        };

        int quantityOfFlips;
        do
        {
            Console.WriteLine("Введите количество подбрасываний монеты:");
        } while (!int.TryParse(Console.ReadLine(), out quantityOfFlips) || quantityOfFlips <= 0);

        int quantityOfExperiments;
        do
        {
            Console.WriteLine("Введите количество экспериментов:");
        } while (!int.TryParse(Console.ReadLine(), out quantityOfExperiments) || quantityOfExperiments <= 0);

        double confidenceInterval;
        do
        {
            Console.WriteLine("Введите доверительный интервал:");
        } while (!double.TryParse(Console.ReadLine(), out confidenceInterval) || confidenceInterval <= 0 || confidenceInterval > 1);

        model.Axes.Add(new LogarithmicAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Количество подбрасываний",
        });
        model.Axes.Add(new OxyPlot.Axes.LinearAxis
        {
            Position = OxyPlot.Axes.AxisPosition.Left,
        });

        List<List<double>> experimentSeries = Experiment.CoinFlipSeries(quantityOfFlips, quantityOfExperiments);
        var meanValues = new List<double>();
        for (int j = 0; j < quantityOfFlips; j++)
        {
            double sum = 0;
            for (int i = 0; i < quantityOfExperiments; i++)
            {
                sum += experimentSeries[i][j];
            }
            meanValues.Add(sum / quantityOfExperiments);
        }
        
        for (int i = 0; i < experimentSeries.Count; i++)
        {
            var data = new List<DataPoint>();
            for (int j = 1; j <= quantityOfFlips; j++)
            {
                data.Add(new DataPoint(j, experimentSeries[i][j - 1]));
            }
            var lineSeries = new LineSeries 
            { 
                ItemsSource = data, 
                Color = OxyColors.Black 
            };
            model.Series.Add(lineSeries);
        }
        var meanData = new List<DataPoint>();
        for (int j = 0; j < quantityOfFlips; j++)
        {
            meanData.Add(new DataPoint(j + 1, meanValues[j]));
        }
        var meanLineSeries = new LineSeries 
        { 
            ItemsSource = meanData, 
            Color = OxyColors.Red 
        };
        model.Series.Add(meanLineSeries);

        var confidencialIntervalValuesMax = new List<double>();
        var confidencialIntervalValuesMin = new List<double>();
        int elementsToDrop = Convert.ToInt32((quantityOfExperiments * ((1 - confidenceInterval) / 2)));
        for (int j = 0; j < quantityOfFlips; j++)
        {
            var dataList = new List<double>();
            for (int i = 0; i < quantityOfExperiments; i++)
            {
                dataList.Add(experimentSeries[i][j]);
            }
            dataList.Sort();
            var trimmedList = (dataList.Skip(elementsToDrop)).Take(quantityOfExperiments - elementsToDrop * 2).ToList();
            confidencialIntervalValuesMin.Add(trimmedList.First());
            confidencialIntervalValuesMax.Add(trimmedList.Last());

        }
        var confidencialIntervalValuesMinData = new List<DataPoint>();
        var confidencialIntervalValuesMaxData = new List<DataPoint>();
        for (int j = 0; j < quantityOfFlips; j++)
        {
            confidencialIntervalValuesMinData.Add(new DataPoint(j + 1, confidencialIntervalValuesMin[j]));
            confidencialIntervalValuesMaxData.Add(new DataPoint(j + 1, confidencialIntervalValuesMax[j]));
        }

        var confidencialIntervalValuesMinLineSeries = new LineSeries 
        { 
            ItemsSource = confidencialIntervalValuesMinData, 
            Color = OxyColors.Blue 
        };
        var confidencialIntervalValuesMaxLineSeries = new LineSeries 
        { 
            ItemsSource = confidencialIntervalValuesMaxData, 
            Color = OxyColors.Blue 
        };

        model.Series.Add(confidencialIntervalValuesMaxLineSeries);
        model.Series.Add(confidencialIntervalValuesMinLineSeries);

        var plotViewForExp = new OxyPlot.WindowsForms.PlotView()
        {
            Model = model,
            Dock = System.Windows.Forms.DockStyle.Fill,
        };
        Thread plotThread1 = new Thread(() =>
        {
            var firstForm = new Form()
            {
                Size = new System.Drawing.Size(1280, 720),
            };
            firstForm.Controls.Add(plotViewForExp);
            Application.Run(firstForm);
        });
        plotThread1.SetApartmentState(ApartmentState.STA);
        plotThread1.Start();

        var modelForErrors = new PlotModel 
        {
            Title = "Графики ошибки",
        };
        modelForErrors.Axes.Add(new LogarithmicAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Количество подбрасываний",
        });
        modelForErrors.Axes.Add(new OxyPlot.Axes.LinearAxis
        {
            Position = OxyPlot.Axes.AxisPosition.Left,
        });

        List<DataPoint> expError = new List<DataPoint>();
        for (int i = 0; i < quantityOfFlips; i++)
        {
            expError.Add(new DataPoint(i + 1, ((confidencialIntervalValuesMax[i] - confidencialIntervalValuesMin[i]) / 2)));
        }
        modelForErrors.Series.Add(new LineSeries() 
        {
            ItemsSource = expError,
            Title = "Эксперементальная ошибка"
            
        });

        double coefficient = Сalculations.NormalQuantile((1 + confidenceInterval) / 2);
        List<DataPoint> theoryError = new List<DataPoint>();
        for (int i = 0; i < quantityOfFlips; i++)
        {
            theoryError.Add(new DataPoint(i + 1, coefficient * Math.Sqrt(0.5 * 0.5 / (i + 1))));
        }
        modelForErrors.Series.Add(new LineSeries()
        {
            ItemsSource = theoryError,
            Title = "Теоретическая ошибка"
        });
        var plotViewForError = new OxyPlot.WindowsForms.PlotView()
        {
            Model = modelForErrors,
            Dock = System.Windows.Forms.DockStyle.Fill,
        };
        Thread plotThread2 = new Thread(() =>
        {
            var secondForm = new Form()
            {
                Size = new System.Drawing.Size(1280, 720),
            };
            secondForm.Controls.Add(plotViewForError);
            Application.Run(secondForm);
        });
        plotThread2.SetApartmentState(ApartmentState.STA);
        plotThread2.Start();
        Console.WriteLine($"Оценка вероятности выпадения орла и ошибки вычисления:\n{meanValues.Last()} +- {(confidencialIntervalValuesMax.Last() - confidencialIntervalValuesMin.Last()) / 2}");
    }
}