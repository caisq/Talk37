﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public class NonInvokeGazeTargetItem<TElement> : GazeTargetItem<TElement>
        where TElement : class
    {
        private readonly IGazeTarget<TElement> _target;

        public NonInvokeGazeTargetItem(IGazeTarget<TElement> target)
        {
            _target = target;
        }

        public override TElement Cursor => _target.DefaultCursor;

        protected internal override bool IsInvokable
        {
            get { return false; }
        }

        protected internal override TimeSpan GetElementStateDelay(PointerState pointerState, TimeSpan defaultValue) => TimeSpan.Zero;

        protected internal override TimeSpan GetElementRepeatDelay(TimeSpan defaultValue) => TimeSpan.Zero;

        protected internal override int GetMaxDwellRepeatCount() => 0;

        protected internal override void Invoke()
        {
        }

        protected override void ShowFeedback(DwellProgressState state, double progress)
        {
        }
    }
}