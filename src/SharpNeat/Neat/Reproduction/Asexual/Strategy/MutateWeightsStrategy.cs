﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using Redzen.Random;
using Redzen.Structures;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Reproduction.Asexual.WeightMutation;

namespace SharpNeat.Neat.Reproduction.Asexual.Strategy;

/// <summary>
/// A NEAT genome asexual reproduction strategy based on mutation of connection weights.
/// </summary>
/// <typeparam name="T">Connection weight data type.</typeparam>
/// <remarks>
/// Offspring genomes are created by taking a clone of a single parent genome and applying a weight
/// mutation scheme to the connection weights of the clone.
/// </remarks>
public sealed class MutateWeightsStrategy<T> : IAsexualReproductionStrategy<T>
    where T : struct
{
    readonly INeatGenomeBuilder<T> _genomeBuilder;
    readonly Int32Sequence _genomeIdSeq;
    readonly Int32Sequence _generationSeq;
    readonly WeightMutationScheme<T> _weightMutationScheme;

    /// <summary>
    /// Construct a new instance.
    /// </summary>
    /// <param name="genomeBuilder">NeatGenome builder.</param>
    /// <param name="genomeIdSeq">Genome ID sequence; for obtaining new genome IDs.</param>
    /// <param name="generationSeq">Generation sequence; for obtaining the current generation number.</param>
    /// <param name="weightMutationScheme">Connection weight mutation scheme.</param>
    public MutateWeightsStrategy(
        INeatGenomeBuilder<T> genomeBuilder,
        Int32Sequence genomeIdSeq,
        Int32Sequence generationSeq,
        WeightMutationScheme<T> weightMutationScheme)
    {
        _genomeBuilder = genomeBuilder;
        _genomeIdSeq = genomeIdSeq;
        _generationSeq = generationSeq;
        _weightMutationScheme = weightMutationScheme;
    }

    /// <inheritdoc/>
    public NeatGenome<T> CreateChildGenome(NeatGenome<T> parent, IRandomSource rng)
    {
        // Clone the parent's connection weight array.
        var weightArr = (T[])parent.ConnectionGenes._weightArr.Clone();

        // Apply mutation to the connection weights.
        _weightMutationScheme.MutateWeights(weightArr, rng);

        // Create the child genome's ConnectionGenes object.
        // Note. The parent genome's connection arrays are re-used; these remain unchanged because we are mutating
        // connection *weights* only, so we can avoid the cost of cloning these arrays.
        var connGenes = new ConnectionGenes<T>(
            parent.ConnectionGenes._connArr,
            weightArr);

        // Create and return a new genome.
        // Note. The parent's ConnectionIndexArray and HiddenNodeIdArray can be re-used here because the new genome
        // has the same set of connections (same neural net structure).
        return _genomeBuilder.Create(
            _genomeIdSeq.Next(),
            _generationSeq.Peek,
            connGenes,
            parent.HiddenNodeIdArray,
            parent.NodeIndexByIdMap,
            parent.DirectedGraph,
            parent.ConnectionIndexMap);
    }
}
