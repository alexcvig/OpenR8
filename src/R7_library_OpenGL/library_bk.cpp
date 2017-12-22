/*
Copyright (c) 2004-2017 Open Robot Club. All rights reserved.
COM library for R7.
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

#include "wx/toolbar.h"


using namespace std;
using namespace cv;


cv::Mat inputImage;
cv::Mat outputImage;

static double scale = 1;

// the rendering context used by all GL canvases
class TestGLContext : public wxGLContext
{
public:
	TestGLContext(wxGLCanvas *canvas);
	
	// double scale = 0.5;

	// render the cube showing it at given angles
	void DrawRotatedCube(float xangle, float yangle);

private:
	// textures for the cube faces
	GLuint m_textures[6];
};

// Define a new application type
class MyApp : public wxApp
{
public:
	MyApp() { m_glContext = NULL; m_glStereoContext = NULL; }

	// Returns the shared context used by all frames and sets it as current for
	// the given canvas.
	TestGLContext& GetContext(wxGLCanvas *canvas, bool useStereo);

	// virtual wxApp methods
	virtual bool OnInit() wxOVERRIDE;
	virtual int OnExit() wxOVERRIDE;

private:
	// the GL context we use for all our mono rendering windows
	TestGLContext *m_glContext;
	// the GL context we use for all our stereo rendering windows
	TestGLContext *m_glStereoContext;
};

// Define a new frame type
class MyFrame : public wxFrame
{
public:
	// Constructor
	MyFrame(bool stereoWindow = false);
	
	// button
	wxButton *HelloWorld;

private:
	// Event handler functions
	void OnClose(wxCommandEvent& event);
	void OnAbout(wxCommandEvent& event);
	void OnScaleZoomIn(wxCommandEvent& event);
	void OnScaleZoomOut(wxCommandEvent& event);
	void OnSave(wxCommandEvent& event);
	
	void OnNewWindow(wxCommandEvent& event);
	void OnNewStereoWindow(wxCommandEvent& event);

	wxDECLARE_EVENT_TABLE();
};


//--- Scroll window
class ScrolledWidgetsPane : public wxScrolledWindow
{
public:
    ScrolledWidgetsPane(wxWindow* parent, wxWindowID id) : wxScrolledWindow(parent, id)
    {
        // the sizer will take care of determining the needed scroll size
        // (if you don't use sizers you will need to manually set the viewport size)
        wxBoxSizer* sizer = new wxBoxSizer(wxVERTICAL);
 
        this->SetSizer(sizer);
 
        // this part makes the scrollbars show up
        this->FitInside(); // ask the sizer about the needed size
        this->SetScrollRate(5, 5);
    }
 
};


class TestGLCanvas : public wxGLCanvas
{
public:
	TestGLCanvas(wxWindow *parent, int *attribList = NULL);


private:
	void OnPaint(wxPaintEvent& event);
	void Spin(float xSpin, float ySpin);
	void OnKeyDown(wxKeyEvent& event);
	void OnSpinTimer(wxTimerEvent& WXUNUSED(event));

	// angles of rotation around x- and y- axis
	float m_xangle,
		m_yangle;

	wxTimer m_spinTimer;
	bool m_useStereo,
		m_stereoWarningAlreadyDisplayed;

	wxDECLARE_EVENT_TABLE();
};

enum
{
	NEW_STEREO_WINDOW = wxID_HIGHEST + 1, 
	BUTTON_Hello
};





// ----------------------------------------------------------------------------
// constants
// ----------------------------------------------------------------------------

// control ids
enum
{
	SpinTimer = wxID_HIGHEST + 1
};

// ----------------------------------------------------------------------------
// helper functions
// ----------------------------------------------------------------------------

static void CheckGLError()
{
	printf("CheckGLError \n");
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

// function to draw the texture for cube faces
//這沒用到
/*
static wxImage DrawDice(int size, unsigned num)
{
	printf("DrawDice \n");
	wxASSERT_MSG(num >= 1 && num <= 6, wxT("invalid dice index"));

	const int dot = size / 16;        // radius of a single dot
	const int gap = 5 * size / 32;      // gap between dots

	wxBitmap bmp(size, size);
	wxMemoryDC dc;
	dc.SelectObject(bmp);
	dc.SetBackground(*wxWHITE_BRUSH);
	dc.Clear();
	dc.SetBrush(*wxBLACK_BRUSH);

	// the upper left and lower right points
	if (num != 1)
	{
		dc.DrawCircle(gap + dot, gap + dot, dot);
		dc.DrawCircle(size - gap - dot, size - gap - dot, dot);
	}

	// draw the central point for odd dices
	if (num % 2)
	{
		dc.DrawCircle(size / 2, size / 2, dot);
	}

	// the upper right and lower left points
	if (num > 3)
	{
		dc.DrawCircle(size - gap - dot, gap + dot, dot);
		dc.DrawCircle(gap + dot, size - gap - dot, dot);
	}

	// finally those 2 are only for the last dice
	if (num == 6)
	{
		dc.DrawCircle(gap + dot, size / 2, dot);
		dc.DrawCircle(size - gap - dot, size / 2, dot);
	}

	dc.SelectObject(wxNullBitmap);

	return bmp.ConvertToImage();
}
*/

