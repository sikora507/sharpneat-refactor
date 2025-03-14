﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using System.Globalization;
using SharpNeat.Graphs;
using SharpNeat.IO.Models;

namespace SharpNeat.Neat.Genome.IO;

/// <summary>
/// Static methods for converting between <see cref="NeatGenome{T}"/> and <see cref="NetFileModel"/>.
/// </summary>
public static class NeatGenomeConverter
{
    /// <summary>
    /// Convert a <see cref="NeatGenome{T}"/> into a <see cref="NetFileModel"/> instance, suitable for saving to
    /// file.
    /// </summary>
    /// <typeparam name="T">Neural net numeric data type.</typeparam>
    /// <param name="genome">The genome to convert.</param>
    /// <returns>A new instance of <see cref="NetFileModel"/>.</returns>
    public static NetFileModel ToNetFileModel<T>(
        NeatGenome<T> genome)
        where T : struct
    {
        // Convert input and output counts, and cyclic/acyclic indicator.
        int inputCount = genome.MetaNeatGenome.InputNodeCount;
        int outputCount = genome.MetaNeatGenome.OutputNodeCount;
        bool isAcyclic = genome.MetaNeatGenome.IsAcyclic;

        // Convert connections.
        ConnectionGenes<T> connGenes = genome.ConnectionGenes;
        DirectedConnection[] connArr = connGenes._connArr;
        T[] weightArr = connGenes._weightArr;

        List<ConnectionLine> connList = new(connGenes.Length);

        for(int i=0; i < connGenes.Length; i++)
        {
            DirectedConnection conn = connArr[i];

            // Note. The neat genome may use 'double' or 'float' typed weights; whereas NetFileModel is uses
            // 'double' only, so we some conversion is required here.
            double weight = Convert.ToDouble(weightArr[i], CultureInfo.InvariantCulture);
            ConnectionLine connLine = new(conn.SourceId, conn.TargetId, weight);
            connList.Add(connLine);
        }

        // Convert activation function(s).
        // Note. By convention we use the activation function type short name as function code (e.g. "ReLU",
        // or "Logistic").
        ActivationFnLine actFnLine = new(0, genome.MetaNeatGenome.ActivationFn.GetType().Name);
        List<ActivationFnLine> actFnLines = new()
        {
            actFnLine
        };

        return new NetFileModel(
            inputCount, outputCount,
            isAcyclic,
            genome.MetaNeatGenome.CyclesPerActivation,
            connList, actFnLines);
    }

    /// <summary>
    /// Convert a <see cref="NetFileModel"/> instance into a <see cref="NeatGenome{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">Neural net numeric data type.</typeparam>
    /// <param name="model">The <see cref="NetFileModel"/> instance to convert from.</param>
    /// <param name="metaNeatGenome">A <see cref="MetaNeatGenome{T}"/> instance; required to construct a new
    /// <see cref="MetaNeatGenome{T}"/>.</param>
    /// <param name="genomeId">The ID to assign to the created genome instance.</param>
    /// <param name="throwIfActivationFnMismatch">If true then an exception is thrown if the activation function
    /// defined on <paramref name="model"/> does not match the one defined on <paramref name="metaNeatGenome"/>.
    /// If false, and there is a mismatch, then the activation defined on the <see cref="MetaNeatGenome{T}"/> is used.</param>
    /// <returns>A new instance of <see cref="MetaNeatGenome{T}"/>.</returns>
    public static NeatGenome<T> ToNeatGenome<T>(
        NetFileModel model,
        MetaNeatGenome<T> metaNeatGenome,
        int genomeId,
        bool throwIfActivationFnMismatch = true)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(metaNeatGenome);

        // Check that the IsAcyclic flag is the same on metaNeatGenome and netFileModel.
        if(model.IsAcyclic ^ metaNeatGenome.IsAcyclic)
        {
            throw new ArgumentException(
                $"The {nameof(MetaNeatGenome<T>)} and {nameof(NetFileModel)} arguments specify different values for the IsAcyclic flag.",
                nameof(model));
        }

        // Check input and output counts.
        if(model.InputCount != metaNeatGenome.InputNodeCount)
        {
            throw new ArgumentException(
                $"The {nameof(MetaNeatGenome<T>)} and {nameof(NetFileModel)} arguments specify different input node counts.",
                nameof(model));
        }

        if(model.OutputCount != metaNeatGenome.OutputNodeCount)
        {
            throw new ArgumentException(
                $"The {nameof(MetaNeatGenome<T>)} and {nameof(NetFileModel)} arguments specify different output node counts.",
                nameof(model));
        }

        // Optionally check if the metaNeatGenome and netFileModel specify a different activation function.
        if (throwIfActivationFnMismatch
            && !string.Equals(model.ActivationFns[0].Code, metaNeatGenome.ActivationFn.GetType().Name, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"The {nameof(MetaNeatGenome<T>)} and {nameof(NetFileModel)} arguments specify different activation functions.",
                nameof(model));
        }

        // Convert the connections.
        ConnectionGenes<T> connGenes = ToConnectionGenes<T>(model.Connections);

        // Get a genome builder instance.
        // Note. the builder type depends on IsAcyclic.
        INeatGenomeBuilder<T> genomeBuilder = NeatGenomeBuilderFactory<T>.Create(metaNeatGenome, true);

        // Create a genome instance.
        NeatGenome<T> genome = genomeBuilder.Create(genomeId, 0, connGenes);
        return genome;
    }

    private static ConnectionGenes<T> ToConnectionGenes<T>(
        List<ConnectionLine> connList)
        where T : struct
    {
        ConnectionGenes<T> connGenes = new(connList.Count);
        DirectedConnection[] connArr = connGenes._connArr;
        T[] weightArr = connGenes._weightArr;

        for (int i=0; i < connArr.Length; i++)
        {
            ConnectionLine connLine = connList[i];
            connArr[i] = new DirectedConnection(connLine.SourceId, connLine.TargetId);
            weightArr[i] = (T)Convert.ChangeType(connLine.Weight, typeof(T), CultureInfo.InvariantCulture);
        }

        // Note. It is necessary for the connection to be sorted by sourceId, and secondary sorted by targetId.
        connGenes.Sort();

        return connGenes;
    }
}
