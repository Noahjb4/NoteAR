using System;
using UnityEngine;

namespace Unity.MARS
{
    [Serializable]
    public enum ContextViewType
    {
        Undefined = -1,
        SimulationView = 0,
        DeviceView,
        NormalSceneView,
        PrefabIsolation,
        GameView,
        OtherView,
    }

    internal interface ICompositeView
    {
        ContextViewType ContextViewType { get; }

        Camera TargetCamera { get; }

        RenderTextureDescriptor CameraTargetDescriptor { get; }

        Color BackgroundColor { get; }

        bool ShowImageEffects { get; }

        bool BackgroundSceneActive { get; }

        bool DesaturateComposited { get; }

        bool UseXRay { get; }
    }
}