// ============================================================================
// implementation
// ============================================================================

// ----------------------------------------------------------------------------
// TestGLContext
// ----------------------------------------------------------------------------

TestGLContext::TestGLContext(wxGLCanvas *canvas)
	: wxGLContext(canvas)
{
	printf("TestGLContext::TestGLContext \n");
	SetCurrent(*canvas);
	/*
	if (false) {
		// set up the parameters we want to use
		glEnable(GL_CULL_FACE);
		glEnable(GL_DEPTH_TEST);
		glEnable(GL_LIGHTING);
		glEnable(GL_LIGHT0);
		glEnable(GL_TEXTURE_2D);

		// add slightly more light, the default lighting is rather dark
		GLfloat ambient[] = { 0.5, 0.5, 0.5, 0.5 };
		glLightfv(GL_LIGHT0, GL_AMBIENT, ambient);

		// set viewing projection
		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		glFrustum(-0.5f, 0.5f, -0.5f, 0.5f, 1.0f, 3.0f);

		// create the textures to use for cube sides: they will be reused by all
		// canvases (which is probably not critical in the case of simple textures
		// we use here but could be really important for a real application where
		// each texture could take many megabytes)
		glGenTextures(WXSIZEOF(m_textures), m_textures);

		for (unsigned i = 0; i < WXSIZEOF(m_textures); i++)
		{
			glBindTexture(GL_TEXTURE_2D, m_textures[i]);

			glTexEnvf(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE);
			glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP);
			glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);

			const wxImage img(DrawDice(256, i + 1));

			glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
			glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, img.GetWidth(), img.GetHeight(),
				0, GL_RGB, GL_UNSIGNED_BYTE, img.GetData());
		}
	}
	*/

	glEnable(GL_TEXTURE_2D);
	//以下四行根據 https://stackoverflow.com/questions/7380773/glteximage2d-segfault-related-to-width-height
	//是當圖像寬度不整除4時避免圖形歪斜用的 (實際上後三行可以省略因為它們預設值本來就是0)
	glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
	glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
	glPixelStorei(GL_UNPACK_SKIP_PIXELS, 0);
	glPixelStorei(GL_UNPACK_SKIP_ROWS, 0);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

	/*
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
	glTexEnvf(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_REPLACE);
	*/


	CheckGLError();
}

