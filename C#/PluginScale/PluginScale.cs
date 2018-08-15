/*
  Copyright (C) 2014 Birunthan Mohanathas

  This program is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Rainmeter;

// Overview: This example demonstrates the basic concept of Rainmeter C# plugins.

// Sample skin:
/*
    [Rainmeter]
    Update=1000
    BackgroundMode=2
    SolidColor=000000

    [mString]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=String

    [mMajor]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=Major

    [mMinor]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=Minor

    [mNumber]
    Measure=Plugin
    Plugin=SystemVersion.dll
    Type=Number

    [Text1]
    Meter=STRING
    MeasureName=mString
    MeasureName2=mMajor
    MeasureName3=mMinor
    MeasureName4=mNumber
    X=5
    Y=5
    W=300
    H=70
    FontColor=FFFFFF
    Text="String: %1#CRLF#Major: %2#CRLF#Minor: %3#CRLF#Number: %4#CRLF#"

    [Text2]
    Meter=STRING
    MeasureName=mString
    MeasureName2=mMajor
    MeasureName3=mMinor
    MeasureName4=mNumber
    NumOfDecimals=1
    X=5
    Y=5R
    W=300
    H=70
    FontColor=FFFFFF
    Text="String: %1#CRLF#Major: %2#CRLF#Minor: %3#CRLF#Number: %4#CRLF#"
*/



namespace PluginScale {
  public class Measure {
    private const string DLL_NAME = "Scale.dll";
    private const float SCALE_NORM = 1 / 96f;



    private bool _dpiAwareSetted;



    internal void Reload(API rm, ref double maxValue) { }



    internal double Update() {
      return GetScalingFactor();
    }



    internal string GetString() {
      return GetScaling();
    }



    [DllImport( "gdi32.dll" )]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);



    public enum DeviceCap {
      VERTRES = 10,
      DESKTOPVERTRES = 117,
      LOGPIXELSX = 88,
      LOGPIXELSY = 90

      // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
    }



    [DllImport( "user32.dll" )]
    static extern bool SetProcessDPIAware();



    [DllImport( "user32.dll" )]
    static extern IntPtr GetDC(IntPtr hWnd);



    public string GetScaling() {
      if (!_dpiAwareSetted)
        try {
          SetProcessDPIAware();
          _dpiAwareSetted = true;
        }
        catch (Exception) {
          API.Log( API.LogType.Error, $"{DLL_NAME}: Error calling SetProcessDPIAware" );
        }

      return GetScalingFactor().ToString( CultureInfo.InvariantCulture );
    }



    private float GetScalingFactor() {
      var desktop = GetDC( IntPtr.Zero );
/*
      var logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
      var physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
      return physicalScreenHeight / (float)logicalScreenHeight; // 1.25 = 125%
*/
      var dpiY = GetDeviceCaps( desktop, (int) DeviceCap.LOGPIXELSY );
      return dpiY * SCALE_NORM;
    }
  }



  public static class Plugin {
    static IntPtr _stringBuffer = IntPtr.Zero;

    private static readonly Measure Measure = new Measure();



    [DllExport]
    public static void Initialize(ref IntPtr data, IntPtr rm) {
      data = GCHandle.ToIntPtr( GCHandle.Alloc( Measure ) );
    }



    [DllExport]
    public static void Finalize(IntPtr data) {
      GCHandle.FromIntPtr( data ).Free();

      if (_stringBuffer == IntPtr.Zero)
        return;
      Marshal.FreeHGlobal( _stringBuffer );
      _stringBuffer = IntPtr.Zero;
    }



    [DllExport]
    public static void Reload(IntPtr data, IntPtr rm, ref double maxValue) {
      ( (Measure) GCHandle.FromIntPtr( data ).Target ).Reload( new API( rm ), ref maxValue );
    }



    [DllExport]
    public static double Update(IntPtr data) {
      return ( (Measure) GCHandle.FromIntPtr( data ).Target ).Update();
    }



    [DllExport]
    public static IntPtr GetString(IntPtr data) {
      var measure = (Measure) GCHandle.FromIntPtr( data ).Target;
      if (_stringBuffer != IntPtr.Zero) {
        Marshal.FreeHGlobal( _stringBuffer );
        _stringBuffer = IntPtr.Zero;
      }

      var stringValue = measure.GetString();
      if (stringValue != null)
        _stringBuffer = Marshal.StringToHGlobalUni( stringValue );
      return _stringBuffer;
    }
  }
}