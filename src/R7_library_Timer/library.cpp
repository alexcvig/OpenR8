/*
Copyright (c) 2004-2017 Open Robot Club. All rights reserved.
Timer library for R7.
*/

#include <windows.h> 

#include "R7.hpp"


using namespace std;


#ifdef __cplusplus
extern "C"
{
#endif


typedef struct {
	LARGE_INTEGER frequency;
	LARGE_INTEGER startTime;
	LARGE_INTEGER stopTime;
	bool isStarted;
	bool startAndStop;
} Timer_t;


static int Timer_GetMicrosecond(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -1;
	}
	if (variableObject == NULL) {
		R7_Printf(r7Sn, "ERROR! variableObject == NULL");
		return -2;
	}

	Timer_t *timer = (Timer_t *)variableObject;

	if (timer->isStarted) {
		R7_Printf(r7Sn, "ERROR! Timer is not Stopped!");
		return -3;
	} else if (!timer->startAndStop) {
		R7_Printf(r7Sn, "ERROR! Timer is not Started!");
		return -4;
	}

	double microsecond = (timer->stopTime.QuadPart - timer->startTime.QuadPart) * 1000.0 / timer->frequency.QuadPart;

	res = R7_SetVariableDouble(r7Sn, functionSn, 2, microsecond);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_SetVariableDouble = %d", res);
		return -5;
	}

	return 1;
}

static int Timer_Init(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_InitVariableObject(r7Sn, functionSn, 1, sizeof(Timer_t));
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_InitVariableObject = %d", res);
		return -1;
	}
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	
	Timer_t *timer = ((Timer_t*)variableObject);

	QueryPerformanceFrequency(&timer->frequency);

	timer->startTime = { 0 };
	timer->stopTime = { 0 };

	timer->isStarted = false;
	timer->startAndStop = false;

	return 1;
}

static int Timer_Release(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -1;
	}
	if (variableObject == NULL) {
		R7_Printf(r7Sn, "ERROR! variableObject == NULL");
		return -2;
	}

	res = R7_ReleaseVariableObject(r7Sn, functionSn, 1);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_ReleaseVariableObject = %d", res);
		return -1;
	}

	return 1;
}

static int Timer_Reset(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -1;
	}
	if (variableObject == NULL) {
		R7_Printf(r7Sn, "ERROR! variableObject == NULL");
		return -2;
	}

	Timer_t *timer = ((Timer_t*)variableObject);

	QueryPerformanceCounter(&timer->startTime);
	timer->stopTime = { 0 };
	
	timer->isStarted = true;
	timer->startAndStop = false;

	return 1;
}

static int Timer_Start(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -1;
	}
	if (variableObject == NULL) {
		R7_Printf(r7Sn, "ERROR! variableObject == NULL");
		return -2;
	}

	Timer_t *timer = ((Timer_t*)variableObject);

	if (timer->isStarted) {
		R7_Printf(r7Sn, "ERROR! Timer is Already Started!");
		return -3;
	}

	QueryPerformanceCounter(&timer->startTime);

	timer->isStarted = true;

	return 1;
}

