﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Graphs;
using SharpNeat.Neat.Genome;

namespace SharpNeat.Neat.DistanceMetrics.Double;

// TODO: Include coefficients and constant present on ManhattanDistance metric.
/// <summary>
/// Euclidean distance metric.
///
/// The Euclidean distance is given by sqrt(sum(delta^2))
/// Where [delta] is the absolute position difference in a given dimension (on a given axis).
///
/// There may be good reasons to not use this distance metric in NEAT; see this link for some discussion of this:
///   https://stats.stackexchange.com/questions/99171/why-is-euclidean-distance-not-a-good-metric-in-high-dimensions .
/// </summary>
public sealed class EuclideanDistanceMetric : IDistanceMetric<double>
{
    /// <inheritdoc/>
    public double CalcDistance(ConnectionGenes<double> p1, ConnectionGenes<double> p2)
    {
        DirectedConnection[] connArr1 = p1._connArr;
        DirectedConnection[] connArr2 = p2._connArr;
        double[] weightArr1 = p1._weightArr;
        double[] weightArr2 = p2._weightArr;

        // Store these heavily used values locally.
        int length1 = connArr1.Length;
        int length2 = connArr2.Length;

        // Test for special cases.
        if(length1 == 0 && length2 == 0)
        {   // Both arrays are empty. No disparities, therefore the distance is zero.
            return 0.0;
        }

        double distance = 0.0;
        if(length1 == 0)
        {   // All p2 genes are mismatches.
            for(int i=0; i < length2; i++)
                distance += weightArr2[i] * weightArr2[i];

            return Math.Sqrt(distance);
        }

        if(length2 == 0)
        {   // All p1 elements are mismatches.
            for(int i=0; i < length1; i++)
                distance += weightArr1[i] * weightArr1[i];

            return Math.Sqrt(distance);
        }

        // Both arrays contain elements.
        int arr1Idx = 0;
        int arr2Idx = 0;
        DirectedConnection conn1 = connArr1[arr1Idx];
        DirectedConnection conn2 = connArr2[arr2Idx];
        double weight1 = weightArr1[arr1Idx];
        double weight2 = weightArr2[arr2Idx];

        while(true)
        {
            if(conn1 < conn2)
            {
                // p2 doesn't specify a value in this dimension therefore we take its position to be 0.
                distance += weight1 * weight1;

                // Move to the next element in p1.
                arr1Idx++;
            }
            else if(conn1 == conn2)
            {
                // Matching elements. Note that abs() isn't required because we square the result.
                double tmp = weight1 - weight2;
                distance += tmp * tmp;

                // Move to the next element in both arrays.
                arr1Idx++;
                arr2Idx++;
            }
            else // conn2 > conn1
            {
                // p1 doesn't specify a value in this dimension therefore we take its position to be 0.
                distance += weight2 * weight2;

                // Move to the next element in p2.
                arr2Idx++;
            }

            // Check if we have exhausted one or both of the arrays.
            if(arr1Idx == length1)
            {
                // All remaining p2 elements are mismatches.
                for(int i = arr2Idx; i < length2; i++)
                    distance += weightArr2[i] * weightArr2[i];

                return Math.Sqrt(distance);
            }

            if(arr2Idx == length2)
            {   // All remaining arr1 elements are mismatches.
                for(int i = arr1Idx; i < weightArr1.Length; i++)
                    distance += weightArr1[i] * weightArr1[i];

                return Math.Sqrt(distance);
            }

            conn1 = connArr1[arr1Idx];
            conn2 = connArr2[arr2Idx];
            weight1 = weightArr1[arr1Idx];
            weight2 = weightArr2[arr2Idx];
        }
    }

    /// <inheritdoc/>
    public bool TestDistance(ConnectionGenes<double> p1, ConnectionGenes<double> p2, double threshold)
    {
        // Instead of calculating the euclidean distance we calculate distance squared (we skip the final sqrt
        // part of the formula). If we then square the threshold value this obviates the need to take the square
        // root when comparing our accumulating calculated distance with the threshold.
        threshold *= threshold;

        DirectedConnection[] connArr1 = p1._connArr;
        DirectedConnection[] connArr2 = p2._connArr;
        double[] weightArr1 = p1._weightArr;
        double[] weightArr2 = p2._weightArr;

        // Store these heavily used values locally.
        int length1 = connArr1.Length;
        int length2 = connArr2.Length;

        // Test for special cases.
        if(length1 == 0 && length2 == 0)
        {
            // Both arrays are empty. No disparities, therefore the distance is zero.
            return threshold > 0.0;
        }

        double distance = 0.0;
        if(length1 == 0)
        {
            // All p2 elements are mismatches.
            // p1 doesn't specify a value in these dimensions therefore we take its position to be 0 in all of them.
            for(int i=0; i < length2; i++)
                distance += weightArr2[i] * weightArr2[i];

            return distance < threshold;
        }

        if(length2 == 0)
        {
            // All p1 elements are mismatches.
            // p2 doesn't specify a value in these dimensions therefore we take its position to be 0 in all of them.
            for(int i=0; i < length1; i++)
                distance += weightArr1[i] * weightArr1[i];

            return distance < threshold;
        }

        // Both arrays contain elements. Compare the contents starting from the ends where the greatest discrepancies
        // between coordinates are expected to occur. Generally this should result in less element comparisons
        // before the threshold is passed and we exit the method.
        int arr1Idx = length1 - 1;
        int arr2Idx = length2 - 1;

        DirectedConnection conn1 = connArr1[arr1Idx];
        DirectedConnection conn2 = connArr2[arr2Idx];
        double weight1 = weightArr1[arr1Idx];
        double weight2 = weightArr2[arr2Idx];

        while(true)
        {
            if(conn1 > conn2)
            {
                // p2 doesn't specify a value in this dimension therefore we take its position to be 0.
                distance += weight1 * weight1;

                // Move to the next element in p1.
                arr1Idx--;
            }
            else if(conn1 == conn2)
            {
                // Matching elements. Note that abs() isn't required because we square the result.
                double tmp = weight1 - weight2;
                distance += tmp * tmp;

                // Move to the next element in both lists.
                arr1Idx--;
                arr2Idx--;
            }
            else // conn2 > conn1
            {
                // p1 doesn't specify a value in this dimension therefore we take its position to be 0.
                distance += weight2 * weight2;

                // Move to the next element in p2.
                arr2Idx--;
            }

            // Test the threshold.
            if(distance >= threshold)
                return false;

            // Check if we have exhausted one or both of the arrays.
            if(arr1Idx < 0)
            {   // Any remaining arr2 elements are mismatches.
                for(int i = arr2Idx; i > -1; i--)
                    distance += weightArr2[i] * weightArr2[i];

                return distance < threshold;
            }

            if(arr2Idx < 0)
            {   // All remaining arr1 elements are mismatches.
                for(int i = arr1Idx; i > -1; i--)
                    distance += weightArr1[i] * weightArr1[i];

                return distance < threshold;
            }

            conn1 = connArr1[arr1Idx];
            conn2 = connArr2[arr2Idx];
            weight1 = weightArr1[arr1Idx];
            weight2 = weightArr2[arr2Idx];
        }
    }

    /// <inheritdoc/>
    public ConnectionGenes<double> CalculateCentroid(
        ReadOnlySpan<ConnectionGenes<double>> points)
    {
        return DistanceMetricUtils.CalculateEuclideanCentroid(points);
    }
}
