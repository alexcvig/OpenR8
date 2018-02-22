/*
Copyright (c) 2004-2018 Open Robot Club. All rights reserved.
OpenGL library for R7.
*/


#include "R7.hpp"
#include "library.hpp"


#define OPENGL_SCROLL_BAR_WIDTH 17


using namespace std;
using namespace cv;


class OpenGLFrame : public wxFrame {
public:
	OpenGLFrame(OpenGL_t *openGL, wxString title);
	~OpenGLFrame();
	
	OpenGLCanvas *openGLCanvas;
	wxScrollBar *sbh;
	wxScrollBar *sbv;
	Mat screenshot;
	
	//--- Scrollbar variable
	const int start_pos = 0;
	int scrollX = 0;
	int scrollY = 0;
	int viewableWidth = 500;
	int viewableHeight = 500;
	int mapWidth = 1500;
	int mapHeight = 1500;
	
	OpenGL_t *openGL;

	
	void hScroll(wxScrollEvent &evt) {
		scrollX = sbh->GetThumbPosition();
		openGLCanvas->setX(scrollX);
		openGLCanvas->Refresh();
	}

	void vScroll(wxScrollEvent &evt) {
		scrollY = sbv->GetThumbPosition();
		openGLCanvas->setY(scrollY);
		openGLCanvas->Refresh();
	}
	
	void SetScrollbarFit(void);
	void OnScreenShot(void);

private:
	// Event handler functions
	void OnClose(wxCommandEvent& event);
	void OnFPS(wxCommandEvent& event);
	void OnCloseWindow(wxCloseEvent& event);
	
	wxDECLARE_EVENT_TABLE();
};


enum {
	wxID_FPS = wxID_HIGHEST + 1, 
	wxID_SCROLL_H,
	wxID_SCROLL_V
};


// control ids
enum {
	SpinTimer = wxID_HIGHEST + 1
};


// ----------------------------------------------------------------------------
// helper functions
// ----------------------------------------------------------------------------

static void CheckGLError() {
	GLenum errLast = GL_NO_ERROR;

	for (;; )
	{
		GLenum err = glGetError();
		if (err == GL_NO_ERROR)
			return;

		// normally the error is reset by the call to glGetError() but if
		// glGetError() itself returns an error, we risk looping forever here
		// so check that we get a different error than the last time
		if (err == errLast)
		{
			wxLogError(wxT("OpenGL error state couldn't be reset."));
			return;
		}

		errLast = err;

		wxLogError(wxT("OpenGL error %d"), err);
	}
}

// ----------------------------------------------------------------------------
// OpenGLContext
// ----------------------------------------------------------------------------

OpenGLContext::OpenGLContext(wxGLCanvas *canvas) : wxGLContext(canvas) {
	SetCurrent(*canvas);

	glEnable(GL_TEXTURE_2D);
	glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
	glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
	glPixelStorei(GL_UNPACK_SKIP_PIXELS, 0);
	glPixelStorei(GL_UNPACK_SKIP_ROWS, 0);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

	CheckGLError();
}

void OpenGLContext::DrawImage(Mat &mat, int windowW, int windowH, int x, int y, double scale) {
	if (mat.cols == 0) {
		return;
	}
	
	glClear(GL_COLOR_BUFFER_BIT);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();

	if (mat.channels() == 3) {		
		glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, mat.cols, mat.rows, 0, 0x80E0, GL_UNSIGNED_BYTE, mat.ptr());	
	} else {
		glTexImage2D(GL_TEXTURE_2D, 0, GL_LUMINANCE, mat.cols, mat.rows, 0, GL_LUMINANCE, GL_UNSIGNED_BYTE, mat.ptr());
	}

	glBegin(GL_QUADS);
	
	glTexCoord2f(0.0f, 0.0f); glVertex2f(-1, 1);
	glTexCoord2f(1.0f, 0.0f); glVertex2f(1, 1);
	glTexCoord2f(1.0f, 1.0f); glVertex2f(1, -1);
	glTexCoord2f(0.0f, 1.0f); glVertex2f(-1, -1);
	
	glEnd();
	glFlush();

	return;
}

// ----------------------------------------------------------------------------
// OpenGLCanvas
// ----------------------------------------------------------------------------


