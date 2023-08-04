# Unity Voice Processor

[![GitHub release](https://img.shields.io/github/release/Picovoice/unity-voice-processor.svg)](https://github.com/Picovoice/unity-voice-processor/releases)
[![GitHub](https://img.shields.io/github/license/Picovoice/unity-voice-processor)](https://github.com/Picovoice/unity-voice-processor/)

Made in Vancouver, Canada by [Picovoice](https://picovoice.ai)

<!-- markdown-link-check-disable -->
[![Twitter URL](https://img.shields.io/twitter/url?label=%40AiPicovoice&style=social&url=https%3A%2F%2Ftwitter.com%2FAiPicovoice)](https://twitter.com/AiPicovoice)
<!-- markdown-link-check-enable -->
[![YouTube Channel Views](https://img.shields.io/youtube/channel/views/UCAdi9sTCXLosG1XeqDwLx7w?label=YouTube&style=social)](https://www.youtube.com/channel/UCAdi9sTCXLosG1XeqDwLx7w)

The Unity Voice Processor is an asynchronous audio capture library designed for real-time audio
processing. Given some specifications, the library delivers frames of raw audio data to the user via
listeners.

## Table of Contents

- [Unity Voice Processor](#unity-voice-processor)
    - [Table of Contents](#table-of-contents)
    - [Compatibility](#compatibility)
    - [Installation](#installation)
    - [Usage](#usage)
        - [Capturing with Multiple Listeners](#capturing-with-multiple-listeners)
    - [Demo](#demo)

## Compatibility

[`Unity Voice Processor` package](./unity-voice-processor-1.0.0.unitypackage) unity package is for on **Unity 2017.4+** on the following platforms:

- Android 5.0+ (API 21+) (ARM only)
- iOS 11.0+
- Windows (x86_64)
- macOS (x86_64, arm64)
- Linux (x86_64)

## Installation

The easiest way to install the `Unity Voice Processor` is to import [unity-voice-processor-1.0.0.unitypackage](./unity-voice-processor-1.0.0.unitypackage) into your Unity projects by either dropping it into the Unity editor or going to _Assets>Import Package>Custom Package..._

## Usage

Access the singleton instance of `VoiceProcessor`:

```csharp
using Pv.Unity;

VoiceProcessor voiceProcessor = VoiceProcessor.Instance;
```

Create and add listeners for audio frames:

```csharp
void onFrameCaptured(short[] frame) {
    // use audio data
}

voiceProcessor.addFrameListener(onFrameCaptured);
```

Start audio capture with the desired frame length and audio sample rate:

```csharp
readonly int frameLength = 512;
readonly int sampleRate = 16000;

voiceProcessor.start(frameLength, sampleRate);
```

Stop audio capture:
```csharp
voiceProcessor.stop();
```

Once audio capture has started successfully, any frame listeners assigned to the `VoiceProcessor`
will start receiving audio frames with the given `frameLength` and `sampleRate`.

### Capturing with Multiple Listeners

Any number of listeners can be added to and removed from the `VoiceProcessor` instance. However,
the instance can only record audio with a single audio configuration (`frameLength` and `sampleRate`),
which all listeners will receive once a call to `start()` has been made. To add multiple listeners:
```csharp
void onFrameCaptured1(short[] frame) { }
void onFrameCaptured2(short[] frame) { }

VoiceProcessorFrameListener[] listeners = new VoiceProcessorFrameListener[] {
        listener1, listener2 
};

voiceProcessor.addFrameListeners(listeners);

voiceProcessor.removeFrameListeners(listeners);
// or
voiceProcessor.clearFrameListeners();
```

## Demo

The [Unity Voice Processor Demo](./Assets/UnityVoiceProcessor/Demo/) demonstrates how to ask for user permissions and capture output from
the `Unity Voice Processor`.
