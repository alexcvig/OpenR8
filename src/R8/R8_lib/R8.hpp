#ifndef __R8_HPP__
#define __R8_HPP__


#define R8_API __declspec(dllexport)


#ifdef __cplusplus
extern "C"
{
#endif

	R8_API int R8_InitLib();
	R8_API int R8_CloseLib();

	R8_API int R8_GetVariableNum();
	R8_API int R8_GetVariableName(char *str, int strSize, int typeSn);
	R8_API int R8_GetVariableType(char *str, int strSize, int typeSn);
	R8_API int R8_GetFunctionGroupNum();
	R8_API int R8_GetGetFunctionNum();
	R8_API int R8_GetFunctionNumInGroup(int groupSn);
	R8_API int R8_GetFunctionGroupName(char *str, int strSize, int groupSn);
	R8_API int R8_GetFunctionName(char *str, int strSize, int groupSn, int functionSn);
	R8_API int R8_GetFunctionDoc(char *str, int strSize, int groupSn, int functionSn);//leo: 20170308 增加，取得對應的 API 說明頁面之 url
	R8_API int R8_GetVariableNumInFunction(int groupSn, int functionSn);
	R8_API int R8_GetVariableNameInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn);
	R8_API int R8_GetVariableTypeInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn);
	R8_API int R8_GetVariableOptionInFunction(char *str, int strSize, int groupSn, int functionSn, int variableSn);
	R8_API int R8_GetVariableNum();
	R8_API int R8_GetSupportListByFileName(char *fileName);
	R8_API int R8_AddLibrarySupportListByFileName(char *fileName);

	//20170417 Macro 相關
	R8_API int R8_StartMacroSupportList();
	R8_API int R8_AddMacroSupportListByFileName(char* fileName);
	R8_API int R8_EndOfMacroSupportList();//在 MacroSupportList 都設置後，還需要 push 
	
	R8_API int R8_LogW(wchar_t *format, ...);

#ifdef __cplusplus
}
#endif

#endif // __R8_HPP__ 
