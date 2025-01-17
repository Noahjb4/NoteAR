﻿namespace Unity.MARS
{
    public class MenuConstants
    {
        public const string MenuPrefix =
#if MARS_MENU
            "MARS/";
#else
            "Window/MARS/";
#endif

        public const string DevMenuPrefix = MenuPrefix + "Developer/";

        // 0 = beginning of Window menu, after 'Previous Window'
        // 1000 = before 'Asset Store'
        // 3000 = after XR/, before Experimental/
        // 3020 = end of Window menu, after Experimental/
        // Note: may need to reboot the editor for change to take effect.
        const int k_BasePriority = 1000;

        // Priorities with a difference over 10 get a divider between them.
        const int k_Divider = 11;

        public const int PanelPriority = k_BasePriority + k_Divider;
        public const int SimulationViewPriority = PanelPriority + k_Divider;
        public const int DeviceViewPriority = SimulationViewPriority + 1;
        public const int SimTestRunnerPriority = DeviceViewPriority + 1;
        public const int SimSettingsPriority = SimTestRunnerPriority + 1;
        public const int TemplatePriority = SimSettingsPriority + k_Divider;
        public const int ImportContentPriority = TemplatePriority + k_Divider;
        public const int ElectiveExtensionsRunBuildCheckPriority = ImportContentPriority + k_Divider;

        public const int DeveloperPriority =
#if MARS_MENU
            k_BasePriority + 0;
#else
            k_BasePriority + ElectiveExtensionsRunBuildCheckPriority + k_Divider;
#endif
        public const int QueryPipelineViewerPriority = DeveloperPriority;
        public const int MatchRatingExplorerPriority = QueryPipelineViewerPriority + 1;
        public const int QueryViewerPriority = MatchRatingExplorerPriority + 1;
        public const int DatabaseViewerPriority = QueryViewerPriority + 1;
        public const int SimTestWindowPriority = DatabaseViewerPriority + 1;
        public const int ModuleLoaderPriority = SimTestWindowPriority + k_Divider;
        public const int UpdateSimEnvironmentsPriority = ModuleLoaderPriority + k_Divider;
        public const int RefreshSessionRecordingsPriority = UpdateSimEnvironmentsPriority + 1;
    }
}