void TestGLContext::DrawRotatedCube(float xangle, float yangle)
{
	printf("TestGLContext::DrawRotatedCube \n");
	
	/*
	if (false) {
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		glMatrixMode(GL_MODELVIEW);
		glLoadIdentity();
		glTranslatef(0.0f, 0.0f, -2.0f);
		glRotatef(xangle, 1.0f, 0.0f, 0.0f);
		glRotatef(yangle, 0.0f, 1.0f, 0.0f);

		// draw six faces of a cube of size 1 centered at (0, 0, 0)
		glBindTexture(GL_TEXTURE_2D, m_textures[0]);
		glBegin(GL_QUADS);
		glNormal3f(0.0f, 0.0f, 1.0f);
		glTexCoord2f(0, 0); glVertex3f(0.5f, 0.5f, 0.5f);
		glTexCoord2f(1, 0); glVertex3f(-0.5f, 0.5f, 0.5f);
		glTexCoord2f(1, 1); glVertex3f(-0.5f, -0.5f, 0.5f);
		glTexCoord2f(0, 1); glVertex3f(0.5f, -0.5f, 0.5f);
		glEnd();

		glBindTexture(GL_TEXTURE_2D, m_textures[1]);
		glBegin(GL_QUADS);
		glNormal3f(0.0f, 0.0f, -1.0f);
		glTexCoord2f(0, 0); glVertex3f(-0.5f, -0.5f, -0.5f);
		glTexCoord2f(1, 0); glVertex3f(-0.5f, 0.5f, -0.5f);
		glTexCoord2f(1, 1); glVertex3f(0.5f, 0.5f, -0.5f);
		glTexCoord2f(0, 1); glVertex3f(0.5f, -0.5f, -0.5f);
		glEnd();

		glBindTexture(GL_TEXTURE_2D, m_textures[2]);
		glBegin(GL_QUADS);
		glNormal3f(0.0f, 1.0f, 0.0f);
		glTexCoord2f(0, 0); glVertex3f(0.5f, 0.5f, 0.5f);
		glTexCoord2f(1, 0); glVertex3f(0.5f, 0.5f, -0.5f);
		glTexCoord2f(1, 1); glVertex3f(-0.5f, 0.5f, -0.5f);
		glTexCoord2f(0, 1); glVertex3f(-0.5f, 0.5f, 0.5f);
		glEnd();

		glBindTexture(GL_TEXTURE_2D, m_textures[3]);
		glBegin(GL_QUADS);
		glNormal3f(0.0f, -1.0f, 0.0f);
		glTexCoord2f(0, 0); glVertex3f(-0.5f, -0.5f, -0.5f);
		glTexCoord2f(1, 0); glVertex3f(0.5f, -0.5f, -0.5f);
		glTexCoord2f(1, 1); glVertex3f(0.5f, -0.5f, 0.5f);
		glTexCoord2f(0, 1); glVertex3f(-0.5f, -0.5f, 0.5f);
		glEnd();

		glBindTexture(GL_TEXTURE_2D, m_textures[4]);
		glBegin(GL_QUADS);
		glNormal3f(1.0f, 0.0f, 0.0f);
		glTexCoord2f(0, 0); glVertex3f(0.5f, 0.5f, 0.5f);
		glTexCoord2f(1, 0); glVertex3f(0.5f, -0.5f, 0.5f);
		glTexCoord2f(1, 1); glVertex3f(0.5f, -0.5f, -0.5f);
		glTexCoord2f(0, 1); glVertex3f(0.5f, 0.5f, -0.5f);
		glEnd();

		glBindTexture(GL_TEXTURE_2D, m_textures[5]);
		glBegin(GL_QUADS);
		glNormal3f(-1.0f, 0.0f, 0.0f);
		glTexCoord2f(0, 0); glVertex3f(-0.5f, -0.5f, -0.5f);
		glTexCoord2f(1, 0); glVertex3f(-0.5f, -0.5f, 0.5f);
		glTexCoord2f(1, 1); glVertex3f(-0.5f, 0.5f, 0.5f);
		glTexCoord2f(0, 1); glVertex3f(-0.5f, 0.5f, -0.5f);
		glEnd();

		glFlush();

		CheckGLError();
	}
	*/

	glClear(GL_COLOR_BUFFER_BIT);


	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	
	glScalef(scale, scale, scale);

	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, outputImage.cols, outputImage.rows, 0, GL_BGR_EXT, GL_UNSIGNED_BYTE, outputImage.ptr());

	//設置螢幕長寬要用 glViewport()，參考 http://blog.csdn.net/shizhipeng/article/details/4939529
	//glViewport(0, 0, inputImage.cols, inputImage.rows); //有這行，圖形會被固定長寬，否則會由 reshape() 縮放
	glBegin(GL_QUADS);

	//openGL座標，原點是螢幕正中央，向右為正，向上為正，預設範圍 -1~1
	//而圖片座標，原點是左上角，向右為正，向下為正，範圍 0~1
	glTexCoord2f(0.0f, 0.0f); glVertex2f(-1, 1);
	glTexCoord2f(1.0f, 0.0f); glVertex2f(1, 1);
	glTexCoord2f(1.0f, 1.0f); glVertex2f(1, -1);
	glTexCoord2f(0.0f, 1.0f); glVertex2f(-1, -1);



	glEnd();
	glFlush();

	CheckGLError();
}


