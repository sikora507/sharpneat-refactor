﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Experiments.ConfigModels;

/// <summary>
/// Model type for NEAT evolution algorithm configuration.
/// </summary>
public class NeatEvolutionAlgorithmConfig
{
    /// <summary>
    /// The species count.
    /// </summary>
    public int? SpeciesCount { get; set; }

    /// <summary>
    /// Elitism proportion.
    /// We sort species genomes by fitness and keep the top N%, the other genomes are
    /// removed to make way for the offspring.
    /// </summary>
    public double? ElitismProportion { get; set; }

    /// <summary>
    /// Selection proportion.
    /// We sort species genomes by fitness and select parent genomes for producing offspring from
    /// the top N%. Selection is performed prior to elitism being applied, therefore selecting from more
    /// genomes than will be made elite is possible.
    /// </summary>
    public double? SelectionProportion { get; set; }

    /// <summary>
    /// The proportion of offspring to be produced from asexual reproduction (mutation).
    /// </summary>
    public double? OffspringAsexualProportion { get; set; }

    /// <summary>
    /// The proportion of offspring to be produced from sexual reproduction.
    /// </summary>
    public double? OffspringSexualProportion { get; set; }

    /// <summary>
    /// The proportion of sexual reproductions that will use genomes from different species.
    /// </summary>
    public double? InterspeciesMatingProportion { get; set; }

    /// <summary>
    /// Length of the history buffer used for calculating the moving average for best fitness, mean fitness and mean complexity.
    /// </summary>
    public int? StatisticsMovingAverageHistoryLength { get; set; }
}
