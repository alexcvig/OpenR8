/*
Copyright (c) 2004-2017 Open Robot Club. All rights reserved.
OpenCV library for R7.
*/

#include <opencv2/opencv.hpp>

#include "R7.hpp"


using namespace std;
using namespace cv;

typedef struct {
	VideoCapture *videoCapture;
	int deviceNum;
	int apiID;
	Mat capturedImage;
} OpenCV_t;

vector<VideoCapture *> videoCapture_Vector;

#ifdef __cplusplus
extern "C"
{
#endif

static int OpenCV_VideoCapture_Init(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;
	res = R7_InitVariableObject(r7Sn, functionSn, 1, sizeof(OpenCV_t));
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_InitVariableObject = %d", res);
		return -1;
	}
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenCV_t *videoCapturePtr = ((OpenCV_t*)variableObject);

	// Initial values of OpenCV_t.
	videoCapturePtr->videoCapture = new VideoCapture();
	videoCapturePtr->deviceNum = 0;
	videoCapturePtr->apiID = CAP_ANY;
	videoCapturePtr->capturedImage = Mat();


	videoCapture_Vector.push_back(videoCapturePtr->videoCapture);
	return 1;
}

static int OpenCV_VideoCapture_Release(int r7Sn, int functionSn) {
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

	OpenCV_t *videoCapturePtr = ((OpenCV_t*)variableObject);

	videoCapturePtr->capturedImage.release();
	videoCapturePtr->videoCapture->~VideoCapture();
	videoCapturePtr->videoCapture = NULL;

	res = R7_ReleaseVariableObject(r7Sn, functionSn, 1);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_ReleaseVariableObject = %d", res);
		return -3;
	}
	
	return 1;
}

static int OpenCV_VideoCapture_Open(int r7Sn, int functionSn) {
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

	OpenCV_t *videoCapturePtr = ((OpenCV_t*)variableObject);

	int deviceID = 0;
	res = R7_GetVariableInt(r7Sn, functionSn, 2, &deviceID);
	if (res > 0) {
		videoCapturePtr->deviceNum = deviceID;
	}

	videoCapturePtr->videoCapture->open(videoCapturePtr->deviceNum + videoCapturePtr->apiID);
	if (!videoCapturePtr->videoCapture->isOpened()) {
		R7_Printf(r7Sn, "ERROR! Unable to open the Camera!");
		return -4;
	}

	// Workaround for dark image at beginning.
	Mat frame;
	for (int i = 0; i < 10; i++) {
		videoCapturePtr->videoCapture->grab();
		videoCapturePtr->videoCapture->retrieve(frame);
	}
	frame.release();

	return 1;
}

static int OpenCV_VideoCapture_Grab(int r7Sn, int functionSn) {
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

	OpenCV_t *videoCapturePtr = ((OpenCV_t*)variableObject);

	videoCapturePtr->videoCapture->grab();
	
	return 1;
}

static int OpenCV_VideoCapture_Retrieve(int r7Sn, int functionSn) {
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

	OpenCV_t *videoCapturePtr = ((OpenCV_t*)variableObject);

	videoCapturePtr->videoCapture->retrieve(videoCapturePtr->capturedImage);

	if (videoCapturePtr->capturedImage.empty()) {
		R7_Printf(r7Sn, "ERROR! Retrieved image is empty!");
		return -3;
	}

	res = R7_SetVariableMat(r7Sn, functionSn, 2, videoCapturePtr->capturedImage);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_SetVariableMat = %d", res);
		return -4;
	}

	return 1;
}

R7_API int R7Library_Init(void) {
	// Register your functions in this API.

	R7_RegisterFunction("OpenCV_VideoCapture_Grab", (R7Function_t)&OpenCV_VideoCapture_Grab);
	R7_RegisterFunction("OpenCV_VideoCapture_Init", (R7Function_t)&OpenCV_VideoCapture_Init);
	R7_RegisterFunction("OpenCV_VideoCapture_Open", (R7Function_t)&OpenCV_VideoCapture_Open);
	R7_RegisterFunction("OpenCV_VideoCapture_Release", (R7Function_t)&OpenCV_VideoCapture_Release);
	R7_RegisterFunction("OpenCV_VideoCapture_Retrieve", (R7Function_t)&OpenCV_VideoCapture_Retrieve);
		
	return 1;
}

R7_API int R7Library_Close(void) {
	// If you have something to do before close R7(ex: free memory), you should handle them in this API.
	for (int i = 0; i < (int)videoCapture_Vector.size(); i++) {
		if (videoCapture_Vector[i] != NULL) {
			videoCapture_Vector[i]->~VideoCapture();
			videoCapture_Vector[i] = NULL;
		}
	}
	videoCapture_Vector.clear();
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
	json_object_set_new(functionGroupObject, "name", json_string("OpenCV"));
	json_array_append(functionGroupArray, functionGroup);

	functionArray = json_array();
	json_object_set_new(functionGroupObject, "functions", functionArray);
		
	// OpenCV_VideoCapture_Grab
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenCV_VideoCapture_Grab"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "VideoCaptureObject", "object", "IN");

	// OpenCV_VideoCapture_Init
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenCV_VideoCapture_Init"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "VideoCaptureObject", "object", "INOUT");

	// OpenCV_VideoCapture_Open
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenCV_VideoCapture_Open"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "VideoCaptureObject", "object", "IN");
	r7_AppendVariable(variableArray, "DeviceNumber", "int", "IN");

	// OpenCV_VideoCapture_Release
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenCV_VideoCapture_Release"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "VideoCaptureObject", "object", "INOUT");

	// OpenCV_VideoCapture_Retrieve
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenCV_VideoCapture_Retrieve"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	r7_AppendVariable(variableArray, "VideoCaptureObject", "object", "IN");
	r7_AppendVariable(variableArray, "GrabbedImage", "image", "OUT");


	sprintf_s(str, strSize, "%s", json_dumps(root, 0));

	json_decref(root);

	return 1;
}

#ifdef __cplusplus
}
#endif