// ----------------------------------------------------------------------------
// MyApp: the application object
// ----------------------------------------------------------------------------

//wxIMPLEMENT_APP(MyApp);

IMPLEMENT_APP_NO_MAIN(MyApp);
IMPLEMENT_WX_THEME_SUPPORT;

bool MyApp::OnInit()
{
	printf("MyApp::OnInit \n");
	if (!wxApp::OnInit())
		return false;

	new MyFrame();

	return true;
}

int MyApp::OnExit()
{
	printf("MyApp::OnExit \n");
	delete m_glContext;
	delete m_glStereoContext;

	return wxApp::OnExit();
}

TestGLContext& MyApp::GetContext(wxGLCanvas *canvas, bool useStereo)
{
	printf("MyApp::GetContext \n");
	TestGLContext *glContext;
	if (useStereo)
	{
		if (!m_glStereoContext)
		{
			// Create the OpenGL context for the first stereo window which needs it:
			// subsequently created windows will all share the same context.
			m_glStereoContext = new TestGLContext(canvas);
		}
		glContext = m_glStereoContext;
	}
	else
	{
		if (!m_glContext)
		{
			// Create the OpenGL context for the first mono window which needs it:
			// subsequently created windows will all share the same context.
			m_glContext = new TestGLContext(canvas);
		}
		glContext = m_glContext;
	}

	glContext->SetCurrent(*canvas);

	return *glContext;
}

// ----------------------------------------------------------------------------
// TestGLCanvas
// ----------------------------------------------------------------------------


wxBEGIN_EVENT_TABLE(TestGLCanvas, wxGLCanvas)
EVT_PAINT(TestGLCanvas::OnPaint)
EVT_KEY_DOWN(TestGLCanvas::OnKeyDown)
EVT_TIMER(SpinTimer, TestGLCanvas::OnSpinTimer)
wxEND_EVENT_TABLE()


TestGLCanvas::TestGLCanvas(wxWindow *parent, int *attribList)
// With perspective OpenGL graphics, the wxFULL_REPAINT_ON_RESIZE style
// flag should always be set, because even making the canvas smaller should
// be followed by a paint event that updates the entire canvas with new
// viewport settings.
	: wxGLCanvas(parent, wxID_ANY, attribList,
		wxDefaultPosition, wxDefaultSize,
		wxFULL_REPAINT_ON_RESIZE),
	m_xangle(30.0),
	m_yangle(30.0),
	m_spinTimer(this, SpinTimer),
	m_useStereo(false),
	m_stereoWarningAlreadyDisplayed(false)
{
	printf("TestGLCanvas::TestGLCanvas \n");
	if (attribList)
	{
		int i = 0;
		while (attribList[i] != 0)
		{
			if (attribList[i] == WX_GL_STEREO)
				m_useStereo = true;
			++i;
		}
	}
}

