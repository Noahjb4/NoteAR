﻿using System;
using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS.Providers
{
    [AddComponentMenu("")]
    [ProviderSelectionOptions(ProviderPriorities.StubProviderPriority)]
    class StubFaceTrackingProvider : StubProvider, IProvidesFaceTracking
    {
#pragma warning disable 67
        public event Action<IMRFace> FaceAdded;
        public event Action<IMRFace> FaceUpdated;
        public event Action<IMRFace> FaceRemoved;
#pragma warning restore 67

        public override void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesFaceTracking>(obj);
        }

        public int GetMaxFaceCount() { return 0; }

        public void GetFaces(List<IMRFace> faces) { }
    }
}