wxBEGIN_EVENT_TABLE(OpenGLCanvas, wxGLCanvas)
EVT_PAINT(OpenGLCanvas::OnPaint)
EVT_TIMER(SpinTimer, OpenGLCanvas::OnSpinTimer)
wxEND_EVENT_TABLE()


OpenGLCanvas::OpenGLCanvas(wxWindow *parent, int *attribList, OpenGLFrame *frame)
// With perspective OpenGL graphics, the wxFULL_REPAINT_ON_RESIZE style
// flag should always be set, because even making the canvas smaller should
// be followed by a paint event that updates the entire canvas with new
// viewport settings.
	: wxGLCanvas(parent, wxID_ANY, attribList,
		wxDefaultPosition, wxDefaultSize,
		wxFULL_REPAINT_ON_RESIZE),
	xangle(30.0),
	yangle(30.0),
	spinTimer(this, SpinTimer),
	useStereo(false),
	stereoWarningAlreadyDisplayed(false)
{
	Mat black(480, 640, CV_8UC3, cv::Scalar::all(0));
	
	openGLFrame = frame;
	image = black.clone();
	
	if (attribList)
	{
		int i = 0;
		while (attribList[i] != 0)
		{
			if (attribList[i] == WX_GL_STEREO)
				useStereo = true;
			++i;
		}
	}
}


OpenGLContext& OpenGLCanvas::GetContext(wxGLCanvas *canvas)
{
	OpenGLContext *glContext;
	if (!openGLContext)
	{
		openGLContext = new OpenGLContext(canvas);
	}
	glContext = openGLContext;
	glContext->SetCurrent(*canvas);

	return *glContext;
}


void OpenGLCanvas::OnPaint(wxPaintEvent &WXUNUSED(event))
{
	// Test fps
	if (isTestFps)
	{
		if (isFirstCount)
		{
			isFirstCount = false;
			current_ticks = clock();
		}
		else
		{
			frameCount++;
			
			if (frameCount >= 5) {

				delta_ticks = clock() - current_ticks; //the time, in ms, that took to render the scene

				fps = CLOCKS_PER_SEC * frameCount / delta_ticks;

				current_ticks = clock();
				frameCount = 0;
			}
		}

		if (fps > 0) {
			char text_buf[64];
			sprintf(text_buf, "%d fps", fps);
			putText(image, string(text_buf), Point(image.size().width - 120, 30), 0, 1, Scalar(34, 221, 34), 2);
		}
	}
	
	const wxSize ClientSize = GetClientSize();		//--- size of canvas
	
	OpenGLContext& canvas = GetContext(this);

	glViewport(0 - x, ClientSize.y - image.size().height * 1 + y, image.size().width * 1, image.size().height * 1);

	// Render the graphics and swap the buffers.
	GLboolean quadStereoSupported;
	glGetBooleanv(GL_STEREO, &quadStereoSupported);
	if (quadStereoSupported)
	{
		glDrawBuffer(GL_BACK_LEFT);
		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		glFrustum(-0.47f, 0.53f, -0.5f, 0.5f, 1.0f, 3.0f);
		canvas.DrawImage(image, ClientSize.x, ClientSize.y, x, y, 1);
		CheckGLError();
		glDrawBuffer(GL_BACK_RIGHT);
		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		glFrustum(-0.53f, 0.47f, -0.5f, 0.5f, 1.0f, 3.0f);
		canvas.DrawImage(image, ClientSize.x, ClientSize.y, x, y, 1);
		CheckGLError();
	} else {
		canvas.DrawImage(image, ClientSize.x, ClientSize.y, x, y, 1);
		if (useStereo && !stereoWarningAlreadyDisplayed)
		{
			stereoWarningAlreadyDisplayed = true;
			wxLogError("Stereo not supported by the graphics card.");
		}
	}
	
	SwapBuffers();	// swap the buffers of the OpenGL canvas and thus show current output
}

void OpenGLCanvas::Spin(float xSpin, float ySpin)
{
	printf("OpenGLCanvas::Spin \n");
	xangle += xSpin;
	yangle += ySpin;

	Refresh(false);
}

void OpenGLCanvas::OnSpinTimer(wxTimerEvent& WXUNUSED(event))
{
	Spin(0.0, 4.0);
}


// ----------------------------------------------------------------------------
// OpenGLFrame: main application window
// ----------------------------------------------------------------------------

