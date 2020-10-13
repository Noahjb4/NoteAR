using UnityEditor;
using UnityEngine;
using Unity.MARS.Build;

namespace Unity.MARS
{
    class ElectiveExtensionsToolbarMenu : Editor
    {
        const string k_BuildSettingsTitle = "Build Settings Check";

        [MenuItem(MenuConstants.MenuPrefix + k_BuildSettingsTitle, priority = MenuConstants.ElectiveExtensionsRunBuildCheckPriority)]
        static void RunElectiveExtensionsBuildCheck()
        {
            var result = "";
#if PLATFORM_ANDROID
            result = ElectiveExtensions.RunReport(ElectiveExtensionsConfigurationAndroid.ConfigAndroid);
#elif PLATFORM_IOS
            result = Build.ElectiveExtensions.RunReport(ElectiveExtensionsConfigurationIOS.ConfigIOS);
#elif PLATFORM_LUMIN
            result = Build.ElectiveExtensions.RunReport(ElectiveExtensionsConfigurationLumin.ConfigLumin);
#else
            result = ("ElectiveExtensions is not supported on this platform \n " +
                "ElectiveExtensions reports are supported on Android, iOS, and Lumin build targets.");
#endif
            EditorUtility.DisplayDialog(k_BuildSettingsTitle, result, "OK");
        }
    }
}
