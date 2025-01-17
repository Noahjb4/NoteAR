# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2020-06-18
- Fix moving image tracking

## [1.0.1] - 2020-06-02
- Updated Unity MARS license

## [1.0.0] - 2020-05-29
- Set version number to 1.0.0 for public release

## [0.1.25-preview] - 2020-05-27
- Bump dependency version of MARS

## [0.1.24-preview] - 2020-05-25
- Bump dependency version of MARS

## [0.1.23-preview] - 2020-05-22
- Bump dependency version of MARS

## [0.1.22-preview] - 2020-05-21
- Rename package and all namespaces to remove "Labs"
- Bump dependency versions

## [0.1.21-preview] - 2020-05-19
- Bump dependency version of MARS

## [0.1.20-preview] - 2020-05-19
- Bump dependency version of MARS and Module Loader

## [0.1.19-preview] - 2020-05-15
- Update version defines to resolve compile errors on various versions of AR foundation
- Remove explicit dependency on com.unity.xr.legacyinputhelpers to avoid package resolution errors
- Add documentation, including recommended AR Foundation and XR Plugin versions
- Destroy meshes on tracking loss in ARFoundationFaceTrackingProvider

## [0.1.18-preview] - 2020-05-11
- Fix issues with importing image marker libraries

## [0.1.17-preview] - 2020-05-11
- republishing since 0.1.16 artifact is corrupt

## [0.1.16-preview] - 2020-05-10
- Bump dependency version of mars

## [0.1.15-preview] - 2020-05-06
- Bump dependency version of mars

## [0.1.14-preview] - 2020-05-06
- Bump dependency version of mars

## [0.1.13-preview] - 2020-05-06
- Fix version defines to bring back AR Foundation 3.x features
- Add version defines for AR Foundation 4.x
- Add Light Estimation provider

## [0.1.12-preview] - 2020-04-24
- Bump dependency version of mars

## [0.1.11-preview] - 2020-04-22
- Bump dependency version of mars

## [0.1.10-preview] - 2020-04-22
- Remove unsafe code for converting TrackableId to MarsTrackableId
- Add version defines to make ARKit face tracking package optional and allow use with AR Foundation 2.x

## [0.1.9-preview] - 2020-04-16
- Fix an issue where scenes with existing ARSession components didn't work with MARS
- Always return eye open expression on AR Core due to low accuracy
- Add CameraFacingDirection to ARFoundationCameraProvider and update GetCameraTexture to return a Texture instaed of a Texture2D

## [0.1.8-preview] - 2020-03-19
- Bump dependency version of mars

## [0.1.7-preview] - 2020-03-17
- Add ExcludedPlatforms for all provider classes to indicate incompatibility with Editor/Standalone
- Fix an issue where MarkerProviderSettings could lose track of image libraries
- Rename provider prefix from XRSDK to ARFoundation to reflect the current implementation
- Bump dependency version of mars

## [0.1.6-preview] - 2020-03-03
- Remove ScriptableSettingsPath attribute where it is not needed
- Bump dependency version of mars

## [0.1.5-preview] - 2020-02-25
- Bump dependency version of mars

## [0.1.4-preview] - 2020-02-20
- Bump dependency version of mars

## [0.1.3-preview] - 2020-02-20
- Bump dependency version of mars

## [0.1.2-preview] - 2020-02-17
- Bump dependency version of mars

## [0.1.1-preview] - 2020-02-04
- Update package dependencies

## [0.1.0-preview] - 2020-01-28

### This is the first release of *MARS AR Foundation Providers*.

This version serves as the foundation for the initial package release of MARS AR Foundation Providers
