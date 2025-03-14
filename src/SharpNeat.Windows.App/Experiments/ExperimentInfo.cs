﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
namespace SharpNeat.Windows.App.Experiments;

public class ExperimentInfo
{
    public string Name { get; set; }
    public ExperimentFactoryInfo ExperimentFactory { get; set; }
    public string ConfigFile { get; set; }
    public string DescriptionFile { get; set; }
    public ExperimentUIFactoryInfo ExperimentUIFactory { get; set; }
}