wxBEGIN_EVENT_TABLE(OpenGLFrame, wxFrame)
EVT_MENU(wxID_CLOSE, OpenGLFrame::OnClose)
EVT_MENU(wxID_FPS, OpenGLFrame::OnFPS)
EVT_COMMAND_SCROLL(wxID_SCROLL_H, OpenGLFrame::hScroll)
EVT_COMMAND_SCROLL(wxID_SCROLL_V, OpenGLFrame::vScroll)
EVT_CLOSE(OpenGLFrame::OnCloseWindow)
wxEND_EVENT_TABLE()

OpenGLFrame::OpenGLFrame(OpenGL_t  *openGL, wxString title = wxT("OpenGL Window"))
	: wxFrame(NULL, wxID_ANY, title)
{
	this->openGL = openGL;
	int stereoAttribList[] = { WX_GL_RGBA, WX_GL_DOUBLEBUFFER, WX_GL_STEREO, 0 };
	
	screenshot = Mat();

	//--- support all available image formats
	wxInitAllImageHandlers();
	
	wxBoxSizer *sizer = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *sizer2 = new wxBoxSizer(wxHORIZONTAL);
	
	openGLCanvas = new OpenGLCanvas(this, NULL, this);
	sbh = new wxScrollBar(this, wxID_SCROLL_H, wxDefaultPosition, wxDefaultSize, wxSB_HORIZONTAL);
	sbv = new wxScrollBar(this, wxID_SCROLL_V, wxDefaultPosition, wxDefaultSize, wxSB_VERTICAL);

	//--- Create a menu
	wxMenu *fileMenu = new wxMenu;
	wxMenu *settingMenu = new wxMenu;
	
    fileMenu->Append(wxID_CLOSE, wxT("&Exit"),
                     wxT("Quit this program"));
					 
	settingMenu->Append(wxID_FPS, wxT("&Show Frame Rate"),
                     wxT(""));
					 
	wxMenuBar *menuBar = new wxMenuBar();
    menuBar->Append(fileMenu, wxT("&File"));
	menuBar->Append(settingMenu, wxT("&Setting"));
    SetMenuBar(menuBar);
	
    CreateStatusBar(1);
	
	SetScrollbarFit();
	
	sizer2->Add(openGLCanvas, 1, wxEXPAND);
	sizer2->Add(sbv, 0, wxEXPAND);
	sizer->Add(sizer2, 1, wxEXPAND);
	sizer->Add(sbh, 0, wxEXPAND);

	SetSizer(sizer);

	Center();

	// Important! otherwise, image saving will be wrong
	SetClientSize((openGLCanvas->image.cols) + OPENGL_SCROLL_BAR_WIDTH, (openGLCanvas->image.rows)+OPENGL_SCROLL_BAR_WIDTH);

}

OpenGLFrame::~OpenGLFrame()
{

}

void OpenGLFrame::SetScrollbarFit(void)
{
	viewableWidth = openGLCanvas->image.size().width;
	viewableHeight = openGLCanvas->image.size().height;
		
	mapWidth = openGLCanvas->image.size().width;
	mapHeight = openGLCanvas->image.size().height;

	sbh->SetScrollbar(start_pos, viewableWidth, mapWidth, viewableWidth / 4);
	sbv->SetScrollbar(start_pos, viewableHeight, mapHeight, viewableHeight / 4);
	
	openGLCanvas->setX(0);
	openGLCanvas->setY(0);
}

void OpenGLFrame::OnClose(wxCommandEvent& WXUNUSED(event))
{
	Close(true);
}

void OpenGLFrame::OnCloseWindow(wxCloseEvent& WXUNUSED(event))
{
	openGL->isClosingFrame = 1;

	openGL->openGLFrame = NULL;

	Sleep(3000);

	Destroy();
}

void OpenGLFrame::OnFPS(wxCommandEvent& WXUNUSED(event))
{
	if (this->openGLCanvas->isTestFps == false)
	{
		this->openGLCanvas->isTestFps = true;

		wxMenuBar *mb = this->GetMenuBar();

		mb->SetLabel(wxID_FPS, wxT("Hide Frame Rate"));
	}
	else
	{
		this->openGLCanvas->isTestFps = false;

		wxMenuBar *mb = this->GetMenuBar();

		mb->SetLabel(wxID_FPS, wxT("Show Frame Rate"));
	}
}

