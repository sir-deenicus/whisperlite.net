using System;
using System.Runtime.InteropServices;

namespace WhisperLite.Net
{
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WhisperNewSegmentCallback(IntPtr ctx, IntPtr state, int n_new, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WhisperProgressCallback(IntPtr ctx, IntPtr state, int progress, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate byte WhisperEncoderBeginCallback(IntPtr ctx, IntPtr state, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WhisperLogitsFilterCallback(IntPtr ctx, IntPtr state, IntPtr tokens, int n_tokens, IntPtr logits, IntPtr user_data);
 
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate byte GgmlAbortCallback(IntPtr data);


    [StructLayout(LayoutKind.Sequential)]
    public struct WhisperContextParams
    {
        public byte UseGPU;
        public int GPUDevice;
    }

    public enum WhisperSamplingStrategy
    {
        WhisperSamplingGreedy,      // similar to OpenAI's GreedyDecoder
        WhisperSamplingBeamSearch, // similar to OpenAI's BeamSearchDecoder
    } 

    [StructLayout(LayoutKind.Sequential)]
    public struct Greedy
    {
        public int best_of;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BeamSearch
    {
        public int beam_size;
        public float patience;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WhisperFullParams
    {
        public WhisperSamplingStrategy strategy;
        public int n_threads;
        public int n_max_text_ctx;
        public int offset_ms;
        public int duration_ms;
        public byte translate;
        public byte no_context;
        public byte no_timestamps;
        public byte single_segment;
        public byte print_special;
        public byte print_progress;
        public byte print_realtime;
        public byte print_timestamps;
        public byte token_timestamps;
        public float thold_pt;
        public float thold_ptsum;
        public int max_len;
        public byte split_on_word;
        public int max_tokens;
        public byte speed_up;
        public byte debug_mode;
        public int audio_ctx;
        public byte tdrz_enable;
 
        public IntPtr initial_prompt;
        public IntPtr prompt_tokens;
        public int prompt_n_tokens;
 
        public IntPtr language;
        public byte detect_language;
        public byte suppress_blank;
        public byte suppress_non_speech_tokens;
        public float temperature;
        public float max_initial_ts;
        public float length_penalty;
        public float temperature_inc;
        public float entropy_thold;
        public float logprob_thold;
        public float no_speech_thold;
        public Greedy greedy;
        public BeamSearch beam_search;
        public WhisperNewSegmentCallback new_segment_callback;

        public IntPtr new_segment_callback_user_data;
        public WhisperProgressCallback progress_callback;
        public IntPtr progress_callback_user_data;
        public WhisperEncoderBeginCallback encoder_begin_callback;
        public IntPtr encoder_begin_callback_user_data;
        public GgmlAbortCallback abort_callback;
        public IntPtr abort_callback_user_data;
        public WhisperLogitsFilterCallback logits_filter_callback;
        public IntPtr logits_filter_callback_user_data;
        public IntPtr grammar_rules;
        public UIntPtr n_grammar_rules;
        public UIntPtr i_start_rule;
        public float grammar_penalty;
    }

    public static class NativeMethods
    {
        const string libraryName = "whisper.dll"; 

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern WhisperContextParams whisper_context_default_params(); 
        
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr whisper_init_from_file_with_params(string path, WhisperContextParams parameters);
 
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern WhisperFullParams whisper_full_default_params(WhisperSamplingStrategy strategy);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int whisper_full(IntPtr ctx, WhisperFullParams parameters, float[] samples, int n_samples);
 
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int whisper_full_n_segments(IntPtr ctx);
          
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr whisper_full_get_segment_text(IntPtr ctx, int i_segment);

        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void whisper_free(IntPtr ctx);
 
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int whisper_full_n_tokens(IntPtr ctx, int i_segment);
 
        [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int whisper_full_get_token_id(IntPtr ctx, int i_segment, int i_token);        
    }
}
