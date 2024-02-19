using System.Collections.ObjectModel;
using System.Numerics;

namespace MSLab1
{
    internal class Experiment
    {
        static private List<double> CoinFlip(int quantity)
        {
            Random random = new Random();
            List<double> frequencies = new List<double>();

            int headsCount = 0;
            for (int i = 0; i < quantity; i++)
            {
                int result = random.Next(0, 2);
                if (result == 1)
                {
                    headsCount++;
                }
                double frequency = (double)headsCount / (i + 1);
                frequencies.Add(frequency);
            }

            return frequencies;
        }
        static public List<List<double>> CoinFlipSeries(int quantityOfFlips, int quantityOfExperiment)
        {
            List<List<double>> experimentResult = new List<List<double>>();
            /*for (int i = 0; i < quantityOfExperiment; i++)
            {
                experimentResult.Add(CoinFlip(quantityOfFlips));
            }*/

            Parallel.For(0, quantityOfExperiment, i =>
            {
                lock (experimentResult)
                {
                    experimentResult.Add(CoinFlip(quantityOfFlips));
                }
            });

            return experimentResult;
        }
    }
}