void OpenGLFrame::OnScreenShot(void)
{
	//--- snapshot
	const wxSize ClientSize = GetClientSize();

	Mat img((ClientSize.y)-OPENGL_SCROLL_BAR_WIDTH, (ClientSize.x)-OPENGL_SCROLL_BAR_WIDTH, CV_8UC3);
	
	Mat img_flipped;
	
	//--- Read gl front buffer
	glReadBuffer( GL_FRONT );
	
	//use fast 4-byte alignment (default anyway) if possible
	glPixelStorei(GL_PACK_ALIGNMENT, (img.step & 3) ? 1 : 4);
	
	//set length of one complete row in destination data (doesn't need to equal img.cols)
	glPixelStorei(GL_PACK_ROW_LENGTH, img.step/img.elemSize());
	
	glReadPixels(0, 0, img.cols, img.rows, GL_BGR_EXT, GL_UNSIGNED_BYTE, img.data);
	
	cv::flip(img, img_flipped, 0);

	screenshot = img_flipped.clone();		
}


#ifdef __cplusplus
extern "C"
{
#endif


static int OpenGL_NewWindowCallback(void *data) {
	int r7Sn, functionSn;
	int res;
	void *variableObject = NULL;
	OpenGLCallBack_t *cbPtr = ((OpenGLCallBack_t *)data);

	r7Sn = cbPtr->r7Sn;
	functionSn = cbPtr->functionSn;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}

	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);

	openglPtr->image = Mat();

	openglPtr->screenShot = Mat();

	openglPtr->status = 0;

	openglPtr->isClosingFrame = 0;
	
	char command[R7_STRING_SIZE];
	command[0] = '\0';

	res = R7_GetVariableString(r7Sn, functionSn, 2, command, R7_STRING_SIZE);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableString = %d", res);
		return -4;
	}
	
	int strSize = (int)(strlen(command));
	
	command[strSize] = '\0';

	openglPtr->openGLFrame = new OpenGLFrame(openglPtr, wxString::FromUTF8(command));

	openglPtr->openGLFrame->Show();

	return 1;
}

static int OpenGL_NewWindow(int r7Sn, int functionSn) {

	OpenGLCallBack_t *cbPtr = (OpenGLCallBack_t*)malloc(sizeof(OpenGLCallBack_t));

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	int res;
	void *variableObject = NULL;
	res = R7_InitVariableObject(r7Sn, functionSn, 1, sizeof(OpenGL_t));
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_InitVariableObject = %d", res);
		return -1;
	}

	R7_QueueWxEvent((R7CallbackHandler)OpenGL_NewWindowCallback, (void*)cbPtr);

	R7_ProcessWxPendingEvents();

	return 1;
}

static int OpenGL_ShowWindowCallback(void *data) {
	int r7Sn, functionSn;
	int res;
	void *variableObject = NULL;
	OpenGLCallBack_t *cbPtr = ((OpenGLCallBack_t *)data);

	r7Sn = cbPtr->r7Sn;
	functionSn = cbPtr->functionSn;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);

	if (openglPtr->status == 0)
	{
		openglPtr->openGLFrame->Show(true);
		openglPtr->status = 1;
	}
	else
	{
		//printf("Status error! \n");
		return 3;
	}
	
	return 1;
}

static int OpenGL_ShowWindow(int r7Sn, int functionSn) {
	OpenGLCallBack_t *cbPtr = (OpenGLCallBack_t*)malloc(sizeof(OpenGLCallBack_t));

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	R7_QueueWxEvent((R7CallbackHandler)OpenGL_ShowWindowCallback, (void*)cbPtr);

	R7_ProcessWxPendingEvents();

	return 1;
}

static int OpenGL_HideWindowCallback(void *data) {
	int r7Sn, functionSn;

	OpenGLCallBack_t *cbPtr = (OpenGLCallBack_t *)data;

	r7Sn = cbPtr->r7Sn;
	functionSn = cbPtr->functionSn;
	
	int res;
	void *variableObject = NULL;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	if (openglPtr->status == 1)
	{

		openglPtr->openGLFrame->Show(false);
		
		openglPtr->status = 0;
	}
	else
	{
		//printf("Status error! \n");
		return 3;
	}
	
	return 1;
}

