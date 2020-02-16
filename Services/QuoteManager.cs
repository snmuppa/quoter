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
        // TODO: These messages can be a part of the resource file and localized
        private const string NO_ACTIVE_QUOTE_MESSAGE = "No active quote found for this quote to update.";
        private const string EXPIRED_QUOTE_MESSAGE = "Cannot add/update an expired quote";
        private const string ZERO_REQUESTED_VOLUME_MESSAGE = "Requested valume cannot be 0.";
        private const string NULL_QUOTE_ARGUMENT_MESSAGE = "Quote cannot be null.";
        private const string NULL_SYMBOL_MESSAGE = "Symbol cannot be null.";
        private const string EMPTY_GUID_MESSAGE = "Id cannot be an empty GUID.";

        // TODO: Implement logging 

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
            // TODO: Concurrency Notes - We execute this action only when the removalAll for this symbol is not in progress
            //                           When the RemoveQuote for this GUID is not in progress
            //                           When the Trade Execution this symbol is not in progress
            //                           When the Add or Update for this GUID is not in progress

            // Extra notes: Always the locking is for a given stock SYMBOL / GUID depending on the operation

            // The time complexity if proportional to log2(no. of items for that quote's symbol) 

            if (quote == null)
            {
                throw new ArgumentNullException(NULL_QUOTE_ARGUMENT_MESSAGE);
            }

            if (IsExpiredQuote(quote))
            {
                throw new ArgumentException(EXPIRED_QUOTE_MESSAGE);
            }

            if (quote.Id != null)
            {
                if (!quoteBook.ContainsKey(quote.Id))
                {
                    throw new KeyNotFoundException(NO_ACTIVE_QUOTE_MESSAGE);
                }

                UpdateQuote(quote);
            }
            else
            {
                AddQuote(Guid.NewGuid(), quote);
            }
        }

        /// <inheritdoc/>
        public ITradeResult ExecuteTrade(string symbol, uint volumeRequested)
        {
            // TODO: Concurrency Notes - We execute this action only when the removalAll for this symbol is not in progress
            //                           When the RemoveQuote for this GUID is not in progress
            //                           When the Trade Execution this symbol is not in progress
            //                           When the Add or Update for this GUID is not in progress

            // Extra notes: Always the locking is for a given stock SYMBOL/ GUID depending on the operation

            if (symbol == null)
            {
                throw new ArgumentNullException(NULL_SYMBOL_MESSAGE);
            }

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

                // calculating remaining volume to execute trade from next quote
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
                VolumeWeightedAveragePrice = GetWeightedAveragePrice(volumeExecuted, tradeTotalPrice)
            };
        }

        /// <inheritdoc/>
        public IQuote GetBestQuoteWithAvailableVolume(string symbol)
        {
            // If this is an eventually consistent GET, we don't have to wait for Add, Update, Execute Trade and Removal operations

            // In most cases  this is a very fast constant time operations
            // In the worst case this is a linear operation proportional to the count of Quotes for a Symbol

            if (symbol == null)
            {
                throw new ArgumentNullException(NULL_SYMBOL_MESSAGE);
            }

            IQuote quote = null;

            if (symbolsQuoteBook.ContainsKey(symbol))
            {
                IPriorityQueue<IQuote, double> pricingPriorityQueue = symbolsQuoteBook[symbol];

                int maxQueueIterations = pricingPriorityQueue.Count;

                 // This is implementation and is faster as this removes invalid entries i.e. any quote that has 0 volume or expired date
                 // This is an assumption i have made
                while (pricingPriorityQueue.Count != 0 && maxQueueIterations != 0)
                {
                    var firstQuote = pricingPriorityQueue.First;
                    if (IsValidQuote(firstQuote))
                    {
                        //  ensure that the symbols book entry is updated as this loop might have updated the pricing priority queue
                        symbolsQuoteBook[symbol] = pricingPriorityQueue;
                        quote = firstQuote;
                        break;
                    }
                    else
                    {
                        // the quote with the lowest price point has expired, so remove from the priority queue and the quote book
                        // this is an optimization to keep the pricing priority queue hot for fast fetches

                        // Assumption - here I made an assumption that we can delete the expired quotes
                        pricingPriorityQueue.Dequeue(); // dequeue removes the  first item, time comlexity here O(1)
                        if (pricingPriorityQueue.Count == 0) {
                            symbolsQuoteBook.Remove(firstQuote.Symbol);
                        }
                        quoteBook.Remove(firstQuote.Id);
                    }

                    --maxQueueIterations;
                }
            }

            return quote;
        }

        /// <inheritdoc/>
        public void RemoveAllQuotes(string symbol)
        {
            // TODO: Concurrency Notes - We execute this action only when the removalAll for this symbol is not in progress
            //                           When the Trade Execution this symbol is not in progress
            //                           When the Add or Update for this GUID is not in progress
            //      We don't have to worry about the RemoveQuote as in the worst  case this may  try to remove the same item from the same
            //      Symbol (in other  words it is an idempotent request)

            // Extra notes: Always the locking is for a given stock SYMBOL / GUID depending on the operation

            // This is a linear time operation propertional to the no.of Quotes for a Symbol

            if (symbol == null)
            {
                throw new ArgumentNullException(NULL_SYMBOL_MESSAGE);
            }

            if (!symbolsQuoteBook.ContainsKey(symbol))
            {
                throw new KeyNotFoundException(NO_ACTIVE_QUOTE_MESSAGE);
            }

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

        /// <inheritdoc/>
        public void RemoveQuote(Guid id)
        {
            // TODO: Concurrency Notes - We execute this action only 
            //                           When the Trade Execution this symbol is not in progress
            //                           When the Add or Update for this GUID is not in progress

            // Extra notes: Always the locking is for a given stock SYMBOL / GUID depending on the operation

            // This is a constant time operation


            if(id.Equals(Guid.Empty))
            {
                throw new ArgumentException(EMPTY_GUID_MESSAGE);
            }

            if (!quoteBook.ContainsKey(id))
            {
                throw new KeyNotFoundException(NO_ACTIVE_QUOTE_MESSAGE);
            }

            IQuote quote = quoteBook[id];
            IPriorityQueue<IQuote, double> pricingPriorityQueue = symbolsQuoteBook[quote.Symbol];

            pricingPriorityQueue.Remove(quote);

            if (pricingPriorityQueue.Count == 0)
            {
                symbolsQuoteBook.Remove(quote.Symbol);
            }
            else
            {
                symbolsQuoteBook[quote.Symbol] = pricingPriorityQueue;
            }

            quoteBook.Remove(id);
        }

        /// <summary>
        /// Validates if a give quote is valid or not
        /// </summary>
        /// <param name="quote"></param>
        /// <returns>True or false</returns>
        private bool IsValidQuote(IQuote quote)
        {
            return HasAvailableVolume(quote) && !IsExpiredQuote(quote);
        }

        /// <summary>
        /// Checks if the quote is expired or not
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        private bool IsExpiredQuote(IQuote quote)
        {
            return DateTime.Now > quote.ExpirationDate;
        }

        /// <summary>
        /// Checks if a given quote has a valid number of available volume
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        private bool HasAvailableVolume(IQuote quote)
        {
            return quote.AvailableVolume > 0;
        }

        /// <summary>
        /// Updates an exisiting quote
        /// </summary>
        /// <param name="quote"></param>
        /// <exception cref="ArgumentException">Throws an exception if the quote is not found or if it's an invalid quote</exception>
        private void UpdateQuote(IQuote quote)
        {
            // Here I took liberty of updating the  baseQuote's date as well from the quote which is not called out in the requirement
            // This is because I thought we may in some cases where we want to extend or decrease the expiry time

            // This type of Remove and Add opertions keeps the integrity of the 2 internal data structures - quoteBook and the symbolsQuoteBook
            RemoveQuote((Guid)quote.Id);

            AddQuote(quote);
        }

        /// <summary>
        /// Adds new quote
        /// </summary>
        /// <param name="quote"></param>
        private void AddQuote(IQuote quote)
        {
            // The time complexity if proportional to log2(no. of items for that quote's symbol) 

            IPriorityQueue<IQuote, double> pricingPriorityQueue;

            if (!symbolsQuoteBook.ContainsKey(quote.Symbol))
            {
                pricingPriorityQueue = new SimplePriorityQueue<IQuote, double>();   
            }
            else
            {
                pricingPriorityQueue = symbolsQuoteBook[quote.Symbol];
            }

            pricingPriorityQueue.Enqueue(quote, quote.Price);
            symbolsQuoteBook[quote.Symbol] = pricingPriorityQueue;

            quoteBook[quote.Id] = quote;
        }

        
        private void AddQuote(Guid id, IQuote quote)
        {
            quote.Id = id;
            AddQuote(quote);
        }

        /// <summary>
        /// Gets the weighted average price for a given volume
        /// </summary>
        /// <param name="volumeExecuted"></param>
        /// <param name="tradeTotalPrice"></param>
        /// <returns></returns>
        private double GetWeightedAveragePrice(uint volumeExecuted, double tradeTotalPrice)
        {
            return (volumeExecuted != 0) ? tradeTotalPrice / volumeExecuted : 0;
        }
    }
}
