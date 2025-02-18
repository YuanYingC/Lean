/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Securities.Future;
using QuantConnect.Securities.Option;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This regression algorithm tests that we only receive the option chain for a single future contract
    /// in the option universe filter.
    /// </summary>
    public class AddFutureOptionSingleOptionChainSelectedInUniverseFilterRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private bool _invested;
        private bool _onDataReached;
        private bool _optionFilterRan;
        private readonly HashSet<Symbol> _symbolsReceived = new HashSet<Symbol>();
        private readonly HashSet<Symbol> _expectedSymbolsReceived = new HashSet<Symbol>();
        private readonly Dictionary<Symbol, List<QuoteBar>> _dataReceived = new Dictionary<Symbol, List<QuoteBar>>();

        private Future _es;

        public override void Initialize()
        {
            SetStartDate(2020, 1, 4);
            SetEndDate(2020, 1, 8);

            _es = AddFuture(Futures.Indices.SP500EMini, Resolution.Minute, Market.CME);
            _es.SetFilter((futureFilter) =>
            {
                return futureFilter.Expiration(0, 365).ExpirationCycle(new[] { 3, 6 });
            });

            AddFutureOption(_es.Symbol, optionContracts =>
            {
                _optionFilterRan = true;

                var expiry = new HashSet<DateTime>(optionContracts.Select(x => x.Underlying.ID.Date)).SingleOrDefault();
                // Cast to IEnumerable<Symbol> because OptionFilterContract overrides some LINQ operators like `Select` and `Where`
                // and cause it to mutate the underlying Symbol collection when using those operators.
                var symbol = new HashSet<Symbol>(((IEnumerable<Symbol>)optionContracts).Select(x => x.Underlying)).SingleOrDefault();

                if (expiry == null || symbol == null)
                {
                    throw new InvalidOperationException("Expected a single Option contract in the chain, found 0 contracts");
                }

                var enumerator = optionContracts.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    _expectedSymbolsReceived.Add(enumerator.Current);
                }

                return optionContracts;
            });
        }

        public override void OnData(Slice data)
        {
            if (!data.HasData)
            {
                return;
            }

            _onDataReached = true;

            var hasOptionQuoteBars = false;
            foreach (var qb in data.QuoteBars.Values)
            {
                if (qb.Symbol.SecurityType != SecurityType.FutureOption)
                {
                    continue;
                }

                hasOptionQuoteBars = true;

                _symbolsReceived.Add(qb.Symbol);
                if (!_dataReceived.ContainsKey(qb.Symbol))
                {
                    _dataReceived[qb.Symbol] = new List<QuoteBar>();
                }

                _dataReceived[qb.Symbol].Add(qb);
            }

            if (_invested || !hasOptionQuoteBars)
            {
                return;
            }

            foreach (var chain in data.OptionChains.Values)
            {
                var futureInvested = false;
                var optionInvested = false;

                foreach (var option in chain.Contracts.Keys)
                {
                    if (futureInvested && optionInvested)
                    {
                        return;
                    }

                    var future = option.Underlying;

                    if (!optionInvested && data.ContainsKey(option))
                    {
                        var optionContract = Securities[option];
                        var marginModel = optionContract.BuyingPowerModel as FuturesOptionsMarginModel;
                        if (marginModel.InitialIntradayMarginRequirement == 0
                            || marginModel.InitialOvernightMarginRequirement == 0
                            || marginModel.MaintenanceIntradayMarginRequirement == 0
                            || marginModel.MaintenanceOvernightMarginRequirement == 0)
                        {
                            throw new Exception("Unexpected margin requirements");
                        }

                        if (marginModel.GetInitialMarginRequirement(optionContract, 1) == 0)
                        {
                            throw new Exception("Unexpected Initial Margin requirement");
                        }
                        if (marginModel.GetMaintenanceMargin(optionContract) != 0)
                        {
                            throw new Exception("Unexpected Maintenance Margin requirement");
                        }

                        MarketOrder(option, 1);
                        _invested = true;
                        optionInvested = true;

                        if (marginModel.GetMaintenanceMargin(optionContract) == 0)
                        {
                            throw new Exception("Unexpected Maintenance Margin requirement");
                        }
                    }
                    if (!futureInvested && data.ContainsKey(future))
                    {
                        MarketOrder(future, 1);
                        _invested = true;
                        futureInvested = true;
                    }
                }
            }
        }

        public override void OnEndOfAlgorithm()
        {
            if (!_optionFilterRan)
            {
                throw new InvalidOperationException("Option chain filter was never ran");
            }
            if (!_onDataReached)
            {
                throw new Exception("OnData() was never called.");
            }
            if (_symbolsReceived.Count != _expectedSymbolsReceived.Count)
            {
                throw new AggregateException($"Expected {_expectedSymbolsReceived.Count} option contracts Symbols, found {_symbolsReceived.Count}");
            }

            var missingSymbols = new List<Symbol>();
            foreach (var expectedSymbol in _expectedSymbolsReceived)
            {
                if (!_symbolsReceived.Contains(expectedSymbol))
                {
                    missingSymbols.Add(expectedSymbol);
                }
            }

            if (missingSymbols.Count > 0)
            {
                throw new Exception($"Symbols: \"{string.Join(", ", missingSymbols)}\" were not found in OnData");
            }

            foreach (var expectedSymbol in _expectedSymbolsReceived)
            {
                var data = _dataReceived[expectedSymbol];
                var nonDupeDataCount = data.Select(x =>
                {
                    x.EndTime = default(DateTime);
                    return x;
                }).Distinct().Count();

                if (nonDupeDataCount < 1000)
                {
                    throw new Exception($"Received too few data points. Expected >=1000, found {nonDupeDataCount} for {expectedSymbol}");
                }
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 588142;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "2"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "347.065%"},
            {"Drawdown", "0.400%"},
            {"Expectancy", "0"},
            {"Net Profit", "1.951%"},
            {"Sharpe Ratio", "15.548"},
            {"Probabilistic Sharpe Ratio", "95.977%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "1.885"},
            {"Beta", "1.066"},
            {"Annual Standard Deviation", "0.155"},
            {"Annual Variance", "0.024"},
            {"Information Ratio", "13.528"},
            {"Tracking Error", "0.142"},
            {"Treynor Ratio", "2.258"},
            {"Total Fees", "$3.57"},
            {"Estimated Strategy Capacity", "$760000.00"},
            {"Lowest Capacity Asset", "ES XCZJLDQX2SRO|ES XCZJLC9NOB29"},
            {"Fitness Score", "0.403"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "79228162514264337593543950335"},
            {"Return Over Maximum Drawdown", "79228162514264337593543950335"},
            {"Portfolio Turnover", "0.403"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$0"},
            {"Total Accumulated Estimated Alpha Value", "$0"},
            {"Mean Population Estimated Insight Value", "$0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "738240babf741f1bf79f85ea5026ec4c"}
        };
    }
}
