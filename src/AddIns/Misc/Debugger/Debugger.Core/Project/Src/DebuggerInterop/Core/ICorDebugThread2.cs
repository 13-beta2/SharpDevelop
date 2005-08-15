namespace DebuggerInterop.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("2BD956D9-7B07-4BEF-8A98-12AA862417C5"), ComConversionLoss]
    public interface ICorDebugThread2
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetActiveFunctions([In] uint cFunctions, out uint pcFunctions, [In, Out] IntPtr pFunctions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetConnectionID(out uint pdwConnectionId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTaskID(out ulong pTaskId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetVolatileOSThreadID(out uint pdwTid);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InterceptCurrentException([In, MarshalAs(UnmanagedType.Interface)] ICorDebugFrame pFrame);
    }
}

