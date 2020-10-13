![Unity MARS Overview](images/mars-landing-image.png)

## Introduction

Unity Mixed and Augmented Reality Studio (Unity MARS) is a Unity extension that adds new functionality to support augmented and mixed reality content creation.

Augmented Reality has revealed that the tools we use to author 3D content are just a piece of a larger puzzle.
The canvas has morphed from being a strictly controlled and known digital environment to one intertwined with the real world.
Content authored for this new medium is not consumed in isolation anymore.
Instead, it has to be reactive and work with the world and other apps in ways never required before.

MARS is a Unity extension and a set of companion apps that provide the ability to address real-world objects and events as GameObjects.
It comes with a new UI and controls for this dynamic content.
MARS  includes an entirely new Simulation mode which lets you test your content in different real-world mockups with an incredibly tight iteration time, and helps you author content in the context of a real-world environment.

Most importantly, Unity MARS responds to real-world and other AR content by default.
Reality is your build target.
Think big, design to match, and start making content to change the world!

[Getting started](GettingStarted.md) is a guide for new users. It takes you through how to write a small but complete application that reacts to the environment.

This documentation contains the following in-depth guides for each section of Unity MARS:

* [Unity MARS UI overview](UIOverview.md) - Overview of the different MARS windows.
* [Overview of core Unity MARS concepts](MARSConcepts.md) - Commonly used terms and concepts across MARS.
* [Working with Unity MARS](WorkingWithMARS.md) - Common workflows for creating AR apps with MARS.
* __Basic app creation:__
    * [Basic face tracking](FaceTracking.md) - Tracking faces in MARS and attaching virtual objects to face features.
    * [Image marker tracking](Markers.md) - Creating and working with images and tracking them.
    * [Forces for tuning alignment](Forces.md) - Creating and working with Forces to configure alignment regions and spatial orientations.
* __Unity MARS Simulation:__
    * [Creating Simulation environments](SimulationEnvironments.md) - Set up custom synthetic environments and recordings to quickly preview how your content will behave to found surfaces and other world data, using environments that are relevant to your project.
    * [Session Recordings](SessionRecordings.md) - Record data from real and simulated MR sessions for later testing on your  MARS app.
* __Developer topics:__
    * [Landmarks](Landmarks.md) - Working with spatial data such as points, edges, polygons that break data from the real world into useful parts for anchoring or aligning virtual content.
    * [Traits](Traits.md) - Pieces of data that represent how information is stored and how conditions, actions, and providers manipulate them inside MARS.
    * [Priority](Priority.md) - Specifying the relative importance of different parts of your scene's content.
    * [Software development guide](SoftwareDevelopmentGuide.md) - Check this if you need to do something really specific for writing your own custom behaviors or if you want to extend MARS.


You can find a list of new features and deprecations in the [Unity MARS Changelog](../CHANGELOG.md).

The [FAQ](FAQ.md) answers many common questions that have been answered previously about design, implementation, and use of Unity MARS.

Finally, to quickly understand the terminology used across this documentation, the [Glossary](Glossary.md) is the place where common keywords are defined.


## Requirements
The current version of Unity MARS is compatible with the following versions of the Unity Editor:

* 2019.2.x
* 2019.3.x
* 2020.1 alpha - Not fully tested, MARS placement UI missing.
* Unity MARS uses the `.NET 4.x Equivalent` runtime and is incompatible with the legacy scripting runtime.