void TestGLCanvas::OnPaint(wxPaintEvent& WXUNUSED(event))
{
	printf("TestGLCanvas::OnPaint \n");
	// This is required even though dc is not 	 otherwise.
	wxPaintDC dc(this);
	
	//dc.SetUserScale(factor, factor);
	
	// Set the OpenGL viewport according to the client size of this canvas.
	// This is done here rather than in a wxSizeEvent handler because our
	// OpenGL rendering context (and thus viewport setting) is used with
	// multiple canvases: If we updated the viewport in the wxSizeEvent
	// handler, changing the size of one canvas causes a viewport setting that
	// is wrong when next another canvas is repainted.
	const wxSize ClientSize = GetClientSize();

	
	TestGLContext& canvas = wxGetApp().GetContext(this, m_useStereo);
	glViewport(0, 0, ClientSize.x, ClientSize.y);


	// Render the graphics and swap the buffers.
	GLboolean quadStereoSupported;
	glGetBooleanv(GL_STEREO, &quadStereoSupported);
	if (quadStereoSupported)
	{
		glDrawBuffer(GL_BACK_LEFT);
		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		glFrustum(-0.47f, 0.53f, -0.5f, 0.5f, 1.0f, 3.0f);
		canvas.DrawRotatedCube(m_xangle, m_yangle);
		CheckGLError();
		glDrawBuffer(GL_BACK_RIGHT);
		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		glFrustum(-0.53f, 0.47f, -0.5f, 0.5f, 1.0f, 3.0f);
		canvas.DrawRotatedCube(m_xangle, m_yangle);
		CheckGLError();
	}
	else
	{
		canvas.DrawRotatedCube(m_xangle, m_yangle);
		if (m_useStereo && !m_stereoWarningAlreadyDisplayed)
		{
			m_stereoWarningAlreadyDisplayed = true;
			wxLogError("Stereo not supported by the graphics card.");
		}
	}
	
	SwapBuffers();	// swap the buffers of the OpenGL canvas and thus show current output

}

void TestGLCanvas::Spin(float xSpin, float ySpin)
{
	printf("TestGLCanvas::Spin \n");
	m_xangle += xSpin;
	m_yangle += ySpin;

	Refresh(false);
}

void TestGLCanvas::OnKeyDown(wxKeyEvent& event)
{
	float angle = 5.0;

	switch (event.GetKeyCode())
	{
	case WXK_RIGHT:
		Spin(0.0, -angle);
		break;

	case WXK_LEFT:
		Spin(0.0, angle);
		break;

	case WXK_DOWN:
		Spin(-angle, 0.0);
		break;

	case WXK_UP:
		Spin(angle, 0.0);
		break;

	case WXK_SPACE:
		if (m_spinTimer.IsRunning())
			m_spinTimer.Stop();
		else
			m_spinTimer.Start(25);
		break;

	default:
		event.Skip();
		return;
	}
}

void TestGLCanvas::OnSpinTimer(wxTimerEvent& WXUNUSED(event))
{
	Spin(0.0, 4.0);
}

wxString glGetwxString(GLenum name)
{
	const GLubyte *v = glGetString(name);
	if (v == 0)
	{
		// The error is not important. It is GL_INVALID_ENUM.
		// We just want to clear the error stack.
		glGetError();

		return wxString();
	}

	return wxString((const char*)v);
}


// ----------------------------------------------------------------------------
// MyFrame: main application window
// ----------------------------------------------------------------------------

wxBEGIN_EVENT_TABLE(MyFrame, wxFrame)
EVT_MENU(wxID_NEW, MyFrame::OnNewWindow)
EVT_MENU(NEW_STEREO_WINDOW, MyFrame::OnNewStereoWindow)
EVT_MENU(wxID_ABOUT, MyFrame::OnAbout)
EVT_MENU(wxID_CLOSE, MyFrame::OnClose)
EVT_MENU(wxID_ZOOM_IN, MyFrame::OnScaleZoomIn)
EVT_MENU(wxID_ZOOM_OUT, MyFrame::OnScaleZoomOut)
EVT_MENU(wxID_SAVE, MyFrame::OnSave)
//EVT_BUTTON(BUTTON_Hello, MyFrame::OnClose)
wxEND_EVENT_TABLE()

