/*
Copyright (c) 2004-2018 Open Robot Club. All rights reserved.
CGI library for R7.
*/


#include "R7.hpp"
#include <tchar.h>
#include <wx/string.h>

using namespace std;


#ifdef __cplusplus
extern "C"
{
#endif

// CGI_ParseRequest
typedef struct {
	int hasValue;
	int isEnv;
	int isGet;
	int isPost;
	wxString name;
	wxString strValue;
	int binarySize;
	void *binaryValue;
} CGI_VARIABLE_t;
static CGI_VARIABLE_t cgiVariable[64];



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

	wchar_t *envW = L"CONTENT_LENGTH";
	wchar_t *nameGetW;
	nameGetW = _wgetenv(envW);

	char nameGet[R7_STRING_SIZE];
	WideCharToMultiByte(CP_UTF8, 0, nameGetW, -1, nameGet, R7_STRING_SIZE * 2, NULL, NULL);
	int result = int(atoi(nameGet));
	if (result == 0) {
		return -1;
	}

	HANDLE hDstFile = GetStdHandle(STD_INPUT_HANDLE);
	if (hDstFile == INVALID_HANDLE_VALUE) {
		return -2;
	}

	//W_CHART
	if (false) {
		wchar_t *wchr = new wchar_t[result];
		DWORD dw = 0;
		ReadFile(hDstFile, wchr, sizeof(wchar_t) * result, &dw, NULL);

		char *nameGets;
		nameGets = new char[result];
		WideCharToMultiByte(CP_UTF8, 0, wchr, -1, nameGets, result * 1, NULL, NULL);
		int results = int(atoi(nameGets));

		char *test = new char[result];
		wcstombs(test, wchr, result);

		wchar_t * wText = wchr;
		DWORD dwNum = result;
		char *psText;
		psText = new char[dwNum];
		WideCharToMultiByte(CP_UTF8, NULL, wchr, -1, psText, dwNum, NULL, FALSE);
		string szDst = psText;

		CloseHandle(hDstFile);
		delete[]psText;
	}

	//string
	if (true) {
		char *string1, *tmpBuf;
		DWORD dwBytesToRead, dwBytesRead;
		string1 = (char *)malloc(result + 1);
		memset(string1, 0, sizeof(char) * result + 1);
		dwBytesToRead = result;
		dwBytesRead = 0;

		tmpBuf = string1;
		do {
			ReadFile(hDstFile, string1, dwBytesToRead, &dwBytesRead, NULL);
			if (dwBytesRead == 0) {
				break;
			}

			dwBytesToRead -= dwBytesRead;
			tmpBuf += dwBytesRead;

		} while (dwBytesToRead > 0);

		//decoding
		if (true) {
			char name1[R7_STRING_SIZE];
			sprintf(name1, "\"%s\"", name);

			char *test = strstr(string1, name1);
			int address = int(test - string1);

			char name2[R7_STRING_SIZE];
			int count = 0;
			for (int i = address + int(strlen(name1)); i < result; i++) {
				name2[count] = string1[i];
				count++;
			}

			if (count == 0) {
				return 1;
			}

			char name3[R7_STRING_SIZE];
			sprintf(name3, "------WebKitFormBoundary");
			char *test2 = strstr(name2, name3);
			int address2 = int(test2 - name2);

			char name4[R7_STRING_SIZE];
			strncpy(name4, name2, address2);

			R7_SetVariableInt(r7Sn, functionSn, 1, result);
			R7_SetVariableString(r7Sn, functionSn, 2, name4);
		}

		CloseHandle(hDstFile);
		hDstFile = NULL;
		delete[]string1;
		string1 = NULL;
	}
	return 1;
}


