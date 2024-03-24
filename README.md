# WhisperLite

A barebones C# wrapper for `whisper.cpp`.

## Usage

Here's a simple example of how to use the Whisper wrapper:

```csharp
// Create an instance of the Whisper class
Whisper whisper = new Whisper("path_to_model_file", useGPU = false);

// Initialize to default parameters
whisper.InitToDefaultParameters();
```

## TranscribeAudio Method

The TranscribeAudio method is a key function of the Whisper wrapper. It transcribes the provided audio samples into text.

```csharp
// Transcribe audio samples
float[] samples = GetAudioSamples(); // Replace with your method to get audio samples
string transcription = whisper.TranscribeAudio(samples);

Console.WriteLine(transcription);
```

The `TranscribeAudio` method takes two parameters:

- `samples`: The audio samples to transcribe.
- `keepAudio`: Optional parameter. If set to true, a portion of the audio samples and (all of) the most recent transcription tokens are retained for use in the next call. This facilitates continuous audio data processing, which should be useful for implementing streaming-like scenarios externally. Default value is false.
 
***Note**: actual implementation of streaming needs to be handled externally.*
*Tested on whisper.cpp v1.5.4 (Jan 5, 2024)*