/*
Copyright (c) LEADERG INC. All rights reserved.
*/

#include "R8.hpp"

//#include <curl/curl.h>
#include <windows.h>
#include <vector>


#include <opencv2\opencv.hpp>
#include <opencv2\core\core.hpp>
#include <opencv2\highgui\highgui.hpp>
#include <opencv2\imgproc\imgproc.hpp>

//#include "FlyCapture2_C.h"
//#include "FlyCapture2Defs_C.h"

#include <jansson.h>

#include "tinyxml2.h"

#include <codecvt>

#define R8_STRING_SIZE 4096
#define R8_TYPE_NAME_SIZE 4096

struct R8_Variable {
	char type[R8_STRING_SIZE];
	char name[R8_STRING_SIZE];
	char value[R8_STRING_SIZE];
	//char direction[R8_STRING_SIZE];
	char option[R8_STRING_SIZE];
};

struct R8_Function {
	char name[R8_STRING_SIZE];
	char doc[R8_STRING_SIZE];
	std::vector<R8_Variable> variables;
};

struct R8_FunctionGroup {
	char name[R8_STRING_SIZE];
	std::vector<R8_Function> functions;
};


static HANDLE r8Log = INVALID_HANDLE_VALUE;


using namespace cv;
using namespace std;


//20170428 add utf-8 string 轉換
//來源 http://stackoverflow.com/questions/4358870/convert-wstring-to-string-encoded-in-utf-8
std::wstring utf8_to_wstring(const std::string& str)
{
	std::wstring_convert<std::codecvt_utf8<wchar_t>> myconv;
	return myconv.from_bytes(str);
}

// convert wstring to UTF-8 string
std::string wstring_to_utf8(const std::wstring& str)
{
	std::wstring_convert<std::codecvt_utf8<wchar_t>> myconv;
	return myconv.to_bytes(str);
}