static int CGI_GetGetString(int r7Sn, int functionSn) {
	int result = 0;
	char name[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, name, R7_STRING_SIZE);

	wchar_t nameW[R7_STRING_SIZE];
	memset(nameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, name, -1, nameW, R7_STRING_SIZE * 2);

	char nameGet[R7_STRING_SIZE];
	WideCharToMultiByte(CP_UTF8, 0, nameW, -1, nameGet, R7_STRING_SIZE * 2, NULL, NULL);

	int i;
	char cstring[1024];
	char *string = (char*)malloc(1024);
	for (i = 0; i < 64; i++) {	
		if (cgiVariable[i].isGet == 0) {
			continue;
		}

		strncpy(cstring, (const char*)cgiVariable[i].name.mb_str(wxConvUTF8), 1023);
		if (strcmp(cstring, nameGet) == 0) {
			strncpy(string, (const char*)cgiVariable[i].strValue.mb_str(wxConvUTF8), 1023);
			result = strlen(cstring);
		//	printf("name = %s, value = %s", cstring, string);
			i = 64;
		}

		if (i == 65) {
			R7_SetVariableInt(r7Sn, functionSn, 1, -1);
			free(string);
		}
	}

	R7_SetVariableInt(r7Sn, functionSn, 1, result);
	R7_SetVariableString(r7Sn, functionSn, 2, string);
	free(string);
	return 1;
}


char toChar(char c1, char c2) {
	char c = 0;

	if (c1 >= '0' && c1 <= '9') {
		c = c1 - '0';
	}
	else if (c1 >= 'a' && c1 <= 'z') {
		c = c1 - 'a' + 10;
	}
	else if (c1 >= 'A' && c1 <= 'Z') {
		c = c1 - 'A' + 10;
	}

	if (c2 >= '0' && c2 <= '9') {
		c = c * 16 + c2 - '0';
	}
	else if (c2 >= 'a' && c2 <= 'z') {
		c = c * 16 + c2 - 'a' + 10;
	}
	else if (c2 >= 'A' && c2 <= 'Z') {
		c = c * 16 + c2 - 'A' + 10;
	}

	return c;
}


int aToI(const char *src, char *des) {
	if (src == NULL || des == NULL) {
		return -1;
	}

	int i, j;
	int strLen = (int)strlen(src);

	for (i = 0, j = 0; i < strLen;) {
		if (src[i] == '%') {
			des[j++] = toChar(src[i + 1], src[i + 2]);
			i += 3;
		}
		else if (src[i] == '+') {
			des[j++] = ' ';
			i++;
		}
		else {
			des[j++] = src[i++];
		}
	}

	des[strLen] = '\0';

	return 1;
}


int findNameValues(const char *src, vector<string> &names, vector<string> &values, int &index) {
	if (src == NULL) {
		return -1;
	}

	int i = 0, j = 0;
	int nameF = 0, nameL = 0;
	int valueF = 0, valueL = 0;
	int count = 0;
	int temp = 0;
	int srcLength = (int)strlen(src);
	string srcString(src);
	string strTmp = "";

	for (i = 0; i < srcLength; i++) {
		if (src[i] == '=') {
			//printf("i = %d<br>", i);
			nameL = i - 1;
			j = valueF = i + 1;
			while (src[j] != '&' && j < srcLength) {
				//printf("j = %d<br>", j);
				j++;
			}
			valueL = j - 1;

			if (count > 0) {
				nameF = temp;
			}

			strTmp = srcString.substr(nameF, nameL - nameF + 1);
			char *nameTmp = (char *)calloc(strlen(strTmp.c_str()), 1);
			aToI(strTmp.c_str(), nameTmp);
			names.push_back(nameTmp);

			strTmp = srcString.substr(valueF, valueL - valueF + 1);
			char *valueTmp = (char *)calloc(strlen(strTmp.c_str()), 1);
			aToI(strTmp.c_str(), valueTmp);
			values.push_back(valueTmp);
			//printf("index = %d, name = %s, value = %s", index, nameTmp, valueTmp);


			wxString wxName = wxString::FromUTF8(nameTmp);
			wxString wxValue = wxString::FromUTF8(valueTmp);

			cgiVariable[index].name = wxName;
			cgiVariable[index].isEnv = 0;
			cgiVariable[index].isGet = 1;
			cgiVariable[index].isPost = 0;
			cgiVariable[index].strValue = wxValue;

			count++;
			index++;

			temp = valueL + 2;

			free(nameTmp);
			free(valueTmp);
		}
	}

	return 1;
}


