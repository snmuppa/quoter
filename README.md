
# Project - *Quote Manager*

**QUOTER** Is a console based .NET core application to transact financial stock quotes.

Time spent: **5** hours spent in total

## User Stories

The following **required** functionality is completed:

* [x] Add or update the quote (specified by Id) in symbol's book.
  * [x] If quote is new or no longer in the book, add it.
  * [x] Otherwise update it to match the given **price, volume, and symbol**.
* [x] Remove quote by Id, if quote is no longer in symbol's book do nothing.
* [x] Remove all quotes on the specified symbol's book.
* [x] Get the best (i.e. lowest) price in the symbol's book that still has available volume.
  * [x] If there is no quote on the symbol's book with available volume, return null.
  * [x] Otherwise return a Quote object with all the fields set.
  * [x] Don't return any quote which is past its expiration time, or has been removed.
* [x] Request that a trade be executed.
  * [x] Do not trade expired quotes.

The following **optional** features are implemented:

* [x] High performance fetching using priority queues.
* [x] Comments on concurrency issues and how it can be resolved.
* [X] Time complexity of each operation is written explained in comments for each API method

## Video Walkthrough
Here's a walkthrough of implemented user stories:

<img src='' title='Video Walkthrough' width='' alt='Video Walkthrough' />

GIF created with [LiceCap](http://www.cockos.com/licecap/).

## Notes
With this implementation concurrency is an issue. We can solve the concurrency issue by using various techniques.
One of the technique is to leverage an Action Queue/ Operations Queue. The idea of an action queue is that all the actions such as
* AddOrUpdateQuote (AQ/ UQ)
* RemoveQuote (RQ)
* RemoveAllQuotes (RAQ)
* GetBestQuoteWithAvailableVolume (GB)
* ExecuteTrade (ET)
Essentially if we queue actions like the following GB (Get) -> ET (Execute Trade) -> RAQ(Remove All) and process them accordingly

### Here are the rules that we will have to follow get a deterministic behavior on each trade:
#### Add or Update Quote
Checks for not in progress:
1.  Trade execution for this Symbol.
2.	Remove all for this Symbol.
3.	Remove Quote with this GUID is not in progress.
4.	Add or Update for this GUID is in progress.

#### Delete Quote
Checks for not in progress:
1.	Trade execution for this Symbol
2.	Add or Update for this GUID is in progress. 

#### Remove All with Symbol
Checks for not in progress:
1.	Trade execution for this Symbol
2.	Add or Update for this GUID is in progress.

#### Get Best Price for given a Symbol
If this is an eventually consistent GET, we don't have to wait for Add, Update, Execute Trade and Removal operations

#### Execute trade
Checks for not in progress:
1.	Trade Execution for this Symbol
2.	Add or Update for this GUID is in progress

## Open-source libraries used

- [NUnit](https://github.com/nunit/nunit) - NUnit 3 Framework.
- [Priority Queue](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp) - A C# priority queue optimized for pathfinding applications scrolling.

## License

    Copyright [2020] [Sai N Muppa]

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