static int OpenGL_HideWindow(int r7Sn, int functionSn) {
	OpenGLCallBack_t *cbPtr = (OpenGLCallBack_t*)malloc(sizeof(OpenGLCallBack_t));

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	R7_QueueWxEvent((R7CallbackHandler)OpenGL_HideWindowCallback, (void *)cbPtr);

	R7_ProcessWxPendingEvents();

	return 1;
}

static int OpenGL_ShowImageCallback(void *data) {
	int r7Sn, functionSn;
	int res;
	void *variableObject = NULL;
	Mat getImage;

	OpenGLCallBack_t *cbPtr = ((OpenGLCallBack_t *)data);

	r7Sn = cbPtr->r7Sn;
	functionSn = cbPtr->functionSn;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	if (openglPtr->openGLFrame == NULL) {
		return -4;
	}

	res = R7_GetVariableMat(r7Sn, functionSn, 2, &getImage);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! OpenGL_ShowImage get image = %d", res);
		return -2;
	}

	if (openglPtr->status == 1 || openglPtr->status == 2)
	{
		openglPtr->openGLFrame->openGLCanvas->image = getImage.clone();

		openglPtr->openGLFrame->SetScrollbarFit();

		openglPtr->openGLFrame->SetClientSize((openglPtr->openGLFrame->openGLCanvas->image.cols)+OPENGL_SCROLL_BAR_WIDTH, (openglPtr->openGLFrame->openGLCanvas->image.rows)+OPENGL_SCROLL_BAR_WIDTH);

		openglPtr->openGLFrame->openGLCanvas->Refresh();

		openglPtr->status = 2;
	}
	else
	{
		//printf("Status error! \n");

		// return 3 or -3 here won't stop r7
		return 3;
	}

	return 1;
}

static int OpenGL_ShowImage(int r7Sn, int functionSn) {
	int res;
	void *variableObject = NULL;

	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);

	if (openglPtr->isClosingFrame == 1) {

		// if frame is closed, then stop r7
		return -4;
	}
	
	OpenGLCallBack_t *cbPtr = (OpenGLCallBack_t*)malloc(sizeof(OpenGLCallBack_t));

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	R7_QueueWxEvent((R7CallbackHandler)OpenGL_ShowImageCallback, (void *)cbPtr);

	R7_ProcessWxPendingEvents();
	
	return 1;
}

static int OpenGL_GetImageCallback(void *data) {
	int res;
	int r7Sn, functionSn;
	void *variableObject = NULL;
	Mat getImage;
	
	OpenGLCallBack_t *cbPtr = ((OpenGLCallBack_t *)data);

	r7Sn = cbPtr->r7Sn;
	functionSn = cbPtr->functionSn;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	if (openglPtr->openGLFrame == NULL) {
		return -4;
	}
	
	if (openglPtr->status == 2)
	{
		openglPtr->openGLFrame->OnScreenShot();
		
		res = R7_SetVariableMat(r7Sn, functionSn, 2, openglPtr->openGLFrame->screenshot);
		if (res <= 0) {
			R7_Printf(r7Sn, "ERROR! R7_SetVariableMat = %d", res);
			return -4;
		}
	}
	else
	{
		//printf("Status error! \n");
		return 3;
	}
	
	return 1;
}

static int OpenGL_GetImage(int r7Sn, int functionSn) {

	int res;
	void *variableObject = NULL;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	if (openglPtr->isClosingFrame == 1) {
		return -4;
	}
	
	OpenGLCallBack_t *cbPtr = (OpenGLCallBack_t*)malloc(sizeof(OpenGLCallBack_t));

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	R7_QueueWxEvent((R7CallbackHandler)OpenGL_GetImageCallback, (void *)cbPtr);

	R7_ProcessWxPendingEvents();
	
	return 1;
}


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
	json_object_set_new(variableObject, "name", json_string("OpenGLWindow"));
	json_object_set_new(variableObject, "type", json_string("object"));
	json_object_set_new(variableObject, "direction", json_string("OUT"));
	json_array_append(variableArray, variable);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("OpenGLWindowTitle"));
	json_object_set_new(variableObject, "type", json_string("string"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
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
	json_object_set_new(variableObject, "name", json_string("OpenGLWindow"));
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
	json_object_set_new(variableObject, "name", json_string("OpenGLWindow"));
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
	json_object_set_new(variableObject, "name", json_string("OpenGLWindow"));
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
	json_object_set_new(variableObject, "name", json_string("OpenGLWindow"));
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
