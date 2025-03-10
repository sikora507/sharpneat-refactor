﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using System.Diagnostics;
using SharpNeat.Tasks.FunctionRegression;

namespace SharpNeat.Tasks.GenerativeFunctionRegression;

/// <summary>
/// For probing and recording the responses of instances of <see cref="IBlackBox{T}"/>.
/// </summary>
public sealed class GenerativeBlackBoxProbe : IBlackBoxProbe
{
    readonly int _sampleCount;
    readonly double _offset;
    readonly double _scale;

    /// <summary>
    /// Construct a new instance.
    /// </summary>
    /// <param name="sampleCount">The number of generative samples to take.</param>
    /// <param name="offset">Offset to apply to each black box output response.</param>
    /// <param name="scale">Scaling factor to apply to each black box output response.</param>
    public GenerativeBlackBoxProbe(int sampleCount, double offset, double scale)
    {
        _sampleCount = sampleCount;
        _offset = offset;
        _scale = scale;
    }

    /// <inheritdoc/>
    public void Probe(IBlackBox<double> box, double[] responseArr)
    {
        Debug.Assert(responseArr.Length == _sampleCount);

        // Reset black box internal state.
        box.Reset();

        // Get the blackbox input and output spans.
        var inputs = box.Inputs.Span;
        var outputs = box.Outputs.Span;

        // Take the required number of samples.
        for(int i=0; i < _sampleCount; i++)
        {
            // Set bias input.
            inputs[0] = 1.0;

            // Activate the black box.
            box.Activate();

            // Get the black box's output value.
            // TODO: Review this scheme. This replicates the behaviour in SharpNEAT 2.x but not sure if it's ideal,
            // for one it depends on the output range of the neural net activation function in use.
            double output = outputs[0];
            Clip(ref output);
            responseArr[i] = ((output - 0.5) * _scale) + _offset;
        }
    }

    private static void Clip(ref double x)
    {
        if(x < 0.0) x = 0.0;
        else if(x > 1.0) x = 1.0;
    }
}
