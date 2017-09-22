using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Windows.Media.Imaging
{
    internal static class NativeMethods
    {
        [TargetedPatchingOptOut("Internal method only, inlined across NGen boundaries for performance reasons")]
        internal static unsafe void CopyUnmanagedMemory(byte* srcPtr, int srcOffset, byte* dstPtr, int dstOffset, int count)
        {
            srcPtr += srcOffset;
            dstPtr += dstOffset;

            memcpy(dstPtr, srcPtr, count);
        }

        [TargetedPatchingOptOut("Internal method only, inlined across NGen boundaries for performance reasons")]
        internal static void SetUnmanagedMemory(IntPtr dst, int filler, int count)
        {
            memset(dst, filler, count);
        }

        // Win32 memory copy function
        //[DllImport("ntdll.dll")]
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern unsafe void* memcpy(
            void* dst,
            void* src,
            int count);
        
        // Win32 memory set function
        //[DllImport("ntdll.dll")]
        //[DllImport("coredll.dll", EntryPoint = "memset", SetLastError = false)]
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern void memset(
            IntPtr dst,
            int filler,
            int count);
    }
}
