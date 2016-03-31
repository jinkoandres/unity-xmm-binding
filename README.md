# Unity XMM binding

This repository is a Unity 5 project showing a simple use case of the XMM library.
It contains a folder containing XMM as a native plugin project for Android.
The native plugin is already compiled and the apk file is also provided for convenience.

## Compilation and installation

### XMM-Demo

In Unity 5, choose "Open project" and select the repository's folder.  
Select "File > Build & Run" and click "Build and Run".  
If you have a smartphone in debug mode connected to your computer, this will build, install and launch the application.

### XMM Native Plugin for Android

In order to modify / recompile the native plugin, you will have to download the latest [Android NDK](http://developer.android.com/tools/sdk/ndk/index.html), and add the NDK folder's path to your PATH.  
You will also need the [XMM library](https://github.com/julesfrancoise/xmm). Download it and add a symlink to the xmm folder in `XmmNativeAndroidPlugin/engine/`.  
You can then run the `build.sh` script in `XmmNativeAndroidPlugin/` which will compile and install the library in `Assets/Plugins/` so that the Unity project gets automatically updated.

## What it actually does

This application uses GMMs (Gaussian Mixture Models) together with gesture descriptors to recognize motion attitudes, such as "Still", "Walk", and "Run".  
The software gets the gyroscope data on each frame, estimates its energy, frequency and periodicity, and uses these three features to train the models and to perform the recognition.  
To use it, you have to train the model first : you do this by recording some mouvement sequence, selecting a label (e.g. "Still", "Walk" or "Run"), then adding the last recording with the current label to the training set (this will automatically trigger the training and the models will be updated in real-time).  
The text label at the bottom of the screen shows the currently estimated label.

## Contact

<joseph.larralde@ircam.fr>