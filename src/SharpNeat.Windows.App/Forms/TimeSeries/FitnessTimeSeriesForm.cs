﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.EvolutionAlgorithm;
using ZedGraph;
using static SharpNeat.Windows.App.Forms.ZedGraphUtils;

namespace SharpNeat.Windows.App.Forms.TimeSeries;

public class FitnessTimeSeriesForm : StatsGraphForm
{
    const int __HistoryLength = 1_000;
    readonly RollingPointPairList _bestPpl;
    readonly RollingPointPairList _meanPpl;

    #region Constructor

    public FitnessTimeSeriesForm()
        : base("Fitness (Best and Mean)", "Generation", "Fitness", null)
    {
        _bestPpl = new RollingPointPairList(__HistoryLength);
        LineItem lineItem = _graphPane.AddCurve("Best", _bestPpl, Color.FromArgb(0xff, 0x1d, 0x26), SymbolType.None);
        ApplyLineStyle(lineItem);

        _meanPpl = new RollingPointPairList(__HistoryLength);
        lineItem = _graphPane.AddCurve("Mean", _meanPpl, Color.FromArgb(0xed, 0x9c, 0x96), SymbolType.None);
        ApplyLineStyle(lineItem);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update the time series data.
    /// </summary>
    /// <param name="eaStats">Evolution algorithm statistics object.</param>
    /// <param name="popStats">Population statistics object.</param>
    public override void UpdateData(
        EvolutionAlgorithmStatistics eaStats,
        PopulationStatistics popStats)
    {
        _bestPpl.Add(eaStats.Generation, popStats.BestFitness.PrimaryFitness);
        _meanPpl.Add(eaStats.Generation, popStats.MeanFitness);
        RefreshGraph();
    }

    /// <summary>
    /// Clear the time series data.
    /// </summary>
    public override void Clear()
    {
        _bestPpl.Clear();
        _meanPpl.Clear();
        RefreshGraph();
    }

    #endregion
}