static int Timer_Stop(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -1;
	}
	if (variableObject == NULL) {
		R7_Printf(r7Sn, "ERROR! variableObject == NULL");
		return -2;
	}

	Timer_t *timer = ((Timer_t*)variableObject);

	if (!timer->isStarted) {
		R7_Printf(r7Sn, "ERROR! Timer is not Started!");
		return -3;
	}

	QueryPerformanceCounter(&timer->stopTime);
	
	timer->isStarted = false;
	timer->startAndStop = true;

	return 1;
}

	R7_API int R7Library_Init(void) {
		// Register your functions in this library.
		R7_RegisterFunction("Timer_GetMicrosecond", (R7Function_t)&Timer_GetMicrosecond);
		R7_RegisterFunction("Timer_Init", (R7Function_t)&Timer_Init);
		R7_RegisterFunction("Timer_Release", (R7Function_t)&Timer_Release);
		R7_RegisterFunction("Timer_Reset", (R7Function_t)&Timer_Reset);
		R7_RegisterFunction("Timer_Start", (R7Function_t)&Timer_Start);
		R7_RegisterFunction("Timer_Stop", (R7Function_t)&Timer_Stop);
		
		return 1;
	}

	R7_API int R7Library_Close(void) {
		// If you have something to do before close R7(ex: free memory), you may write them in this API.

		return 1;
	}

	inline void r7_AppendVariable(json_t *variableArray, const char *name, const char *type, const char *option) {
		if (!variableArray) {
			return;
		}

		json_t *variable;
		json_t *variableObject;

		variableObject = json_object();
		variable = json_object();
		json_object_set_new(variable, "variable", variableObject);
		json_object_set_new(variableObject, "name", json_string(name));
		json_object_set_new(variableObject, "type", json_string(type));
		json_object_set_new(variableObject, "option", json_string(option));
		json_array_append(variableArray, variable);

		return;
	}

	R7_API int R7Library_GetSupportList(char *str, int strSize) {
		// Define your functions and variables in this library.
		json_t *root = json_object();
		json_t *functionGroupArray;
		json_t *functionGroup;
		json_t *functionGroupObject;
		json_t *functionArray;
		json_t *function;
		json_t *functionObject;
		json_t *variableArray;

		functionGroupArray = json_array();
		json_object_set_new(root, "functionGroups", functionGroupArray);

		functionGroup = json_object();
		functionGroupObject = json_object();
		json_object_set_new(functionGroup, "functionGroup", functionGroupObject);
		json_object_set_new(functionGroupObject, "name", json_string("Timer"));
		json_array_append(functionGroupArray, functionGroup);

		functionArray = json_array();
		json_object_set_new(functionGroupObject, "functions", functionArray);

		// Functions
		// Timer_GetMicrosecond
		function = json_object();
		functionObject = json_object();
		json_object_set_new(function, "function", functionObject);
		json_object_set_new(functionObject, "name", json_string("Timer_GetMicrosecond"));
		json_object_set_new(functionObject, "doc", json_string(""));
		json_array_append(functionArray, function);
		variableArray = json_array();
		json_object_set_new(functionObject, "variables", variableArray);
		r7_AppendVariable(variableArray, "TimerObject", "object", "IN");
		r7_AppendVariable(variableArray, "Microsecond", "double", "OUT");

		// Timer_Init
		function = json_object();
		functionObject = json_object();
		json_object_set_new(function, "function", functionObject);
		json_object_set_new(functionObject, "name", json_string("Timer_Init"));
		json_object_set_new(functionObject, "doc", json_string(""));
		json_array_append(functionArray, function);
		variableArray = json_array();
		json_object_set_new(functionObject, "variables", variableArray);
		r7_AppendVariable(variableArray, "TimerObject", "object", "IN");

		// Timer_Release
		function = json_object();
		functionObject = json_object();
		json_object_set_new(function, "function", functionObject);
		json_object_set_new(functionObject, "name", json_string("Timer_Release"));
		json_object_set_new(functionObject, "doc", json_string(""));
		json_array_append(functionArray, function);
		variableArray = json_array();
		json_object_set_new(functionObject, "variables", variableArray);
		r7_AppendVariable(variableArray, "TimerObject", "object", "IN");

		// Timer_Reset
		function = json_object();
		functionObject = json_object();
		json_object_set_new(function, "function", functionObject);
		json_object_set_new(functionObject, "name", json_string("Timer_Reset"));
		json_object_set_new(functionObject, "doc", json_string(""));
		json_array_append(functionArray, function);
		variableArray = json_array();
		json_object_set_new(functionObject, "variables", variableArray);
		r7_AppendVariable(variableArray, "TimerObject", "object", "IN");

		// Timer_Start
		function = json_object();
		functionObject = json_object();
		json_object_set_new(function, "function", functionObject);
		json_object_set_new(functionObject, "name", json_string("Timer_Start"));
		json_object_set_new(functionObject, "doc", json_string(""));
		json_array_append(functionArray, function);
		variableArray = json_array();
		json_object_set_new(functionObject, "variables", variableArray);
		r7_AppendVariable(variableArray, "TimerObject", "object", "IN");

		// Timer_Stop
		function = json_object();
		functionObject = json_object();
		json_object_set_new(function, "function", functionObject);
		json_object_set_new(functionObject, "name", json_string("Timer_Stop"));
		json_object_set_new(functionObject, "doc", json_string(""));
		json_array_append(functionArray, function);
		variableArray = json_array();
		json_object_set_new(functionObject, "variables", variableArray);
		r7_AppendVariable(variableArray, "TimerObject", "object", "IN");


		sprintf_s(str, strSize, "%s", json_dumps(root, 0));

		json_decref(root);

		return 1;
	}
#ifdef __cplusplus
}
#endif
