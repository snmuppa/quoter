using System;
using Model;
using NUnit.Framework;
using Services;

namespace Test
{
    [TestFixture]
    public class QuoteManagerTests
    {
        private IQuoteManager quoteManager;
        private IQuote quote;

        [SetUp]
        public void Setup() => quoteManager = new QuoteManager();

        [Test]
        public void AddQuoteInitial_Test()
        {
            quote = BuildQuote("FRC", 177.38, 1000, DateTime.Today.AddDays(1));
            quoteManager.AddOrUpdateQuote(quote);

            var addedQuote = quoteManager.GetBestQuoteWithAvailableVolume("FRC");

            Assert.AreEqual(quote, addedQuote);


        }

        private IQuote BuildQuote(string symbol, double price, uint volume, DateTime expirationDate) => new Quote
        {
            Symbol = symbol,
            Price = price,
            AvailableVolume = volume,
            ExpirationDate = expirationDate
        };
    }
}