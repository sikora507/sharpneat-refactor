﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Experiments;
using SharpNeat.IO;
using SharpNeat.NeuralNets;
using SharpNeat.Tasks.FunctionRegression;
using SharpNeat.Tasks.GenerativeFunctionRegression.ConfigModels;
using static SharpNeat.Experiments.ModelUtils;

namespace SharpNeat.Tasks.GenerativeFunctionRegression;

/// <summary>
/// A factory for creating instances of <see cref="INeatExperiment{T}"/> for the generative sinewave task.
/// </summary>
public sealed class GenerativeFnRegressionExperimentFactory : INeatExperimentFactory
{
    /// <inheritdoc/>
    public string Id => "generative-sinewave";

    /// <inheritdoc/>
    public INeatExperiment<double> CreateExperiment(Stream jsonConfigStream)
    {
        // Load experiment JSON config.
        GenerativeFnRegressionExperimentConfig experimentConfig = JsonUtils.Deserialize<GenerativeFnRegressionExperimentConfig>(jsonConfigStream);

        // Read custom evaluation scheme config.
        ReadEvaluationSchemeConfig(
            experimentConfig,
            out Func<double, double> fn,
            out ParamSamplingInfo paramSamplingInfo,
            out double gradientMseWeight);

        // Create an evaluation scheme object for the generative sinewave task; using the evaluation scheme
        // config read from json.
        var evalScheme = new GenerativeFnRegressionEvaluationScheme(fn, paramSamplingInfo, gradientMseWeight);

        // Create a NeatExperiment object with the evaluation scheme,
        // and assign some default settings (these can be overridden by config).
        var experiment = new NeatExperiment<double>(evalScheme, Id)
        {
            IsAcyclic = false,
            CyclesPerActivation = 1,
            ActivationFnName = ActivationFunctionId.LeakyReLU.ToString()
        };

        // Apply configuration to the experiment instance.
        experiment.Configure(experimentConfig);
        return experiment;
    }

    /// <inheritdoc/>
    public INeatExperiment<float> CreateExperimentSinglePrecision(Stream jsonConfigStream)
    {
        throw new NotImplementedException();
    }

    #region Private Static Methods

    private static void ReadEvaluationSchemeConfig(
        GenerativeFnRegressionExperimentConfig experimentConfig,
        out Func<double, double> fn,
        out ParamSamplingInfo paramSamplingInfo,
        out double gradientMseWeight)
    {
        // Get the customEvaluationSchemeConfig section.
        if(experimentConfig.CustomEvaluationSchemeConfig is null)
            throw new ConfigurationException("customEvaluationSchemeConfig not defined.");

        GenerativeFnRegressionCustomConfig customConfig = experimentConfig.CustomEvaluationSchemeConfig;

        // Read function ID.
        string functionIdStr = GetMandatoryProperty(() => customConfig.FunctionId);
        FunctionId functionId = (FunctionId)Enum.Parse(typeof(FunctionId), functionIdStr);
        fn = FunctionFactory.GetFunction(functionId);

        // Read sample interval min and max, and sample resolution.
        double sampleIntervalMin = GetMandatoryProperty(() => customConfig.SampleIntervalMin);
        double sampleIntervalMax = GetMandatoryProperty(() => customConfig.SampleIntervalMax);
        int sampleResolution = GetMandatoryProperty(() => customConfig.SampleResolution);
        paramSamplingInfo = new ParamSamplingInfo(sampleIntervalMin, sampleIntervalMax, sampleResolution);

        // Read the weight to apply to the gradientMse readings in the final fitness score.
        // 0 means don't use the gradient measurements, 1 means give them equal weight to the y position readings at each x sample point.
        gradientMseWeight = GetMandatoryProperty(() => customConfig.GradientMseWeight);
    }

    #endregion
}
