using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace WhisperLite.Net
{
    public class Whisper : IDisposable
    {
        public IntPtr WhisperContext { get; private set; }
        public WhisperFullParams parameters;

        public List<int> promptTokens = new List<int>();

        public float[] prevAudioData;

        int sampleRate = 16000;

        int keepSamplesLen = 0;

        int previousSamplesLen = 0;

        public string Language
        {
            get
            {
                if (parameters.language == IntPtr.Zero)
                    return "";
                return Marshal.PtrToStringAnsi(parameters.language);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Whisper class.
        /// </summary>
        /// <param name="path">The path to the model file.</param>
        /// <param name="useGPU">Optional parameter. If set to true, GPU is used for processing. Default value is false.</param>
        /// <param name="keepAudioSecs">Optional parameter. The length of audio data to keep for the next transcription, in seconds. Default value is 0.0f.</param> 
        public Whisper(string path, bool useGPU = false, float keepAudioSecs = 0.0f)
        {
            WhisperContextParams cparameters = NativeMethods.whisper_context_default_params();
            cparameters.UseGPU = useGPU ? (byte)1 : (byte)0; 

            WhisperContext = NativeMethods.whisper_init_from_file_with_params(path, cparameters);
            if (WhisperContext == IntPtr.Zero)
            {
                throw new Exception("Failed to initialize Whisper from file");
            }
  
            keepSamplesLen = (int)(keepAudioSecs * sampleRate); 
            prevAudioData = new float[keepSamplesLen];
        }

        /// <summary>
        /// Sets the length of audio data to keep for the next transcription.
        /// </summary>
        /// <param name="keepAudioSecs">The length of audio data to keep, in seconds.</param> 
        public void SetKeepAudioSecs(float keepAudioSecs)
        { 
            keepSamplesLen = (int)(keepAudioSecs * sampleRate);
            prevAudioData = new float[keepSamplesLen];
        }

        /// <summary>
        /// Initializes the Whisper instance to its default parameters.
        /// </summary>
        /// <param name="strategy">The sampling strategy to use. Default is WhisperSamplingStrategy.WhisperSamplingGreedy.</param>
        /// <param name="DoTranslate">Whether to perform translation. Default is false.</param>
        /// <param name="NumThreads">The number of threads to use. Default is 4.</param>
        /// <param name="Language">The language to use for transcription. Default is "en".</param> 
        public void InitToDefaultParameters(WhisperSamplingStrategy strategy = WhisperSamplingStrategy.WhisperSamplingGreedy, bool DoTranslate = false, int NumThreads = 4, string Language = "en")
        {
            parameters = NativeMethods.whisper_full_default_params(strategy);  
            parameters.translate = DoTranslate ? (byte)1 : (byte)0;
            parameters.n_threads = NumThreads;
            parameters.language = Marshal.StringToHGlobalAnsi(Language);
        }

        /// <summary>
        /// Transcribes the provided audio samples into text.
        /// </summary>
        /// <param name="samples">The audio samples to transcribe.</param>
        /// <param name="keepAudio">Optional parameter. If set to true, a portion of the audio samples and transcription tokens is retained for use in the next call. This facilitates continuous audio data processing, which is crucial for implementing streaming-like scenarios externally. Default value is false.</param>
        /// <returns>The transcription of the audio samples as a string.</returns>
        public string TranscribeAudio(float[] samples, bool keepAudio = false)
        {  
            float[] audiosamples = (previousSamplesLen != 0) ? prevAudioData.Take(previousSamplesLen).Concat(samples).ToArray() : samples;

            IntPtr promptptr = IntPtr.Zero;

            if(previousSamplesLen != 0)
            { 
                promptptr = Marshal.AllocHGlobal(promptTokens.Count * sizeof(int));
                Marshal.Copy(promptTokens.ToArray(), 0, promptptr, promptTokens.Count);

                parameters.prompt_n_tokens = promptTokens.Count;
                parameters.prompt_tokens = promptptr;  
            }

            NativeMethods.whisper_full(WhisperContext, parameters, audiosamples, audiosamples.Length);
 
            if(promptptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(promptptr); 
                promptptr = IntPtr.Zero;
            }
            
            StringBuilder sb = new StringBuilder();
            promptTokens.Clear();

            if(keepAudio)
            {
                var keeplen = Math.Min(keepSamplesLen, samples.Length);
                previousSamplesLen = keeplen; 
                Array.Copy(samples, samples.Length - keeplen, prevAudioData, 0, keeplen); 
            }

            var nsegments = NativeMethods.whisper_full_n_segments(WhisperContext);
            for(int i = 0; i < nsegments; i++)
            {
                var segment = NativeMethods.whisper_full_get_segment_text(WhisperContext, i);  
                sb.Append(Marshal.PtrToStringAnsi(segment)); 

                if(keepAudio)
                {
                    var tokenslen = NativeMethods.whisper_full_n_tokens(WhisperContext, i); 
                    for(int j = 0; j < tokenslen; j++)
                    {
                        promptTokens.Add(NativeMethods.whisper_full_get_token_id(WhisperContext, i, j));
                    }
                }
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            if (parameters.language != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(parameters.language);
                parameters.language = IntPtr.Zero;
            }

            if (WhisperContext != IntPtr.Zero)
            {
                NativeMethods.whisper_free(WhisperContext);
                WhisperContext = IntPtr.Zero;
            }  
        }
    }
}
          