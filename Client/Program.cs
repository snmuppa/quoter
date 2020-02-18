using System;
using System.ComponentModel;
using Model;
using Services;

namespace Client
{
    class Program
    {
        /// <summary>
        /// Delegate definition for validating the user input before accepting
        /// </summary>
        /// <param name="input">Console input string</param>
        /// <returns></returns>
        delegate bool ValidateInput(string input);

        static void Main(string[] args)
        {
            string line;
            Console.WriteLine("Enter one or more lines of text (press CTRL+Z to exit):");
            Console.WriteLine("Enter AQ for Adding to the Quote");
            Console.WriteLine("Enter UQ for Updating a Quote");
            Console.WriteLine("Enter RQ for Removing a single Quote By Id");
            Console.WriteLine("Enter RAQ for Removing all quotes by a Symbol");
            Console.WriteLine("Enter GB to get the best quote with available volume");
            Console.WriteLine("Enter ET to execute a trade");
            Console.WriteLine("Enter RT to run  tests and exit");
            Console.WriteLine("Enter EXIT to run  tests and quit the application");

            Console.WriteLine();

            IQuoteManager quoteManager = new QuoteManager();

            do
            {
                Console.Write("Enter a quote manager action: ");
                line = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    switch (line.ToUpper())
                    {
                        case "RT":
                            RunTests();
                            Environment.Exit(0);
                            return;

                        case "AQ":
                            AddQuote(quoteManager);
                            break;

                        case "UQ":
                            UpdatedQuote(quoteManager);
                            break;

                        case "RQ":
                            RemoveQuote(quoteManager);
                            break;

                        case "RAQ":
                            RemoveAllQuotes(quoteManager);
                            break;

                        case "GB":
                            GetBestQuote(quoteManager);
                            break;

                        case "ET":
                            ExecuteTrade(quoteManager);
                        break;

                        case "EXIT":
                            Environment.Exit(0);
                            break;
                    }
                }

            } while (line != null);
        }

        /// <summary>
        /// Generates the interactive console input for executing a trade
        /// </summary>
        /// <param name="quoteManager"></param>
        private static void ExecuteTrade(IQuoteManager quoteManager)
        {
            Console.WriteLine("Entering get execute trade operation ...");

            var symbol = GetInput<string>(IsValidSymbolInput, "Enter a valid symbol (e.g. HIGH): ");

            var volumeRequested = GetInput<uint>(IsValidVolumeInput, "Enter a valid requesting volume (e.g. 100): ");

            var tradeResult = quoteManager.ExecuteTrade(symbol, volumeRequested);

            Console.WriteLine("Finished executing the trade, here is the summary of the trade: {0}", tradeResult.ToString());
        }

        /// <summary>
        /// Generates the interactive console input for getting a best quote
        /// </summary>
        /// <param name="quoteManager"></param>
        private static void GetBestQuote(IQuoteManager quoteManager)
        {
            Console.WriteLine("Entering get best quote operation ...");

            var symbol = GetInput<string>(IsValidSymbolInput, "Enter a valid symbol (e.g. HIGH): ");

            var bestQuote = quoteManager.GetBestQuoteWithAvailableVolume(symbol);

            if (bestQuote != null)
            {
                Console.WriteLine("Best  quote for {0} : {1}.", symbol, bestQuote.ToString());
            }
            else
            {
                Console.WriteLine("Not active best quotes found, try another symbol.");
            }
        }

