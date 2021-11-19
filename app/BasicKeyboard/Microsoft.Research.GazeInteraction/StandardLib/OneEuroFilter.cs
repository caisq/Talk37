// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*
 * http://www.lifl.fr/~casiez/1euro/
 * http://www.lifl.fr/~casiez/publications/CHI2012-casiez.pdf
*/

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public class OneEuroFilter : IGazeFilter
    {
        private const float ONEEUROFILTER_DEFAULT_BETA = 5.0f;
        private const float ONEEUROFILTER_DEFAULT_CUTOFF = 0.1f;
        private const float ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF = 1.0f;

        public OneEuroFilter()
        {
            _lastTimestamp = TimeSpan.Zero;
            Beta = ONEEUROFILTER_DEFAULT_BETA;
            Cutoff = ONEEUROFILTER_DEFAULT_CUTOFF;
            VelocityCutoff = ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF;
        }

        public OneEuroFilter(float cutoff, float beta)
        {
            _lastTimestamp = TimeSpan.Zero;
            Beta = beta;
            Cutoff = cutoff;
            VelocityCutoff = ONEEUROFILTER_DEFAULT_VELOCITY_CUTOFF;
        }

        PointF IGazeFilter.Update(TimeSpan timestamp, PointF location)
        {
            if (_lastTimestamp == TimeSpan.Zero)
            {
                _lastTimestamp = timestamp;
                _pointFilter = new LowpassFilter(location);
                _deltaFilter = new LowpassFilter(default);
                return location;
            }

            var gazePoint = location;

            // Reducing _beta increases lag. Increasing beta decreases lag and improves response time
            // But a really high value of beta also contributes to jitter
            float beta = Beta;

            // This simply represents the cutoff frequency. A lower value reduces jiiter
            // and higher value increases jitter
            float cf = Cutoff;
            var cutoff = new PointF(cf, cf);

            // determine sampling frequency based on last time stamp
            // TODO: This calculation looks suspect - the magic 10^N number does not match TimeSpan.TicksPerSecond!
            float samplingFrequency = 100000000.0f / Math.Max(1, (timestamp - _lastTimestamp).Ticks);
            _lastTimestamp = timestamp;

            // calculate change in distance...
            PointF deltaDistance;
            deltaDistance.X = gazePoint.X - _pointFilter.Previous.X;
            deltaDistance.Y = gazePoint.Y - _pointFilter.Previous.Y;

            // ...and velocity
            var velocity = new PointF(deltaDistance.X * samplingFrequency, deltaDistance.Y * samplingFrequency);

            // find the alpha to use for the velocity filter
            float velocityAlpha = Alpha(samplingFrequency, VelocityCutoff);
            var velocityAlphaPoint = new PointF(velocityAlpha, velocityAlpha);

            // find the filtered velocity
            PointF filteredVelocity = _deltaFilter.Update(velocity, velocityAlphaPoint);

            // ignore sign since it will be taken care of by deltaDistance
            filteredVelocity.X = Math.Abs(filteredVelocity.X);
            filteredVelocity.Y = Math.Abs(filteredVelocity.Y);

            // compute new cutoff to use based on velocity
            cutoff.X += beta * filteredVelocity.X;
            cutoff.Y += beta * filteredVelocity.Y;

            // find the new alpha to use to filter the points
            var distanceAlpha = new PointF(Alpha(samplingFrequency, (float)cutoff.X), Alpha(samplingFrequency, (float)cutoff.Y));

            // find the filtered point
            PointF filteredPoint = _pointFilter.Update(gazePoint, distanceAlpha);

            // compute the new args
            return filteredPoint;
        }

        void IGazeFilter.LoadSettings(IDictionary<string, object> settings)
        {
            if (settings.ContainsKey("OneEuroFilter.Beta"))
            {
                Beta = (float)settings["OneEuroFilter.Beta"];
            }

            if (settings.ContainsKey("OneEuroFilter.Cutoff"))
            {
                Cutoff = (float)settings["OneEuroFilter.Cutoff"];
            }

            if (settings.ContainsKey("OneEuroFilter.VelocityCutoff"))
            {
                VelocityCutoff = (float)settings["OneEuroFilter.VelocityCutoff"];
            }
        }

        public float Beta { get; set; }

        public float Cutoff { get; set; }

        public float VelocityCutoff { get; set; }

        private float Alpha(float rate, float cutoff)
        {
            const float PI = 3.14159265f;
            float te = 1.0f / rate;
            float tau = (float)(1.0f / (2 * PI * cutoff));
            float alpha = te / (te + tau);
            return alpha;
        }

        private TimeSpan _lastTimestamp;
        private LowpassFilter _pointFilter;
        private LowpassFilter _deltaFilter;
    }
}
