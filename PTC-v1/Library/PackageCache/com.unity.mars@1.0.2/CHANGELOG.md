# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2020-06-18
### Fixed
- Simulated environment no longer affects 3D physics of simulated content
- Device view movement is locked when no MARSSession / controlling camera is present in the scene.
- Missing warning for scene that can't be simulated when the scene has a functionality user but no MARS Session
- Fix issue where zoom controls still worked in Device view when not simulating temporally and could cause issues with the clipping planes.
- Simulation view is no longer stuck in "Back" view mode for newly created environments
- New environments now use reasonable default values for PlaneExtractionSettings
- Geolocation shortcut buttons correctly pass longitude in the `shortcutAction` callback.
- Simulation Environment GameObject Inspectors are no longer locked when URP or Composite Fallback is enabled.
- Simulation Environment lighting will not be lost when undoing after environment is loaded or after editor nUnit tests are run.
- Fix crash on startup of the editor when using URP and have a Simulation View open.
- Fix error when moving a GameObject to the Simulation Scene Root when it is not a root GameObject.

### Changed
- GeneratedPlanesRoot no longer keeps track of modifications. Saving planes from simulation or extracting planes now always warns the user that previous planes will be destroyed, explains that they can preserve planes by moving them out of the root, and asks confirmation to continue.

## [1.0.1] - 2020-06-02
- Updated Unity MARS license

## [1.0.0] - 2020-05-29
### Fixed
- Game template scenes leaving duplicates of character in simulation when cycling environments
- Issue where proxy group update events were not being called
- Simulation selection mode changes when adding a synthetic image marker

### API Changes
- Added public `GetSimulatedObjectsRoot` delegate to `EditorOnlyDelegates`

## [0.9.16-preview] - 2020-05-27
### Fixed
- Exceptions when discovering simulated planes with debug vertices drawing enabled
- Documentation table of contents

## [0.9.15-preview] - 2020-05-25
### Fixed
- Performance issue in non-synthetic temporal simulation
- Issue where Simulation View or Device View would close itself on maximizing
- Ensure Sim Test Runner doesn't run during play Mode
- Memory/GC optimizations for simulated discovery
- Updated all documentation images
- Issue where exceptions are logged when choosing a template
- Issue where exceptions are logged when opening Database Viewer
- Issue where names in Templates window were clipped

## [0.9.14-preview] - 2020-05-22
### Fixed
- Misc issues that can occur when entitlements check fails

## [0.9.13-preview] - 2020-05-21
### Changed
- Rename package and all namespaces to remove "Labs"
- Updated entitlements logic to use production server
- Final docs review

## [0.9.12-preview] - 2020-05-19
### Added
- Give user ability to manually tweak focal length settings per-video playable asset, and have those changes reflected in the Simulation View. Will fall back to value defined in SimulationVideoContextSettings in Live mode.

### Changed
- Composite Rendering automatically enters fallback rendering mode when a Scriptable Render Pipeline (SRP) is in use.

## [0.9.11-preview] - 2020-05-19
### Fixed
- Reloading in play mode no longer causes errors in the Composite Renderer.
- Pressing "record" button with a synthetic recording selected in simulation now starts simulation in manual movement mode instead of playing the recording.
- Generated planes are now added to the `Simulation Environment` layer when created via plane extraction and saving planes from simulation.
- UI hints remain dismissed between Editor sessions.

### Added
- Camera permission dialog for Live simulation mode
- 'Simulation Data Visual Settings' Project Settings section.  This includes an option to set the Rating Gradient, the colors that are used to display feedback on the Compare Tool.
- Support for scriptable simulation environment mode.
- Game and training template moved to installable sample pack.

### Removed
- `CompositeRenderModule.SetupGameView()` & `CompositeRenderModule.TearDownGameView` have been made private.

## [0.9.10-preview] - 2020-05-15
### Fixed
- Specify earlier versions of Timeline and TextMeshPro to fix package dependency errors in later patch versions of 2019.3
- Simulation no longer stays in temporal mode when switching from Live or Recorded environment mode to Synthetic mode
- Fixed an issue where the MarkerCondition inspector after doesn't draw after Player builds
- Disabling simulate in play mode no longer causes errors when entering play mode - use DisallowAutoCreation flag for simulated discovery providers so that they do not get created unless the SimulatedDiscovery functionality island is active.
- Unity MARS enables the `Generate all .csproj files` preference on first import to fix an issue where IDE projects fail to find references to MARS types
- Pressing "play" button during temporal simulation to stop simulation now resets the state to a single-frame simulation, instead of leaving simulation in a state where data is gone but proxies are still matched.
- Game and training templates added

### Added
- MARSSession.EnsureSessionConfigured now checks if existing camera's near plane is greater than a max value, and logs a warning and sets it to the max value if so. It also sets the near plane for a newly created camera to a smaller default value.

## [0.9.9-preview] - 2020-05-11
### Fixed
- Use version defines to handle conditional compiling related to Post Processing package--fixes errors when Post Processing package is removed
- Use filesystem to copy default Unity MARS content into Assets folder

