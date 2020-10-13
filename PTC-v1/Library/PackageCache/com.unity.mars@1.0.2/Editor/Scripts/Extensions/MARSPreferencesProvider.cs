using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    class MARSPreferencesProvider : SettingsProvider
    {
        const string k_CameraPermissionLabel = "Camera permission";
        const string k_OSXWithPermissionsHelpBox = "MARS Camera permissions are handled by the operating system. To allow or deny acces to the camera, go to System Preferences > Security & Privacy > Camera and toggle Unity Hub";

        [SettingsProvider]
        public static SettingsProvider CreateMARSPreferencesProvider()
        {
            return new MARSPreferencesProvider();
        }

        MARSPreferencesProvider(string path = "Preferences/MARS", SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            if (CameraPermissionUtils.IsOSXWithPermissions())
            {
                EditorGUILayout.HelpBox(k_OSXWithPermissionsHelpBox, MessageType.Info);
            }
            else
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var granted = EditorPrefs.GetBool(CameraPermissionUtils.CameraPermissionPref, false);
                    granted = EditorGUILayout.Toggle(k_CameraPermissionLabel, granted);

                    if (check.changed)
                        EditorPrefs.SetBool(CameraPermissionUtils.CameraPermissionPref, granted);
                }
            }
        }
    }
}
