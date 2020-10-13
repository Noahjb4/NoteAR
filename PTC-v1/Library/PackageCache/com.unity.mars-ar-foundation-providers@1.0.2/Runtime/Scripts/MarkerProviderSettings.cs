using System.Collections.Generic;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.MARS.Providers
{
    [ScriptableSettingsPath(MARSCore.SettingsFolder)]
    public class MarkerProviderSettings : ScriptableSettings<MarkerProviderSettings>, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        class MarkerAssetPostprocessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (var asset in importedAssets)
                {
                    if (!typeof(MarsMarkerLibrary).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(asset)))
                        continue;

                    var marsLibrary = AssetDatabase.LoadAssetAtPath<MarsMarkerLibrary>(asset);
                    var xrLibraryPath = GetXRLibraryPath(asset);
                    var xrLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(xrLibraryPath);
                    if (xrLibrary == null)
                        continue;

                    instance.Add(marsLibrary, xrLibrary);
                }
            }

            static string GetXRLibraryPath(string asset)
            {
                const int extensionLength = 6;
                var withoutExtension = asset.Substring(0, asset.Length - extensionLength);
                return string.Format(ARFLibrarySuffixFormat, withoutExtension) + k_Extension;
            }
        }
#endif

        public const string ARFLibrarySuffixFormat = "{0}_arf";
        const string k_Extension = ".asset";

        /// <summary>
        /// We use this MarkerProvider ScriptableSettings for two purposes:
        /// 1. At build time, to provide a reference to all the XRReferenceImageLibraries we have created
        ///    from MarsMarkerLibraries so that it gets included in the build since it is
        ///    referenced here.
        /// 2. At load time or runtime, we need to provide a mapping from the MarsMarkerLibrary to its
        ///     corresponding XRReferenceImageLibrary, so we recreate the dictionary that was used
        ///     to build this asset.
        /// </summary>

        [SerializeField]
        XRReferenceImageLibrary[] m_XrReferenceImageLibraries = new XRReferenceImageLibrary[0];

        [SerializeField]
        MarsMarkerLibrary[] m_MarsMarkerLibraries = new MarsMarkerLibrary[0];

        internal readonly Dictionary<MarsMarkerLibrary, XRReferenceImageLibrary> MarsToXRLibraryMap = new Dictionary<MarsMarkerLibrary, XRReferenceImageLibrary>();
        readonly Dictionary<XRReferenceImageLibrary, MarsMarkerLibrary> m_XRToMarsLibraryMap = new Dictionary<XRReferenceImageLibrary, MarsMarkerLibrary>();

        public bool TryFind(MarsMarkerLibrary activeMarsLibrary, out XRReferenceImageLibrary xrReferenceImageLibrary)
        {
            return MarsToXRLibraryMap.TryGetValue(activeMarsLibrary, out xrReferenceImageLibrary) && xrReferenceImageLibrary != null;
        }

        public bool TryFind(XRReferenceImageLibrary xrLibrary, out MarsMarkerLibrary marsLibrary)
        {
            return m_XRToMarsLibraryMap.TryGetValue(xrLibrary, out marsLibrary) && marsLibrary != null;
        }

        public void Add(MarsMarkerLibrary marsLibrary, XRReferenceImageLibrary xrLibrary)
        {
            //Remove from dictionary to clear out wrapper objects for destroyed assets
            MarsToXRLibraryMap.Remove(marsLibrary);
            MarsToXRLibraryMap[marsLibrary] = xrLibrary;
            m_XRToMarsLibraryMap.Remove(xrLibrary);
            m_XRToMarsLibraryMap[xrLibrary] = marsLibrary;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Implement this method to receive a callback before Unity serializes your object.
        /// </summary>
        public void OnBeforeSerialize()
        {
            // TODO: bring back checks for deleted assets when AssetDatabase bug is resolved (https://fogbugz.unity3d.com/f/cases/1152124)
            var marsLibraryList = new List<MarsMarkerLibrary>();
            var xrLibraryList = new List<XRReferenceImageLibrary>();
            foreach (var kvp in MarsToXRLibraryMap)
            {
                marsLibraryList.Add(kvp.Key);
                xrLibraryList.Add(kvp.Value);
            }

            m_MarsMarkerLibraries = marsLibraryList.ToArray();
            m_XrReferenceImageLibraries = xrLibraryList.ToArray();
        }

        /// <summary>
        /// Implement this method to receive a callback after Unity deserializes your object.
        /// </summary>
        public void OnAfterDeserialize()
        {
            var marsLibraryLength = m_MarsMarkerLibraries.Length;
            var xrLibraryLength = m_XrReferenceImageLibraries.Length;
            if (m_XrReferenceImageLibraries.Length != marsLibraryLength)
                Debug.LogWarning("Length mismatch between MARSMarkerLibraries and XRReferenceImageLibraries. This can happen if the MarkerProviderSettings asset is modified outside of Unity. Missing and mismatched will be removed when MarkerProviderSettings is saved.");

            for (var i = 0; i < marsLibraryLength; i++)
            {
                var marsLibrary = m_MarsMarkerLibraries[i];
                if (marsLibrary == null)
                {
                    Debug.LogWarning("Encountered empty array element when deserializing MARSMarkerLibrary list. This can happen if a MARSMarkerLibrary asset is deleted or its GUID changed outside of Unity.");
                    continue;
                }

                if (i >= xrLibraryLength)
                    break;

                var xrLibrary = m_XrReferenceImageLibraries[i];
                if (xrLibrary == null)
                {
                    Debug.LogWarning("Encountered empty array element when deserializing XRReferenceLibrary list. This can happen if a XRReferenceLibrary asset is deleted or its GUID changed outside of Unity.");
                    continue;
                }

                MarsToXRLibraryMap[marsLibrary] = xrLibrary;
                m_XRToMarsLibraryMap[xrLibrary] = marsLibrary;
            }
        }

        public void Remove(MarsMarkerLibrary marsLibrary)
        {
            if (MarsToXRLibraryMap.TryGetValue(marsLibrary, out var xrLibrary))
            {
                MarsToXRLibraryMap.Remove(marsLibrary);
                m_XRToMarsLibraryMap.Remove(xrLibrary);

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }
    }
}