- Add PostProcessUtils to Bootstrap assembly to strip UNITY_POST_PROCESSING_STACK_V2 from player defines if post processing package is missing

## [0.9.8-preview] - 2020-05-10
### Changed
- Removed `bool forceTemporal` parameter from simulation restart methods and replaced with method `RequestSimulationModeSelection`, which enables explicit control over whether the next simulation is single-frame or temporal.
- Environment manager now auto-selects mode when loaded in edit mode as well as play mode. Fixes issue of mode mapped to camera facing getting potentially overwritten by accident when exiting play mode from a scene other than the opened edit scene.

### Added
- Support for face tracking and expressions data recordings in simulation
- Support for looping data recordings in simulation
- Face tracking and expressions data has been added to sample video recordings

### Removed
- A user-available layer is no longer used for simulation environment objects.
- The simulation content can no longer be hidden or locked in the Simulation View display options.

## [0.9.7-preview] - 2020-05-06
- Iterate on auto-import fix

## [0.9.6-preview] - 2020-05-06
- Fix content samples auto-import

## [0.9.5-preview] - 2020-05-06
### Added
- **Create Tool window**: 'Max Count' option. A non-1 value will set up a Replicator parent of the created Proxy. If 'Max Count' is disabled, the Replicator count will be unbounded (infinite).
- Compare Tool now supports Height Above Floor Condition
- **Set Pose Action**: 'Align with World Up' option. Allows for Proxies to keep their Y or Z axis aligned with the world Up direction.
- Live (Face) simulation environment mode now lets the user select which camera to use.
- Light Estimation visualizer to drive a light from ARCore/ARKit lighting estimates, and a simple simulation counterpart.

### Changed
- Synthetic image marker assets are now saved to `Assets/SyntheticImageMarkers`, rather than a folder associated with the active scene.
- Re-organized settings assets to reduce clutter in Assets folder
- Proxy Inspector: The 'Add MARS Component' menu will now only present the type of component being filtered, eg. if you have 'Conditions' selected as the filter, the button will only show Conditions to be added.

## [0.9.4-preview] - 2020-04-23
### Fixed
- Drag+Drop functionality restored
- Blank 'Create' window in Unity 2019.3.10
- 'Create' window colors when using Personal editor theme
- Face landmark droptargets (drag+drop to attach content to specific face regions) functionality restored for face proxies created from scratch

## [0.9.3-preview] - 2020-04-22
### Fixed
- Unused Default Environment is shown on first import

## [0.9.2-preview] - 2020-04-22
### API Changes
- Added `GetMarsSessionInScene()` and `GetMarsSessionInActiveScene()` to `MarsRuntimeUtils`

## [0.9.1-preview] - 2020-04-16
- Internal draft

## [0.9.0-preview] - 2020-04-16
### Added
- **Simulated Environment Modification**: Users can now save edits to Simulation environment made within the Simulation View. After a change has been made, the user will receive a prompt to save or discard changes upon switching environments or recompile.
	- **Simulated Image Marker Workflow**: Simulated image markers now provide an Inspector similar to the Marker Condition for selecting which library image to simulate. A button has been added to the MARS Panel &gt; Create Panel to add a new simulated marker to the current environment.  
- **Immersion Semantics**: “displayFlat” or “displaySpatial” for if a user is using a flat (phone, laptop, etc.) or spatial (VR, AR, etc.) devices.
- **User Type Support**: users can use their own data Trait types (class MyCondition : Condition<MyType>), and the backend code will auto-regenerate itself.

