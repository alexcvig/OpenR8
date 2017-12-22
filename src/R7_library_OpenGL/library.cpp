/*
Copyright (c) 2004-2017 Open Robot Club. All rights reserved.
OpenGL library for R7.
*/

#include <string>

//#include <wx/wx.h>
//#include <wx/sizer.h>
//#include <wx/glcanvas.h>
//#include <wx/msw/glcanvas.h>
#include "wx/glcanvas.h"
#include "wx/wxprec.h"
#include "wx/wx.h"
//#include <GL/glut.h>
#include <GL/gl.h>

#include "R7.hpp"
#include "library.hpp"

#include "wx/toolbar.h"


using namespace std;
using namespace cv;


class TestFrame : public wxFrame
{
public:
	// Constructor
	TestFrame(const wxString& title, const wxPoint& pos, const wxSize& size);
	~TestFrame();

private:
};


TestFrame::TestFrame(const wxString& title, const wxPoint& pos, const wxSize& size)
//: TemplateFrame(title, pos, size)
	:wxFrame(NULL, wxID_ANY, title, pos, size)
//TemplateFrame(wxWindow *parent, wxWindowID id, const wxString& title, const wxPoint& pos = wxDefaultPosition, const wxSize& size = wxDefaultSize);
//: TemplateFrame()
{
	printf("testFrame init: %s\n", (const char *)(title.ToUTF8()));
	//this->SetTitle(title);
	//this->SetPosition(pos);
	//this->SetSize(size);
	/*
	wxMenu *menuFile = new wxMenu;
	
	menuFile->Append(1000, "&Hello...\tCtrl-H",
		"Help string shown in status bar for this menu item");
	menuFile->AppendSeparator();
	menuFile->Append(wxID_EXIT);
	wxMenu *menuHelp = new wxMenu;
	menuHelp->Append(wxID_ABOUT);
	wxMenuBar *menuBar = new wxMenuBar;
	menuBar->Append(menuFile, "&File");
	menuBar->Append(menuHelp, "&Help");
	SetMenuBar(menuBar);
	CreateStatusBar();	
	SetStatusText("Welcome to wxWidgets!");
	*/
}


TestFrame::~TestFrame() {


}

#ifdef __cplusplus
extern "C"
{
#endif

typedef struct {
	//Mat image;
	//Mat screenShot;
	TestFrame *testFrame;
	int status;
} OpenGL_t;





static int OpenGL_NewWindow(int r7Sn, int functionSn) {
	
	//R7_GetVariableBool(r7Sn, functionSn, 1, &isEnable); //20171101 改為只能開不能關。
	
	//如果 WxWidgetsEnable 為 true 且 WxWidgets還沒 init ，則在這邊 init
	/*
	if (!isWxWidgetsInit) {
		int wxArgc = 0;
		char **wxArgv = NULL;
		wxEntryStart(wxArgc, wxArgv);
		wxTheApp->CallOnInit();
		isWxWidgetsInit = true;
	}*/
	
	int res;
	void *variableObject = NULL;
	res = R7_InitVariableObject(r7Sn, functionSn, 1, sizeof(OpenGL_t));
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_InitVariableObject = %d", res);
		return -1;
	}
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	printf("OpenGL_NewWindow new TestFrame() ++\n");

	// Initial values of OpenGL_t.
	openglPtr->testFrame = new TestFrame(wxT("Test 7"), wxPoint(0, 0), wxSize(400, 400));
	
	printf("OpenGL_NewWindow TestFrame Show ++\n");
	openglPtr->testFrame->Show();
	printf("OpenGL_NewWindow TestFrame Show --\n");
	
	printf("OpenGL_NewWindow new TestFrame() --\n");

	printf("OpenGL_NewWindow --\n");

	
	return 1;
}

static int OpenGL_ShowWindow(int r7Sn, int functionSn) {
	
	return 1;
}

static int OpenGL_HideWindow(int r7Sn, int functionSn) {
	
	return 1;
}

static int OpenGL_ShowImage(int r7Sn, int functionSn) {
	
	return 1;
}



static int OpenGL_GetImage(int r7Sn, int functionSn) {


	return 1;
}


/*
static int OpenGL_Run(int r7Sn, int functionSn) {
	if (isWxWidgetsInit)
	{
		//printf("wxTheApp->OnRun();\n");
		wxTheApp->OnRun();
		//printf("wxTheApp->OnExit();\n");
		wxTheApp->OnExit();
		//printf("wxTheApp->wxEntryCleanup();\n");
		wxEntryCleanup();
		//printf("wxTheApp->wxEntryCleanup(); OK\n");
	}

	return 1;
}
*/
R7_API int R7Library_Init(void) {
	// Register your functions in this API.
	
	R7_RegisterFunction("OpenGL_NewWindow", (R7Function_t)&OpenGL_NewWindow);
	R7_RegisterFunction("OpenGL_ShowWindow", (R7Function_t)&OpenGL_ShowWindow);
	R7_RegisterFunction("OpenGL_HideWindow", (R7Function_t)&OpenGL_HideWindow);
	R7_RegisterFunction("OpenGL_ShowImage", (R7Function_t)&OpenGL_ShowImage);
	R7_RegisterFunction("OpenGL_GetImage", (R7Function_t)&OpenGL_GetImage);

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
	json_object_set_new(functionGroupObject, "name", json_string("OpenGL"));
	json_array_append(functionGroupArray, functionGroup);

	functionArray = json_array();
	json_object_set_new(functionGroupObject, "functions", functionArray);
	
	// OpenGL_NewWindow
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenGL_NewWindow"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("openGLWindow"));
	json_object_set_new(variableObject, "type", json_string("object"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);

	// OpenGL_ShowWindow
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenGL_ShowWindow"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("openGLWindow"));
	json_object_set_new(variableObject, "type", json_string("object"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);

	// OpenGL_HideWindow
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenGL_HideWindow"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("openGLWindow"));
	json_object_set_new(variableObject, "type", json_string("object"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);
	
	// OpenGL_ShowImage
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenGL_ShowImage"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("openGLWindow"));
	json_object_set_new(variableObject, "type", json_string("object"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);
	variable = json_object();
	variableObject = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Image"));
	json_object_set_new(variableObject, "type", json_string("image"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);

	
	// OpenGL_GetImage
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("OpenGL_GetImage"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("openGLWindow"));
	json_object_set_new(variableObject, "type", json_string("object"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);
	variable = json_object();
	variableObject = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Image"));
	json_object_set_new(variableObject, "type", json_string("image"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);


	sprintf_s(str, strSize, "%s", json_dumps(root, 0));

	json_decref(root);

	return 1;
}



#ifdef __cplusplus
}
#endif