        /// <summary>
        /// Generates the interactive console input for removing all the quotes for a Symbol
        /// </summary>
        /// <param name="quoteManager"></param>
        private static void RemoveAllQuotes(IQuoteManager quoteManager)
        {
            Console.WriteLine("Entering removal all quotes for a symbol operation ...");

            var symbol = GetInput<string>(IsValidSymbolInput, "Enter a valid symbol (e.g. HIGH): ");

            try
            {
                quoteManager.RemoveAllQuotes(symbol);

                Console.WriteLine("Finished removing all the quotes for the symbol: {0}", symbol);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Generates the interactive console input for removing a quote by Id
        /// </summary>
        /// <param name="quoteManager"></param>
        private static void RemoveQuote(IQuoteManager quoteManager)
        {
            Console.WriteLine("Entering quote removal by id ...");

            var id = GetInput<Guid>(IsValidGuidInput, "Enter a valid quote Id (e.g. 1436fd88-5366-4a72-893d-58d7aefe3b1e): ");

            try
            {
                quoteManager.RemoveQuote(id);

                Console.WriteLine("Finished removing the quote for id: {0}", id);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Generates the interactive console input for updating a quote
        /// </summary>
        /// <param name="quoteManager"></param>
        private static void UpdatedQuote(IQuoteManager quoteManager)
        {
            Console.WriteLine("Entering update quote operation ...");

            var id = GetInput<Guid>(IsValidGuidInput, "Enter a valid id (e.g. 1436fd88-5366-4a72-893d-58d7aefe3b1e): ");

            var symbol = GetInput<string>(IsValidSymbolInput, "Enter a valid symbol (e.g. HIGH): ");

            var price = GetInput<double>(IsValidPriceInput, "Enter a valid price (e.g. 117.38): ");

            var availableVolume = GetInput<uint>(IsValidVolumeInput, "Enter a valid volume (e.g. 1000 (greater than 0)): ");

            var expirationDate = GetInput<DateTime>(IsValidNullableExpirationDateInput, "Enter an valid expiration date (e.g. 02/18/2020 (Greater than current time)) or leave empty: ");

            var quote = new Quote
            {
                Id = id,
                Symbol = symbol,
                Price = price,
                AvailableVolume = availableVolume,
                ExpirationDate = expirationDate
            };

            try
            {
                quoteManager.AddOrUpdateQuote(quote);

                Console.WriteLine("Updated the quote: {0}", quote.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Generates the interactive console input for adding a new quote
        /// </summary>
        /// <param name="quoteManager"></param>
        private static void AddQuote(IQuoteManager quoteManager)
        {
            Console.WriteLine("Entering add Add quote operation ...");

            var symbol = GetInput<string>(IsValidSymbolInput, "Enter a valid symbol (e.g. HIGH): ");

            var price = GetInput<double>(IsValidPriceInput, "Enter a valid price (e.g. 117.38): ");

            var availableVolume = GetInput<uint>(IsValidVolumeInput, "Enter a valid volume (e.g. 1000 (greater than 0)): ");

            var expirationDate = GetInput<DateTime>(IsValidExpirationDateInput, "Enter an valid expiration date (e.g. 02/18/2020 (Greater than current time)): ");

            var quote = new Quote
            {
                Symbol = symbol,
                Price = price,
                AvailableVolume = availableVolume,
                ExpirationDate = expirationDate
            };

            try
            {
                quoteManager.AddOrUpdateQuote(quote);

                Console.WriteLine("Added a new quote: {0}", quote.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Generic method to accept input from console and return a strong type, by converting it to the requested type <T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validInput"></param>
        /// <param name="inputTextMessage"></param>
        /// <returns></returns>
        private static T GetInput<T>(ValidateInput validInput, string inputTextMessage)
        {
            string input;
            do
            {
                Console.Write(inputTextMessage);
                input = Console.ReadLine();
            } while (!validInput(input));

            return Convert<T>(input);
        }

        /// <summary>
        /// Generic type converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private static T Convert<T>(string input)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(input);
                }
                return default;
            }
            catch (NotSupportedException)
            {
                return default;
            }
        }

        private static bool IsValidSymbolInput(string input)
        {
            return !IsNullOrWhiteSpace(input);
        }

        private static bool IsNullOrWhiteSpace(string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        private static bool IsValidPriceInput(string input)
        {
            return !IsNullOrWhiteSpace(input) && double.TryParse(input, out _);
        }

        private static bool IsValidVolumeInput(string input)
        {
            return !IsNullOrWhiteSpace(input) && uint.TryParse(input, out _);
        }

        private static bool IsValidExpirationDateInput(string input)
        {
            return !IsNullOrWhiteSpace(input) && DateTime.TryParse(input, out DateTime dateTime) && dateTime > DateTime.Now;
        }

        private static bool IsValidNullableExpirationDateInput(string input)
        {
            return IsNullOrWhiteSpace(input) || (IsValidExpirationDateInput(input));
        }

        private static bool IsValidGuidInput(string input)
        {
            return !IsNullOrWhiteSpace(input) && Guid.TryParse(input, out _);
        }

        #region TEST BED - NO REVIEW

        /// <summary>
        /// Runs all various tests on QUOTE MANAGER to ensure it is functionaing as per the business requirements.
        /// </summary>
        private static void RunTests()
        {
            // TODO: Move these to the UNIT Test project and code refactor this giant test method

            // NOTES: This is just for the sake of integration run to prove the algorithm is working, please do not review this section critically :)
            Console.WriteLine("Running Tests on QuoteManager operations ...");

            IQuoteManager quoteManager = new QuoteManager();

            Console.WriteLine("Fetching on an empty quote manager ...");

            IQuote quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            if (quote == null)
            {
                Console.WriteLine("Correctly returning null when not symbols are present on GET");
            }

            Console.WriteLine("Executing trade on an empty quote manager ...");

            ITradeResult executedata = quoteManager.ExecuteTrade("A", 160);

            Console.WriteLine("Trade Result: {0}", executedata.ToString());

            Console.WriteLine("Adding a quote for symbol HIGH ...");

            quoteManager.AddOrUpdateQuote(BuildQuote("HIGH", 177.38, 1000, DateTime.Today.AddDays(2)));

            quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            Console.WriteLine("Fetching from quote manager for symbol HIGH ...");

            Console.WriteLine("Best Quote: {0}", quote.ToString());

            Console.WriteLine("Adding a quote with expired date time in the past ...");

            try
            {
                quoteManager.AddOrUpdateQuote(BuildQuote("RANDOM", 12.90, 500, DateTime.Today.AddDays(-1))); // this will throw exception as this is adding an expired quote
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Fetching from quote manager for symbol HIGH ...");

            quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            Console.WriteLine("Best Quote: {0}", quote.ToString());

            Console.WriteLine("Executing a trade for symbol HIGH ...");

            executedata = quoteManager.ExecuteTrade("HIGH", 980);

            Console.WriteLine("Trade Result: {0}", executedata.ToString());

            Console.WriteLine("Fetching from quote manager for symbol HIGH ...");

            Console.WriteLine("Updating an existing quote for symbol HIGH ...");

            quoteManager.AddOrUpdateQuote(GetUpdatedQuote("HIGH", 1000, 187.98, quote));

            Console.WriteLine("Fetching from quote manager for symbol HIGH ...");

            quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            Console.WriteLine("Best Quote: {0}", quote.ToString());

            quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            Console.WriteLine("Best Quote: {0}", quote.ToString());

            Console.WriteLine("Executing a trade for symbol HIGH ...");

            executedata = quoteManager.ExecuteTrade("HIGH", 480);

            Console.WriteLine("Trade Result: {0}", executedata.ToString());

            Console.WriteLine("Fetching from quote manager for symbol HIGH ...");

            quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            if (quote == null)
            {
                Console.WriteLine("No quote that matches the criteria.");
            }

            Console.WriteLine("Executing a trade for symbol HIGH ...");

            executedata = quoteManager.ExecuteTrade("HIGH", 600);

            Console.WriteLine("Trade Result: {0}", executedata.ToString());

            Console.WriteLine("Adding a new quote for symbol HIGH ...");

            quoteManager.AddOrUpdateQuote(BuildQuote("HIGH", 177.38, 1000, DateTime.Today.AddDays(1)));

            Console.WriteLine("Fetching from quote manager for symbol HIGH ...");

            quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            Console.WriteLine("Best Quote: {0}", quote.ToString());

            Console.WriteLine("Removing all quotes from quote manager for symbol HIGH ...");

            quoteManager.RemoveAllQuotes("HIGH");

            Console.WriteLine("Fetching from quote manager for symbol HIGH ...");

            quote = quoteManager.GetBestQuoteWithAvailableVolume("HIGH");

            if (quote == null)
            {
                Console.WriteLine("No quote items available for HIGH.");
            }

            Console.WriteLine("Adding a quote for symbol AAPL ...");

            quoteManager.AddOrUpdateQuote(BuildQuote("AAPL", 225.90, 1000, DateTime.Today.AddDays(2)));

            Console.WriteLine("Fetching from quote manager for symbol AAPL ...");

            quote = quoteManager.GetBestQuoteWithAvailableVolume("AAPL");

            Console.WriteLine("Best Quote Id for AAPL: {0}, price: {1}", quote.Id, quote.Price);

            Console.WriteLine("Removing a quote by GUID for symbol AAPL ...");

            quoteManager.RemoveQuote((Guid)quote.Id);


            Console.WriteLine("Removing a non-existent quote by GUID ...");

            try
            {
                quoteManager.RemoveQuote((Guid)quote.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Fetching from quote manager for symbol AAPL ...");

            quote = quoteManager.GetBestQuoteWithAvailableVolume("AAPL");

            if (quote == null)
            {
                Console.WriteLine("No quote items available for AAPL.");
            }

            Console.WriteLine("Removing an quote by Empty GUID ...");

            try
            {
                quoteManager.RemoveQuote(Guid.Empty);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Exiting the  App");
        }

        private static IQuote BuildQuote(string symbol, double price, uint volume, DateTime expirationDate) => new Quote
        {
            Symbol = symbol,
            Price = price,
            AvailableVolume = volume,
            ExpirationDate = expirationDate
        };

        private static IQuote GetUpdatedQuote(string symbol, uint volume, double price, IQuote quote)
        {
            quote.Price = price;
            quote.Symbol = symbol;
            quote.AvailableVolume = volume;

            return quote;
        }

        #endregion
    }
}
