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

        public void SetKeepAudioSecs(float keepAudioSecs)
        { 
            keepSamplesLen = (int)(keepAudioSecs * sampleRate);
            prevAudioData = new float[keepSamplesLen];
        }

        public void InitToDefaultParameters(WhisperSamplingStrategy strategy = WhisperSamplingStrategy.WhisperSamplingGreedy, bool DoTranslate = false, int NumThreads = 4, string Language = "en")
        {
            parameters = NativeMethods.whisper_full_default_params(strategy);  
            parameters.translate = DoTranslate ? (byte)1 : (byte)0;
            parameters.n_threads = NumThreads;
            parameters.language = Marshal.StringToHGlobalAnsi(Language);
        }

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
          