MyFrame::MyFrame(bool stereoWindow)
	: wxFrame(NULL, wxID_ANY, wxT("Image Kelvin"))
{
	printf("MyFrame::MyFrame \n");
	int stereoAttribList[] = { WX_GL_RGBA, WX_GL_DOUBLEBUFFER, WX_GL_STEREO, 0 };

	new TestGLCanvas(this, stereoWindow ? stereoAttribList : NULL);

	//SetIcon(wxICON(sample));		// set icon on window
	
	
	//--- Create a menu
	wxMenu *fileMenu = new wxMenu;
    wxMenu *helpMenu = new wxMenu;
	wxMenu *scaleMenu = new wxMenu;
	
    helpMenu->Append(wxID_ABOUT, wxT("&About...\tF1"),
                     wxT("Show about dialog"));
    fileMenu->Append(wxID_CLOSE, wxT("&Exit\tAlt-X"),
                     wxT("Quit this program"));
					 
	scaleMenu->Append(wxID_ZOOM_IN, wxT("Zoom In"),
                     wxT("Scale the window"));
	scaleMenu->AppendSeparator();
	scaleMenu->Append(wxID_ZOOM_OUT, wxT("Zoom Out"),
                     wxT("Scale the window"));
					 
	wxMenuBar *menuBar = new wxMenuBar();
    menuBar->Append(fileMenu, wxT("&File"));
    menuBar->Append(helpMenu, wxT("&Help"));
	menuBar->Append(scaleMenu, wxT("&Scale"));
	//menuBar->Append(scaleMenu, wxT("&ZoomOut"));
    SetMenuBar(menuBar);
					 
	//--- Create a toolbar
	//wxImage::AddHandler(new wxPNGHandler);
	
	//wxBitmap bmpOpen(open_xpm);
	wxBitmap bmpSave(wxT("library/ImageKelvin/save.bmp"), wxBITMAP_TYPE_BMP);
	wxBitmap bmpMinus(wxT("library/ImageKelvin/minus.bmp"), wxBITMAP_TYPE_BMP);
	wxBitmap bmpPlus(wxT("library/ImageKelvin/plus.bmp"), wxBITMAP_TYPE_BMP);
	wxBitmap bmpClose(wxT("library/ImageKelvin/close.bmp"), wxBITMAP_TYPE_BMP);
	
	wxToolBar* toolBar = new wxToolBar(this, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTB_HORIZONTAL|wxNO_BORDER);
	//wxToolBar *toolbar = CreateToolBar();
	
	
	
	toolBar->AddTool(wxID_SAVE, wxT("Save"), bmpSave);
	toolBar->AddSeparator();
	toolBar->AddTool(wxID_ZOOM_OUT, wxT("Zoom Out"), bmpMinus);
	toolBar->AddTool(wxID_ZOOM_IN, wxT("Zoom In"), bmpPlus);
	toolBar->AddSeparator();
	toolBar->AddTool(wxID_CLOSE, wxT("Close"), bmpClose);
	
	toolBar->Realize();
	SetToolBar(toolBar);
	
					 
	//--- Show a button
	//HelloWorld = new wxButton(this, BUTTON_Hello, _T("Hello World"), wxDefaultPosition, wxDefaultSize, 0);
	
	
    CreateStatusBar(3);
    SetStatusText(wxT("Welcome to wxWidgets!"));
	

	// Make a menubar
	/*
	if (false) {
		wxMenu *menu = new wxMenu;
		menu->Append(wxID_NEW);
		menu->Append(NEW_STEREO_WINDOW, "New Stereo Window");
		menu->AppendSeparator();
		menu->Append(wxID_CLOSE);
		wxMenuBar *menuBar = new wxMenuBar;
		menuBar->Append(menu, wxT("&Cube"));

		SetMenuBar(menuBar);

		CreateStatusBar();

		
	}
	*/
	
	/*
	//--- scroll window
	wxBoxSizer* sizer = new wxBoxSizer(wxHORIZONTAL);
	//ScrolledWidgetsPane* ScrollFrame = new ScrolledWidgetsPane(this, wxID_ANY);
	
	wxScrolledWindow* scrolledWindow = new wxScrolledWindow(this, wxID_ANY, wxPoint(0, 0), wxSize(400, 400), wxVSCROLL|wxHSCROLL);
	
	int pixelsPerUnitX = 10;
	int pixelsPerUnitY = 10;
	int noUnitsX = 1000;
	int noUnitsY = 1000;
	
	scrolledWindow->SetScrollbars(pixelsPerUnitX, pixelsPerUnitY, noUnitsX, noUnitsY);
	
    sizer->Add(scrolledWindow, 1, wxEXPAND);
    this->SetSizer(sizer);
	*/
	
	SetClientSize(inputImage.cols, inputImage.rows);
	Show();

	/*
	if (false) {
		// test IsDisplaySupported() function:
		static const int attribs[] = { WX_GL_RGBA, WX_GL_DOUBLEBUFFER, 0 };
		wxLogStatus("Double-buffered display %s supported",
			wxGLCanvas::IsDisplaySupported(attribs) ? "is" : "not");

		if (stereoWindow)
		{
			const wxString vendor = glGetwxString(GL_VENDOR).Lower();
			const wxString renderer = glGetwxString(GL_RENDERER).Lower();
			if (vendor.find("nvidia") != wxString::npos &&
				renderer.find("quadro") == wxString::npos)
				ShowFullScreen(true);
		}
	}
	*/
}

