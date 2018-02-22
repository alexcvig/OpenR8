using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace R8
{
    class R7
    {
        public const int PROGRAMS_SIZE = 1024;
        public const int FUNCTIONS_SIZE = 2048;
        public const int VARIABLES_SIZE = 2048;
        public const int STRING_SIZE = 4096;


        public delegate int R7PrintfCallbackFunctionDelegate(int r7Sn, StringBuilder sb);
        public static R7PrintfCallbackFunctionDelegate r7PrintfCallbackFunctionDelegate = null;

        public delegate int R7LogCallbackFunctionDelegate(StringBuilder sb);
        public static R7LogCallbackFunctionDelegate r7LogCallbackFunctionDelegate = null;


        // R7_API int R7_InitLib();
        [DllImport("R7_lib.dll", EntryPoint = "R7_InitLib")]
        public static extern int InitLib();

        //R7_API int R7_CloseLib();
        [DllImport("R7_lib.dll", EntryPoint = "R7_CloseLib")]
        public static extern int CloseLib();

        //R7_API int R7_New();
        [DllImport("R7_lib.dll", EntryPoint = "R7_New")]
        public static extern int New();

        //R7_API int R7_Release(int r7h);
        [DllImport("R7_lib.dll", EntryPoint = "R7_Release")]
        public static extern int Release(int r7h);


        //20171016 依今天早上討論，原本的 R7_Run 搬到 internal ，然後改名 R7_Run_Internal
        [DllImport("R7_lib.dll", EntryPoint = "R7_Run_Internal")]
        public static extern int Run_Internal(int r7h, Byte[] program, Byte[] variable, Byte[] workspacePath, int isDebug);



        //20170327 leo add: 取得單張影像用
        //ref
        [DllImport("R7_lib.dll", EntryPoint = "R7_ImageDisplay")]
        public static extern int ImageDisplay(int r7h, Byte[] program, Byte[] variable, Byte[] workspacePath, int functionSn, int variableSn, Byte[] result, int resultSize);

        [DllImport("R7_lib.dll", EntryPoint = "R7_ImageGetInfo")]
        public static extern int ImageGetInfo(int r7h, int variableSn,ref int imageW, ref int imageH, ref int imageCh);

        [DllImport("R7_lib.dll", EntryPoint = "R7_ImageGet")]
        public static extern int ImageGet(int r7h, int variableSn, byte[] buffer, int bufferSize);

        //R7_API int R7_GetSupportList(char *str, int strSize)
        //[DllImport("R7_lib.dll", EntryPoint = "R7_GetSupportList")]
        //public static extern int GetSupportList(StringBuilder str, int strSize);

        //[DllImport("R7_lib.dll", EntryPoint = "R7_ShowImage")]
        //public static extern int ShowImage(int r7h);

        //20170405 leo: json 讀取相關功能
        [DllImport("R7_lib.dll", EntryPoint = "R7_RunToTargetFunction")]
        public static extern int RunToTargetFunction(int r7h, Byte[] program, Byte[] variable, Byte[] workspacePath, int functionSn, int isDebug);

        [DllImport("R7_lib.dll", EntryPoint = "R7_GetVariableJsonForR8")]
        public static extern int GetJsonVariable(int r7h, int variableSn, StringBuilder str, int strSize);

        [DllImport("R7_lib.dll", EntryPoint = "R7_GetVersion")]
        public static extern int GetVersion(StringBuilder str, int strSize);

        /*
         20170427 leo: 這邊會跳 error: The DllImport attribute cannot be applied to a method that is generic or contained in a generic type
         //Dll不允許使用 template
                [DllImport("R7_lib.dll", EntryPoint = "R7_GetVariableValue")]
                public static extern int R7_GetVariableValue<T>(int r7Sn, int functionSn, int variableNum, ref T value);
        */
        /*
        //改用一般多載
        [DllImport("R7_lib.dll", EntryPoint = "R7_GetVariableValue")]
        public static extern int GetVariableValue(int r7Sn, int functionSn, int variableNum, ref int value);
        [DllImport("R7_lib.dll", EntryPoint = "R7_GetVariableValue")]
        public static extern int GetVariableValue(int r7Sn, int functionSn, int variableNum, ref float value);
        [DllImport("R7_lib.dll", EntryPoint = "R7_GetVariableValue")]
        public static extern int GetVariableValue(int r7Sn, int functionSn, int variableNum, ref double value);
        [DllImport("R7_lib.dll", EntryPoint = "R7_GetVariableValue")]
        public static extern int GetVariableValue(int r7Sn, int functionSn, int variableNum, ref bool value);
        */
        /*
        [DllImport("R7_lib.dll", EntryPoint = "R7_SetVariableValue")]
        public static extern int SetVariableValue(int r7Sn, int functionSn, int variableNum, int value);
        [DllImport("R7_lib.dll", EntryPoint = "R7_SetVariableValue")]
        public static extern int SetVariableValue(int r7Sn, int functionSn, int variableNum, float value);
        [DllImport("R7_lib.dll", EntryPoint = "R7_SetVariableValue")]
        public static extern int SetVariableValue(int r7Sn, int functionSn, int variableNum, double value);
        [DllImport("R7_lib.dll", EntryPoint = "R7_SetVariableValue")]
        public static extern int SetVariableValue(int r7Sn, int functionSn, int variableNum, bool value);
        */

        [DllImport("R7_lib.dll", EntryPoint = "R7_GetVariableInt")]
        public static extern int GetVariableInt(int r7Sn, int variableSn, ref int value);

        [DllImport("R7_lib.dll", EntryPoint = "R7_SetVariableInt")]
        public static extern int SetVariableInt(int r7Sn, int variableSn, int value);

        // [DllImport("R7_lib.dll", EntryPoint = "R7_SetVariableValue")]
        // public static extern int SetVariableValue(int r7Sn, int functionSn, int variableNum, params object[] numbers);


        public static int LoadLibrary(string workspacePath) {
            //字尾加斜線與 string 轉 byteArray 統一在這邊轉
            StringBuilder workSpacePathBuffer = new StringBuilder(workspacePath + "\\", R7.STRING_SIZE);
            byte[] workSpacePathBytes = new byte[workSpacePathBuffer.Length * sizeof(char)];
            System.Buffer.BlockCopy(workSpacePathBuffer.ToString().ToCharArray(), 0, workSpacePathBytes, 0, workSpacePathBytes.Length);
            return LoadLibrary(workSpacePathBytes);
        }

        //20170428 leo: library 架構更改，有[workspace]下的librarys 需要 load。
        //讀取librarys的時間點先定在[workspace切換後]
        [DllImport("R7_lib.dll", EntryPoint = "R7_LoadLibrary")]
        public static extern int LoadLibrary(Byte[] workspacePath);


        //20170502 leo: 依上午討論， supportList 改為不吃檔案、程式產生
        //R7_API int R7_GetSupportList(char* str)
        [DllImport("R7_lib.dll", EntryPoint = "R7_GetSupportList")]
        public static extern int GetSupportList(StringBuilder str, int strSize);

        


        //20170626 leo: 雖然這邊有把 R7_OpenLog 與 R7_CloseLog 開出來，但實際上 R7 在 init 時會自己開一個 R7.log ，
        //所以這邊是要 log 不同檔名才需要使用
        [DllImport("r7_lib.dll", EntryPoint = "R7_OpenLogW")]
        public static extern int OpenLogW(Byte[] fileName);

        [DllImport("r7_lib.dll", EntryPoint = "R7_OpenLog")]
        public static extern int OpenLog(string fileName);

        [DllImport("r7_lib.dll", EntryPoint = "R7_CloseLog")]
        public static extern int CloseLog();

        [DllImport("r7_lib.dll", EntryPoint = "R7_RegisterPrintfCallbackFunction")]
        public static extern int RegisterPrintfCallbackFunction(int R7Sn, R7PrintfCallbackFunctionDelegate R7PCFD);

        [DllImport("r7_lib.dll", EntryPoint = "R7_RegisterLogCallbackFunction")]
        public static extern int RegisterLogCallbackFunction(R7LogCallbackFunctionDelegate R7LCFD);


        //R7_API int R7_Login(char *buffer, int bufferLength, char *userName, char *password);
        [DllImport("r7_lib.dll", EntryPoint = "R7_Login")]
        public static extern int Login(StringBuilder buffer, int bufferLength, StringBuilder userName, StringBuilder password);

        //R7_API int R7_CheckLicense(char *licenseKey, int *year, int *month, int *day);
        [DllImport("r7_lib.dll", EntryPoint = "R7_CheckLicense")]
        public static extern int CheckLicense(StringBuilder licenseKey, ref int year, ref int month, ref int day);


        //20180124 R7_GetMacAddress 開放給 R8 用(About 頁面要顯示 Mac Address)
        //R7_API int R7_GetMacAddress(char *address, int addressSize);
        [DllImport("r7_lib.dll", EntryPoint = "R7_GetMacAddress")]
        public static extern int GetMacAddress(StringBuilder address, int addressSize);
    }
}
