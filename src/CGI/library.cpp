/*
Copyright (c) 2004-2018 Open Robot Club. All rights reserved.
CGI library for R7.
*/


#include "R7.hpp"
#include <tchar.h>

using namespace std;


#ifdef __cplusplus
extern "C"
{
#endif


static int CGI_GetBinary(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}

	return 1;
}


static int CGI_GetString(int r7Sn, int functionSn) {
	char name[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, name, R7_STRING_SIZE);

	wchar_t nameW[R7_STRING_SIZE];
	memset(nameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, name, -1, nameW, R7_STRING_SIZE * 2);

	wchar_t *nameGetW;
	nameGetW = _wgetenv(nameW);

	char nameGet[R7_STRING_SIZE];
	WideCharToMultiByte(CP_UTF8, 0, nameGetW, -1, nameGet, R7_STRING_SIZE * 2, NULL, NULL);

	int result = int(strlen(nameGet));

	if (result < 1) {
		R7_Log(R7_ERROR, __FUNCTION__, "nameGet < 1");
		return 1;
	}

	R7_SetVariableInt(r7Sn, functionSn, 1, result);
	R7_SetVariableString(r7Sn, functionSn, 2, nameGet);

	return 1;
}


static int CGI_ParseRequest(int r7Sn, int functionSn) {

	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}

	return 1;
}

static int CGI_Print(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}

	return 1;
}


static int CGI_PrintBinary(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}

	return 1;
}


static int CGI_PrintInfo(int r7Sn, int functionSn) {
	int i = 1;	//extern wchar_t **_wenviron;
	wchar_t *s = *_wenviron;

	for (; s; i++) {
		char nameGet[R7_STRING_SIZE];
		WideCharToMultiByte(CP_UTF8, 0, s, -1, nameGet, R7_STRING_SIZE * 2, NULL, NULL);
		R7_Printf(r7Sn, "%s<br>", nameGet);
		s = *(_wenviron + i);
	}

	return 1;
}


//static int CGI_PrintInfo(int r7Sn, int functionSn) {
//	/*int res;
//	void *variableObject = NULL;
//
//	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
//	if (res <= 0) {
//	R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
//	return -2;
//	}*/
//	LPTSTR lpszVariable;
//	LPTCH lpvEnv;
//
//	lpvEnv = GetEnvironmentStrings();
//
//	if (lpvEnv == NULL) {
//		//printf("GetEnvironmentStrins failed(%d)/n", GetLastError());
//		return 1;
//	}
//
//	lpszVariable = (LPTSTR)lpvEnv;
//
//	while (*lpszVariable) {
//		/*_tprintf(TEXT("%s/n"), lpszVariable);*/
//		//printf("%ls", lpszVariable);
//		lpszVariable += lstrlen(lpszVariable) + 1;
//	}
//
//	FreeEnvironmentStrings(lpvEnv);
//	return 1;
//}


static int CGI_Println(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}

	return 1;
}


R7_API int R7Library_Init(void) {
	// Register your functions in this API.
	
	R7_RegisterFunction("CGI_GetBinary", (R7Function_t)&CGI_GetBinary);
	R7_RegisterFunction("CGI_GetString", (R7Function_t)&CGI_GetString);
	R7_RegisterFunction("CGI_ParseRequest", (R7Function_t)&CGI_ParseRequest);
	R7_RegisterFunction("CGI_Print", (R7Function_t)&CGI_Print);
	R7_RegisterFunction("CGI_PrintBinary", (R7Function_t)&CGI_PrintBinary);
	R7_RegisterFunction("CGI_PrintInfo", (R7Function_t)&CGI_PrintInfo);
	R7_RegisterFunction("CGI_Println", (R7Function_t)&CGI_Println);

	return 1;
}


R7_API int R7Library_Close(void) {
	// If you have something to do before close R7(ex: free memory), you should handle them in this API.
	

	return 1;
}


R7_API int R7Library_GetSupportList(char *str, int strSize) {
	// Define your functions and parameters in this API.

	json_t *root = json_object();
	json_t *functionGroupArray;
	json_t *functionGroup;
	json_t *functionGroupObject;
	json_t *functionArray;
	json_t *function;
	json_t *functionObject;
	json_t *variableArray;
	json_t *variable;
	json_t *variableObject;

	functionGroupArray = json_array();
	json_object_set_new(root, "functionGroups", functionGroupArray);

	functionGroup = json_object();
	functionGroupObject = json_object();
	json_object_set_new(functionGroup, "functionGroup", functionGroupObject);
	json_object_set_new(functionGroupObject, "name", json_string("CGI"));
	json_array_append(functionGroupArray, functionGroup);

	functionArray = json_array();
	json_object_set_new(functionGroupObject, "functions", functionArray);
	
	// CGI_GetBinary
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_GetBinary"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Binary"));
	json_object_set_new(variableObject, "type", json_string("binary"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Name"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);

	// CGI_GetString
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_GetString"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("String"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Name"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);

	// CGI_ParseRequest
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_ParseRequest"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	
	// CGI_Print
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_Print"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("String"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);
	
	// CGI_PrintBinary
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_PrintBinary"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Binary"));
	json_object_set_new(variableObject, "type", json_string("binary"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);

	// CGI_PrintInfo
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_PrintInfo"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);

	// CGI_Println
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_Println"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("String"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);
	

	sprintf_s(str, strSize, "%s", json_dumps(root, 0));

	json_decref(root);

	return 1;
}

#ifdef __cplusplus
}
#endif
