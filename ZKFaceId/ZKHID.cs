﻿using DemoCamApp.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZKFaceId
{
    internal class ZKHID
    {

        #region C++ library import

        private const string PathToDll = "lib/x64/ZKHIDLib.dll";


        [DllImport(PathToDll)]
        private static extern int ZKHID_Open(int index, out IntPtr handle);

        [DllImport(PathToDll)]
        private static extern int ZKHID_Close(IntPtr handle);

        [DllImport(PathToDll)]
        private static extern int ZKHID_Init();

        [DllImport(PathToDll)]
        private static extern int ZKHID_Terminate();

        [DllImport(PathToDll)]
        private static extern int ZKHID_GetCount(out int count);

        [DllImport(PathToDll)]
        private static extern int ZKHID_GetConfig(IntPtr handle, int type, out string json,out int len);

        [DllImport(PathToDll)]
        private static extern int ZKHID_SetConfig(IntPtr handle, int type, string json);

        [DllImport(PathToDll, CallingConvention = CallingConvention.StdCall)]
        private static extern int ZKHID_RegisterFace(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string json, StringBuilder faceData, out int len);

        [DllImport(PathToDll)]
        private static extern int ZKHID_SnapShot(IntPtr handle, int snapType, StringBuilder snapData, out int size);

        #endregion

        private int Index;

        private IntPtr Handle;

        public ZKHID(int index)
        {
            Index = index;
        }
        public int Init()
        {
            return ZKHID_Init();
        }
        public int Terminate()
        {
            return ZKHID_Terminate();
        }

        public void StartDevice()
        {
            Init();
            Open();
        }

        public int GetCount(out int count)
        {
            return ZKHID_GetCount(out count);
        }

        public int Open()
        {
            return ZKHID_Open(Index, out Handle);
        }

        public int Close()
        {
            return ZKHID_Close(Handle);
        }

        public RegistrationData RegisterFace(RegisterFaceConfig config)
        {
            int length = 1024 * 1024;
        
            string json = JsonSerializer.Serialize(config);

            StringBuilder faceData = new StringBuilder(new String(' ', length));

            var res = ZKHID_RegisterFace(Handle, json, faceData, out length);

            return JsonSerializer.Deserialize<RegistrationData>(faceData.ToString());
            //return faceData.ToString();
        }
    }
}