/*
Copyright (c) 2004-2017 Open Robot Club. All rights reserved.
COM library for R7.
*/


#include <string>

#include "R7.hpp"


using namespace std;


typedef struct {
	HANDLE handleCOM;
	DCB dcb;
	COMMTIMEOUTS commTimeouts;
} COM_t;


#ifdef __cplusplus
extern "C"
{
#endif


static int COM_Init(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;
	res = R7_InitVariableObject(r7Sn, functionSn, 1, sizeof(COM_t));
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_InitVariableObject = %d", res);
		return -1;
	}
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -1;
	}

	COM_t *com = ((COM_t*)variableObject);

	// Initial parameters of COM port.
	com->handleCOM = INVALID_HANDLE_VALUE;
	com->dcb = { 0 };

	GetCommState(com->handleCOM, &com->dcb);
	com->dcb.BaudRate = CBR_115200;
	com->dcb.ByteSize = 8;
	com->dcb.Parity = NOPARITY;
	com->dcb.StopBits = ONESTOPBIT;
	com->dcb.fRtsControl = RTS_CONTROL_DISABLE;

	return 1;
}

static int COM_SetBaudRate(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	int baudRate = 0;
	res = R7_GetVariableInt(r7Sn, functionSn, 2, &baudRate);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableInt = %d", res);
		return -3;
	}

	com->dcb.BaudRate = baudRate;

	return 1;
}

static int COM_SetVariableBits(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	int byteSize = 0;
	res = R7_GetVariableInt(r7Sn, functionSn, 2, &byteSize);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableInt = %d", res);
		return -3;
	}

	com->dcb.ByteSize = byteSize;

	return 1;
}

static int COM_SetParityBit(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	int parityBit = 0;
	res = R7_GetVariableInt(r7Sn, functionSn, 2, &parityBit);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableInt = %d", res);
		return -3;
	}

	com->dcb.Parity = parityBit;

	return 1;
}

static int COM_SetStopBit(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	int stopBit = 0;
	res = R7_GetVariableInt(r7Sn, functionSn, 2, &stopBit);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableInt = %d", res);
		return -3;
	}

	com->dcb.StopBits = stopBit;

	return 1;
}

static int COM_Open(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	int comPortNum;
	res = R7_GetVariableInt(r7Sn, functionSn, 2, &comPortNum);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableInt = %d", res);
		return -3;
	}

