using System;
using Model;
using Services;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            IQuoteManager quoteManager = new QuoteManager();
            var quote = quoteManager.GetBestQuoteWithAvailableVolume("FRC");

            quoteManager.AddOrUpdateQuote(BuildQuote("FRC", 177.38, 1000, DateTime.Today.AddDays(1)));

            Console.WriteLine("Best Quote Id for FRC: {0}", quoteManager.GetBestQuoteWithAvailableVolume("FRC").Id);

            var executedata = quoteManager.ExecuteTrade("A", 160);

            Console.WriteLine("Executed volume: {0}", executedata.VolumeExecuted);
        }

        private static IQuote BuildQuote(string symbol, double price, uint volume, DateTime expirationDate) => new Quote
        {
            Symbol = symbol,
            Price = price,
            AvailableVolume = volume,
            ExpirationDate = expirationDate
        };
    }
}
