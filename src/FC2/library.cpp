/*
Copyright (c) 2004-2017 Open Robot Club. All rights reserved.
FC2 library for R7.
*/


#include <string>
#include <sstream>

#include <opencv2/opencv.hpp>

#include "FlyCapture2_C.h"
#include "FlyCapture2Defs_C.h"

#include "R7.hpp"


using namespace cv;
using namespace std;


#define FC2_STRING_SIZE 32


typedef struct {
	unsigned int cameraSN;

	fc2Context context;
	fc2PGRGuid guid;
	
	Mat grabbedImage;
	bool isColorCamera;
} FC2_t;


#ifdef __cplusplus
extern "C"
{
#endif

int fc2_GetErrorStr(fc2Error error, string &str) {
	switch (error) {
	case FC2_ERROR_UNDEFINED: {
		str = "Undefined";
		break;
	}
	case FC2_ERROR_OK: {
		str = "Function returned with no errors";
		break;
	}
	case FC2_ERROR_FAILED: {
		str = "General failure";
		break;
	}
	case FC2_ERROR_NOT_IMPLEMENTED: {
		str = "Function has not been implemented";
		break;
	}
	case FC2_ERROR_FAILED_BUS_MASTER_CONNECTION: {
		str = "Could not connect to Bus Master";
		break;
	}
	case FC2_ERROR_NOT_CONNECTED: {
		str = "Camera has not been connected";
		break;
	}
	case FC2_ERROR_INIT_FAILED: {
		str = "Initialization failed";
		break;
	}
	case FC2_ERROR_NOT_INTITIALIZED: {
		str = "Camera has not been initialized";
		break;
	}
	case FC2_ERROR_INVALID_PARAMETER: {
		str = "Invalid parameter passed to function";
		break;
	}
	case FC2_ERROR_INVALID_SETTINGS: {
		str = "Setting set to camera is invalid";
		break;
	}
	case FC2_ERROR_INVALID_BUS_MANAGER: {
		str = "Invalid Bus Manager object";
		break;
	}
	case FC2_ERROR_MEMORY_ALLOCATION_FAILED: {
		str = "Could not allocate memory";
		break;
	}
	case FC2_ERROR_LOW_LEVEL_FAILURE: {
		str = "Low level error";
		break;
	}
	case FC2_ERROR_NOT_FOUND: {
		str = "Device not found";
		break;
	}
	case FC2_ERROR_FAILED_GUID: {
		str = "GUID failure";
		break;
	}
	case FC2_ERROR_INVALID_PACKET_SIZE: {
		str = "Packet size set to camera is invalid";
		break;
	}
	case FC2_ERROR_INVALID_MODE: {
		str = "Invalid mode has been passed to function";
		break;
	}
	case FC2_ERROR_NOT_IN_FORMAT7: {
		str = "Error due to not being in Format7";
		break;
	}
	case FC2_ERROR_NOT_SUPPORTED: {
		str = "This feature is unsupported";
		break;
	}
	case FC2_ERROR_TIMEOUT: {
		str = "Timeout error";
		break;
	}
	case FC2_ERROR_BUS_MASTER_FAILED: {
		str = "Bus Master Failure";
		break;
	}
	case FC2_ERROR_INVALID_GENERATION: {
		str = "Generation Count Mismatch";
		break;
	}
	case FC2_ERROR_LUT_FAILED: {
		str = "Look Up Table failure";
		break;
	}
	case FC2_ERROR_IIDC_FAILED: {
		str = "IIDC failure";
		break;
	}
	case FC2_ERROR_STROBE_FAILED: {
		str = "Strobe failure";
		break;
	}
	case FC2_ERROR_TRIGGER_FAILED: {
		str = "Trigger failure";
		break;
	}
	case FC2_ERROR_PROPERTY_FAILED: {
		str = "Property failure";
		break;
	}
	case FC2_ERROR_PROPERTY_NOT_PRESENT: {
		str = "Property is not present";
		break;
	}
	case FC2_ERROR_REGISTER_FAILED: {
		str = "Register access failed";
		break;
	}
	case FC2_ERROR_READ_REGISTER_FAILED: {
		str = "Register read failed";
		break;
	}
	case FC2_ERROR_WRITE_REGISTER_FAILED: {
		str = "Register write failed";
		break;
	}
	case FC2_ERROR_ISOCH_FAILED: {
		str = "Isochronous failure";
		break;
	}
	case FC2_ERROR_ISOCH_ALREADY_STARTED: {
		str = "Isochronous transfer has already been started";
		break;
	}
	case FC2_ERROR_ISOCH_NOT_STARTED: {
		str = "Isochronous transfer has not been started";
		break;
	}
	case FC2_ERROR_ISOCH_START_FAILED: {
		str = "Isochronous start failed";
		break;
	}
	case FC2_ERROR_ISOCH_RETRIEVE_BUFFER_FAILED: {
		str = "Isochronous retrieve buffer failed";
		break;
	}
	case FC2_ERROR_ISOCH_STOP_FAILED: {
		str = "Isochronous stop failed";
		break;
	}
	case FC2_ERROR_ISOCH_SYNC_FAILED: {
		str = "Isochronous image synchronization failed";
		break;
	}
	case FC2_ERROR_ISOCH_BANDWIDTH_EXCEEDED: {
		str = "Isochronous bandwidth exceeded";
		break;
	}
	case FC2_ERROR_IMAGE_CONVERSION_FAILED: {
		str = "Image conversion failed";
		break;
	}
	case FC2_ERROR_IMAGE_LIBRARY_FAILURE: {
		str = "Image library failure";
		break;
	}
	case FC2_ERROR_BUFFER_TOO_SMALL: {
		str = "Buffer is too small";
		break;
	}
	case FC2_ERROR_IMAGE_CONSISTENCY_ERROR: {
		str = "There is an image consistency error";
		break;
	}
	case FC2_ERROR_INCOMPATIBLE_DRIVER: {
		str = "The installed driver is not compatible with the library";
		break;
	}
	case FC2_ERROR_FORCE_32BITS: {
		str = "Force 32 bits";
		break;
	}
	default: {
		str = "";
		break;
	}
	}

	return 1;
}

int fc2_StrToInt(string s, unsigned int &value) {
	if (s.empty()) {
		return -1;
	}

	int v;
	stringstream ss(s);
	ss >> v;

	if (v < 0) {
		return -2;
	}

	value = v;
	
	return 1;
}

static int FC2_Grab(int r7Sn, int functionSn) {
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

	FC2_t *fc2Ptr = ((FC2_t*)variableObject);

	if (fc2Ptr->context == NULL) {
		R7_Printf(r7Sn, "ERROR! Camera is not opened");
		return -3;
	}

	fc2Image rawImage;
	fc2Image convertedImage;
	fc2Error error;

	error = fc2CreateImage(&rawImage);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Create image = %s", errorStr.c_str());
		return -4;
	}

	error = fc2CreateImage(&convertedImage);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Create image = %s", errorStr.c_str());
		fc2DestroyImage(&rawImage);
		return -5;
	}

	error = fc2RetrieveBuffer(fc2Ptr->context, &rawImage);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Retrieve buffer = %s", errorStr.c_str());
		fc2DestroyImage(&rawImage);
		fc2DestroyImage(&convertedImage);
		return -6;
	}

	error = fc2ConvertImageTo(FC2_PIXEL_FORMAT_BGR, &rawImage, &convertedImage);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Convert image = %s", errorStr.c_str());
		fc2DestroyImage(&rawImage);
		fc2DestroyImage(&convertedImage);
		return -7;
	}

	int imgHeight = convertedImage.rows;
	int imgWidth = convertedImage.cols;

	int imgType;
	if (fc2Ptr->isColorCamera) {
		imgType = CV_8UC3;
	}
	else {
		imgType = CV_8UC1;
	}

	fc2Ptr->grabbedImage = Mat(convertedImage.rows, convertedImage.cols, imgType);

	memcpy(fc2Ptr->grabbedImage.data, convertedImage.pData, convertedImage.dataSize);

	error = fc2DestroyImage(&rawImage);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Destroy image = %s", errorStr.c_str());
		return -8;
	}

	error = fc2DestroyImage(&convertedImage);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Destroy image = %s", errorStr.c_str());
		return -9;
	}

	return 1;
}

