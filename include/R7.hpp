/*
Export functions to 3rdparty.
R7 dll library uses UTF-8 encoding inside.
*/


#ifndef __R7_HPP__
#define __R7_HPP__


#include <windows.h>

#include <jansson.h>
#include <opencv2\opencv.hpp>


#define R7_VERSION "18.10.0"


#define R7_STRING_SIZE 4096
#define R7_TYPE_NAME_SIZE 4096
#define R7_PROGRAMS_SIZE 1024
#define R7_VARIABLES_SIZE 2048
#define R7_FUNCTIONS_SIZE 2048
#define R7_PAGE_SIZE 4096
#define R7_RESULT_SIZE 1048576


// R7_Log level
#define R7_EMERG     -3    /* system is unusable */
#define R7_ALERT     -2    /* action must be taken immediately */
#define R7_CRITICAL  -1    /* critical conditions */
#define R7_ERROR      0    /* error conditions */
#define R7_WARNING    1    /* warning conditions */
#define R7_NOTICE     2    /* normal but significant */
#define R7_INFO       3    /* informational */
#define R7_DEBUG      4    /* debug-level messages */


#define R7_API __declspec(dllexport)


R7_API std::vector<int> R7_GetFunctionSnSort(int r7Sn);


#ifdef __cplusplus
extern "C"
{
#endif

	typedef int(*R7Function_t)(int r7Sn, int functionSn);

	typedef int(*R7Library_Init_t)(void);
	typedef int(*R7Library_Close_t)(void);
	typedef int(*R7Library_GetSupportList_t)(char *str, int strSize);

	R7_API int R7_InitLib();
	R7_API int R7_CloseLib();

	R7_API int R7_New();
	R7_API int R7_Release(int sn);

	R7_API int R7_GetVersion(char *str, int strSize);

	R7_API int R7_Run(int sn, char *workspacePath, int isDebug);
	
	R7_API int R7_RegisterFunction(char *name, R7Function_t r7Function);
	
	R7_API int R7_GetFunctionEnable(int r7Sn, int functionSn, bool *enable);
	R7_API int R7_SetFunctionEnable(int r7Sn, int functionSn, bool enable);

	R7_API int R7_GetVariableSn(int r7Sn, int functionSn, int variableNum, int *variableSn);
	R7_API int R7_GetVariableType(int r7Sn, int variableSn, int *variableType);
	R7_API int R7_GetVariableString(int r7Sn, int functionSn, int variableNum, char *str, int strSize);
	R7_API int R7_SetVariableString(int r7Sn, int functionSn, int variableNum, char *str);
	R7_API int R7_GetVariableBinary(int r7Sn, int functionSn, int variableNum, void *binary, int binarySize);
	R7_API int R7_GetVariableBinarySize(int r7Sn, int functionSn, int variableNum);
	R7_API int R7_SetVariableBinary(int r7Sn, int functionSn, int variableNum, void *binary, int binarySize);
	R7_API int R7_GetVariableInt(int r7Sn, int functionSn, int variableNum, int *variableValue);
	R7_API int R7_SetVariableInt(int r7Sn, int functionSn, int variableNum, int variableValue);
	R7_API int R7_GetVariableBool(int r7Sn, int functionSn, int variableNum, bool *variableValue);
	R7_API int R7_SetVariableBool(int r7Sn, int functionSn, int variableNum, bool variableValue);
	R7_API int R7_GetVariableFloat(int r7Sn, int functionSn, int variableNum, float *variableValue);
	R7_API int R7_SetVariableFloat(int r7Sn, int functionSn, int variableNum, float variableValue);
	R7_API int R7_GetVariableDouble(int r7Sn, int functionSn, int variableNum, double *variableValue);
	R7_API int R7_SetVariableDouble(int r7Sn, int functionSn, int variableNum, double variableValue);
	R7_API int R7_GetVariableMat(int r7Sn, int functionSn, int variableNum, cv::Mat *variableValue);
	R7_API int R7_SetVariableMat(int r7Sn, int functionSn, int variableNum, cv::Mat &variableValue);
	R7_API int R7_GetVariableJson(int r7Sn, int functionSn, int variableNum, json_t **variableValue);
	R7_API int R7_SetVariableJson(int r7Sn, int functionSn, int variableNum, json_t *variableValue);
	
	R7_API int R7_InitVariableObject(int r7Sn, int functionSn, int variableNum, int variableSize);
	R7_API int R7_ReleaseVariableObject(int r7Sn, int functionSn, int variableNum);
	R7_API int R7_GetVariableObject(int r7Sn, int functionSn, int variableNum, void **variable);
	R7_API int R7_SetVariableObject(int r7Sn, int functionSn, int variableNum, void *variable);
	R7_API int R7_SetVariableObjectClassName(int r7Sn, int functionSn, int variableNum, char *str);
	R7_API int R7_GetVariableObjectClassName(int r7Sn, int functionSn, int variableNum, char *str, int strSize);
	
	R7_API int R7_GetWorkspacePath(int r7Sn, char *path, int pathStrSize);
	R7_API int R7_SetWorkspacePath(int r7Sn, char *path, int pathStrSize);
	
	R7_API int R7_GetVariableName(int r7Sn, int variableSn, char *str, int strSize);

	R7_API int R7_LoadLibrary(char *path);

	R7_API int R7_GetFunctionName(int r7Sn, int functionSn, char *str, int strSize);
	
	R7_API int R7_GetDebug(int r7Sn, int *isDebug);
	R7_API int R7_SetDebug(int r7Sn, int &isDebug);
	
	R7_API int R7_Printf(int r7Sn, char *format, ...);

	R7_API int R7_GetSupportList(char *str, int strSize);

	R7_API int R7_Log(int level, char *functionName, char *format, ...);

	R7_API int R7_AddVariable(int r7Sn, char *name, char *type, char *value);
	R7_API int R7_AddFunction(int r7Sn, char *name, ...);

	typedef int(*R7CallbackHandler)(void *data);
	R7_API int R7_QueueWxEvent(R7CallbackHandler r7CallbackHandler, void *data);
	R7_API int R7_ProcessWxPendingEvents();
	R7_API void *R7_GetWxApp();

#ifdef __cplusplus
}
#endif

#endif // __R7_HPP__ 
