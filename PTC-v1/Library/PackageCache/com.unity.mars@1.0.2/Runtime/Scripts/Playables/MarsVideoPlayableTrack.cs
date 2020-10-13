using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.MARS.Recording
{
    [Serializable]
    [TrackClipType(typeof(MarsVideoPlayableAsset))]
    public class MarsVideoPlayableTrack : TrackAsset
    {
    }
}
