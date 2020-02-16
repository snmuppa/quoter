using System;
using System.Collections.Generic;
using Model;
using Priority_Queue;

namespace Services
{
    /// <summary>
    /// Implementation to the <see cref="IQuote" />
    /// </summary>
    public class QuoteManager : IQuoteManager
    {
        private const string NO_ACTIVE_QUOTE_MESSAGE = "No active quote found for this quote to update.";
        private const string EXPIRED_QUOTE_MESSAGE = "Cannot add an expired quote";
        private const string ZERO_REQUESTED_VOLUME_MESSAGE = "Requested valume cannot be 0.";
        private const string NULL_QUOTE_ARGUMENT_MESSAGE = "Quote cannot be null.";

        /// <summary>
        /// Quote book dictionary to quickly look up the quote by GUID
        /// </summary>
        private readonly IDictionary<Guid?, IQuote> quoteBook = new Dictionary<Guid?, IQuote>();

        /// <summary>
        /// Quote book dictionary to quickly look up the quote by SYMBOL and sort the quotes by the pricing priority
        /// </summary>
        private readonly IDictionary<string, IPriorityQueue<IQuote, double>> symbolsQuoteBook = new Dictionary<string, IPriorityQueue<IQuote, double>>();

        /// <inheritdoc/>
        public void AddOrUpdateQuote(IQuote quote)
        {
            if (quote == null)
            {
                throw new ArgumentNullException(NULL_QUOTE_ARGUMENT_MESSAGE);
            }

            if (quote.Id != null)
            {
                if (!quoteBook.ContainsKey(quote.Id))
                {
                    throw new ArgumentException(NO_ACTIVE_QUOTE_MESSAGE);
                }

                UpdateQuote(quote);
            }
            else
            {
                if (!isUnExpiredQuote(quote))
                {
                    throw new ArgumentException(EXPIRED_QUOTE_MESSAGE);
                }
                AddQuote(quote);
            }
        }

        /// <inheritdoc/>
        public ITradeResult ExecuteTrade(string symbol, uint volumeRequested)
        {
            if (volumeRequested == 0)
            {
                throw new ArgumentException(ZERO_REQUESTED_VOLUME_MESSAGE);
            }

            var remainingVolumeRequested = volumeRequested;
            uint volumeExecuted = 0;
            double tradeTotalPrice = 0;

            while (remainingVolumeRequested != 0)
            {
                IQuote bestQuote = GetBestQuoteWithAvailableVolume(symbol);
                if (bestQuote == null)
                {
                    break;
                }

                var quoteVolume = bestQuote.AvailableVolume;

                // update avaialble volume to the volumes used.
                // Adding following if conditions since uint does allow negative, we checking the numbers
                bestQuote.AvailableVolume = (remainingVolumeRequested < quoteVolume) ? quoteVolume - remainingVolumeRequested : 0;

                // volume used per quote. using this for calculating the VolumeWeightedAveragePrice
                var quoteVolumeUsed = (bestQuote.AvailableVolume == 0) ? quoteVolume : (remainingVolumeRequested);

                // calculating remaining volume to exectue trade from next quote
                remainingVolumeRequested = (remainingVolumeRequested < quoteVolume) ? 0 : remainingVolumeRequested - quoteVolume;

                // updating traderesult to update the total volume executed
                volumeExecuted = volumeRequested - remainingVolumeRequested;

                // calculating total trade price which will be used to VolumeWeightedAveragePrice - (price*volume used)
                tradeTotalPrice += (bestQuote.Price * Convert.ToDouble(quoteVolumeUsed));

                // Updating quote with volume used
                UpdateQuote(bestQuote);
            }

            return new TradeResult
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                VolumeExecuted = volumeExecuted,
                VolumeRequested = volumeRequested,
                VolumeWeightedAveragePrice = getWeightedAveragePrice(volumeExecuted, tradeTotalPrice)
            };
        }

