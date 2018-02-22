/*
Copyright (c) 2004-2018 Open Robot Club. All rights reserved.
File library for R7.
*/


#include "R7.hpp"


using namespace std;


#ifdef __cplusplus
extern "C"
{
#endif

	
static int File_DeleteFile(int r7Sn, int functionSn) {

	char fileName[R7_STRING_SIZE];

	R7_GetVariableString(r7Sn, functionSn, 2, fileName, R7_STRING_SIZE);

	//recipe path wchar_t
	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);

	wchar_t workSpacePathW[R7_STRING_SIZE];
	memset(workSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, workSpacePathW, R7_STRING_SIZE * 2);
	
	wchar_t fileNameW[R7_STRING_SIZE];
	memset(fileNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, fileName, -1, fileNameW, R7_STRING_SIZE * 2);

	wchar_t filePathW[R7_STRING_SIZE];
	memset(filePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	wsprintf(filePathW, L"%s%s\0\0", workSpacePathW, fileNameW);

	HANDLE file = CreateFile(filePathW, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);

	if (file == INVALID_HANDLE_VALUE) {
		//printf("File does not exist!\n");
		R7_SetVariableInt(r7Sn, functionSn, 1, -2);
		CloseHandle(file);
		return 1;
	}

	CloseHandle(file);

	SHFILEOPSTRUCT dFile;

	memset(&dFile, 0, sizeof(SHFILEOPSTRUCT));
	dFile.wFunc = FO_DELETE;
	dFile.fFlags = FOF_ALLOWUNDO; // | FOF_NOCONFIRMATION;
	dFile.pFrom = filePathW;
	dFile.fAnyOperationsAborted = true;
	
	int result = ::SHFileOperation(&dFile);
	int result2;

	if (result == 0) {
		result2 = 1;
		//printf("Delete file success\n");
	} else {
		result2 = -1;
		//printf("Delete file fail\n");
	}

	R7_SetVariableInt(r7Sn, functionSn, 1, result2);

	return 1;
}

static int File_DeleteDir(int r7Sn, int functionSn) {
	char dirName[R7_STRING_SIZE];

	R7_GetVariableString(r7Sn, functionSn, 2, dirName, R7_STRING_SIZE);

	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);
	
	wchar_t workSpacePathW[R7_STRING_SIZE];
	memset(workSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, workSpacePathW, R7_STRING_SIZE * 2);
	
	wchar_t dirNameW[R7_STRING_SIZE];
	memset(dirNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, dirName, -1, dirNameW, R7_STRING_SIZE * 2);

	wchar_t DirPath[R7_STRING_SIZE] = L"";
	memset(DirPath, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	wsprintf(DirPath, L"%s%s", workSpacePathW, dirNameW);

	LPCWSTR p = DirPath;
	DWORD ftyp = GetFileAttributesW(p);

	if (ftyp == INVALID_FILE_ATTRIBUTES) {
		//printf("Dir does not exist!\n");
		R7_SetVariableInt(r7Sn, functionSn, 1, -2);
		return 1;
	}
	
	SHFILEOPSTRUCT dDir;

	memset(&dDir, 0, sizeof(SHFILEOPSTRUCT));
	dDir.wFunc = FO_DELETE;
	dDir.fFlags = FOF_ALLOWUNDO; // | FOF_NOCONFIRMATION;
	dDir.pFrom = DirPath;
	dDir.fAnyOperationsAborted = true;
	
	int result = ::SHFileOperation(&dDir);
	int result2;

	if (result == 0) {
		result2 = 1;
		//printf("remove folder success\n");
	} else {
		result2 = -1;
		//printf("remove folder fail\n");
	}

	R7_SetVariableInt(r7Sn, functionSn, 1, result2);
	return 1;
}

static int File_ReadImage(int r7Sn, int functionSn) {
	char fileName[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, fileName, R7_STRING_SIZE);

	cv::Mat mat = cv::Mat();

	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);

	wchar_t fileNameW[R7_STRING_SIZE];
	memset(fileNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, fileName, -1, fileNameW, R7_STRING_SIZE * 2);
	
	wchar_t WorkSpacePathW[R7_STRING_SIZE];
	memset(WorkSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, WorkSpacePathW, R7_STRING_SIZE * 2);
	
	wchar_t FilePathW[R7_STRING_SIZE];
	memset(FilePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	wsprintf(FilePathW, L"%s%s", WorkSpacePathW, fileNameW);

	HANDLE file = CreateFile(FilePathW, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (file == INVALID_HANDLE_VALUE) {
		file = CreateFile(fileNameW, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);
		if (file == INVALID_HANDLE_VALUE)
		{
			CloseHandle(file);
			R7_SetVariableInt(r7Sn, functionSn, 1, 0);
			return 1;
		}
	}

	DWORD dwBytesRead, dwBytesToRead, fileSize;
	char *buffer, *tmpBuf;

	fileSize = GetFileSize(file, NULL);//get file size

	buffer = (char *)malloc(fileSize);
	//ZeroMemory(buffer, fileSize);

	dwBytesToRead = fileSize;
	dwBytesRead = 0;
	tmpBuf = buffer;

	do { 
		ReadFile(file, tmpBuf, dwBytesToRead, &dwBytesRead, NULL);
		if (dwBytesRead == 0) {
			break;
		}
		dwBytesToRead -= dwBytesRead;
		tmpBuf += dwBytesRead;
	} while (dwBytesToRead > 0);

	mat = cv::imdecode(cv::Mat(1, fileSize, CV_8UC3, buffer), CV_LOAD_IMAGE_COLOR);

	if (mat.cols == 0) {
		R7_SetVariableInt(r7Sn, functionSn, 1, 0);
		return 1;
	}
	free(buffer);
	CloseHandle(file);

	R7_SetVariableInt(r7Sn, functionSn, 1, fileSize);
	R7_SetVariableMat(r7Sn, functionSn, 2, mat);


	mat.release();

	return 1;
}

static int File_SaveImage(int r7Sn, int functionSn) {
	cv::Mat image;

	R7_GetVariableMat(r7Sn, functionSn, 2, &image);
	if (image.cols == 0) {
		R7_SetVariableInt(r7Sn, functionSn, 1, -2);
		return -1;
	}

	char fileName[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, fileName, R7_STRING_SIZE);
	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);

	wchar_t fileNameW[R7_STRING_SIZE];
	memset(fileNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, fileName, -1, fileNameW, R7_STRING_SIZE * 2);

	wchar_t WorkSpacePathW[R7_STRING_SIZE];
	memset(WorkSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, WorkSpacePathW, R7_STRING_SIZE * 2);

	wchar_t FilePathW[R7_STRING_SIZE];
	memset(FilePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	wsprintf(FilePathW, L"%s%s", WorkSpacePathW, fileNameW);

	char drive[5], dir[100], fileNames[100], fileType[10];
	_splitpath(fileName, drive, dir, fileNames, fileType);

	std::vector<uchar> buffer;

	if (strcmp(fileType, ".png") == 0) {
		cv::imencode(".png", image, buffer);
	}
	else if (strcmp(fileType, ".jpg") == 0 || strcmp(fileType, "jpeg") == 0 || strcmp(fileType, ".jpe") == 0) {
		cv::imencode(".jpg", image, buffer);
	}
	else if (strcmp(fileType, ".bmp") == 0 || strcmp(fileType, ".dib") == 0) {
		cv::imencode(".bmp", image, buffer);
	}
	else if (strcmp(fileType, ".jp2") == 0) {
		cv::imencode(".jp2", image, buffer);
	}
	else if (strcmp(fileType, ".pbm") == 0) {
		cv::imencode(".pbm", image, buffer);
	}
	else if (strcmp(fileType, ".pgm") == 0) {
		cv::imencode(".pgm", image, buffer);
	}
	else if (strcmp(fileType, ".ppm") == 0) {
		cv::imencode(".ppm", image, buffer);
	}
	else if (strcmp(fileType, ".tif") == 0 || strcmp(fileType, ".tiff") == 0) {
		cv::imencode(".tif", image, buffer);
	}
	else {
		cv::imencode(".png", image, buffer);
	}


	HANDLE file;
	DWORD dwBytesWrite, dwBytesToWrite;
	char *tmpBuf;

	file = CreateFile(FilePathW, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

	if (file == INVALID_HANDLE_VALUE) {
		file = CreateFile(fileNameW, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

		if (file == INVALID_HANDLE_VALUE) {
			CloseHandle(file);
			R7_SetVariableInt(r7Sn, functionSn, 1, -1);
			return -1;
		}
	}

	dwBytesToWrite = DWORD(buffer.size());
	dwBytesWrite = 0;
	tmpBuf = (char*)&buffer[0];

	do {                                    
		WriteFile(file, tmpBuf, dwBytesToWrite, &dwBytesWrite, NULL);
		dwBytesToWrite -= dwBytesWrite;
		tmpBuf += dwBytesWrite;

	} while (dwBytesToWrite > 0);

	CloseHandle(file);
	R7_SetVariableInt(r7Sn, functionSn, 1, int(buffer.size()));

	image.release();
	buffer.clear();

	return 1;
}

static int File_ReadString(int r7Sn, int functionSn) {
	//result
	//return -1:file name is NULL
	//return -2:file is not exist

	int result = 0;

	//output
	char *string;
	//intput
	char fileName[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, fileName, R7_STRING_SIZE);

	if (fileName[0] == '\0') {
		//R7_Printf(r7Sn, "\nERROR! fileName\n");
		result = -1;
		R7_SetVariableInt(r7Sn, functionSn, 1, result);
		return -1;
	}

	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);

	wchar_t workSpacePathW[R7_STRING_SIZE];
	memset(workSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, workSpacePathW, R7_STRING_SIZE * 2);

	wchar_t fileNameW[R7_STRING_SIZE];
	memset(fileNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, fileName, -1, fileNameW, R7_STRING_SIZE * 2);

	wchar_t filePathW[R7_STRING_SIZE];
	memset(filePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	wsprintf(filePathW, L"%s%s\0", workSpacePathW, fileNameW);

	HANDLE hDstFile;
	hDstFile = CreateFile(filePathW, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hDstFile == INVALID_HANDLE_VALUE) {
		hDstFile = CreateFile(fileNameW, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hDstFile == INVALID_HANDLE_VALUE) {
			CloseHandle(hDstFile);
			//R7_Printf(r7Sn, "\nERROR! fileName is not exist\n");
			result = -2;
			R7_SetVariableInt(r7Sn, functionSn, 1, result);
			return -2;
		}
	}

	result = GetFileSize(hDstFile, NULL);
	char *string1, *tmpBuf;
	DWORD dwBytesToRead, dwBytesRead;
	string1 = (char *)malloc(result + 1);
	//ZeroMemory(string, result + 1);

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

	CloseHandle(hDstFile);

	//some utf 8 file in the first will add BOM¡Athis BOM make first string having spece
	if (string1[0] == '\xEF' && string1[1] == '\xBB' && string1[2] == '\xBF') {
		result -= 3;
		string = (char *)malloc(result + 1 - 3);
		char *tempStr = &string1[3];
		strncpy(string, tempStr, result);
		string[result] = '\0';
	}
	else {
		string1[result] = '\0';
		string = string1;
	}

	// TODO: Detect string encoding format. Then convert to UTF-8.

	R7_SetVariableInt(r7Sn, functionSn, 1, result);
	R7_SetVariableString(r7Sn, functionSn, 2, string); // UTF-8

	free(string1);

	return 1;
}

static int File_SaveString(int r7Sn, int functionSn) {
	//result
	//return -1:file name is NULL
	//return -2:fileName  is not exist
	int result = 0;

	//input
	char string[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 2, string, R7_STRING_SIZE);
	//intput
	char fileName[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, fileName, R7_STRING_SIZE);

	if (fileName[0] == '\0') {
		//R7_Printf(r7Sn, "\nERROR! fileName\n");
		result = -1;
		R7_SetVariableInt(r7Sn, functionSn, 1, result);
		return -1;
	}

	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);

	wchar_t workSpacePathW[R7_STRING_SIZE];
	memset(workSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, workSpacePathW, R7_STRING_SIZE * 2);

	wchar_t fileNameW[R7_STRING_SIZE];
	memset(fileNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, fileName, -1, fileNameW, R7_STRING_SIZE * 2);

	wchar_t filePathW[R7_STRING_SIZE];
	memset(filePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	wsprintf(filePathW, L"%s%s\0", workSpacePathW, fileNameW);

	//check path exist 
	//CREATE_NEW : create file ,if exist will return -1
	//CREATE_ALWAYS : create file , if exist will cover
	//OPEN_EXISTING : create file, if not exist will return -1
	HANDLE hDstFile;
	hDstFile = CreateFile(filePathW, GENERIC_WRITE | GENERIC_READ, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hDstFile == INVALID_HANDLE_VALUE) {
		hDstFile = CreateFile(fileNameW, GENERIC_WRITE | GENERIC_READ, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hDstFile == INVALID_HANDLE_VALUE) {
			CloseHandle(hDstFile);
			//R7_Printf(r7Sn, "\nERROR! fileName is not exist\n");
			result = -2;
			R7_SetVariableInt(r7Sn, functionSn, 1, result);
			return -2;
		}
	}

	result = (int)strlen(string);
	char *string1;
	DWORD dwBytesToWrite, dwBytesWrite;
	string1 = (char *)malloc(result + 1);
	dwBytesToWrite = result;
	dwBytesWrite = 0;

	string1 = string;

	do {
		WriteFile(hDstFile, &string, result, &dwBytesWrite, NULL);
		dwBytesToWrite -= dwBytesWrite;
		string1 += dwBytesWrite;

	} while (dwBytesToWrite > 0);

	CloseHandle(hDstFile);

	// TODO: Detect string encoding format. Then convert to UTF-8.

	R7_SetVariableInt(r7Sn, functionSn, 1, result);

	//free(string1);
	//free(string);


	return 1;
}

static int File_ReadBinary(int r7Sn, int functionSn) {
	//result
	//return -1:file name is NULL
	//return -2:file is not exist

	int result = 0;

	//output
	unsigned char *binary;

	//intput
	char fileName[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, fileName, R7_STRING_SIZE);

	if (fileName[0] == '\0') {
		//R7_Printf(r7Sn, "\nERROR! fileName\n");
		result = -1;
		R7_SetVariableInt(r7Sn, functionSn, 1, result);
		return -1;
	}

	//recipe path
	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);

	//recipe path wchar_t
	wchar_t workSpacePathW[R7_STRING_SIZE];
	memset(workSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, workSpacePathW, R7_STRING_SIZE * 2);

	wchar_t fileNameW[R7_STRING_SIZE];
	memset(fileNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, fileName, -1, fileNameW, R7_STRING_SIZE * 2);

	wchar_t filePathW[R7_STRING_SIZE];
	memset(filePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	wsprintf(filePathW, L"%s%s\0", workSpacePathW, fileNameW);

	HANDLE hDstFile;
	hDstFile = CreateFile(filePathW, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hDstFile == INVALID_HANDLE_VALUE) {
		hDstFile = CreateFile(fileNameW, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hDstFile == INVALID_HANDLE_VALUE) {
			CloseHandle(hDstFile);
			//R7_Printf(r7Sn, "\nERROR! fileName is not exist\n");
			result = -2;
			R7_SetVariableInt(r7Sn, functionSn, 1, result);
			return -2;
		}
	}
	//GetFileSize
	result = GetFileSize(hDstFile, NULL);
	unsigned char *tmpBuf;
	DWORD dwBytesToRead, dwBytesRead;
	binary = (unsigned char *)malloc(result);
	//ZeroMemory(binary, result);

	dwBytesToRead = result;
	dwBytesRead = 0;
	tmpBuf = binary;

	do {
		ReadFile(hDstFile, tmpBuf, dwBytesToRead, &dwBytesRead, NULL);
		if (dwBytesRead == 0) {
			break;
		}
		dwBytesToRead -= dwBytesRead;
		tmpBuf += dwBytesRead;
	} while (dwBytesToRead > 0);
	//binary[result] = '\0';
	CloseHandle(hDstFile);
	R7_SetVariableInt(r7Sn, functionSn, 1, result);
	R7_SetVariableBinary(r7Sn, functionSn, 2, binary, result);
	free(binary);

	return 1;
}
static int File_SaveBinary(int r7Sn, int functionSn) {
	//result
	//return -1:file name is NULL
	//return -2:fileName  is not exist
	int result = 0;

	//input binary
	int binarySize = R7_GetVariableBinarySize(r7Sn, functionSn, 2);

	unsigned char *binary = (unsigned char *)malloc(binarySize);
	R7_GetVariableBinary(r7Sn, functionSn, 2, binary, binarySize);

	//intput
	char fileName[R7_STRING_SIZE];
	R7_GetVariableString(r7Sn, functionSn, 3, fileName, R7_STRING_SIZE);

	if (fileName[0] == '\0') {
		//R7_Printf(r7Sn, "\nERROR! fileName\n");
		result = -1;
		R7_SetVariableInt(r7Sn, functionSn, 1, result);
		return -1;
	}

	char workSpacePath[R7_STRING_SIZE];
	R7_GetWorkspacePath(r7Sn, workSpacePath, R7_STRING_SIZE);

	wchar_t workSpacePathW[R7_STRING_SIZE];
	memset(workSpacePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, workSpacePath, -1, workSpacePathW, R7_STRING_SIZE * 2);

	wchar_t fileNameW[R7_STRING_SIZE];
	memset(fileNameW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	MultiByteToWideChar(CP_UTF8, 0, fileName, -1, fileNameW, R7_STRING_SIZE * 2);

	wchar_t filePathW[R7_STRING_SIZE];
	memset(filePathW, 0, sizeof(wchar_t) * R7_STRING_SIZE);
	//wsprintf(filePathW, L"%s%s\0", workSpacePathW, fileNameW);

	char drive[5];
	char dir[100];
	char filenames[100];
	char fileext[10];
	_splitpath(fileName, drive, dir, filenames, fileext);
	int isBinFile = strcmp(fileext, ".bin");
	if (isBinFile != 0) {
		wsprintf(filePathW, L"%s%s.bin\0", workSpacePathW, fileNameW);
		wsprintf(fileNameW, L"%s.bin\0", fileNameW);
	}
	else {
		wsprintf(filePathW, L"%s%s\0", workSpacePathW, fileNameW);
	}

	//check path exist 
	//CREATE_NEW : create file ,if exist will return -1
	//CREATE_ALWAYS : create file , if exist will cover
	//OPEN_EXISTING : create file, if not exist will return -1
	HANDLE hDstFile;
	hDstFile = CreateFile(filePathW, GENERIC_WRITE | GENERIC_READ, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hDstFile == INVALID_HANDLE_VALUE) {
		hDstFile = CreateFile(fileNameW, GENERIC_WRITE | GENERIC_READ, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hDstFile == INVALID_HANDLE_VALUE) {
			CloseHandle(hDstFile);
			//R7_Printf(r7Sn, "\nERROR! fileName is not exist\n");
			result = -2;
			R7_SetVariableInt(r7Sn, functionSn, 1, result);
			return -2;
		}
	}

	unsigned char *string1;
	result = (int)strlen((char *)binary);
	
	
	DWORD dwBytesToWrite, dwBytesWrite;
	
	dwBytesToWrite = result;
	dwBytesWrite = 0;

	string1 = binary;

	do {
		WriteFile(hDstFile, &binary, result, &dwBytesWrite, NULL);
		dwBytesToWrite -= dwBytesWrite;
		string1 += dwBytesWrite;

	} while (dwBytesToWrite > 0);

	CloseHandle(hDstFile);
	// TODO: Detect string encoding format. Then convert to UTF-8.

	R7_SetVariableInt(r7Sn, functionSn, 1, result);

	//free(string1);


	return 1;
}
R7_API int R7Library_Init(void) {
	SetConsoleOutputCP(65001);
	setlocale(LC_ALL, "en_US.UTF-8");

	// Register your functions in this API.
	
	R7_RegisterFunction("File_ReadImage", (R7Function_t)&File_ReadImage);
	R7_RegisterFunction("File_SaveImage", (R7Function_t)&File_SaveImage);
	R7_RegisterFunction("File_ReadBinary", (R7Function_t)&File_ReadBinary);
	R7_RegisterFunction("File_SaveBinary", (R7Function_t)&File_SaveBinary);
	R7_RegisterFunction("File_ReadString", (R7Function_t)&File_ReadString);
	R7_RegisterFunction("File_SaveString", (R7Function_t)&File_SaveString);
	R7_RegisterFunction("File_DeleteDir", (R7Function_t)&File_DeleteDir);
	R7_RegisterFunction("File_DeleteFile", (R7Function_t)&File_DeleteFile);
		
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
	json_object_set_new(functionGroupObject, "name", json_string("File"));
	json_array_append(functionGroupArray, functionGroup);

	functionArray = json_array();
	json_object_set_new(functionGroupObject, "functions", functionArray);

	//File_ReadImage
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_ReadImage"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);	

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Image"));
	json_object_set_new(variableObject, "type", json_string("image"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("fileName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FILE_PATH"));
	json_array_append(variableArray, variable);

	//File_SaveImage
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_SaveImage"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Image"));
	json_object_set_new(variableObject, "type", json_string("image"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("fileName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FILE_PATH"));
	json_array_append(variableArray, variable);
	
	//File_DeleteDir
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_DeleteDir"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("dirName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FOLDER_PATH"));
	json_array_append(variableArray, variable);


	//File_DeleteFile
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_DeleteFile"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);

	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("fileName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FILE_PATH"));
	json_array_append(variableArray, variable);


	//File_ReadBinary
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_ReadBinary"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();

	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("binary"));
	json_object_set_new(variableObject, "type", json_string("binary"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("fileName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FILE_PATH"));
	json_array_append(variableArray, variable);


	//File_ReadString
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_ReadString"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();

	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("string"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("fileName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FILE_PATH"));
	json_array_append(variableArray, variable);

	//File_SaveString
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_SaveString"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();

	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("string"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("fileName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FILE_PATH"));
	json_array_append(variableArray, variable);

	//File_SaveBinary
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("File_SaveBinary"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();

	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("result"));
	json_object_set_new(variableObject, "type", json_string("int"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("binary"));
	json_object_set_new(variableObject, "type", json_string("binary"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("fileName"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN, FILE_PATH"));
	json_array_append(variableArray, variable);


	sprintf_s(str, strSize, "%s", json_dumps(root, 0));

	json_decref(root);

	return 1;
}

#ifdef __cplusplus
}
#endif
