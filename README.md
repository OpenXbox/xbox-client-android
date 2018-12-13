# Xbox One Gamestreaming Android app

[![Build status](https://ci.appveyor.com/api/projects/status/irv30kc612dbn75x/branch/master?svg=true)](https://ci.appveyor.com/project/tuxuser/xbox-client-android/branch/master)
[![Discord](https://img.shields.io/badge/discord-OpenXbox-blue.svg)](https://discord.gg/E8kkJhQ)

Use **Visual Studio** or **Visual Studio for Mac** to build this project.

The app is based on .NET library [OpenXbox.SmartGlass](https://github.com/OpenXbox/xbox-smartglass-csharp)

## Dependencies

- [Xamarin Android Framework](https://docs.microsoft.com/en-us/xamarin/android/)

## Building manually

```bash
export ANDROID_SDK_PATH="/path_to/Android/Sdk"
export ANDROID_NDK_PATH="/path_to/Android/Sdk/ndk-bundle"
export BUILD_TYPE="Debug"
# Or use:
# export BUILD_TYPE="Release"

# Build APK
msbuild /p:Configuration=${BUILD_TYPE} /p:AndroidSdkDirectory=${ANDROID_SDK_PATH} /t:PackageForAndroid

# Build APK & push to device
msbuild /p:Configuration=${BUILD_TYPE} /p:AndroidSdkDirectory=${ANDROID_SDK_PATH} /t:install
```

## Sneak preview

[![xNano Android Xbox Gamestreaming - alpha](https://img.youtube.com/vi/kHDYIsiFNaM/0.jpg)](https://www.youtube.com/watch?v=kHDYIsiFNaM "xNano Android Xbox Gamestreaming - alpha")
