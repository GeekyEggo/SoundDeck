﻿/*
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2021 Antoine Aflalo
 * ref: https://github.com/Belphemur/SoundSwitch
 */

namespace SoundDeck.Core.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using NAudio.CoreAudioApi;

    [Guid(ComGuid.POLICY_CONFIG)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPolicyConfig
    {
        [PreserveSig]
        int GetMixFormat(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In] IntPtr ppFormat);

        [PreserveSig]
        int GetDeviceFormat(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In][MarshalAs(UnmanagedType.Bool)] bool bDefault,
            [In] IntPtr ppFormat);

        [PreserveSig]
        int ResetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);

        [PreserveSig]
        int SetDeviceFormat(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In] IntPtr pEndpointFormat,
            [In] IntPtr mixFormat);

        [PreserveSig]
        int GetProcessingPeriod(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In][MarshalAs(UnmanagedType.Bool)] bool bDefault,
            [In] IntPtr pmftDefaultPeriod,
            [In] IntPtr pmftMinimumPeriod);

        [PreserveSig]
        int SetProcessingPeriod(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In] IntPtr pmftPeriod);

        [PreserveSig]
        int GetShareMode(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In] IntPtr pMode);

        [PreserveSig]
        int SetShareMode(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In] IntPtr mode);

        [PreserveSig]
        int GetPropertyValue(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In][MarshalAs(UnmanagedType.Bool)] bool bFxStore,
            [In] IntPtr key,
            [In] IntPtr pv);

        [PreserveSig]
        int SetPropertyValue(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In][MarshalAs(UnmanagedType.Bool)] bool bFxStore,
            [In] IntPtr key,
            [In] IntPtr pv);

        [PreserveSig]
        int SetDefaultEndpoint(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In][MarshalAs(UnmanagedType.U4)] Role role);

        [PreserveSig]
        int SetEndpointVisibility(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName,
            [In][MarshalAs(UnmanagedType.Bool)] bool bVisible);
    }
}