#ifdef __cplusplus
extern "C"
{
#endif



unsigned char* jsonBuffer = NULL;
//char version[R8_STRING_SIZE];

//20170120 leo: type 與 functionFunction 改為放在 dll

//type 會根據 接到的 json 動態設定，所以不能直接 typedef enum 寫死
//char* type[TYPE_NAME_SIZE] = NULL;
std::vector<R8_Variable> types;

//std::vector<R8Function> functions;
std::vector<R8_FunctionGroup> functionGroups;


/*
    name: OpenImage
	name : Image File Name
	type : string
	name : Image
	type : image
	name : SaveImage
	name : Image
	type : image
	name : Image File Name
	type : string
	name : Binarize
	name : Input Image
	type : image
	name : Threshold
	type : int
	name : Output Image
	type : image
	functionGroupName : debug
	name : Debug Image
	name : Image
	type : image
*/

R8_API int R8_InitLib() {

	return 1;
}


R8_API int R8_CloseLib() {
	if (jsonBuffer) {
		free(jsonBuffer);
		jsonBuffer = NULL;
	}
	types.clear();
	return 1;
}

//這些東西基本都搬到 R7 去了 
//20170301 依今天晨會討論，搬回來

inline int getJsonString(json_t *root, char *key, char *value, int valueLength) {
	json_t *variable;
	variable = json_object_get(root, key);
	if (!json_is_string(variable)) {
		return -1;
	}
	strcpy_s(value, valueLength, json_string_value(variable));
	return (int)json_string_length(variable); //20170206 leo: 這邊回傳 variable 長度
}

R8_API int R8_GetSupportListByFileName(char *fileName)
{
	int res;
	int i, j, k;

	json_t *root = NULL;
	json_t *functionGroup;
	json_t *functionArray;
	json_t *function;
	json_t *variableArray;
	json_t *variable;
	json_t *jsonObject = NULL;
	json_t *jsonValue = NULL;
	json_t *functionGroupsArray = NULL;
	json_error_t error;
	int functionGroupsArraySize = 0;
	int functionArraySize = 0;
	int variableArraySize = 0;

	//FILE *fp;
	//fp = fopen(fileName, "rb");
	FILE *fp;
	wstring fileNameStrW;
	fileNameStrW.assign((wchar_t *)fileName);
	fp = _wfopen(fileNameStrW.c_str(), L"rb");

	if (!fp) {
		return -1;
	}
	root = json_loadf(fp, 0, &error);

	/*jsonRoot = json_loads((const char *)str, 0, &error);
	if (!jsonRoot) {
	return -3;
	}*/

	//version 直接吃 R8_PROJECT_VERSION
	//getJsonString(root, "version", version, R8_STRING_SIZE);

	functionGroups.clear();
	functionGroupsArray = json_object_get(root, "functionGroups");
	if (functionGroupsArray != NULL) {
		functionGroupsArraySize = (int)json_array_size(functionGroupsArray);
		if (functionGroupsArraySize > 0) {
			for (i = 0; i < functionGroupsArraySize; i++) {
				functionGroup = json_array_get(functionGroupsArray, i);
				functionGroup = json_object_get(functionGroup, "functionGroup");//目前不是傳純 Array ，裡面有多一層[物件]，所以這裡也要多接一層
				R8_FunctionGroup R8FunctionGroup;

				getJsonString(functionGroup, "name", R8FunctionGroup.name, R8_STRING_SIZE);

				functionArray = json_object_get(functionGroup, "functions");
				functionArraySize = (int)json_array_size(functionArray);
				if (functionArraySize > 0) {
					for (j = 0; j < functionArraySize; j++) {
						function = json_array_get(functionArray, j);
						function = json_object_get(function, "function");//這也是裡面有多一層[物件]，也要多接一層
						R8_Function R8Function;
						res = getJsonString(function, "name", R8Function.name, R8_STRING_SIZE);
						if (res >= 0) {
							R8Function.name[res] = '\0';
						}
						else {
							R8Function.name[0] = '\0';
						}
						res = getJsonString(function, "doc", R8Function.doc, R8_STRING_SIZE);
						if (res >= 0) {
							R8Function.doc[res] = '\0';
						}
						else {
							R8Function.doc[0] = '\0';
						}
						//doc
						//fprintf(dbgFp, "name: %s\n", testString);

						variableArray = json_object_get(function, "variables");
						variableArraySize = (int)json_array_size(variableArray);
						if (variableArraySize > 0) {
							for (k = 0; k < variableArraySize; k++) {
								variable = json_array_get(variableArray, k);
								variable = json_object_get(variable, "variable");

								R8_Variable R8Variable;
								res = getJsonString(variable, "name", R8Variable.name, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.name[res] = '\0';
								}
								else {
									R8Variable.name[0] = '\0';
								}
								res = getJsonString(variable, "type", R8Variable.type, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.type[res] = '\0';
								}
								else {
									R8Variable.type[0] = '\0';
								}
								//json_object_set_new(variableObject, "direction", json_string(direction));

								//20171024 direction 要變成 option 然後還要支援舊版 
								res = getJsonString(variable, "option", R8Variable.option, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.option[res] = '\0';
								}
								else {
									//R8Variable.option[0] = '\0';
									res = getJsonString(variable, "direction", R8Variable.option, R8_STRING_SIZE);
									if (res >= 0) {
										R8Variable.option[res] = '\0';
									}
									else {
										R8Variable.option[0] = '\0';
									}
								}
								R8Function.variables.push_back(R8Variable);

							}
						}
						R8FunctionGroup.functions.push_back(R8Function);
					}
				}
				functionGroups.push_back(R8FunctionGroup);
			}
		}
	}
	//然後還有 root 層的 variable型別定義
	variableArray = json_object_get(root, "variables");
	variableArraySize = (int)json_array_size(variableArray);
	types.clear();
	if (variableArraySize > 0) {
		for (i = 0; i < variableArraySize; i++) {
			variable = json_array_get(variableArray, i);
			variable = json_object_get(variable, "variable");
			R8_Variable R8Variable;
			res = getJsonString(variable, "type", R8Variable.type, R8_STRING_SIZE);
			R8Variable.name[res] = '\0';
			types.push_back(R8Variable);
		}
	}

	// 釋放 JSON 使用的記憶體
	json_decref(root);
	return 1;
}

R8_API int R8_AddLibrarySupportListByFileName(char *fileName)
{
	int res;
	int i, j, k;

	json_t *root = NULL;
	json_t *functionGroup;
	json_t *functionArray;
	json_t *function;
	json_t *variableArray;
	json_t *variable;
	json_t *jsonObject = NULL;
	json_t *jsonValue = NULL;
	json_t *functionGroupsArray = NULL;
	json_error_t error;
	int functionGroupsArraySize = 0;
	int functionArraySize = 0;
	int variableArraySize = 0;

	FILE *fp;
	wstring fileNameStrW;
	fileNameStrW.assign((wchar_t *)fileName);
	fp = _wfopen(fileNameStrW.c_str(), L"rb");
	if (!fp) {
		return -1;
	}
	root = json_loadf(fp, 0, &error);


	//functionGroups.clear();
	functionGroupsArray = json_object_get(root, "functionGroups");
	if (functionGroupsArray != NULL) {
		functionGroupsArraySize = (int)json_array_size(functionGroupsArray);
		if (functionGroupsArraySize > 0) {
			for (i = 0; i < functionGroupsArraySize; i++) {
				functionGroup = json_array_get(functionGroupsArray, i);
				functionGroup = json_object_get(functionGroup, "functionGroup");//目前不是傳純 Array ，裡面有多一層[物件]，所以這裡也要多接一層
				R8_FunctionGroup R8FunctionGroup;

				getJsonString(functionGroup, "name", R8FunctionGroup.name, R8_STRING_SIZE);

				functionArray = json_object_get(functionGroup, "functions");
				functionArraySize = (int)json_array_size(functionArray);
				if (functionArraySize > 0) {
					for (j = 0; j < functionArraySize; j++) {
						function = json_array_get(functionArray, j);
						function = json_object_get(function, "function");//這也是裡面有多一層[物件]，也要多接一層
						R8_Function R8Function;
						res = getJsonString(function, "name", R8Function.name, R8_STRING_SIZE);
						if (res >= 0) {
							R8Function.name[res] = '\0';
						}
						else {
							R8Function.name[0] = '\0';
						}
						res = getJsonString(function, "doc", R8Function.doc, R8_STRING_SIZE);
						if (res >= 0) {
							R8Function.doc[res] = '\0';
						}
						else {
							R8Function.doc[0] = '\0';
						}
						//doc
						//fprintf(dbgFp, "name: %s\n", testString);

						variableArray = json_object_get(function, "variables");
						variableArraySize = (int)json_array_size(variableArray);
						if (variableArraySize > 0) {
							for (k = 0; k < variableArraySize; k++) {
								variable = json_array_get(variableArray, k);
								variable = json_object_get(variable, "variable");

								R8_Variable R8Variable;
								res = getJsonString(variable, "name", R8Variable.name, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.name[res] = '\0';
								}
								else {
									R8Variable.name[0] = '\0';
								}
								res = getJsonString(variable, "type", R8Variable.type, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.type[res] = '\0';
								}
								else {
									R8Variable.type[0] = '\0';
								}
								//json_object_set_new(variableObject, "direction", json_string(direction));
								//20171024 direction 要變成 option 然後還要支援舊版
								res = getJsonString(variable, "option", R8Variable.option, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.option[res] = '\0';
								}
								else {
									res = getJsonString(variable, "direction", R8Variable.option, R8_STRING_SIZE);
									if (res >= 0) {
										R8Variable.option[res] = '\0';
									}
									else {
										R8Variable.option[0] = '\0';
									}
								}
								R8Function.variables.push_back(R8Variable);

							}
						}
						R8FunctionGroup.functions.push_back(R8Function);
					}
				}
				functionGroups.push_back(R8FunctionGroup);
			}
		}
	}
	json_decref(root);
	return 1;
}

//20170417 macro 相關
//bool isMacroExist = false;
R8_FunctionGroup macroGroup;
R8_API int R8_StartMacroSupportList() {
	sprintf(macroGroup.name, "Macros");
	macroGroup.functions.clear();
	return 1;
}

R8_API int R8_AddMacroSupportListByFileName(char* fileName) {
	// functionGroup -> functions -> variables

	if (false) {
		R8_FunctionGroup macroGroup;
		R8_Function r8Function;
		R8_Variable r8Variable;

		sprintf(macroGroup.name, "MacroTest");
		

		sprintf(r8Function.doc, "");
		sprintf(r8Function.name, "testName");
		
		sprintf(r8Variable.option, "IN");
		sprintf(r8Variable.name, "Variable");
		sprintf(r8Variable.type, "int");
		sprintf(r8Variable.value, "0");

		r8Function.variables.push_back(r8Variable);
		macroGroup.functions.push_back(r8Function);
		functionGroups.push_back(macroGroup);
		return 2;
	}

	std::ostringstream oss;
	// 載入 XML 檔
	tinyxml2::XMLDocument xmlDoc;

	FILE *fp = NULL;
	wstring fileNameStrW;
	fileNameStrW.assign((wchar_t *)fileName);
	fp = _wfopen(fileNameStrW.c_str(), L"rb");

	if (xmlDoc.LoadFile(fp) != tinyxml2::XML_SUCCESS) {
		return -1;
	}
	tinyxml2::XMLElement* xmlRoot = xmlDoc.RootElement();

	//如果 macroGroup 未讀則讀入
	//if (!isMacroExist) {
	//	sprintf(macroGroup.name, "Macros");
	//	functionGroups.push_back(macroGroup);
	//	isMacroExist = true;
	//}


	R8_Function r8Function;
	//sprintf(r8Function.name, "Macro%d", (int)macroGroup.functions.size());
	string fileNameString;
	fileNameString.assign(fileName);
	fileNameString = fileNameString.substr(fileNameString.find_last_of("\\") + 1, fileNameString.find_last_of(".") - fileNameString.find_last_of("\\") - 1);
	sprintf(r8Function.name, fileNameString.c_str());
	r8Function.doc[0] = '\0';
	
	tinyxml2::XMLElement* pVariable = xmlRoot->FirstChildElement("variables")->FirstChildElement("variable");
	tinyxml2::XMLElement* pVariableNode = NULL;
	
	while (pVariable != NULL) {
		R8_Variable r8Variable;
		sprintf(r8Variable.option, "IN");//從 program 無法得知 variable 方向，先通通填 Input
		sprintf(r8Variable.value, "");
		pVariableNode = pVariable->FirstChildElement("name");		
		if (pVariableNode->GetText() == NULL) {
			//variable.name.assign("");
			sprintf(r8Variable.name, "Empty");
		}
		else {
			sprintf(r8Variable.name, pVariableNode->GetText());			
		}

		pVariableNode = pVariable->FirstChildElement("type");
		if (pVariableNode->GetText() == NULL) {
			//variable.name.assign("");
			sprintf(r8Variable.type, "int");
		}
		else {
			sprintf(r8Variable.type, pVariableNode->GetText());
		}
		r8Function.variables.push_back(r8Variable);
		pVariable = pVariable->NextSiblingElement();
	}

	macroGroup.functions.push_back(r8Function);
	if (fp) {
		fclose(fp);
		fp = NULL;
	}
	return 1;
}

R8_API int R8_EndOfMacroSupportList() {
	functionGroups.push_back(macroGroup);
	return 1;
}


R8_API int R8_BuildSupportListByString(char *str)
{
	int res;
	int i, j, k;

	json_t *root = NULL;
	json_t *functionGroup;
	json_t *functionArray;
	json_t *function;
	json_t *variableArray;
	json_t *variable;
	json_t *jsonObject = NULL;
	json_t *jsonValue = NULL;
	json_t *functionGroupsArray = NULL;
	json_error_t error;
	int functionGroupsArraySize = 0;
	int functionArraySize = 0;
	int variableArraySize = 0;

	root = json_loads((const char *)str, 0, &error);
	if (!root) {
		return -3;
	}

	//version 直接吃 R8_PROJECT_VERSION
	//getJsonString(root, "version", version, R8_STRING_SIZE);

	functionGroups.clear();
	functionGroupsArray = json_object_get(root, "functionGroups");
	if (functionGroupsArray != NULL) {
		functionGroupsArraySize = (int)json_array_size(functionGroupsArray);
		if (functionGroupsArraySize > 0) {
			for (i = 0; i < functionGroupsArraySize; i++) {
				functionGroup = json_array_get(functionGroupsArray, i);
				functionGroup = json_object_get(functionGroup, "functionGroup");//目前不是傳純 Array ，裡面有多一層[物件]，所以這裡也要多接一層
				R8_FunctionGroup R8FunctionGroup;

				getJsonString(functionGroup, "name", R8FunctionGroup.name, R8_STRING_SIZE);

				functionArray = json_object_get(functionGroup, "functions");
				functionArraySize = (int)json_array_size(functionArray);
				if (functionArraySize > 0) {
					for (j = 0; j < functionArraySize; j++) {
						function = json_array_get(functionArray, j);
						function = json_object_get(function, "function");//這也是裡面有多一層[物件]，也要多接一層
						R8_Function R8Function;
						res = getJsonString(function, "name", R8Function.name, R8_STRING_SIZE);
						if (res >= 0) {
							R8Function.name[res] = '\0';
						}
						else {
							R8Function.name[0] = '\0';
						}
						res = getJsonString(function, "doc", R8Function.doc, R8_STRING_SIZE);
						if (res >= 0) {
							R8Function.doc[res] = '\0';
						}
						else {
							R8Function.doc[0] = '\0';
						}
						//doc
						//fprintf(dbgFp, "name: %s\n", testString);

						variableArray = json_object_get(function, "variables");
						variableArraySize = (int)json_array_size(variableArray);
						if (variableArraySize > 0) {
							for (k = 0; k < variableArraySize; k++) {
								variable = json_array_get(variableArray, k);
								variable = json_object_get(variable, "variable");

								R8_Variable R8Variable;
								res = getJsonString(variable, "name", R8Variable.name, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.name[res] = '\0';
								}
								else {
									R8Variable.name[0] = '\0';
								}
								res = getJsonString(variable, "type", R8Variable.type, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.type[res] = '\0';
								}
								else {
									R8Variable.type[0] = '\0';
								}
								//json_object_set_new(variableObject, "direction", json_string(direction));
								res = getJsonString(variable, "option", R8Variable.option, R8_STRING_SIZE);
								if (res >= 0) {
									R8Variable.option[res] = '\0';
								}
								else {
									res = getJsonString(variable, "direction", R8Variable.option, R8_STRING_SIZE);
									if (res >= 0) {
										R8Variable.option[res] = '\0';
									}
									else {
										R8Variable.option[0] = '\0';
									}
								}
								R8Function.variables.push_back(R8Variable);

							}
						}
						R8FunctionGroup.functions.push_back(R8Function);
					}
				}
				functionGroups.push_back(R8FunctionGroup);
			}
		}
	}
	//然後還有 root 層的 variable型別定義
	variableArray = json_object_get(root, "variables");
	variableArraySize = (int)json_array_size(variableArray);
	types.clear();
	if (variableArraySize > 0) {
		for (i = 0; i < variableArraySize; i++) {
			variable = json_array_get(variableArray, i);
			variable = json_object_get(variable, "variable");
			R8_Variable R8Variable;
			res = getJsonString(variable, "type", R8Variable.type, R8_STRING_SIZE);
			R8Variable.name[res] = '\0';
			types.push_back(R8Variable);
		}
	}

	// 釋放 JSON 使用的記憶體
	json_decref(root);
	return 1;
}

R8_API int R8_GetVariableNum()
{
	return (int)types.size();
}

R8_API int R8_GetVariableName(char *str, int strSize, int typeSn)
{
	if (typeSn >= 0 && typeSn < (int)types.size()) {
		sprintf_s(str, strSize, "%s\0", types[typeSn].name);
		return 1;
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableType(char *str, int strSize, int typeSn)
{
	if (typeSn >= 0 && typeSn < (int)types.size()) {
		sprintf_s(str, strSize, "%s\0", types[typeSn].type);
		return 1;
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetFunctionGroupNum()
{
	return (int)functionGroups.size();
}

R8_API int R8_GetGetFunctionNum()
{
	int count = 0;
	for (int i = 0; i < (int)functionGroups.size(); i++) {
		count += (int)(functionGroups[i].functions.size());
	}
	return count;
}

R8_API int R8_GetFunctionNumInGroup(int groupSn)
{
	return (int)(functionGroups[groupSn].functions.size());
}

R8_API int R8_GetFunctionGroupName(char *str, int strSize, int groupSn)
{
	if (groupSn >= 0 && groupSn < (int)functionGroups.size()) {
		sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].name);
		return 1;
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetFunctionName(char *str, int strSize, int groupSn, int functionSn)
{
	if (groupSn >= 0 && groupSn < (int)functionGroups.size()) {
		if (functionSn >= 0 && functionSn < (int)functionGroups[groupSn].functions.size()) {
			sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].name);
			return 1;
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetFunctionDoc(char *str, int strSize, int groupSn, int functionSn)
{
	if (groupSn >= 0 && groupSn < (int)functionGroups.size()) {
		if (functionSn >= 0 && functionSn < (int)functionGroups[groupSn].functions.size()) {
			sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].doc);
			return 1;
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableNumInFunction(int groupSn, int functionSn)
{
	if (groupSn >= 0 && groupSn < (int)functionGroups.size()) {
		if (functionSn >= 0 && functionSn < (int)functionGroups[groupSn].functions.size()) {
			return (int)(functionGroups[groupSn].functions[functionSn].variables.size());
		}
		else {
		}
	}
	else {
	}
	return 0;
}

R8_API int R8_GetVariableNameInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn)
{
	if (groupSn >= 0 && groupSn < (int)functionGroups.size()) {
		if (functionSn >= 0 && functionSn < (int)functionGroups[groupSn].functions.size()) {
			if (variableSn >= 0 && variableSn < (int)functionGroups[groupSn].functions[functionSn].variables.size()) {
				sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].variables[variableSn].name);
				return 1;
			}
			else {
				return -1;
			}
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableOptionInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn)
{
	if (groupSn >= 0 && groupSn < (int)functionGroups.size()) {
		if (functionSn >= 0 && functionSn < (int)functionGroups[groupSn].functions.size()) {
			if (variableSn >= 0 && variableSn < (int)functionGroups[groupSn].functions[functionSn].variables.size()) {
				sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].variables[variableSn].option);
				return 1;
			}
			else {
				return -1;
			}
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableTypeInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn)
{
	if (groupSn >= 0 && groupSn < (int)functionGroups.size()) {
		if (functionSn >= 0 && functionSn < (int)functionGroups[groupSn].functions.size()) {
			if (variableSn >= 0 && variableSn < (int)functionGroups[groupSn].functions[functionSn].variables.size()) {
				sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].variables[variableSn].type);
				return 1;
			}
			else {
				return -1;
			}
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}


/*
int getJsonInt(json_t *root, char *key, int &value) {
	json_t *variable;
	variable = json_object_get(root, key);
	if (!json_is_string(variable)) {
		return -1;
	}
	value = atoi(json_string_value(variable));
	return 1;
}


int getJsonBool(json_t *root, char *key, bool &value) {
	json_t *variable;
	variable = json_object_get(root, key);
	if (!json_is_string(variable)) {
		//dbg("%s", key);
		return -1;
	}
	int intValue = atoi(json_string_value(variable));
	if (intValue == 0) {
		value = false;
	}
	else {
		value = true;
	}
	return 1;
}


int getJsonDouble(json_t *root, char *key, double &value) {
	json_t *variable;
	variable = json_object_get(root, key);
	if (!json_is_string(variable)) {
		//	dbg("%s", key);
		return -1;
	}
	value = atof(json_string_value(variable));
	return 1;
}


int getJsonString(json_t *root, char *key, char *value, int valueLength) {
	json_t *variable;
	variable = json_object_get(root, key);
	if (!json_is_string(variable)) {
		return -1;
	}
	strcpy_s(value, valueLength, json_string_value(variable));
	return (int)json_string_length(variable);
}

R8_API int R8_LoadList(char *str, int strSize)
{
	//debug 用
	//FILE *dbgFp;
	//dbgFp = fopen("C:\\Users\\a1s\\Documents\\debug.txt", "w");
	//fprintf(dbgFp, "\nR8_ReadList: %s \n", fileName);
	//char testString[STRING_SIZE] = "";

	//開啟 json 檔案
	int res;
	int i, j, k;

	//開始解析 json 
	json_t *jsonRoot = NULL;
	json_t *jsonObject = NULL;
	json_t *jsonValue = NULL;
	json_t *functionGroupsArray = NULL;
	json_t *functionGroup = NULL;
	json_t *functionArray = NULL;
	json_t *function = NULL;
	json_t *variableArray = NULL;
	json_t *variable = NULL;
	json_error_t error;
	int functionGroupsArraySize = 0;
	int functionArraySize = 0;
	int variableArraySize = 0;

	jsonRoot = json_loads((const char *)str, 0, &error);
	if (!jsonRoot) {
		return -3;
	}

	getJsonString(jsonRoot, "version", version, STRING_SIZE);

	functionGroups.clear();
	functionGroupsArray = json_object_get(jsonRoot, "functionGroups");
	if (functionGroupsArray != NULL) {
		functionGroupsArraySize = (int)json_array_size(functionGroupsArray);
		if (functionGroupsArraySize > 0) {
			for (i = 0; i < functionGroupsArraySize; i++) {
				functionGroup = json_array_get(functionGroupsArray, i);
				functionGroup = json_object_get(functionGroup, "functionGroup");//目前不是傳純 Array ，裡面有多一層[物件]，所以這裡也要多接一層
				R8FunctionGroup R8FunctionGroup;

				getJsonString(functionGroup, "name", R8FunctionGroup.name, STRING_SIZE);

				functionArray = json_object_get(functionGroup, "functions");
				functionArraySize = (int)json_array_size(functionArray);
				if (functionArraySize > 0) {
					for (j = 0; j < functionArraySize; j++) {
						function = json_array_get(functionArray, j);
						function = json_object_get(function, "function");//這也是裡面有多一層[物件]，也要多接一層
						R8Function R8Function;
						res = getJsonString(function, "name", R8Function.name, STRING_SIZE);
						R8Function.name[res] = '\0';
						//fprintf(dbgFp, "name: %s\n", testString);

						variableArray = json_object_get(function, "variables");
						variableArraySize = (int)json_array_size(variableArray);
						if (variableArraySize > 0) {
							for (k = 0; k < variableArraySize; k++) {
								variable = json_array_get(variableArray, k);
								//variable = json_object_get(variable, "variable"); //這邊目前沒多一層物件所以沒多接一層，但估計之後可能也會加上，所以這行先留著

								R8Variable R8Variable;
								res = getJsonString(variable, "name", R8Variable.name, STRING_SIZE);
								R8Variable.name[res] = '\0';
								res = getJsonString(variable, "type", R8Variable.type, STRING_SIZE);
								R8Variable.type[res] = '\0';
								R8Function.variables.push_back(R8Variable);

							}
						}
						R8FunctionGroup.functions.push_back(R8Function);
					}
				}
				functionGroups.push_back(R8FunctionGroup);
			}
		}
	}
	//然後還有 root 層的 variable型別定義
	variableArray = json_object_get(jsonRoot, "variables");
	variableArraySize = (int)json_array_size(variableArray);
	types.clear();
	if (variableArraySize > 0) {
		for (i = 0; i < variableArraySize; i++) {
			variable = json_array_get(variableArray, i);
			//variable = json_object_get(variable, "variable"); //這邊目前沒多一層物件所以沒多接一層，但估計之後可能也會加上，所以這行先留著
			R8Variable R8Variable;
			res = getJsonString(variable, "type", R8Variable.type, STRING_SIZE);
			R8Variable.name[res] = '\0';
			types.push_back(R8Variable);
		}
	}

	return 1;
}


R8_API int R8_ReadList(char *fileName) {
	//debug 用
	//FILE *dbgFp;
	//dbgFp = fopen("C:\\Users\\a1s\\Documents\\debug.txt", "w");
	//fprintf(dbgFp, "\nR8_ReadList: %s \n", fileName);
	//char testString[STRING_SIZE] = "";

	//開啟 json 檔案
	int res;
	int i, j, k;
	int jsonBufferLength = 0;
	FILE *fp;
	fp = fopen(fileName, "rb");
	if (!fp) {
		return -1;
	}

	fseek(fp, 0, SEEK_END);
	jsonBufferLength = ftell(fp);
	fseek(fp, 0, SEEK_SET);

	if (jsonBufferLength <= 0) {
		return -2;
	}

	if (jsonBuffer) {
		free(jsonBuffer);
	}
	jsonBuffer = (unsigned char*)malloc(jsonBufferLength + 1);
	fread(jsonBuffer, jsonBufferLength, 1, fp);
	jsonBuffer[jsonBufferLength] = '\0';
	fclose(fp);


	//fprintf(dbgFp, "jsonBuffer: %s \n", jsonBuffer);

	//開始解析 json 

	json_t *jsonRoot = NULL;
	json_t *jsonObject = NULL;
	json_t *jsonValue = NULL;
	json_t *functionGroupsArray = NULL;
	json_t *functionGroup = NULL;
	json_t *functionArray = NULL;
	json_t *function = NULL;
	json_t *variableArray = NULL;
	json_t *variable = NULL;
	json_error_t error;
	int functionGroupsArraySize = 0;
	int functionArraySize = 0;
	int variableArraySize = 0;

	jsonRoot = json_loads((const char *)jsonBuffer, 0, &error);
	if (!jsonRoot) {
		return -3;
	}

	getJsonString(jsonRoot, "version", version, STRING_SIZE);

	functionGroups.clear();
	functionGroupsArray = json_object_get(jsonRoot, "functionGroups");
	if (functionGroupsArray != NULL) {
		functionGroupsArraySize = (int)json_array_size(functionGroupsArray);
		if (functionGroupsArraySize > 0) {
			for (i = 0; i < functionGroupsArraySize; i++) {
				functionGroup = json_array_get(functionGroupsArray, i);				
				functionGroup = json_object_get(functionGroup, "functionGroup");//目前不是傳純 Array ，裡面有多一層[物件]，所以這裡也要多接一層
				R8FunctionGroup R8FunctionGroup;

				getJsonString(functionGroup, "name", R8FunctionGroup.name, STRING_SIZE);
				
				functionArray = json_object_get(functionGroup, "functions");				
				functionArraySize = (int)json_array_size(functionArray);
				if (functionArraySize > 0) {
					for (j = 0; j < functionArraySize; j++) {
						function = json_array_get(functionArray, j);
						function = json_object_get(function, "function");//這也是裡面有多一層[物件]，也要多接一層
						R8Function R8Function;
						res = getJsonString(function, "name", R8Function.name, STRING_SIZE);
						R8Function.name[res] = '\0';
						//fprintf(dbgFp, "name: %s\n", testString);

						variableArray = json_object_get(function, "variables");
						variableArraySize = (int)json_array_size(variableArray);
						if (variableArraySize > 0) {
							for (k = 0; k < variableArraySize; k++) {
								variable = json_array_get(variableArray, k);
								//variable = json_object_get(variable, "variable"); //這邊目前沒多一層物件所以沒多接一層，但估計之後可能也會加上，所以這行先留著

								R8Variable R8Variable;
								res = getJsonString(variable, "name", R8Variable.name, STRING_SIZE);
								R8Variable.name[res] = '\0';
								res = getJsonString(variable, "type", R8Variable.type, STRING_SIZE);
								R8Variable.type[res] = '\0';
								R8Function.variables.push_back(R8Variable);

							}
						}
						R8FunctionGroup.functions.push_back(R8Function);
					}				
				}
				functionGroups.push_back(R8FunctionGroup);
			}		
		}
	}
	//然後還有 root 層的 variable型別定義
	variableArray = json_object_get(jsonRoot, "variables");
	variableArraySize = (int)json_array_size(variableArray);
	types.clear();
	if (variableArraySize > 0) {
		for (i = 0; i < variableArraySize; i++) {
			variable = json_array_get(variableArray, i);
			//variable = json_object_get(variable, "variable"); //這邊目前沒多一層物件所以沒多接一層，但估計之後可能也會加上，所以這行先留著
			R8Variable R8Variable;
			res = getJsonString(variable, "type", R8Variable.type, STRING_SIZE);
			R8Variable.name[res] = '\0';
			types.push_back(R8Variable);
		}
	}
	return 1;
}

R8_API int R8_GetVariableNum() {
	return (int)types.size();
}

R8_API int R8_GetFunctionGroupNum() {
	return (int)functionGroups.size();
}


R8_API int R8_GetGetFunctionNum() {
	int count = 0;
	for (int i = 0; i < functionGroups.size(); i++) {
		count += (int)(functionGroups[i].functions.size());
	}
	return count;
}

R8_API int R8_GetFunctionNumInGroup(int groupSn) {
	return (int)(functionGroups[groupSn].functions.size());
}

R8_API int R8_GetVariableName(char *str, int strSize, int typeSn) {
	if (typeSn >= 0 && typeSn < types.size()) {
		sprintf_s(str, strSize, "%s\0", types[typeSn].name);
		return 1;
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableType(char *str, int strSize, int typeSn) {
	if (typeSn >= 0 && typeSn < types.size()) {
		sprintf_s(str, strSize, "%s\0", types[typeSn].type);
		return 1;
	}
	else {
		return -1;
	}
	return 1;
}


R8_API int R8_GetFunctionGroupName(char *str, int strSize, int groupSn) {
	if (groupSn >= 0 && groupSn < functionGroups.size()) {
		sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].name);
		return 1;
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetFunctionName(char *str, int strSize, int groupSn, int functionSn) {
	if (groupSn >= 0 && groupSn < functionGroups.size()) {
		if (functionSn >= 0 && functionSn < functionGroups[groupSn].functions.size()) {
			sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].name);
			return 1;
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableNameInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn) {
	if (groupSn >= 0 && groupSn < functionGroups.size()) {
		if (functionSn >= 0 && functionSn < functionGroups[groupSn].functions.size()) {
			if (variableSn >= 0 && variableSn < functionGroups[groupSn].functions[functionSn].variables.size()) {
				sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].variables[variableSn].name);
				return 1;
			}
			else {
				return -1;
			}
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableTypeInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn) {
	if (groupSn >= 0 && groupSn < functionGroups.size()) {
		if (functionSn >= 0 && functionSn < functionGroups[groupSn].functions.size()) {
			if (variableSn >= 0 && variableSn < functionGroups[groupSn].functions[functionSn].variables.size()) {
				sprintf_s(str, strSize, "%s\0", functionGroups[groupSn].functions[functionSn].variables[variableSn].type);
				return 1;
			}
			else {
				return -1;
			}
		}
		else {
			return -1;
		}
	}
	else {
		return -1;
	}
	return 1;
}

R8_API int R8_GetVariableNumInFunction(int groupSn, int functionSn) {
	if (groupSn >= 0 && groupSn < functionGroups.size()) {
		if (functionSn >= 0 && functionSn < functionGroups[groupSn].functions.size()) {
			return (int)(functionGroups[groupSn].functions[functionSn].variables.size());
		}
		else {
		}
	}
	else {
	}
	return 0;
}

R8_API int R8_GetBrainVersion(char *str, int strSize) {
	sprintf_s(str, strSize, "%s\0", version);
	return 0;
}
*/
/*
FunctionGroup
-FunctionGroupName
-Function
--FunctionName
--Variable
---VariableName
---VariableType
*/

int r8_OpenLog(char *fileName) {
	DWORD pos;
	DWORD bytesWritten;
	wchar_t wstr[R8_STRING_SIZE];

	int res = MultiByteToWideChar(CP_UTF8, 0, fileName, -1, wstr, R8_STRING_SIZE * sizeof(wchar_t));
	if (res <= 0) {
		return -1;
	}

	r8Log = CreateFile((LPCWSTR)wstr, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (r8Log == INVALID_HANDLE_VALUE) {
		return -2;
	}

	pos = SetFilePointer(r8Log, 0, NULL, FILE_END);

	if (pos == 0) {
		// Add utf-8 BOM header at the beginning of the file.
		unsigned char format[] = { 0xEF, 0xBB, 0xBF };
		BOOL res = WriteFile(r8Log, (char*)format, (int)sizeof(format), &bytesWritten, NULL);
		if (!res) {
			return -3;
		}
	}

	return 1;
}

int r8_CloseLog() {
	CloseHandle(r8Log);
	r8Log = INVALID_HANDLE_VALUE;

	return 1;
}

// TODO: Add semaphore.
R8_API int R8_Log(char *format, ...) {
	int res;
	char str[R8_STRING_SIZE];
	char str2[R8_STRING_SIZE];
	va_list args;

	va_start(args, format);

	res = vsprintf_s(str, R8_STRING_SIZE, format, args);

	va_end(args);

	if (res <= 0) {
		return -1;
	}

	SYSTEMTIME sysTime;
	GetLocalTime(&sysTime);
	sprintf_s(str2, R8_STRING_SIZE, "[%d/%02d/%02d %02d:%02d:%02d] %s\r\n", sysTime.wYear, sysTime.wMonth, sysTime.wDay, sysTime.wHour, sysTime.wMinute, sysTime.wSecond, str);
	// TODO: Add program name.

	res = r8_OpenLog("R8.log");
	if (res <= 0) {
		return -2;
	}

	// TODO: Calculate UTF-8 strlen of str2.

	// Write file.
	DWORD bytesWritten;
	BOOL resBool = WriteFile(r8Log, str2, (DWORD)strlen(str2), &bytesWritten, NULL);
	if (!resBool) {
		r8_CloseLog();
		return -3;
	}

	r8_CloseLog();
	return 1;
}

R8_API int R8_LogW(wchar_t *format, ...) {
	wchar_t wstr[R8_STRING_SIZE];
	va_list args;

	va_start(args, format);

	vswprintf_s(wstr, R8_STRING_SIZE, format, args);

	va_end(args);

	char str[R8_STRING_SIZE];
	char str2[R8_STRING_SIZE];
	int res;
	DWORD bytesWritten;

	str[0] = '\0';

	res = WideCharToMultiByte(CP_UTF8, 0, wstr, (int)wcslen(wstr) * 2, str, R8_STRING_SIZE, NULL, NULL);
	if (res <= 0) {
		return -1;
	}

	SYSTEMTIME sysTime;
	GetLocalTime(&sysTime);
	sprintf_s(str2, R8_STRING_SIZE, "[%d/%02d/%02d %02d:%02d:%02d] %s\r\n", sysTime.wYear, sysTime.wMonth, sysTime.wDay, sysTime.wHour, sysTime.wMinute, sysTime.wSecond, str);

	res = r8_OpenLog("R8.log");
	if (res <= 0) {
		return -2;
	}

	// TODO: Chinese bug in str2.
	BOOL resBool = WriteFile(r8Log, str2, (DWORD)strlen(str2), &bytesWritten, NULL);
	//BOOL resBool = WriteFile(r7Log, str2, (DWORD)strlen(str2), &bytesWritten, NULL);
	if (!resBool) {
		r8_CloseLog();
		return -2;
	}

	r8_CloseLog();
	return 1;
}

//20180212 leo: Config 相關
json_t *configJson = NULL;
// configPath 八成是寫死不動...總之端口還是先開出來
R8_API int R8_LoadConfigFile(char *configPath) {

	json_error_t error;
	FILE *fp;
	wstring fileNameStrW;
	fileNameStrW.assign((wchar_t *)configPath);
	fp = _wfopen(fileNameStrW.c_str(), L"rb");

	if (!fp) {
		return -1;
	}
	configJson = json_loadf(fp, 0, &error);

	return 1;
}

R8_API int R8_WriteConfig(char *key, char *value) {
	if (configJson == NULL) {
		configJson = json_object();
	}
	json_object_set_new(configJson, key, json_string(value));
	return 1;
}


R8_API int R8_ReadConfig(char *key, char *value) {
	if (configJson == NULL) {
		value[0] = 0;
		return -1;
	}
	json_t* target = json_object_get(configJson, key);
	if (target != NULL) {
		if (json_is_string(target)) {
			strcpy_s(value, R8_STRING_SIZE, json_string_value(target));
		}
		else {
			value[0] = 0;
		}
	}
	else {
		value[0] = 0;
	}
	return 1;
}

R8_API int R8_SaveConfigFile(char *configPath) {
	if (configJson == NULL) {
		configJson = json_object();
	}
	FILE *fp;
	wstring fileNameStrW;
	fileNameStrW.assign((wchar_t *)configPath);
	fp = _wfopen(fileNameStrW.c_str(), L"wb");

	if (!fp) {
		return -1;
	}
	json_dumpf(configJson, fp, JSON_INDENT(2));
	return 1;
}

//20180212 leo: 語言切換功能相關，下午接到指示格式要改用 json

json_t *languageJson = NULL;
R8_API int R8_LoadLanguageFile(char *path, char *err) {
	json_error_t error;
	FILE *fp;
	wstring fileNameStrW;
	fileNameStrW.assign((wchar_t *)path);
	fp = _wfopen(fileNameStrW.c_str(), L"rb");

	if (!fp) {
		return -1;
	}

	fseek(fp, 0, SEEK_END);
	int fileSize = ftell(fp) + 1;
	fseek(fp, 0, SEEK_SET);
	char *buffer;
	buffer = (char *)malloc(fileSize);
	fread(buffer, 1, fileSize - 1, fp);
	fclose(fp);
	int shift = 0;
	while (shift < fileSize) {
		if (buffer[shift] == '{') {
			break;
		}
		shift++;
	}
	int i;
	for (i = buffer[fileSize - 2]; i > shift; i--) {
		if (buffer[i] == '}') {
			buffer[i + 1] = '\0';
		}
	}
	languageJson = json_loads(buffer + shift, 0, &error);
	//languageJson = json_load_file(path, 0, &error);
	sprintf(err, "%s %s", error.text, error.source);
	free(buffer);
	return 1;
}

R8_API int R8_TranslationString(char *key, char *value) {
	if (key == NULL) {
		value[0] = 0;
		return -1;
	}
	if (languageJson == NULL) {
		strcpy_s(value, R8_STRING_SIZE, key);
		return -2;
	}
	json_t* target = json_object_get(languageJson, key);
	if (target != NULL) {
		if (json_is_string(target)) {
			strcpy_s(value, R8_STRING_SIZE, json_string_value(target));
		}
		else {
			strcpy_s(value, R8_STRING_SIZE, key);
		}
	}
	else {
		strcpy_s(value, R8_STRING_SIZE, key);
	}
	return 1;
}

#ifdef __cplusplus
}
#endif
