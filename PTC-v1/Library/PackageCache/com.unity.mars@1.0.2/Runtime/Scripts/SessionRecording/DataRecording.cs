﻿using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MARS
{
    /// <summary>
    /// Holds information about MR data recorded to a Timeline
    /// </summary>
    public abstract class DataRecording : ScriptableObject
    {
        /// <summary>
        /// Creates recorded data providers and hooks them up to the Timeline. Provider game objects should be created
        /// using GameObjectUtils.Create for compatibility with simulation.
        /// </summary>
        /// <param name="director">Playable Director that should be used to hook up providers to the Timeline</param>
        /// <param name="providers">List to be filled out with newly created providers</param>
        public abstract void SetupDataProviders(PlayableDirector director, List<IFunctionalityProvider> providers);
    }
}
