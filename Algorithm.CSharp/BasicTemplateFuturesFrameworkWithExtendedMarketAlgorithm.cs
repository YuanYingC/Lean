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
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Interfaces;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template futures framework algorithm uses framework components to define an algorithm
    /// that trades futures.
    /// </summary>
    public class BasicTemplateFuturesFrameworkWithExtendedMarketAlgorithm : BasicTemplateFuturesFrameworkAlgorithm
    {
        protected override bool ExtendedMarketHours => true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public override Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public override long DataPoints => 123378;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public override Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "2"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "-92.667%"},
            {"Drawdown", "5.000%"},
            {"Expectancy", "0"},
            {"Net Profit", "-3.314%"},
            {"Sharpe Ratio", "-6.303"},
            {"Probabilistic Sharpe Ratio", "9.333%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-1.465"},
            {"Beta", "0.312"},
            {"Annual Standard Deviation", "0.134"},
            {"Annual Variance", "0.018"},
            {"Information Ratio", "-14.77"},
            {"Tracking Error", "0.192"},
            {"Treynor Ratio", "-2.718"},
            {"Total Fees", "$4.62"},
            {"Estimated Strategy Capacity", "$52000000.00"},
            {"Lowest Capacity Asset", "GC VL5E74HP3EE5"},
            {"Fitness Score", "0.009"},
            {"Kelly Criterion Estimate", "-112.972"},
            {"Kelly Criterion Probability Value", "0.671"},
            {"Sortino Ratio", "-8.421"},
            {"Return Over Maximum Drawdown", "-35.2"},
            {"Portfolio Turnover", "0.548"},
            {"Total Insights Generated", "6"},
            {"Total Insights Closed", "5"},
            {"Total Insights Analysis Completed", "5"},
            {"Long Insight Count", "6"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$-96.12923"},
            {"Total Accumulated Estimated Alpha Value", "$-15.621"},
            {"Mean Population Estimated Insight Value", "$-3.1242"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "18ffd3a774c68da83d867e3b09e3e05d"}
        };
    }
}
