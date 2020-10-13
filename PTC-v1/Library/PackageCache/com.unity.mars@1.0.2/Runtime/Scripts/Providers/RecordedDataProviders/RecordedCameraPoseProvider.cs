using System;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS.Recording.Providers
{
    [ProviderSelectionOptions(ProviderPriorities.RecordedProviderPriority, disallowAutoCreation:true)]
    public class RecordedCameraPoseProvider : MonoBehaviour, IProvidesCameraPose, ISteppableRecordedDataProvider
    {
#pragma warning disable 67
        public event Action<Pose> poseUpdated;
        public event Action<MRCameraTrackingState> trackingStateChanged;
#pragma warning restore 67

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesCameraPose>(obj);
        }

        public void UnloadProvider() { }

        public Pose GetCameraPose() { return transform.GetLocalPose(); }

        public void ClearData() { }

        public void StepRecordedData()
        {
            if (poseUpdated != null)
                poseUpdated(GetCameraPose());
        }
    }
}