        /// <inheritdoc/>
        public IQuote GetBestQuoteWithAvailableVolume(string symbol)
        {
            if (symbolsQuoteBook.ContainsKey(symbol))
            {
                IPriorityQueue<IQuote, double> pricingPriorityQueue = symbolsQuoteBook[symbol];

                while (pricingPriorityQueue.Count != 0)
                {
                    IQuote quote = pricingPriorityQueue.First;
                    if (isValidQuote(quote))
                    {
                        //  ensure that the symbols book entry is updated as this loop might have updated the pricing priority queue
                        symbolsQuoteBook[symbol] = pricingPriorityQueue;

                        return quote;
                    }

                    // the quote with the lowest price point is invalid, so remove from the priority queue and the quote book
                    // this is an optimization to keep the pricing priority queue hot for fast fetches
                    quote = pricingPriorityQueue.Dequeue();
                    quoteBook.Remove(quote.Id);
                }

                // if the execution reaches here, that means there are no valid Quotes available for a symbol, so remove the symbol from the dictionary
                symbolsQuoteBook.Remove(symbol);
            }

            return null;
        }

        /// <inheritdoc/>
        public void RemoveAllQuotes(string symbol)
        {
            if (symbolsQuoteBook.ContainsKey(symbol))
            {
                IPriorityQueue<IQuote, double> pricingPriorityQueue = symbolsQuoteBook[symbol];
                while (pricingPriorityQueue.Count != 0)
                {
                    // removes the items from the priority queue
                    IQuote quote = pricingPriorityQueue.Dequeue();

                    //removes the quote from the quote book
                    quoteBook.Remove(quote.Id);
                }

                // finally removes the symbols quote book entry
                symbolsQuoteBook.Remove(symbol);
            }
        }

        /// <inheritdoc/>
        public void RemoveQuote(Guid id)
        {
            if (quoteBook.ContainsKey(id))
            {
                IQuote quote = quoteBook[id];
                IPriorityQueue<IQuote, double> pricingPriorityQueue = symbolsQuoteBook[quote.Symbol];

                pricingPriorityQueue.Remove(quote);
                symbolsQuoteBook[quote.Symbol] = pricingPriorityQueue;

                quoteBook.Remove(id);
            }
        }

        /// <summary>
        /// Validates if a give quote is valid or not
        /// </summary>
        /// <param name="quote"></param>
        /// <returns>True or false</returns>
        private bool isValidQuote(IQuote quote)
        {
            return hasAvailableVolume(quote) && isUnExpiredQuote(quote);
        }

        /// <summary>
        /// Checks if the quote is expired or not
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        private bool isUnExpiredQuote(IQuote quote)
        {
            return DateTime.Now < quote.ExpirationDate;
        }

        /// <summary>
        /// Checks if a given quote has a valid number of available volume
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        private bool hasAvailableVolume(IQuote quote)
        {
            return quote.AvailableVolume > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        /// <exception cref="ArgumentException">Throws an exception if the quote is not found or if it's an invalid quote</exception>
        private void UpdateQuote(IQuote quote)
        {
            IPriorityQueue<IQuote, double> pricingPriorityQueue = symbolsQuoteBook[quote.Symbol];
            pricingPriorityQueue.UpdatePriority(quote, quote.Price);
            symbolsQuoteBook[quote.Symbol] = pricingPriorityQueue;

            quoteBook[quote.Id] = quote;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        private void RemoveQuote(IQuote quote)
        {
            IPriorityQueue<IQuote, double> pricingPriorityQueue = symbolsQuoteBook[quote.Symbol];
            pricingPriorityQueue.Remove(quote);

            symbolsQuoteBook[quote.Symbol] = pricingPriorityQueue;

            quoteBook.Remove(quote.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quote"></param>
        private void AddQuote(IQuote quote)
        {
            quote.Id = Guid.NewGuid();

            IPriorityQueue<IQuote, double> pricingPriorityQueue;

            if (!symbolsQuoteBook.ContainsKey(quote.Symbol))
            {
                pricingPriorityQueue = new SimplePriorityQueue<IQuote, double>();
                pricingPriorityQueue.Enqueue(quote, quote.Price);
                symbolsQuoteBook.Add(quote.Symbol, pricingPriorityQueue);
            }
            else
            {
                pricingPriorityQueue = symbolsQuoteBook[quote.Symbol];
                pricingPriorityQueue.Enqueue(quote, quote.Price);
                symbolsQuoteBook[quote.Symbol] = pricingPriorityQueue;
            }

            quoteBook.Add(quote.Id, quote);
        }

        /// <summary>
        /// Gets the weighted average price for a given volume
        /// </summary>
        /// <param name="volumeExecuted"></param>
        /// <param name="tradeTotalPrice"></param>
        /// <returns></returns>
        private double getWeightedAveragePrice(uint volumeExecuted, double tradeTotalPrice)
        {
            return (volumeExecuted != 0) ? tradeTotalPrice / volumeExecuted : 0;
        }
    }
}
