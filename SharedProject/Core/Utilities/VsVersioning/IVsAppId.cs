using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FineCodeCoverage.Core.Utilities
{
    [Guid("1EAA526A-0898-11d3-B868-00C04F79F802")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IVsAppId
    {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider pSP);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetProperty(int propid, [MarshalAs(UnmanagedType.Struct)] out object pvar);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetProperty(int propid, [MarshalAs(UnmanagedType.Struct)] object var);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetGuidProperty(int propid, out Guid guid);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetGuidProperty(int propid, ref Guid rguid);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Initialize();
    }
}