static int FC2_Init(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;
	res = R7_InitVariableObject(r7Sn, functionSn, 1, sizeof(FC2_t));
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_InitVariableObject = %d", res);
		return -1;
	}
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	if (variableObject == NULL) {
		R7_Printf(r7Sn, "ERROR! variableObject == NULL");
		return -3;
	}

	FC2_t *fc2Ptr = ((FC2_t*)variableObject);

	//initial values
	fc2Ptr->cameraSN = 0;
	fc2Ptr->context = NULL;
	fc2Ptr->grabbedImage = Mat();
	fc2Ptr->isColorCamera = true;
		
	return 1;
}

static int FC2_Open(int r7Sn, int functionSn) {
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

	FC2_t *fc2Ptr = ((FC2_t*)variableObject);

	fc2Error error;
	error = fc2CreateContext(&(fc2Ptr->context));
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Create context = %s", errorStr.c_str());
		return -3;
	}

	unsigned int cameraNum = 0;
	error = fc2GetNumOfCameras(fc2Ptr->context, &cameraNum);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Create context = %s", errorStr.c_str());
		return -4;
	}
	if (cameraNum == 0) {
		R7_Printf(r7Sn, "ERROR! No camera detected");
		return -5;
	}

	char cameraSN[FC2_STRING_SIZE];
	res = R7_GetVariableString(r7Sn, functionSn, 2, cameraSN, FC2_STRING_SIZE);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableString = %d", res);
		return -6;
	}

	res = fc2_StrToInt(cameraSN, fc2Ptr->cameraSN);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! Serial number is invalid");
		return -7;
	}

	error = fc2GetCameraFromSerialNumber(fc2Ptr->context, fc2Ptr->cameraSN, &(fc2Ptr->guid));
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Get number of cameras = %s", errorStr.c_str());
		return -8;
	}

	error = fc2Connect(fc2Ptr->context, &(fc2Ptr->guid));
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Connect = %s", errorStr.c_str());
		return -9;
	}

	error = fc2StartCapture(fc2Ptr->context);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Start capture = %s", errorStr.c_str());
		return -9;
	}

	fc2CameraInfo cameraInfo;
	error = fc2GetCameraInfo(fc2Ptr->context, &cameraInfo);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Get camera info = %s", errorStr.c_str());
		return -10;
	}

	if (cameraInfo.isColorCamera == 0) {
		fc2Ptr->isColorCamera = false;
	}

	return 1;
}

