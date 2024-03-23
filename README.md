# WhisperLite

Whisper is a barebones C# wrapper for `whisper.cpp`.

## Usage

Here's a simple example of how to use the Whisper wrapper:

```csharp
// Create an instance of the Whisper class
Whisper whisper = new Whisper("path_to_model_file", useGPU = false);

// Initialize to default parameters
whisper.InitToDefaultParameters();

// Transcribe audio samples
float[] samples = GetAudioSamples(); // Replace with your method to get audio samples
string transcription = whisper.TranscribeAudio(samples);

Console.WriteLine(transcription);
```

For continuous audio data processing, set the `keepAudio` parameter to `true`. This retains a portion of the audio samples and transcription tokens from the current call for use in the next call, enabling streaming-type scenarios:

```csharp
// Transcribe first chunk of audio
float[] samples1 = GetFirstAudioChunk(); // Replace with your method to get audio chunks
string transcription1 = whisper.TranscribeAudio(samples1, keepAudio = true);

// Transcribe second chunk of audio
float[] samples2 = GetSecondAudioChunk(); // Replace with your method to get audio chunks
string transcription2 = whisper.TranscribeAudio(samples2, true); 
```

Note: actual implementation of streaming needs to be handled externally. The `TranscribeAudio` method should be called appropriately to process the continuous audio data.