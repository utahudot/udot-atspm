#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.SplitFail/SplitFailDetectorActivationCollection.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.SplitFail
{
    public class SplitFailDetectorActivationCollection
    {
        public SortedList<DateTime, SplitFailDetectorActivation> Activations =
            new SortedList<DateTime, SplitFailDetectorActivation>();

        public void AddActivation(SplitFailDetectorActivation Activation)
        {
            if (Activations.ContainsKey(Activation.DetectorOn))
            {
                do
                {
                    Activation.DetectorOn = Activation.DetectorOn.AddSeconds(.01);
                    Activation.DetectorOff = Activation.DetectorOff.AddSeconds(.01);
                } while (Activations.ContainsKey(Activation.DetectorOn));

                Activations.Add(Activation.DetectorOn, Activation);
            }
            else
            {
                Activations.Add(Activation.DetectorOn, Activation);
            }
        }


        //public double StartOfRedOccupancy(CycleSplitFail cycle, int secondsToWatch)
        //{
        //    DateTime endWatchTime = cycle.EndTime.AddSeconds(secondsToWatch);
        //    double o = 0;
        //    foreach (SplitFailDetectorActivation a in cycle.Activations.Activations.Values)
        //    {
        //           o += FindModifiedActivationDuration(cycle.EndTime, endWatchTime, a);
        //    }
        //    double t = secondsToWatch * 1000;
        //    double result = division(o, t);
        //    return result;
        //}

        //public double FindModifiedActivationDuration(DateTime startTime, DateTime endTime, SplitFailDetectorActivation a)
        //{
        //    double d = 0;
        //    //After start, before end
        //    if ((a.VehicleDetectorOn >= startTime && a.VehicleDetectorOn <= endTime) && (a.VehicleDetectorOff >= startTime && a.VehicleDetectorOff <= endTime))
        //    {
        //        d = a.Duration;
        //    }
        //    //Before start, before end
        //    else if ((a.VehicleDetectorOn <= startTime && a.VehicleDetectorOn <= endTime) && (a.VehicleDetectorOff <= endTime && a.VehicleDetectorOff >= startTime))
        //    {
        //        d = (a.VehicleDetectorOff - startTime).TotalMilliseconds;
        //    }
        //    //After start, After end
        //    else if ((a.VehicleDetectorOn >= startTime && a.VehicleDetectorOn <= endTime) && (a.VehicleDetectorOff >= endTime && a.VehicleDetectorOff >= startTime))
        //    {
        //        d = (endTime - a.VehicleDetectorOn).TotalMilliseconds;
        //    }
        //    //Before Start, After end
        //    else if ((a.VehicleDetectorOn <= startTime && a.VehicleDetectorOn <= endTime) && (a.VehicleDetectorOff >= endTime && a.VehicleDetectorOff >= startTime))
        //    {
        //        d = (endTime - startTime).TotalMilliseconds;
        //    }
        //    // 
        //    else { d = 0; }
        //    return d;
        //}

        //private double division(double first, double second)
        //{
        //    if (first > 0 && second > 0)
        //    {
        //        double i =  first / second;
        //        return i;
        //    }
        //    return 0;
        //}
    }
}