#ifdef UNICODE
	WCHAR wname[R7_STRING_SIZE] = L"";
	wsprintf(wname, L"COM%d", comPortNum);
	com->handleCOM = CreateFile(wname, GENERIC_READ | GENERIC_WRITE, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
#else
	char portName[10];
	sprintf(portName, "COM%d", comPortNum);
	com->handleCOM = CreateFile(portName, GENERIC_READ | GENERIC_WRITE, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
#endif

	if (com->handleCOM == INVALID_HANDLE_VALUE) {
		R7_Printf(r7Sn, "ERROR! com->handleCOM == INVALID_HANDLE_VALUE");
		return -4;
	}

	if (!SetCommState(com->handleCOM, &com->dcb)) {
		R7_Printf(r7Sn, "ERROR! SetCommState is Failure!");
		return -5;
	}

	if (!SetupComm(com->handleCOM, R7_STRING_SIZE, R7_STRING_SIZE)) {
		R7_Printf(r7Sn, "ERROR! SetupComm is Failure!");
		return -6;
	}

	GetCommTimeouts(com->handleCOM, &com->commTimeouts);

	com->commTimeouts.ReadIntervalTimeout = MAXDWORD;
	com->commTimeouts.ReadTotalTimeoutMultiplier = 0;
	com->commTimeouts.ReadTotalTimeoutConstant = 10;
	com->commTimeouts.WriteTotalTimeoutMultiplier = 0;
	com->commTimeouts.WriteTotalTimeoutConstant = 10;

	SetCommTimeouts(com->handleCOM, &com->commTimeouts);

	if (!SetCommMask(com->handleCOM, EV_DSR | EV_RXCHAR | EV_RXFLAG | EV_CTS | EV_BREAK | EV_ERR | EV_RING | EV_TXEMPTY)) {
		R7_Printf(r7Sn, "ERROR! SetupCommMask is Failure!");
		return -7;
	}

	EscapeCommFunction(com->handleCOM, SETDTR);

	PurgeComm(com->handleCOM, PURGE_TXABORT | PURGE_RXABORT | PURGE_TXCLEAR | PURGE_RXCLEAR);

	return 1;
}

static int COM_Close(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	CloseHandle(com->handleCOM);

	com->handleCOM = INVALID_HANDLE_VALUE;

	return 1;
}

static int COM_Write(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	if (com->handleCOM == INVALID_HANDLE_VALUE) {
		R7_Printf(r7Sn, "ERROR! com->handleCOM == INVALID_HANDLE_VALUE");
		return -3;
	}

	char command[R7_STRING_SIZE];
	command[0] = '\0';

	res = R7_GetVariableString(r7Sn, functionSn, 2, command, R7_STRING_SIZE);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableString = %d", res);
		return -4;
	}

	char commandFix[R7_STRING_SIZE];
	commandFix[0] = '\0';
	int strSize = (int)(strlen(command));
	int nowChar = 0;
	for (int i = 0; i < strSize; i++) {
		if (command[i] != '\\') {
			commandFix[nowChar] = command[i];
			nowChar++;
		}
		else {
			if (i + 1 < strSize) {
				switch (command[i + 1]) {
				case 'r':
					commandFix[nowChar] = '\r';
					break;
				case 'n':
					commandFix[nowChar] = '\n';
					break;
				case '\\':
					commandFix[nowChar] = '\\';
					break;
				case 'b':
					commandFix[nowChar] = '\b';
					break;
				case 'f':
					commandFix[nowChar] = '\f';
					break;
				case 't':
					commandFix[nowChar] = '\t';
					break;
				case 'v':
					commandFix[nowChar] = '\v';
					break;
				case '\'':
					commandFix[nowChar] = '\'';
					break;
				case '\"':
					commandFix[nowChar] = '\"';
					break;
				default:
					commandFix[nowChar] = command[i + 1];
					break;
				}
				nowChar++;
				i += 1;
			}
		}
	}
	commandFix[nowChar] = '\0';
	//Sleep(200);

	DWORD dw_write;
	if (!WriteFile(com->handleCOM, commandFix, (DWORD)strlen(commandFix), &dw_write, NULL)) {
		int error = GetLastError();
		if (error != ERROR_IO_PENDING) {
			R7_Printf(r7Sn, "error = %d", error);
			return -5;
		}
	}

	FlushFileBuffers(com->handleCOM);
	
	return 1;
}

static int COM_Read(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	if (com->handleCOM == INVALID_HANDLE_VALUE) {
		R7_Printf(r7Sn, "ERROR! com->handleCOM == INVALID_HANDLE_VALUE");
		return -3;
	}

	char response[R7_STRING_SIZE];
	response[0] = '\0';

	DWORD  dw_read = 0;
	//Sleep(200);
	if (!ReadFile(com->handleCOM, response, (DWORD)R7_STRING_SIZE, &dw_read, NULL)) {
		DWORD error = GetLastError();

		switch (error) {
			case ERROR_IO_PENDING: {
				R7_Printf(r7Sn, "ERROR! ERROR_IO_PENDING");
				break;
			}
			case ERROR_INVALID_USER_BUFFER: {
				R7_Printf(r7Sn, "ERROR! ERROR_INVALID_USER_BUFFER");
				break;
			}
			case ERROR_NOT_ENOUGH_MEMORY: {
				R7_Printf(r7Sn, "ERROR! ERROR_NOT_ENOUGH_MEMORY");
				break;
			}
			case ERROR_NOT_ENOUGH_QUOTA: {
				R7_Printf(r7Sn, "ERROR! ERROR_NOT_ENOUGH_QUOTA");
				break;
			}
			default: {
				R7_Printf(r7Sn, "ERROR! error = %d", error);
				break;
			}
		}

		return -4;
	}
	else {
		response[(int)dw_read] = '\0';
	}

	res = R7_SetVariableString(r7Sn, functionSn, 2, response);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_SetVariableString = %d", res);
		return -5;
	}

	return 1;
}