static int FC2_Release(int r7Sn, int functionSn) {
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

	FC2_t *fc2Ptr = ((FC2_t*)variableObject);

	fc2Error error;
	error = fc2StopCapture(fc2Ptr->context);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Stop capture = %s", errorStr.c_str());
		return -3;
	}

	error = fc2DestroyContext(fc2Ptr->context);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Destroy context = %s", errorStr.c_str());
		return -4;
	}

	fc2Ptr->grabbedImage.release();

	res = R7_ReleaseVariableObject(r7Sn, functionSn, 1);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_ReleaseVariableObject = %d", res);
		return -5;
	}

	return 1;
}

static int FC2_Retrieve(int r7Sn, int functionSn) {
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

	FC2_t *fc2Ptr = ((FC2_t*)variableObject);

	if (fc2Ptr->context == NULL) {
		R7_Printf(r7Sn, "ERROR! Camera is not opened");
		return -3;
	}

	if (fc2Ptr->grabbedImage.empty()) {
		R7_Printf(r7Sn, "ERROR! Did not grab image");
		return -4;
	}
	
	res = R7_SetVariableMat(r7Sn, functionSn, 2, fc2Ptr->grabbedImage);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_SetVariableMat = %d", res);
		return -5;
	}

	return 1;
}

static int FC2_SetGain(int r7Sn, int functionSn) {
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

	FC2_t *fc2Ptr = ((FC2_t*)variableObject);

	float gain = 0;
	res = R7_GetVariableFloat(r7Sn, functionSn, 2, &gain);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableFloat = %d", res);
		return -3;
	}
	if (gain < 0) {
		R7_Printf(r7Sn, "ERROR! Shutter value should be zero or positive");
		return -4;
	}
	
	fc2Property cameraProperty;
	cameraProperty.type = FC2_GAIN;
	fc2GetProperty(fc2Ptr->context, &cameraProperty);//leo add: 沒 get 就 set 會把其他值蓋掉，增加一行
	cameraProperty.onOff = true;
	cameraProperty.autoManualMode = false;
	cameraProperty.absControl = true;
	cameraProperty.absValue = gain;

	fc2Error error;
	error = fc2SetProperty(fc2Ptr->context, &cameraProperty);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Set property = %s", errorStr.c_str());
		return -5;
	}
	
	return 1;
}

