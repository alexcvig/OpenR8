#ifndef LIBRARY_H_PP
#define LIBRARY_H_PP

#include <string>
#include <GL/gl.h>

#include "wx/glcanvas.h"
#include "wx/wxprec.h"
#include "wx/wx.h"
#include "wx/toolbar.h"


using namespace cv;


class OpenGLFrame;


typedef struct {
	Mat image;
	Mat screenShot;
	OpenGLFrame *openGLFrame = NULL;
	int status;
	int isClosingFrame;
} OpenGL_t;


typedef struct {
	int r7Sn = 0;
	int functionSn = 0;
} OpenGLCallBack_t;


// The rendering context used by all GL canvases.
class OpenGLContext : public wxGLContext
{
public:
	OpenGLContext(wxGLCanvas *canvas);

	// render the cube showing it at given angles
	//void DrawImage(double x, double y, int win_width, int win_height);
	void DrawImage(Mat &mat, int windowW, int windowH, int x, int y, double scale);

private:
	// textures for the cube faces
	GLuint textures[6];
};


class OpenGLCanvas : public wxGLCanvas
{
public:
	OpenGLCanvas(wxWindow *parent, int *attribList = NULL, OpenGLFrame *frame = NULL);
	void setX(int x) { OpenGLCanvas::x = x; }
	void setY(int y) { OpenGLCanvas::y = y; }
	void paintEvent(wxPaintEvent& evt);

	OpenGLContext& GetContext(wxGLCanvas *canvas);
	
	OpenGLFrame *openGLFrame;
	Mat image;

	// Test fps
	clock_t current_ticks, delta_ticks;
	clock_t fps = 0;
	int frameCount = 0;
	bool isTestFps = false, isFirstCount = true;
	

private:
	void OnPaint(wxPaintEvent& event);
	void Spin(float xSpin, float ySpin);
	void OnSpinTimer(wxTimerEvent& WXUNUSED(event));
	OpenGLContext *openGLContext;
	
	int x = 0, y = 0;		//--- shift
	// angles of rotation around x- and y- axis
	float xangle, yangle;

	wxTimer spinTimer;
	bool useStereo, stereoWarningAlreadyDisplayed;

	wxDECLARE_EVENT_TABLE();
};

#endif
