using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Threading;
//using NAudio.Wave;
namespace 记事本
{
    public class dll
    {
        [DllImport("iatdemo.dll",EntryPoint = "run_iat", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern public unsafe static void run_iat(ref string src_wav_filename, ref string des_text_filename, ref string param);

        //[DllImport("iatdemo.dll", CallingConvention = CallingConvention.StdCall)]
        //public static extern void run_iat(string src_wav_filename, string des_text_filename, string param);
    }
}