static void Getenv(wchar_t **nameGetW, wchar_t *nameW) {
	*nameGetW = _wgetenv(nameW);
	return;
}


static int CGI_ParseRequest(int r7Sn, int functionSn) {
	int index = 0;
	wchar_t * envVariable[20] = {
		L"CONTENT_LENGTH", _T("QUERY_STRING"), L"SERVER_SOFTWARE", L"SERVER_NAME" , L"SERVER_PROTOCOL",
		L"REQUEST_METHOD", L"PATH_INFO", L"PATH_TRANSLATED", L"SCRIPT_NAME", L"GATEWAY_INTERFACE",
		L"REMOTE_HOST", L"CONTENT_TYPE", L"CONTENT_LENGTH", L"HTTP_COOKIE", L"HTTP_USER_AGENT",
		L"REMOTE_ADDR", L"SCRIPT_FILENAME", L"SERVER_PORT"
	};

	wchar_t *getEnvVariableW = (wchar_t *)malloc(R7_STRING_SIZE);
	for (int i = 0; i < 18; i++) {
		if (envVariable[i] == NULL) {
			//printf("envVariable[%d] =  null<br>", i);
			continue;
		}
		
		Getenv(&getEnvVariableW, envVariable[i]);		
		
		if (getEnvVariableW == NULL) {
			continue;
		}

		if (int(wcslen(getEnvVariableW) * sizeof(wchar_t)) == 0) {
			continue;
		}

		//whcar_t convert to char
		size_t size = wcslen(getEnvVariableW) * sizeof(wchar_t);
		char *getEnvVariable;
		if (!(getEnvVariable = (char*)malloc(size)))
		{
			//printf("malloc fail");
		}
		wcstombs(getEnvVariable, getEnvVariableW, size);

		//get
		if (envVariable[i] == _T("QUERY_STRING")) {
			vector<string> names;
			vector<string> values;
			findNameValues(getEnvVariable, names, values, index);

			char cstring[1024];
			strncpy(cstring, (const char*)cgiVariable[0].name.mb_str(wxConvUTF8), 1023);
			
			free(getEnvVariable);
			continue;
		}
		//post
		else if (envVariable[i] == L"CONTENT_LENGTH") {
			int result = int(atoi(getEnvVariable));
			if (result == 0) {
				return -1;
			}
			HANDLE hDstFile = GetStdHandle(STD_INPUT_HANDLE);
			if (hDstFile == INVALID_HANDLE_VALUE) {
				return -2;
			}
			
			DWORD fdwSaveOldMode = ENABLE_AUTO_POSITION;
			char *string1, *tmpBuf;
			DWORD dwBytesToRead, dwBytesRead;
			string1 = (char *)malloc(result + 1);
			memset(string1, 0, sizeof(char) * result + 1);
			dwBytesToRead = result;
			dwBytesRead = 0;

			tmpBuf = string1;
			do {
				ReadFile(hDstFile, string1, dwBytesToRead, &dwBytesRead, NULL);
				if (dwBytesRead == 0) {
					printf("err \n");
					break;
				}

				dwBytesToRead -= dwBytesRead;
				tmpBuf += dwBytesRead;

			} while (dwBytesToRead > 0);
			//printf("%s \n", string1);

			//TODO: decoding

			CloseHandle(hDstFile);
			hDstFile = NULL;
			delete[]string1;
			string1 = NULL;
			free(getEnvVariable);
			continue;
		}

		//env
		else {
			wxString mystring = wxString::FromUTF8(getEnvVariable);

			cgiVariable[index].name = mystring;
			cgiVariable[index].isEnv = 1;
			cgiVariable[index].isGet = 0;
			cgiVariable[index].isPost = 0;
			index++;

			char cstring[1024];
			strncpy(cstring, (const char*)cgiVariable[i].name.mb_str(wxConvUTF8), 1023);

			free(getEnvVariable);
		}

		//TODO:getBinary
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
	R7_RegisterFunction("CGI_GetGetString", (R7Function_t)&CGI_GetGetString);

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
	
	//CGI_GetGetString
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("CGI_GetGetString"));
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