static int COM_Release(int r7Sn, int functionSn) {
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

	COM_t *com = ((COM_t*)variableObject);

	CloseHandle(com->handleCOM);

	res = R7_ReleaseVariableObject(r7Sn, functionSn, 1);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_ReleaseVariableObject = %d", res);
		return -1;
	}

	return 1;
}


R7_API int R7Library_Init(void) {
	// Register your functions in this API.

	R7_RegisterFunction("COM_Close", (R7Function_t)&COM_Close);
	R7_RegisterFunction("COM_Init", (R7Function_t)&COM_Init);
	R7_RegisterFunction("COM_Open", (R7Function_t)&COM_Open);
	R7_RegisterFunction("COM_Read", (R7Function_t)&COM_Read);
	R7_RegisterFunction("COM_Release", (R7Function_t)&COM_Release);
	R7_RegisterFunction("COM_SetBaudRate", (R7Function_t)&COM_SetBaudRate);
	R7_RegisterFunction("COM_SetVariableBits", (R7Function_t)&COM_SetVariableBits);
	R7_RegisterFunction("COM_SetParityBit", (R7Function_t)&COM_SetParityBit);
	R7_RegisterFunction("COM_SetStopBit", (R7Function_t)&COM_SetStopBit);
	R7_RegisterFunction("COM_Write", (R7Function_t)&COM_Write);

	return 1;
}

R7_API int R7Library_Close(void) {
	// If you have something to do before close R7(ex: free memory), you should handle them in this API.



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
	// Define your functions and parameters in this API.

	json_t *root = json_object();
	json_t *functionGroupArray;
	json_t *functionGroup;
	json_t *functionGroupObject;
	json_t *functionArray;
	json_t *function;
	json_t *functionObject;
	json_t *variableArray;
	//json_t *variable;
	//json_t *variableObject;

	functionGroupArray = json_array();
	json_object_set_new(root, "functionGroups", functionGroupArray);

	functionGroup = json_object();
	functionGroupObject = json_object();
	json_object_set_new(functionGroup, "functionGroup", functionGroupObject);
	json_object_set_new(functionGroupObject, "name", json_string("COM"));
	json_array_append(functionGroupArray, functionGroup);

	functionArray = json_array();
	json_object_set_new(functionGroupObject, "functions", functionArray);

	// COM_Close
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_Close"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");

	// COM_Init
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_Init"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");

	// COM_Open
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_Open"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");
	r7_AppendVariable(variableArray, "ComPortNum", "int", "IN");
		
	// COM_Read
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_Read"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");
	r7_AppendVariable(variableArray, "String", "string", "OUT");

	// COM_Release
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_Release"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");

	// COM_SetBaudRate
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_SetBaudRate"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");
	r7_AppendVariable(variableArray, "BaudRate", "int", "IN");

	// COM_SetVariableBits
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_SetVariableBits"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");
	r7_AppendVariable(variableArray, "DataBits", "int", "IN");

	// COM_SetParityBit
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_SetParityBit"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");
	r7_AppendVariable(variableArray, "ParityBit", "int", "IN");
		
	// COM_SetStopBit
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_SetStopBit"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");
	r7_AppendVariable(variableArray, "StopBit", "object", "IN");
		
	// COM_Write
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("COM_Write"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "ComObject", "object", "IN");
	r7_AppendVariable(variableArray, "String", "string", "IN");

		
	sprintf_s(str, strSize, "%s", json_dumps(root, 0));

	json_decref(root);

	return 1;
}

#ifdef __cplusplus
}
#endif
