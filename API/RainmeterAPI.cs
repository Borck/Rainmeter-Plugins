/*
  Copyright (C) 2011 Birunthan Mohanathas

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
using System.Runtime.InteropServices;

namespace Rainmeter
{
    /// <summary>
    /// Wrapper around the Rainmeter C API.
    /// </summary>
    public class API
    {
        private readonly IntPtr _mRm;

        public API(IntPtr rm)
        {
            _mRm = rm;
        }

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr RmReadString(IntPtr rm, string option, string defValue, bool replaceMeasures);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private static extern double RmReadFormula(IntPtr rm, string option, double defValue);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr RmReplaceVariables(IntPtr rm, string str);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr RmPathToAbsolute(IntPtr rm, string relativePath);

        [DllImport("Rainmeter.dll", EntryPoint = "RmExecute", CharSet = CharSet.Unicode)]
        public static extern void Execute(IntPtr skin, string command);

        [DllImport("Rainmeter.dll")]
        private static extern IntPtr RmGet(IntPtr rm, RmGetType type);

        [DllImport("Rainmeter.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LSLog(LogType type, string unused, string message);

        private enum RmGetType
        {
            MeasureName = 0,
            Skin = 1,
            SettingsFile = 2,
            SkinName = 3,
            SkinWindowHandle = 4
        }

        public enum LogType
        {
            Error = 1,
            Warning = 2,
            Notice = 3,
            Debug = 4
        }

        public string ReadString(string option, string defValue, bool replaceMeasures = true)
        {
            return Marshal.PtrToStringUni(RmReadString(_mRm, option, defValue, replaceMeasures));
        }

        public string ReadPath(string option, string defValue)
        {
            return Marshal.PtrToStringUni(RmPathToAbsolute(_mRm, ReadString(option, defValue)));
        }

        public double ReadDouble(string option, double defValue)
        {
            return RmReadFormula(_mRm, option, defValue);
        }

        public int ReadInt(string option, int defValue)
        {
            return (int)RmReadFormula(_mRm, option, defValue);
        }

        public string ReplaceVariables(string str)
        {
            return Marshal.PtrToStringUni(RmReplaceVariables(_mRm, str));
        }

        public string GetMeasureName()
        {
            return Marshal.PtrToStringUni(RmGet(_mRm, RmGetType.MeasureName));
        }

        public IntPtr GetSkin()
        {
            return RmGet(_mRm, RmGetType.Skin);
        }

        public string GetSettingsFile()
        {
            return Marshal.PtrToStringUni(RmGet(_mRm, RmGetType.SettingsFile));
        }

        public string GetSkinName()
        {
            return Marshal.PtrToStringUni(RmGet(_mRm, RmGetType.SkinName));
        }

        public IntPtr GetSkinWindow()
        {
            return RmGet(_mRm, RmGetType.SkinWindowHandle);
        }

        public static void Log(LogType type, string message)
        {
            LSLog(type, null, message);
        }
    }

    /// <summary>
    /// Dummy attribute to mark method as exported for DllExporter.exe.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DllExport : Attribute
    {
        public DllExport()
        {
        }
    }
}
