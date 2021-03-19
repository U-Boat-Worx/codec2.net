using System;
using System.Runtime.InteropServices;

public class codec2
{
    public static readonly int CODEC2_MODE_3200 = 0;
    public static readonly int CODEC2_MODE_2400 = 1;
    public static readonly int CODEC2_MODE_1600 = 2;
    public static readonly int CODEC2_MODE_1400 = 3;
    public static readonly int CODEC2_MODE_1300 = 4;
    public static readonly int CODEC2_MODE_1200 = 5;
    public static readonly int CODEC2_MODE_700 = 6;
    public static readonly int CODEC2_MODE_700B = 7;
    public static readonly int CODEC2_MODE_700C = 8;
    public static readonly int CODEC2_MODE_450 = 10;
    public static readonly int CODEC2_MODE_450PWB = 11;

    [DllImport("libcodec2")]
    public static extern IntPtr codec2_create(int mode);

    [DllImport("libcodec2")]
    public static extern void codec2_destroy(IntPtr codec2_state);

    [DllImport("libcodec2")]
    public static extern void codec2_encode(IntPtr codec2_state, [In,Out] byte[] bits, short[] speech_in);

    [DllImport("libcodec2")]
    public static extern void codec2_decode(IntPtr codec2_state, [In,Out] short[] speech_out, byte[] bits);

    [DllImport("libcodec2")]
    public static extern void codec2_decode_ber(IntPtr codec2_state, [In,Out] short[] speech_out, byte[] bits, float ber_est);

    [DllImport("libcodec2")]
    public static extern int codec2_samples_per_frame(IntPtr codec2_state);

    [DllImport("libcodec2")]
    public static extern int codec2_bits_per_frame(IntPtr codec2_state);

    // borrowed from sine.h
    [DllImport("libcodec2")]
    public static extern int codec2_rand();

//     // TODO
//     [DllImport("libcodec2")]
//     static extern void codec2_set_lpc_post_filter(ref IntPtr codec2_state, int enable, int bass_boost, float beta, float gamma);
//
//     [DllImport("libcodec2")]
//     static extern int codec2_get_spare_bit_index(ref IntPtr codec2_state);
//
//     [DllImport("libcodec2")]
//     static extern int codec2_rebuild_spare_bit(ref IntPtr codec2_state, int unpacked_bits[]);
//
//     [DllImport("libcodec2")]
//     static extern void codec2_set_natural_or_gray(ref IntPtr codec2_state, int gray);
//
//     [DllImport("libcodec2")]
//     static extern void codec2_set_softdec(ref IntPtr c2, FloatPtr softdec);
//
//     [DllImport("libcodec2")]
//     static extern float codec2_get_energy(ref IntPtr codec2_state,  const string bits);
//
// // support for ML and VQ experiments
//     [DllImport("libcodec2")]
//     static extern void codec2_open_mlfeat(ref IntPtr codec2_state, string filename);
//
//     [DllImport("libcodec2")]
//     static extern void codec2_load_codebook(ref IntPtr codec2_state, int num, string filename);
//
//     [DllImport("libcodec2")]
//     static extern float codec2_get_var(ref IntPtr codec2_state);
//
//     [DllImport("libcodec2")]
//     static extern FloatPtr codec2_enable_user_ratek(ref IntPtr codec2_state, IntPtr K);
//
// // 700C post filter and equaliser
//     [DllImport("libcodec2")]
//     static extern void codec2_700c_post_filter(ref IntPtr codec2_state, int en);
//
//     [DllImport("libcodec2")]
//     static extern void codec2_700c_eq(ref IntPtr codec2_state, int en);
}