void MyFrame::OnClose(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnClose \n");
	// true is to force the frame to close
	Close(true);
}

void MyFrame::OnAbout(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnAbout \n");
	
	wxMessageBox("Hello world!");
	
}

void MyFrame::OnSave(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnSave \n");
	
	cv::Mat img(outputImage.size().height, outputImage.size().width, CV_8UC3);
	cv::Mat img_flipped;
	
	
	// Read gl front buffer
	glReadBuffer( GL_FRONT );
	
	//use fast 4-byte alignment (default anyway) if possible
	glPixelStorei(GL_PACK_ALIGNMENT, (img.step & 3) ? 1 : 4);
	
	//set length of one complete row in destination data (doesn't need to equal img.cols)
	glPixelStorei(GL_PACK_ROW_LENGTH, img.step/img.elemSize());
	
	glReadPixels(0, 0, img.cols, img.rows, GL_BGR_EXT, GL_UNSIGNED_BYTE, img.data);
	
	cv::flip(img, img_flipped, 0);
	
	cv::imwrite("debug_image.png", img_flipped);
	
	wxMessageBox("Save successfully!");
}

void MyFrame::OnScaleZoomIn(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnScaleZoomIn \n");
	
	
	scale *= 2;
	
	if (scale < 0.5)
	{
		scale = 0.5;
		wxMessageBox("Zoom limit is 0.5 ~ 4");
		return;
	}
	else if (scale > 4)
	{
		scale = 4;
		wxMessageBox("Zoom limit is 0.5 ~ 4");
		return;
	}
	
	wxMessageBox("MyFrame::OnScaleZoomIn!");
	
	printf("Before resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	//cv::resize(outputImage, outputImage, cv::Size(outputImage.cols * 2, outputImage.rows * 2), 0, 0, CV_INTER_LINEAR);
	
	printf("After resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	Update();
    Refresh();
}

void MyFrame::OnScaleZoomOut(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnScaleZoomOut \n");
	
	scale *= 0.5;
	
	if (scale < 0.5)
	{
		scale = 0.5;
		wxMessageBox("Zoom limit is 0.5 ~ 4");
		return;
	}
	else if (scale > 4)
	{
		scale = 3;
		wxMessageBox("Zoom limit is 0.5 ~ 4");
		return;
	}
	
	wxMessageBox("MyFrame::OnScaleZoomOut!");
	
	printf("Before resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	//cv::resize(outputImage, outputImage, cv::Size(outputImage.cols * 0.5, outputImage.rows * 0.5), 0, 0, CV_INTER_LINEAR);
	
	printf("After resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	Update();
    Refresh();

}

void MyFrame::OnNewWindow(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnNewWindow \n");
	new MyFrame();
}

void MyFrame::OnNewStereoWindow(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnNewStereoWindow \n");
	new MyFrame(true);
}




#ifdef __cplusplus
extern "C"
{
#endif



static int ImageKelvin_Debug(int r7Sn, int functionSn) {
	printf("ImageKelvin_Debug ++\n");

	int res1, res2;

	// Load image by cv
	res1 = R7_GetVariableMat(r7Sn, functionSn, 1, &inputImage);
	res2 = R7_GetVariableMat(r7Sn, functionSn, 1, &outputImage);

	if (res1 <= 0 || res2 <= 0) {
		R7_Printf(r7Sn, "ERROR! ImageKelvin_Debug = %d", res1);
		return -1;
	}

	//--- Debug
	//cv::imshow("OpenCV_TEST", inputImage);

	//--- OpenGL
	//glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT); // Clear the window

	
	//--- wxWidgets

	int testArgc = 0;
	char ** testArgv = NULL;
	wxEntryStart(testArgc, testArgv);

	wxTheApp->CallOnInit();

	wxTheApp->OnRun();

	wxTheApp->OnExit();

	wxEntryCleanup();

	printf("ImageKelvin_Debug --\n");
	return 1;
}


R7_API int R7Library_Init(void) {
	// Register your functions in this API.

	/*
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
	*/
	R7_RegisterFunction("ImageKelvin_Debug", (R7Function_t)&ImageKelvin_Debug);
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
	json_object_set_new(functionGroupObject, "name", json_string("ImageKelvin"));
	json_array_append(functionGroupArray, functionGroup);

	functionArray = json_array();
	json_object_set_new(functionGroupObject, "functions", functionArray);

	// ImageKelvin_Debug
	function = json_object();
	functionObject = json_object();
	json_object_set_new(function, "function", functionObject);
	json_object_set_new(functionObject, "name", json_string("ImageKelvin_Debug"));
	json_object_set_new(functionObject, "doc", json_string(""));
	json_array_append(functionArray, function);
	variableArray = json_array();
	json_object_set_new(functionObject, "variables", variableArray);
	variableObject = json_object();
	variable = json_object();
	json_object_set_new(variable, "variable", variableObject);
	json_object_set_new(variableObject, "name", json_string("Image"));
	json_object_set_new(variableObject, "type", json_string("image"));
	json_object_set_new(variableObject, "direction", json_string("IN"));
	json_array_append(variableArray, variable);


	sprintf_s(str, strSize, "%s", json_dumps(root, 0));

	json_decref(root);

	return 1;
}


/*
void display2() {
	glClear(GL_COLOR_BUFFER_BIT);


	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();


	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, inputImage.cols, inputImage.rows, 0, 0x80E0, GL_UNSIGNED_BYTE, inputImage.ptr());

	//設置螢幕長寬要用 glViewport()，參考 http://blog.csdn.net/shizhipeng/article/details/4939529
	//glViewport(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT); //有這行，圖形會被固定長寬，否則會由 reshape() 縮放
	glBegin(GL_QUADS);

	//openGL座標，原點是螢幕正中央，向右為正，向上為正，預設範圍 -1~1
	//而圖片座標，原點是左上角，向右為正，向下為正，範圍 0~1
	glTexCoord2f(0.0f, 0.0f); glVertex2f(-1, 1);
	glTexCoord2f(1.0f, 0.0f); glVertex2f(1, 1);
	glTexCoord2f(1.0f, 1.0f); glVertex2f(1, -1);
	glTexCoord2f(0.0f, 1.0f); glVertex2f(-1, -1);



	glEnd();
	glFlush();
	//glutSwapBuffers();
}


void keyboard(unsigned char key, int x, int y)
{
	if (key == 'q' || key == 'Q' || key == ((unsigned char)13)) { //13 = enter
		exit(EXIT_SUCCESS);
	}
	else {
		if (key != NULL) {
			printf("getkey: %d, %d, %d\n", (int)key, x, y);
		}
		//printf("getkey: %c, %d, %d\n", key, x, y);
	}
}
*/

int main(void) {
	printf("testing\n");
	printf("ImageKelvin_Debug ++\n");

//	int res;
	/*
	觸發順序為：
	MyApp::OnInit
	MyFrame::MyFrame
	TestGLCanvas::TestGLCanvas
	TestGLCanvas::OnPaint	// SwapBuffers
	MyApp::GetContext
	TestGLContext::TestGLContext	// SetCurrent
	CheckGLError
	TestGLContext::DrawRotatedCube
	CheckGLError
	*/

	
	inputImage = Mat(cv::Size(301, 500), CV_8UC3, Scalar(0, 0, 255));
	cv::rectangle(inputImage, Rect(20, 20, 100, 100), Scalar(255, 0, 0), -1);

	printf("inputImage Seted \n");

	int testArgc = 0;
	char ** testArgv = NULL;
	wxEntryStart(testArgc, testArgv);

	wxTheApp->CallOnInit();

	wxTheApp->OnRun();

	wxTheApp->OnExit();

	wxEntryCleanup();

	system("PAUSE");
	return 0;
}

#ifdef __cplusplus
}
#endif
