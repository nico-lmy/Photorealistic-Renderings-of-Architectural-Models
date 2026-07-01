# Stereolab

## Overview

This package is to provide an accessible and generalist tool for [CAVE](https://en.wikipedia.org/wiki/Cave_automatic_virtual_environment)-like systems. Thus, it supports independently:

- Multiple displays coherently projected in space;
- Stereoscopy to display 3D images;
- Motion-capture and interaction systems.
## Package contents

- StereoProjection: Tools for multiple displays coherent in space, parallax and stereoscopy;
- Tracking: Tools for motion-capture synchronization with elements in the scene;
- Interactions: Tools to interact with the scene;
- Samples:
	- Workbench: Sample configuration for a workbench system.

## Installation instructions

1. In `Edit/Project settings`, go to `Package Manager`;
2. Add the following entry to the list of scoped registries:
	- Name: `vrbooking`;
	- URL: `https://vrbooking.irisa.fr/npm`;
	- Scope(s): `fr.inria`.
3. Open the `Package Manager`, and in the `+` menu (top-left of the screen), add a package using a git URL;
4. Paste the following link: `https://forge.icube.unistra.fr/invirtuo/stereolab.git?path=/Packages/fr.icube.gaia.stereolab`. A specific tag or branch can be selected by appending it with `#` and the tag/branch name.

## Requirements

- Unity version supported: 2022.3 (tested on [2022.3.10f1](https://unity.com/releases/editor/whats-new/2022.3.10));
- If the stereoscopy option is enabled: active stereoscopy material is required (passive is not currently supported). Thus it requires active glasses and a synchronization hub (ie: [ActivHub-RF50](http://volfoni.com/en/activhub-rf50/));
- If motion-capture is desired: a [VRPN](https://github.com/vrpn/vrpn/wiki) server must be launched separately, as well as the motion-capture software.

## How to use

### Creating a simple multi-screen display

1. Create a prefab inheriting the `StereolabInstance` prefab or with a `StereolabInstance` script as a component;
2. Add as many `ProjectionPlane` instances as there are displays in the setup;
3. Set them up to match the real displays position, rotation, size, resolution and index;
4. Create an empty `GameObject` that will be the user point of view, and add the same amount of `ProjectionPlaneCamera` as children of it;
5. Assign a `ProjectionPlane` per `ProjectionPlaneCamera` in the `Projection screen` property, its parent `GameObject` as `View point controller`, and enable stereoscopy if desired;
6. Integrate the created prefab in your working scene.

By doing so, all the screens display a projection of camera at  `View point controller` on the matching plane. Moving it at runtime will change the display, which might seem weird at a third person point of view but create a perfect parallax effectif the user is positioned exactly at the same place in real life.

### Setting up a simple motion-capture setup

1. Go to `Project settings/VRPN Input`;
2. For each VRPN input source, add an entry with:
	- Input name: Name of the device that will be accessible in the Unity input system;
	- VRPN device name: Name of the VRPN device communicated by the server
	- VRPN server URL: Hostname/IP of the VRPN server
	- Index: Index of the input data to be linked to the device;
	- Input type: Data structure provided by the device
		- Tracker: A structure containing `devicePosition` (Vector3), `deviceRotation` (Quaternion), `isTracked` (boolean) and `trackingState` (integer);
		- Button: `button` (ButtonControl);
		- Axis: `axis` (float).
3. Export the input settings as a JSON file; 
4. Create an Input Action asset, with a position (Value, Vector3) and rotation (Value, Quaternion) action for each tracker previously added;
5. Add the Input Action asset to the `PlayerInput` `GameObject` of your scene;
6. Create an empty `GameObject` and set its transform to serve as the origin of the motion-capture coordinates;
7. For each tracker, create a child with the `Tracker` script component, and set its `InputActionReference` properties to the actions mapped to the position and rotation of the tracker.

> The whole package is meant to be used with the new Unity [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.6/manual/index.html).