static int FC2_SetShutter(int r7Sn, int functionSn) {
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

	FC2_t *fc2Ptr = ((FC2_t*)variableObject);

	float shutter = 0;
	res = R7_GetVariableFloat(r7Sn, functionSn, 2, &shutter);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableFloat = %d", res);
		return -3;
	}
	if (shutter <= 0) {
		R7_Printf(r7Sn, "ERROR! Shutter value should be positive");
		return -4;
	}
	fc2Property cameraProperty;
	cameraProperty.type = FC2_SHUTTER;
	fc2GetProperty(fc2Ptr->context, &cameraProperty);//leo add: 沒 get 就 set 會把其他值蓋掉，增加一行
	cameraProperty.onOff = true;
	cameraProperty.autoManualMode = false;
	cameraProperty.absControl = true;
	cameraProperty.absValue = shutter;

	fc2Error error;
	error = fc2SetProperty(fc2Ptr->context, &cameraProperty);
	if (error != FC2_ERROR_OK) {
		string errorStr;
		fc2_GetErrorStr(error, errorStr);
		R7_Printf(r7Sn, "ERROR! Set property = %s", errorStr.c_str());
		return -5;
	}
	return 1;
}

R7_API int R7Library_Init(void) {
	// Register your functions in this API.

	R7_RegisterFunction("FC2_Grab", (R7Function_t)&FC2_Grab);
	R7_RegisterFunction("FC2_Init", (R7Function_t)&FC2_Init);
	R7_RegisterFunction("FC2_Open", (R7Function_t)&FC2_Open);
	R7_RegisterFunction("FC2_Release", (R7Function_t)&FC2_Release);
	R7_RegisterFunction("FC2_Retrieve", (R7Function_t)&FC2_Retrieve);
	R7_RegisterFunction("FC2_SetGain", (R7Function_t)&FC2_SetGain);
	R7_RegisterFunction("FC2_SetShutter", (R7Function_t)&FC2_SetShutter);
		
	return 1;
}

R7_API int R7Library_Close(void) {
	// If you have something to do before close R7(ex: free mesmory), you should handle them in this API.
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

	functionGroupArray = json_array();
	json_object_set_new(root, "functionGroups", functionGroupArray);

	functionGroup = json_object();
	functionGroupObject = json_object();
	json_object_set_new(functionGroup, "functionGroup", functionGroupObject);
	json_object_set_new(functionGroupObject, "name", json_string("FC2"));
	json_array_append(functionGroupArray, functionGroup);

	functionArray = json_array();
	json_object_set_new(functionGroupObject, "functions", functionArray);
		
	// FC2_Grab
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("FC2_Grab"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "FC2Object", "object", "IN");

	// FC2_Init
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("FC2_Init"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "FC2Object", "object", "INOUT");

	// FC2_Open
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("FC2_Open"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "FC2Object", "object", "IN");
	r7_AppendVariable(variableArray, "CameraSN", "string", "IN");

	// FC2_Release
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("FC2_Release"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "FC2Object", "object", "INOUT");

	// FC2_Retrieve
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("FC2_Retrieve"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "FC2Object", "object", "IN");
	r7_AppendVariable(variableArray, "GrabbedImage", "image", "OUT");

	// FC2_SetGain
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("FC2_SetGain"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "FC2Object", "object", "IN");
	r7_AppendVariable(variableArray, "Gain", "float", "IN");

	// FC2_SetShutter
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("FC2_SetShutter"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "FC2Object", "object", "IN");
	r7_AppendVariable(variableArray, "Shutter", "float", "IN");


	sprintf_s(str, strSize, "%s", json_dumps(root, 0));

	json_decref(root);

	return 1;
}

#ifdef __cplusplus
}
#endif