### API Changes
- Renamed `MARSRuntimeUtils` to `MarsRuntimeUtils`
	- Replaced `MarsRuntimeUtils.GetMainOrSimulatedCamera()` with `GetActiveCamera()` and `GetSessionAssociatedCamera()`. See [Finding the Camera](Documentation~/SoftwareDevelopmentGuide.md#finding-the-camera) documentation.
- Added documentation and exposed data structs in Unity.Labs.MARS.Recording namespace
- `SimulationSettings` properties (`EnvironmentMode`, `EnvironmentPrefab`, etc) are now non-static and require instance access.

### Changed
- Setup of video background in simulation happens entirely in `SimulationVideoContextManager`, removing dependency on face tracking providers to show video in simulation
- Simulation video framing happens via `MARSCamera` setting camera field of view, and framing happens in Device View rather than Simulation View
- Split up `SimulatedCameraProvider` into `SimulationCameraPoseProvider`, which only provides camera pose, and `SimulatedCameraViewProvider`, which provides camera texture, intrinsics, and projection matrix
- Updated color preferences (menu: **Edit &gt; Project Settings &gt; MARS &gt; User Preferences**): added 'Condition Fail Text Color', which is used by the Compare Tool to display unmatched data in the Inspector. Removed color preferences which are no longer being used.
- `SimulationSceneModule` has been changed to use the same scene for `ContentScene` and `EnvironmentScene` when using fallback composite rendering.
- New root game object added to simulation scenes; `ContentRoot` for `ContentScene` and `EnvironmentRoot` for `EnvironmentScene`
  - Objects added to the simulation scene from `AddContentGameObject()` or `AddEnvironmentGameObject()` will be added to that scene's root game object unless `keepAtRoot` is `true`.

### Fixed
- WorldScale now affects all lights and audio sources in a simulation environment.
- The 'Turn Into' options accessed from Unity's main menu: **GameObject &gt; MARS &gt; Turn Into &gt; ...** will now work as expected.
- Graphics issue where content could appear upside-down in Play mode in DX11 graphics mode.
- Recorded videos will now play correctly in simulated views without a face tracking library present.  

### Removed
- **UnityCloudStorageModule**: Cloud storage provider based on Google Cloud Storage

## [0.8.4-preview] - 2020-03-18
### Fixed
- Device View input bug on macOS

## [0.8.3-preview] - 2020-03-18
### Added
- Add support for simulated image markers by default in a new project
- Get & Set API for Unity MARS scene evaluation interval (see `IUsesMarsSceneEvaluation` interface)
- Add `ExcludedProviders` field on the `[ProviderPriority]` attribute, to simplify user setup of platform-specific providers.  See the [Provider Selection](Documentation~/SoftwareDevelopmentGuide.md#Providers) documentation.

### Fixed
- Fix face mask scale: face content is now at 1:1 scale by default
- Improved handling of Proxies being enabled/disabled at runtime
- Fix errors when instantiating SyntheticObject from script
- Fix visual artifacts introduced by composited rendering
- Fix errors in Forces Editor UI
- Removed erroneous component tabs from Proxy Groups and Replicators

### Updated
- All documentation for clarity & consistency
- Post Processing package no longer required, but supported
- Removed wandering horse from test scenes

## [0.8.2-preview] - 2020-03-03
### Added
#### MARS Proxy System
- **Forces**: allows configuration of spring-like and magnet-like forces for layout tuning, multi-occupancy, etc.
- **Priority**: option on Proxy which defines priority for matching against data ids
- **HeightAboveFloorCondition**: easy way to ensure that an object is off of the floor, and optionally by how much (use SemanticTagCondition and “floor” to ensure it is on the floor).
- **Better Filtering**: of component types in Proxy inspector UI.

#### Simulation System
- **Recordings in Play Mode**: Simulation recordings can now be used in Play Mode (select a recording in Simulation Controls, then press normal Play button)
- **Time Scrubbing**: Recordings can be scrubbed using Timeline (Simulation / Menu / Open Timeline).
- **Composite Rendering for Simulation**:
	- Improved multi camera rendering for displaying scene content composited and occluding the simulated environment.
	- Fixed issues with alpha blending in simulation and device views with content scene active.
	- Improved handling of render and image effect settings in composite views.

## [0.8.1-preview] - 2020-02-05
- Added Home Office and Open Office simulation environments
- Simulation environments now support playables
- Existing simulation environments updated with more objects
- Bugfix: adding MARS components with the "All" filter selected won't change the filter
- Add Provider Priority feature

## [0.8.0-preview] - 2020-02-12
- Add SpatialDataModule and dependency on com.unity.labs.spatial-hash: useful for doing efficient spatial queries into complex scenes

## [0.7.2-preview] - 2020-02-18
### Added
- Tabbed Proxy inspector for filtering which MARS components are displayed
- New simulation environments
    - Bedrooms
    - Dining Rooms
- Automated configuration detection on platforms requiring specific settings, including Magic Leap

### Changed
- Newly refreshed & easier to digest documentation.
- [Create](Documentation~/CreateTool.md) & [Compare](Documentation~/CompareTool.md) workflows

### Fixed
- Fix handles block simulation from auto-resync
- GeoLocation fixes

## [0.6.11-preview] - 2020-02-04
### Added
- Time scale to MarsTime
- Bedroom and dining room simulation environments
- Elective Extensions feature to help with Magic Leap builds
### Changed
- Update package dependencies
- Improve Create and Compare tool

## [0.6.10-preview] - 2020-01-30
### Fixed
- Fix issues in codegen templates which cause compile errors on import

## [0.6.9-preview] - 2020-01-30
### Removed
- Disable codegen in batch mode to fix CI issues in packages that depend on Unity MARS

## [0.6.8-preview] - 2020-01-29
### Added
- MARS Content folder

### Fixed
- fix issue with flatfloor condition
- fix issue with multi-relation condition
- fix issue with geolocation not re-evaluating
- fix issue with doubled normals an uvs
- Clean up and fix minor issues

## [0.6.7-preview] - 2020-01-23
### Added
- Image marker support
- Occluder mask to work with simulation environments with 4 walls and ceilings
- New simulation environments

## [0.1.1-preview.1] - 2020-01-10
### Changed
- Update terminology

## [0.1.1-preview] - 2020-01-03
### Changed
- Clean up and update documentation for package release
- Editor UI update

## [0.1.0-preview] - 2019-11-27
### Added
- initial version
