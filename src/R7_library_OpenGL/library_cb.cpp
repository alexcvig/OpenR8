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
#include "library.hpp"

#include "wx/toolbar.h"


using namespace std;
using namespace cv;


Mat inputImage;
Mat outputImage;

bool isMyFrame = false;
bool isMyOpenglFrame = false;

static bool isWxWidgetsShowAnyImage = false;
static bool isWxWidgetsInit = false;


//static double scale = 1.0;

double scale[] = {0.05, 0.1, 0.25, 0.5, 1.0, 2.0, 4.0, 8.0, 16.0};
static int scale_index = 4;		//--- scale = 1.0


// the rendering context used by all GL canvases
class TestGLContext : public wxGLContext
{
public:
	TestGLContext(wxGLCanvas *canvas);
	
	// double scale = 0.5;

	// render the cube showing it at given angles
	//void DrawImage(double x, double y, int win_width, int win_height);
	void DrawImage(Mat &mat, int windowW, int windowH, int x, int y, double scale);

private:
	// textures for the cube faces
	GLuint m_textures[6];
};

/*
class TestGLCanvas : public wxGLCanvas
{
public:
	TestGLCanvas(wxWindow *parent, int *attribList = NULL);
	void setX(int x) { TestGLCanvas::x = x; printf("scroll_x = %d\n", x);}
	void setY(int y) { TestGLCanvas::y = y; printf("scroll_y = %d\n", y);}
	void paintEvent(wxPaintEvent& evt);
	void OnMouseMotionPanel(wxMouseEvent& event);
	
	MyFrame *my_frame;

private:
	void OnPaint(wxPaintEvent& event);
	void Spin(float xSpin, float ySpin);
	//void OnKeyDown(wxKeyEvent& event);
	void OnSpinTimer(wxTimerEvent& WXUNUSED(event));

	//--- shift
	int x=0, y=0;	
	// angles of rotation around x- and y- axis
	float m_xangle,
		m_yangle;

	wxTimer m_spinTimer;
	bool m_useStereo,
		m_stereoWarningAlreadyDisplayed;

	wxDECLARE_EVENT_TABLE();
};
*/


// Define a new frame type
class MyFrame : public wxFrame
{
public:
	// Constructor
	MyFrame(bool stereoWindow = false);
	~MyFrame();
	//int stereoAttribList[] = { WX_GL_RGBA, WX_GL_DOUBLEBUFFER, WX_GL_STEREO, 0 };
	
	TestGLCanvas* img_panel;
	wxScrollBar* sbh;
	wxScrollBar* sbv;
	Mat image_screenshot;
	//Mat inputImage;
	//Mat outputImage;
	//bool isEnableCrop = false;
	
	//--- Scrollbar variable
	const int start_pos = 0;
	int scroll_x = 0;
	int scroll_y = 0;
	int viewable_width = 500;
	int viewable_height = 500;
	int map_width = 1500;
	int map_height = 1500;
	
	// button
	//wxButton *HelloWorld;
	
	void hScroll(wxScrollEvent& evt)
	{
		printf("hScroll...\n");
		scroll_x = sbh->GetThumbPosition();
		img_panel->setX(scroll_x);
		img_panel->Refresh();
	}
	void vScroll(wxScrollEvent& evt)
	{
		printf("vScroll...\n");
		scroll_y = sbv->GetThumbPosition();
		img_panel->setY(scroll_y);
		img_panel->Refresh();
	}
	
	
	void SetStatus(void);
	void OnKeyDown(wxKeyEvent& event);
	void SetScrollbarFit(void);
	void OnMouseMotionToolbar(wxMouseEvent& event);
	void OnScreenShot(void);

private:
	// Event handler functions
	void OnClose(wxCommandEvent& event);
	//void OnAbout(wxCommandEvent& event);
	void OnScaleZoomIn(wxCommandEvent& event);
	void OnScaleZoomOut(wxCommandEvent& event);
	void OnSave(wxCommandEvent& event);
	void OnResize(wxCommandEvent& event);
	void OnBinarize(wxCommandEvent& event);
	void OnRotate(wxCommandEvent& event);
	void OnCrop(wxCommandEvent& event);
	void OnBlur(wxCommandEvent& event);
	void OnErode(wxCommandEvent& event);
	void OnDilate(wxCommandEvent& event);
	void OnReset(wxCommandEvent& event);
	
	void OnNewWindow(wxCommandEvent& event);
	void OnNewStereoWindow(wxCommandEvent& event);
	
	
	wxDECLARE_EVENT_TABLE();
};

enum
{
	NEW_STEREO_WINDOW = wxID_HIGHEST + 1, 
	wxID_SCROLL_H,
	wxID_SCROLL_V,
	wxID_RESETIMG,
	wxID_RESIZE,
	wxID_BINARIZE,
	wxID_ROTATE,
	wxID_CROP,
	wxID_BLUR,
	wxID_ERODE,
	wxID_DILATE,
	wxID_SLIDER_BINARIZE,
	wxID_TC_THRESHOLD,
	wxID_SLIDER_ROTATE,
	wxID_TC_ROTATE,
	wxID_TC_WIDTH,
	wxID_TC_HEIGHT,
	wxID_TC_BLUR_SIZEX,
	wxID_TC_BLUR_SIZEY,
	wxID_TC_ERODE_SIZEX,
	wxID_TC_ERODE_SIZEY,
	wxID_TC_DILATE_SIZEX,
	wxID_TC_DILATE_SIZEY,
	wxID_SCREENSHOT
	//BUTTON_Hello
};


class MyOpenglFrame : public wxFrame
{
public:
	MyOpenglFrame(void);
	~MyOpenglFrame(void);
	
	TestGLCanvas* img_panel;
	wxScrollBar* sbh;
	wxScrollBar* sbv;
	
	//--- Scrollbar variable
	const int start_pos = 0;
	int scroll_x = 0;
	int scroll_y = 0;
	int viewable_width = 500;
	int viewable_height = 500;
	int map_width = 1500;
	int map_height = 1500;
	
	void hScroll(wxScrollEvent& evt)
	{
		printf("hScroll...\n");
		scroll_x = sbh->GetThumbPosition();
		img_panel->setX(scroll_x);
		img_panel->Refresh();
	}
	void vScroll(wxScrollEvent& evt)
	{
		printf("vScroll...\n");
		scroll_y = sbv->GetThumbPosition();
		img_panel->setY(scroll_y);
		img_panel->Refresh();
	}

private:
	void OnExit(wxCommandEvent& event);
	
	wxDECLARE_EVENT_TABLE();
};

MyFrame *my_frame;
//MyOpenglFrame *my_opengl_frame;

// Define a new application type
class MyApp : public wxApp
{
public:
	MyApp() { m_glContext = NULL; m_glStereoContext = NULL; }

	// Returns the shared context used by all frames and sets it as current for
	// the given canvas.
	TestGLContext& GetContext(wxGLCanvas *canvas, bool useStereo);
	
	//MyFrame *my_frame;
	//MyOpenglFrame *my_opengl_frame;
	

	// virtual wxApp methods
	virtual bool OnInit() wxOVERRIDE;
	virtual int OnExit() wxOVERRIDE;
	virtual int OnRun() wxOVERRIDE;

private:
	// the GL context we use for all our mono rendering windows
	TestGLContext *m_glContext;
	// the GL context we use for all our stereo rendering windows
	TestGLContext *m_glStereoContext;
};

//--- Define a resize dialog
class ResizeDialog : public wxDialog
{
public:
  ResizeDialog(const wxString& title, MyFrame *frame);
  MyFrame *my_frame;
  
  void cancelDialog(wxCommandEvent& event);
  void okDialog(wxCommandEvent& event);
  void OnEnter(wxCommandEvent& event );
  
  wxTextCtrl* tc_width;
  wxTextCtrl* tc_height;
  long int resize_w;
  long int resize_h;
  
  wxDECLARE_EVENT_TABLE();

};

wxBEGIN_EVENT_TABLE(ResizeDialog, wxDialog)
EVT_BUTTON(wxID_OK, ResizeDialog::okDialog)
EVT_BUTTON(wxID_CANCEL, ResizeDialog::cancelDialog)
EVT_TEXT_ENTER(wxID_TC_WIDTH,ResizeDialog::OnEnter)
EVT_TEXT_ENTER(wxID_TC_HEIGHT,ResizeDialog::OnEnter)
wxEND_EVENT_TABLE()


//--- Define a binarize dialog
class BinarizeDialog : public wxDialog
{
public:
  BinarizeDialog(const wxString& title, MyFrame *frame);
  MyFrame *my_frame;
  
  void cancelDialog(wxCommandEvent& event);
  void okDialog(wxCommandEvent& event);
  void OnSlider(wxScrollEvent& event);
  void OnEnter(wxCommandEvent& event );
  
  
  wxTextCtrl* tc_threshold;
  double threshold;
  wxSlider* slider;
  Mat orgImage;
  Mat orgImage_gray;
  
  wxDECLARE_EVENT_TABLE();

};

wxBEGIN_EVENT_TABLE(BinarizeDialog, wxDialog)
EVT_BUTTON(wxID_OK, BinarizeDialog::okDialog)
EVT_BUTTON(wxID_CANCEL, BinarizeDialog::cancelDialog)
EVT_COMMAND_SCROLL(wxID_SLIDER_BINARIZE, BinarizeDialog::OnSlider)
EVT_TEXT_ENTER(wxID_TC_THRESHOLD,BinarizeDialog::OnEnter)
wxEND_EVENT_TABLE()


//--- Define a rotate dialog
class RotateDialog : public wxDialog
{
public:
  RotateDialog(const wxString& title, MyFrame *frame);
  MyFrame *my_frame;
  
  void cancelDialog(wxCommandEvent& event);
  void okDialog(wxCommandEvent& event);
  void OnSlider(wxScrollEvent& event);
  void OnEnter(wxCommandEvent& event );
  
  
  wxTextCtrl* tc_rotate;
  double angle;
  wxSlider* slider;
  Mat orgImage;
  
  wxDECLARE_EVENT_TABLE();

};

wxBEGIN_EVENT_TABLE(RotateDialog, wxDialog)
EVT_BUTTON(wxID_OK, RotateDialog::okDialog)
EVT_BUTTON(wxID_CANCEL, RotateDialog::cancelDialog)
EVT_COMMAND_SCROLL(wxID_SLIDER_ROTATE, RotateDialog::OnSlider)
EVT_TEXT_ENTER(wxID_TC_ROTATE,RotateDialog::OnEnter)
wxEND_EVENT_TABLE()


//--- Define a blur dialog
class BlurDialog : public wxDialog
{
public:
  BlurDialog(const wxString& title, MyFrame *frame);
  MyFrame *my_frame;
  
  void cancelDialog(wxCommandEvent& event);
  void okDialog(wxCommandEvent& event);
  void OnEnter(wxCommandEvent& event );
  
  wxTextCtrl* tc_sizex;
  wxTextCtrl* tc_sizey;
  long int blur_x;
  long int blur_y;
  
  wxDECLARE_EVENT_TABLE();

};

wxBEGIN_EVENT_TABLE(BlurDialog, wxDialog)
EVT_BUTTON(wxID_OK, BlurDialog::okDialog)
EVT_BUTTON(wxID_CANCEL, BlurDialog::cancelDialog)
EVT_TEXT_ENTER(wxID_TC_BLUR_SIZEX,BlurDialog::OnEnter)
EVT_TEXT_ENTER(wxID_TC_BLUR_SIZEY,BlurDialog::OnEnter)
wxEND_EVENT_TABLE()

//--- Define a erode dialog
class ErodeDialog : public wxDialog
{
public:
  ErodeDialog(const wxString& title, MyFrame *frame);
  MyFrame *my_frame;
  
  void cancelDialog(wxCommandEvent& event);
  void okDialog(wxCommandEvent& event);
  void OnEnter(wxCommandEvent& event );
  
  wxTextCtrl* tc_sizex;
  wxTextCtrl* tc_sizey;
  long int erode_x;
  long int erode_y;
  
  wxDECLARE_EVENT_TABLE();

};

wxBEGIN_EVENT_TABLE(ErodeDialog, wxDialog)
EVT_BUTTON(wxID_OK, ErodeDialog::okDialog)
EVT_BUTTON(wxID_CANCEL, ErodeDialog::cancelDialog)
EVT_TEXT_ENTER(wxID_TC_ERODE_SIZEX,ErodeDialog::OnEnter)
EVT_TEXT_ENTER(wxID_TC_ERODE_SIZEY,ErodeDialog::OnEnter)
wxEND_EVENT_TABLE()

//--- Define a dilate dialog
class DilateDialog : public wxDialog
{
public:
  DilateDialog(const wxString& title, MyFrame *frame);
  MyFrame *my_frame;
  
  void cancelDialog(wxCommandEvent& event);
  void okDialog(wxCommandEvent& event);
  void OnEnter(wxCommandEvent& event );
  
  wxTextCtrl* tc_sizex;
  wxTextCtrl* tc_sizey;
  long int dilate_x;
  long int dilate_y;
  
  wxDECLARE_EVENT_TABLE();

};

wxBEGIN_EVENT_TABLE(DilateDialog, wxDialog)
EVT_BUTTON(wxID_OK, DilateDialog::okDialog)
EVT_BUTTON(wxID_CANCEL, DilateDialog::cancelDialog)
EVT_TEXT_ENTER(wxID_TC_DILATE_SIZEX,DilateDialog::OnEnter)
EVT_TEXT_ENTER(wxID_TC_DILATE_SIZEY,DilateDialog::OnEnter)
wxEND_EVENT_TABLE()

/*
//--- Define a crop dialog
class CropDialog : public wxDialog
{
public:
  CropDialog(const wxString& title);
  //MyFrame *my_frame;
  
  void cancelDialog(wxCommandEvent& event);
  void okDialog(wxCommandEvent& event);
  //void OnEnter(wxCommandEvent& event );
  
  wxDECLARE_EVENT_TABLE();

};

wxBEGIN_EVENT_TABLE(CropDialog, wxDialog)
EVT_BUTTON(wxID_OK, CropDialog::okDialog)
EVT_BUTTON(wxID_CANCEL, CropDialog::cancelDialog)
//EVT_TEXT_ENTER(wxID_TC_DILATE_SIZEX,CropDialog::OnEnter)
//EVT_TEXT_ENTER(wxID_TC_DILATE_SIZEY,CropDialog::OnEnter)
wxEND_EVENT_TABLE()
*/

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


ResizeDialog::ResizeDialog(const wxString & title, MyFrame *frame)
       : wxDialog(NULL, -1, title, wxDefaultPosition, wxSize(250, 230))
{
	//--- For keyboard events
	//SetFocus();
	
	
	char width_buf[64], height_buf[64];
	int width, height;
	
	my_frame = frame;
	
	width = outputImage.size().width;
	height = outputImage.size().height;
	
	sprintf(width_buf, "%d", width);
	sprintf(height_buf, "%d", height);
	
	wxPanel *panel = new wxPanel(this, -1);
	
	wxBoxSizer *vbox = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *hbox = new wxBoxSizer(wxHORIZONTAL);
	
	wxStaticText* widthLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Width:"),
		wxPoint(15, 20), wxDefaultSize, 0 );
	tc_width = new wxTextCtrl ( panel, wxID_TC_WIDTH, width_buf,
		wxPoint(15, 45), wxDefaultSize, wxTE_PROCESS_ENTER );
	wxStaticText* heightLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Height:"),
		wxPoint(15, 70), wxDefaultSize, 0 );
	tc_height = new wxTextCtrl ( panel, wxID_TC_HEIGHT, height_buf,
		wxPoint(15, 95), wxDefaultSize, wxTE_PROCESS_ENTER );
	
	wxButton *okButton = new wxButton(this, wxID_OK, wxT("Ok"), 
		wxDefaultPosition, wxSize(70, 30));
	wxButton *closeButton = new wxButton(this, wxID_CANCEL, wxT("Cancel"), 
		wxDefaultPosition, wxSize(70, 30));
	
	hbox->Add(okButton, 1);
	hbox->Add(closeButton, 1, wxLEFT, 5);
	
	vbox->Add(panel, 1);
	vbox->Add(hbox, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
	
	SetSizer(vbox);
	
	Centre();
	
	ShowModal();
	Destroy();
	
}

void ResizeDialog::cancelDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("ResizeDialog::cancelDialog\n");
	
	//--- close the dialog
	Destroy();
}

void ResizeDialog::okDialog(wxCommandEvent& event)
{
	printf("ResizeDialog::okDialog\n");
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_width->GetValue().ToLong(&resize_w);
	res_h = tc_height->GetValue().ToLong(&resize_h);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("resize_w = %d\n", (int)resize_w);
	printf("resize_h = %d\n", (int)resize_h);
	
	if (res_w != 1 || res_h != 1 || (int)resize_w < 1 || (int)resize_h < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	cv::resize(outputImage, outputImage, cv::Size((int)resize_w, (int)resize_h), 0, 0, CV_INTER_LINEAR);
	
	my_frame->Refresh();
	my_frame->SetStatus();
	
	Destroy();
}


void ResizeDialog::OnEnter(wxCommandEvent& event )
{
	printf("ResizeDialog::OnEnter\n");
	
	//wxMessageDialog(this, wxT("Got an enter"), wxT("Notice"), wxOK | wxICON_INFORMATION ).ShowModal();
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_width->GetValue().ToLong(&resize_w);
	res_h = tc_height->GetValue().ToLong(&resize_h);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("resize_w = %d\n", (int)resize_w);
	printf("resize_h = %d\n", (int)resize_h);
	
	if (res_w != 1 || res_h != 1 || (int)resize_w < 1 || (int)resize_h < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	cv::resize(outputImage, outputImage, cv::Size((int)resize_w, (int)resize_h), 0, 0, CV_INTER_LINEAR);
	
	my_frame->Refresh();
	my_frame->SetStatus();
	
	event.Skip();
	
	Destroy();
}

BinarizeDialog::BinarizeDialog(const wxString & title, MyFrame *frame)
       : wxDialog(NULL, -1, title, wxDefaultPosition, wxSize(250, 230))
{
	my_frame = frame;
	
	orgImage = outputImage.clone();
	
	wxPanel *panel = new wxPanel(this, -1);
	
	wxBoxSizer *vbox = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *hbox = new wxBoxSizer(wxHORIZONTAL);
	
	wxStaticText* widthLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Threshold:"),
		wxPoint(15, 20), wxDefaultSize, 0 );
	tc_threshold = new wxTextCtrl ( panel, wxID_TC_THRESHOLD, wxT("128"),
		wxPoint(15, 45), wxDefaultSize, wxTE_PROCESS_ENTER );
		
	slider = new wxSlider(panel, wxID_SLIDER_BINARIZE, 128, 0, 255, wxPoint(15, 80), wxSize(180, -1),
		wxSL_HORIZONTAL|wxSL_LABELS);
	
	wxButton *okButton = new wxButton(this, wxID_OK, wxT("Ok"), 
		wxDefaultPosition, wxSize(70, 30));
	wxButton *closeButton = new wxButton(this, wxID_CANCEL, wxT("Cancel"), 
		wxDefaultPosition, wxSize(70, 30));
	
	hbox->Add(okButton, 1);
	hbox->Add(closeButton, 1, wxLEFT, 5);
	
	vbox->Add(panel, 1);
	vbox->Add(hbox, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
	
	SetSizer(vbox);
	Centre();
	
	//--- show binarized image
	if (orgImage.channels() == 3)		//--- RGB to gray
	{
		cv::cvtColor(orgImage, orgImage_gray, CV_BGR2GRAY);
		cv::threshold(orgImage_gray, outputImage, 128, 255, THRESH_BINARY);
	}
	else
	{
		cv::threshold(orgImage, outputImage, 128, 255, THRESH_BINARY);
	}
	
	my_frame->Refresh();
	
	ShowModal();
	Destroy();
	
}

void BinarizeDialog::cancelDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("BinarizeDialog::cancelDialog\n");
	
	outputImage = orgImage.clone();
	
	my_frame->Refresh();
	
	//--- close the dialog
	Destroy();
}

void BinarizeDialog::okDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("BinarizeDialog::okDialog\n");
	
	int res = 0;
	
	res = tc_threshold->GetValue().ToDouble(&threshold);
	
	printf("res = %d\n", res);
	printf("threshold = %d\n", (int)threshold);
	
	if (res != 1 || (int)threshold < 0 || (int)threshold > 255)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	if (orgImage.channels() == 3)		//--- RGB to gray
	{
		cv::threshold(orgImage_gray, outputImage, threshold, 255, THRESH_BINARY);
	}
	else
	{
		cv::threshold(orgImage, outputImage, threshold, 255, THRESH_BINARY);
	}
	
	my_frame->Refresh();
	
	Destroy();
}

void BinarizeDialog::OnEnter(wxCommandEvent& WXUNUSED(event))
{
	printf("BinarizeDialog::OnEnter\n");
	
	int res = 0;
	
	res = tc_threshold->GetValue().ToDouble(&threshold);
	
	printf("res = %d\n", res);
	printf("threshold = %d\n", (int)threshold);
	
	if (res != 1 || (int)threshold < 0 || (int)threshold > 255)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	if (orgImage.channels() == 3)		//--- RGB to gray
	{
		cv::threshold(orgImage_gray, outputImage, threshold, 255, THRESH_BINARY);
	}
	else
	{
		cv::threshold(orgImage, outputImage, threshold, 255, THRESH_BINARY);
	}
	
	my_frame->Refresh();
	
	Destroy();
}

void BinarizeDialog::OnSlider(wxScrollEvent& event)
{
	printf("BinarizeDialog::OnSlider\n");
	
	int slide_index;
	
	slide_index = slider->GetValue();
	printf("Current slider = %d\n", slide_index);
	
	tc_threshold->SetValue(wxString::Format(wxT("%d"), slide_index));
	
	//--- Binarize
	if (orgImage.channels() == 3)		//--- RGB to gray
	{
		cv::threshold(orgImage_gray, outputImage, (double)slide_index, 255, THRESH_BINARY);
	}
	else
	{
		cv::threshold(orgImage, outputImage, (double)slide_index, 255, THRESH_BINARY);
	}
	
	my_frame->Refresh();
}



RotateDialog::RotateDialog(const wxString & title, MyFrame *frame)
       : wxDialog(NULL, -1, title, wxDefaultPosition, wxSize(250, 230))
{
	my_frame = frame;
	
	orgImage = outputImage.clone();
	
	wxPanel *panel = new wxPanel(this, -1);
	
	wxBoxSizer *vbox = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *hbox = new wxBoxSizer(wxHORIZONTAL);
	
	wxStaticText* widthLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Angle:"),
		wxPoint(15, 20), wxDefaultSize, 0 );
	tc_rotate = new wxTextCtrl ( panel, wxID_TC_ROTATE, wxT("0"),
		wxPoint(15, 45), wxDefaultSize, wxTE_PROCESS_ENTER );
		
	slider = new wxSlider(panel, wxID_SLIDER_ROTATE, 0, 0, 360, wxPoint(15, 80), wxSize(180, -1),
		wxSL_HORIZONTAL|wxSL_LABELS);
	
	wxButton *okButton = new wxButton(this, wxID_OK, wxT("Ok"), 
		wxDefaultPosition, wxSize(70, 30));
	wxButton *closeButton = new wxButton(this, wxID_CANCEL, wxT("Cancel"), 
		wxDefaultPosition, wxSize(70, 30));
	
	hbox->Add(okButton, 1);
	hbox->Add(closeButton, 1, wxLEFT, 5);
	
	vbox->Add(panel, 1);
	vbox->Add(hbox, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
	
	SetSizer(vbox);
	Centre();
	
	ShowModal();
	Destroy();
	
}

void RotateDialog::cancelDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("RotateDialog::cancelDialog\n");
	
	outputImage = orgImage.clone();
	
	my_frame->Refresh();
	
	//--- close the dialog
	Destroy();
}


void RotateDialog::okDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("RotateDialog::okDialog\n");
	
	int res = 0;
	
	res = tc_rotate->GetValue().ToDouble(&angle);
	
	printf("res = %d\n", res);
	printf("angle = %d\n", (int)angle);
	
	if (res != 1 || (int)angle < 0 || (int)angle > 360)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	//--- Rotate
	Point2f src_center(orgImage.cols/2.0F, orgImage.rows/2.0F);
	
	Mat rot_mat = getRotationMatrix2D(src_center, angle, 1.0);
	
	warpAffine(orgImage, outputImage, rot_mat, orgImage.size());
	
	my_frame->Refresh();
	
	//--- close the dialog
	Destroy();
}

void RotateDialog::OnEnter(wxCommandEvent& WXUNUSED(event))
{
	printf("RotateDialog::OnEnter\n");
	
		int res = 0;
	
	res = tc_rotate->GetValue().ToDouble(&angle);
	
	printf("res = %d\n", res);
	printf("angle = %d\n", (int)angle);
	
	if (res != 1 || (int)angle < 0 || (int)angle > 360)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	//--- Rotate
	Point2f src_center(orgImage.cols/2.0F, orgImage.rows/2.0F);
	
	Mat rot_mat = getRotationMatrix2D(src_center, angle, 1.0);
	
	warpAffine(orgImage, outputImage, rot_mat, orgImage.size());
	
	my_frame->Refresh();
	
	//--- close the dialog
	Destroy();
}

void RotateDialog::OnSlider(wxScrollEvent& event)
{
	printf("RotateDialog::OnSlider\n");

	int slide_index;
	
	slide_index = slider->GetValue();
	printf("Current slider = %d\n", slide_index);
	
	tc_rotate->SetValue(wxString::Format(wxT("%d"), slide_index));
	
	angle = (double)slide_index;
	printf("angle = %d\n", (int)angle);
	
	//--- Rotate
	Point2f src_center(orgImage.cols/2.0F, orgImage.rows/2.0F);
	
	Mat rot_mat = getRotationMatrix2D(src_center, angle, 1.0);
	
	warpAffine(orgImage, outputImage, rot_mat, orgImage.size());
	
	my_frame->Refresh();
	
	event.Skip();
}


BlurDialog::BlurDialog(const wxString & title, MyFrame *frame)
       : wxDialog(NULL, -1, title, wxDefaultPosition, wxSize(250, 230))
{
	//--- For keyboard events
	//SetFocus();
	
	
	//char width_buf[64], height_buf[64];
	//int width, height;
	
	my_frame = frame;
	
	//width = outputImage.size().width;
	//height = outputImage.size().height;
	
	//sprintf(width_buf, "%d", width);
	//sprintf(height_buf, "%d", height);
	
	wxPanel *panel = new wxPanel(this, -1);
	
	wxBoxSizer *vbox = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *hbox = new wxBoxSizer(wxHORIZONTAL);
	
	wxStaticText* widthLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Size X:"),
		wxPoint(15, 20), wxDefaultSize, 0 );
	tc_sizex = new wxTextCtrl ( panel, wxID_TC_BLUR_SIZEX, wxT("5"),
		wxPoint(15, 45), wxDefaultSize, wxTE_PROCESS_ENTER );
	wxStaticText* heightLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Size Y:"),
		wxPoint(15, 70), wxDefaultSize, 0 );
	tc_sizey = new wxTextCtrl ( panel, wxID_TC_BLUR_SIZEY, wxT("5"),
		wxPoint(15, 95), wxDefaultSize, wxTE_PROCESS_ENTER );
	
	wxButton *okButton = new wxButton(this, wxID_OK, wxT("Ok"), 
		wxDefaultPosition, wxSize(70, 30));
	wxButton *closeButton = new wxButton(this, wxID_CANCEL, wxT("Cancel"), 
		wxDefaultPosition, wxSize(70, 30));
	
	hbox->Add(okButton, 1);
	hbox->Add(closeButton, 1, wxLEFT, 5);
	
	vbox->Add(panel, 1);
	vbox->Add(hbox, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
	
	SetSizer(vbox);
	
	Centre();
	
	ShowModal();
	Destroy();
	
}

void BlurDialog::cancelDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("BlurDialog::cancelDialog\n");
	
	//--- close the dialog
	Destroy();
}

void BlurDialog::okDialog(wxCommandEvent& event)
{
	printf("BlurDialog::okDialog\n");
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_sizex->GetValue().ToLong(&blur_x);
	res_h = tc_sizey->GetValue().ToLong(&blur_y);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("blur_x = %d\n", (int)blur_x);
	printf("blur_y = %d\n", (int)blur_y);
	
	if (res_w != 1 || res_h != 1 || (int)blur_x < 1 || (int)blur_y < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	cv::blur(outputImage, outputImage, cv::Size((int)blur_x, (int)blur_y));
	
	my_frame->Refresh();
	
	Destroy();
}


void BlurDialog::OnEnter(wxCommandEvent& event )
{
	printf("BlurDialog::OnEnter\n");
	
	//wxMessageDialog(this, wxT("Got an enter"), wxT("Notice"), wxOK | wxICON_INFORMATION ).ShowModal();
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_sizex->GetValue().ToLong(&blur_x);
	res_h = tc_sizey->GetValue().ToLong(&blur_y);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("blur_x = %d\n", (int)blur_x);
	printf("blur_y = %d\n", (int)blur_y);
	
	if (res_w != 1 || res_h != 1 || (int)blur_x < 1 || (int)blur_y < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	cv::blur(outputImage, outputImage, cv::Size((int)blur_x, (int)blur_y));
	
	my_frame->Refresh();
	
	event.Skip();
	
	Destroy();
}


ErodeDialog::ErodeDialog(const wxString & title, MyFrame *frame)
       : wxDialog(NULL, -1, title, wxDefaultPosition, wxSize(250, 230))
{
	
	my_frame = frame;

	wxPanel *panel = new wxPanel(this, -1);
	
	wxBoxSizer *vbox = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *hbox = new wxBoxSizer(wxHORIZONTAL);
	
	wxStaticText* widthLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Size X:"),
		wxPoint(15, 20), wxDefaultSize, 0 );
	tc_sizex = new wxTextCtrl ( panel, wxID_TC_ERODE_SIZEX, wxT("3"),
		wxPoint(15, 45), wxDefaultSize, wxTE_PROCESS_ENTER );
	wxStaticText* heightLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Size Y:"),
		wxPoint(15, 70), wxDefaultSize, 0 );
	tc_sizey = new wxTextCtrl ( panel, wxID_TC_ERODE_SIZEY, wxT("3"),
		wxPoint(15, 95), wxDefaultSize, wxTE_PROCESS_ENTER );
	
	wxButton *okButton = new wxButton(this, wxID_OK, wxT("Ok"), 
		wxDefaultPosition, wxSize(70, 30));
	wxButton *closeButton = new wxButton(this, wxID_CANCEL, wxT("Cancel"), 
		wxDefaultPosition, wxSize(70, 30));
	
	hbox->Add(okButton, 1);
	hbox->Add(closeButton, 1, wxLEFT, 5);
	
	vbox->Add(panel, 1);
	vbox->Add(hbox, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
	
	SetSizer(vbox);
	
	Centre();
	
	ShowModal();
	Destroy();
	
}

void ErodeDialog::cancelDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("ErodeDialog::cancelDialog\n");
	
	//--- close the dialog
	Destroy();
}

void ErodeDialog::okDialog(wxCommandEvent& event)
{
	printf("ErodeDialog::okDialog\n");
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_sizex->GetValue().ToLong(&erode_x);
	res_h = tc_sizey->GetValue().ToLong(&erode_y);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("erode_x = %d\n", (int)erode_x);
	printf("erode_y = %d\n", (int)erode_y);
	
	if (res_w != 1 || res_h != 1 || (int)erode_x < 1 || (int)erode_y < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	Mat kernel = getStructuringElement(MORPH_RECT, Size((int)erode_x, (int)erode_y));
	erode(outputImage, outputImage, kernel, cv::Point(-1,-1), 1);
	
	my_frame->Refresh();
	
	Destroy();
}


void ErodeDialog::OnEnter(wxCommandEvent& event )
{
	printf("ErodeDialog::OnEnter\n");
	
	//wxMessageDialog(this, wxT("Got an enter"), wxT("Notice"), wxOK | wxICON_INFORMATION ).ShowModal();
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_sizex->GetValue().ToLong(&erode_x);
	res_h = tc_sizey->GetValue().ToLong(&erode_y);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("erode_x = %d\n", (int)erode_x);
	printf("erode_y = %d\n", (int)erode_y);
	
	if (res_w != 1 || res_h != 1 || (int)erode_x < 1 || (int)erode_y < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	Mat kernel = getStructuringElement(MORPH_RECT, Size((int)erode_x, (int)erode_y));
	erode(outputImage, outputImage, kernel, cv::Point(-1,-1), 1);
	
	my_frame->Refresh();
	
	event.Skip();
	
	Destroy();
}


DilateDialog::DilateDialog(const wxString & title, MyFrame *frame)
       : wxDialog(NULL, -1, title, wxDefaultPosition, wxSize(250, 230))
{
	
	my_frame = frame;

	wxPanel *panel = new wxPanel(this, -1);
	
	wxBoxSizer *vbox = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *hbox = new wxBoxSizer(wxHORIZONTAL);
	
	wxStaticText* widthLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Size X:"),
		wxPoint(15, 20), wxDefaultSize, 0 );
	tc_sizex = new wxTextCtrl ( panel, wxID_TC_DILATE_SIZEX, wxT("3"),
		wxPoint(15, 45), wxDefaultSize, wxTE_PROCESS_ENTER );
	wxStaticText* heightLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Size Y:"),
		wxPoint(15, 70), wxDefaultSize, 0 );
	tc_sizey = new wxTextCtrl ( panel, wxID_TC_DILATE_SIZEY, wxT("3"),
		wxPoint(15, 95), wxDefaultSize, wxTE_PROCESS_ENTER );
	
	wxButton *okButton = new wxButton(this, wxID_OK, wxT("Ok"), 
		wxDefaultPosition, wxSize(70, 30));
	wxButton *closeButton = new wxButton(this, wxID_CANCEL, wxT("Cancel"), 
		wxDefaultPosition, wxSize(70, 30));
	
	hbox->Add(okButton, 1);
	hbox->Add(closeButton, 1, wxLEFT, 5);
	
	vbox->Add(panel, 1);
	vbox->Add(hbox, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
	
	SetSizer(vbox);
	
	Centre();
	
	ShowModal();
	Destroy();
	
}

void DilateDialog::cancelDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("DilateDialog::cancelDialog\n");
	
	//--- close the dialog
	Destroy();
}

void DilateDialog::okDialog(wxCommandEvent& event)
{
	printf("DilateDialog::okDialog\n");
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_sizex->GetValue().ToLong(&dilate_x);
	res_h = tc_sizey->GetValue().ToLong(&dilate_y);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("dilate_x = %d\n", (int)dilate_x);
	printf("dilate_y = %d\n", (int)dilate_y);
	
	if (res_w != 1 || res_h != 1 || (int)dilate_x < 1 || (int)dilate_y < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	Mat kernel = getStructuringElement(MORPH_RECT, Size((int)dilate_x, (int)dilate_y));
	dilate(outputImage, outputImage, kernel, cv::Point(-1,-1), 1);
	
	my_frame->Refresh();
	
	Destroy();
}


void DilateDialog::OnEnter(wxCommandEvent& event )
{
	printf("DilateDialog::OnEnter\n");
	
	//wxMessageDialog(this, wxT("Got an enter"), wxT("Notice"), wxOK | wxICON_INFORMATION ).ShowModal();
	
	int res_w = 0, res_h = 0;
	
	res_w = tc_sizex->GetValue().ToLong(&dilate_x);
	res_h = tc_sizey->GetValue().ToLong(&dilate_y);
	
	printf("res_w = %d\n", res_w);
	printf("res_h = %d\n", res_h);
	printf("dilate_x = %d\n", (int)dilate_x);
	printf("dilate_y = %d\n", (int)dilate_y);
	
	if (res_w != 1 || res_h != 1 || (int)dilate_x < 1 || (int)dilate_y < 1)
	{
		wxMessageBox("Error input!");
		return;
	}
	
	Mat kernel = getStructuringElement(MORPH_RECT, Size((int)dilate_x, (int)dilate_y));
	dilate(outputImage, outputImage, kernel, cv::Point(-1,-1), 1);
	
	my_frame->Refresh();
	
	event.Skip();
	
	Destroy();
}

/*
CropDialog::CropDialog(const wxString & title, MyFrame *frame)
       : wxDialog(NULL, -1, title, wxDefaultPosition, wxSize(250, 230))
{
	
	//my_frame = frame;

	wxPanel *panel = new wxPanel(this, -1);
	
	wxBoxSizer *vbox = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer *hbox = new wxBoxSizer(wxHORIZONTAL);
	
	wxStaticText* widthLabel = new wxStaticText ( panel, wxID_STATIC, wxT("&Are you sure to crop the image?"),
		wxPoint(15, 20), wxDefaultSize, 0 );
	
	
	wxButton *okButton = new wxButton(this, wxID_OK, wxT("Ok"), 
		wxDefaultPosition, wxSize(70, 30));
	wxButton *closeButton = new wxButton(this, wxID_CANCEL, wxT("Cancel"), 
		wxDefaultPosition, wxSize(70, 30));
	
	hbox->Add(okButton, 1);
	hbox->Add(closeButton, 1, wxLEFT, 5);
	
	vbox->Add(panel, 1);
	vbox->Add(hbox, 0, wxALIGN_CENTER | wxTOP | wxBOTTOM, 10);
	
	SetSizer(vbox);
	
	Centre();
	
	ShowModal();
	Destroy();
	
}

void CropDialog::cancelDialog(wxCommandEvent& WXUNUSED(event))
{
	printf("CropDialog::cancelDialog\n");
	
	//--- close the dialog
	Destroy();
}

void CropDialog::okDialog(wxCommandEvent& event)
{
	printf("CropDialog::okDialog\n");

	if (img_panel->rec_x > 2 && img_panel->rec_y > 2)
		{
			cv::Rect myROI(img_panel->mouse_x, img_panel->mouse_y, img_panel->rec_x, img_panel->rec_y);
			cv::Mat croppedImage = outputImage(myROI);
			croppedImage.copyTo(outputImage);
		}
	
	Destroy();

}
*/

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

//void TestGLContext::DrawRotatedCube(double x, double y, int win_width, int win_height)
//實際上 windowW 目前沒用到....但也先傳進來備用
void TestGLContext::DrawImage(Mat &mat, int windowW, int windowH, int x, int y, double scale)
{
	//printf("OpenGLContext::DrawImage \n");
	if (mat.cols == 0) {
		//printf("OpenGLContext::DrawImage mat not set\n");
		return;
	}
	//printf("OpenGLContext::DrawImage mat W = %d channels = %d \n", mat.cols, mat.channels());

	glClear(GL_COLOR_BUFFER_BIT);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();

	/*
	if (scale == 2)
	glTranslatef(1, -1, 0);
	else if (scale == 4)
	glTranslatef(3, -3, 0);
	else if (scale == 0.5)
	glTranslatef(-0.5, 0.5, 0);
	else
	glTranslatef(0, 0, 0);


	//glTranslatef(-x_shift , y_shift , 0);

	glScalef(scale, scale, scale);

	//--- scroll bar

	double x_shift, y_shift;

	x_shift = x / 500;
	y_shift = y / 500;

	glTranslatef(-x_shift, y_shift, 0);
	*/

	// 關於 glTexImage2D 的 internalformat / format： http://blog.csdn.net/csxiaoshui/article/details/27543615
	// 簡單來說當 internalformat 與 format 相同時，因為不用轉換，運行效率會比較高
	// GL_RGB GL_BGR_EXT GL_LUMINANCE
	
	
	//cv::resize(mat, textureImage, Size(mat.cols / 4, mat.rows / 4));
	if (mat.channels() == 3) {		

		//glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, mat.cols, mat.rows, 0, GL_BGR_EXT, GL_UNSIGNED_BYTE, mat.ptr());
		//glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, mat.cols, mat.rows, 0, GL_BGR_EXT, GL_UNSIGNED_BYTE, mat.ptr());
		glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, mat.cols, mat.rows, 0, 0x80E0, GL_UNSIGNED_BYTE, mat.ptr());
		//glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, textureImage.cols, textureImage.rows, 0, 0x80E0, GL_UNSIGNED_BYTE, textureImage.ptr());
		
	}
	else {
		//灰階的用 GL_LUMINANCE
		glTexImage2D(GL_TEXTURE_2D, 0, GL_LUMINANCE, mat.cols, mat.rows, 0, GL_LUMINANCE, GL_UNSIGNED_BYTE, mat.ptr());
		//glTexImage2D(GL_TEXTURE_2D, 0, GL_LUMINANCE, textureImage.cols, textureImage.rows, 0, GL_LUMINANCE, GL_UNSIGNED_BYTE, textureImage.ptr());
	}
	
	//設置螢幕長寬要用 glViewport()，參考 http://blog.csdn.net/shizhipeng/article/details/4939529
	//glViewport(0, 0, mat.cols, mat.rows); //有這行，圖形會被固定長寬，否則會由 reshape() 縮放
										  //20171031 然後 Viewport 的錨點是左下角，一般 windows 或 openCV 的視窗習慣是左上角。
										  //因此再加個轉換。 相關資訊 http://blog.csdn.net/u010579736/article/details/51882046
	//glViewport(0 - x, windowH - (mat.rows * scale) + y, mat.cols * scale, mat.rows * scale);
	
	//glViewport(0, 0, windowW, windowH);

	glBegin(GL_QUADS);

	//openGL座標，原點是螢幕正中央，向右為正，向上為正，預設範圍 -1~1
	//而圖片座標，原點是左上角，向右為正，向下為正，範圍 0~1

	
	glTexCoord2f(0.0f, 0.0f); glVertex2f(-1, 1);
	glTexCoord2f(1.0f, 0.0f); glVertex2f(1, 1);
	glTexCoord2f(1.0f, 1.0f); glVertex2f(1, -1);
	glTexCoord2f(0.0f, 1.0f); glVertex2f(-1, -1);
	
	glEnd();
	glFlush();
	//printf("OpenGLContext::DrawImage end\n");
	return;
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

	/*
	if (isMyFrame)
	{
		printf("isMyFrame = true\n");
		my_frame = new MyFrame();
		//my_frame->Show(true);
	}
	else if (isMyOpenglFrame)
	{
		printf("isMyOpenglFrame = true\n");
		my_opengl_frame = new MyOpenglFrame();
		//my_opengl_frame->Show(true);
	}
	*/
	return true;
}

int MyApp::OnRun()
{
    //R7_Log(R7_INFO, “MyApp::OnRun”, “OnRun ++“);
    //當[沒有任何圖片需要顯示]時，不進入 OnRun
    if (isWxWidgetsShowAnyImage) {		//--- ShowWindow
        wxApp::OnRun();
		
    }
    else {
        //ExitMainLoop();
    }
    //R7_Log(R7_INFO, “MyApp::OnRun”, “OnRun --“);
    return 1;
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
//EVT_KEY_DOWN(TestGLCanvas::OnKeyDown)
EVT_TIMER(SpinTimer, TestGLCanvas::OnSpinTimer)
EVT_LEFT_DOWN(TestGLCanvas::OnMouseDown)
EVT_LEFT_UP(TestGLCanvas::OnMouseUp)
EVT_MOTION(TestGLCanvas::OnMouseMotionPanel)
wxEND_EVENT_TABLE()


TestGLCanvas::TestGLCanvas(wxWindow *parent, int *attribList, MyFrame *frame)
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
	
	//--- initial member variable
	my_frame = frame;
	myImage = Mat();
	
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
	//wxPaintDC dc(this);

	//dc.SetUserScale(factor, factor);
	
	// Set the OpenGL viewport according to the client size of this canvas.
	// This is done here rather than in a wxSizeEvent handler because our
	// OpenGL rendering context (and thus viewport setting) is used with
	// multiple canvases: If we updated the viewport in the wxSizeEvent
	// handler, changing the size of one canvas causes a viewport setting that
	// is wrong when next another canvas is repainted.
	const wxSize ClientSize = GetClientSize();		//--- size of canvas
	
	printf("OnPaint: ClientSize.x = %d\n", ClientSize.x);
	printf("OnPaint: ClientSize.y = %d\n", ClientSize.y);

	
	TestGLContext& canvas = wxGetApp().GetContext(this, m_useStereo);
	//glViewport(0, 0, ClientSize.x, ClientSize.y);
	//glViewport(0, ClientSize.y - outputImage.size().height, outputImage.size().width, outputImage.size().height);
	
	printf("Scale = %f\n", scale[scale_index]);
	printf("glViewport: myImage.size().width*scale = %f\n", myImage.size().width*scale[scale_index]);
	printf("glViewport: myImage.size().height*scale = %f\n", myImage.size().height*scale[scale_index]);
	printf("x = %d, y = %d\n", x, y);
	//glViewport(0, ClientSize.y - outputImage.size().height, outputImage.size().width*scale, outputImage.size().height*scale);
	//glViewport(0, ClientSize.y-outputImage.size().height*scale, outputImage.size().width*scale, outputImage.size().height*scale);
	
		glViewport(0-x, ClientSize.y-myImage.size().height*scale[scale_index]+y, myImage.size().width*scale[scale_index], myImage.size().height*scale[scale_index]);
	
	
		
	
	// Render the graphics and swap the buffers.
	GLboolean quadStereoSupported;
	glGetBooleanv(GL_STEREO, &quadStereoSupported);
	if (quadStereoSupported)
	{
		glDrawBuffer(GL_BACK_LEFT);
		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		glFrustum(-0.47f, 0.53f, -0.5f, 0.5f, 1.0f, 3.0f);
		canvas.DrawImage(myImage, ClientSize.x, ClientSize.y, x, y, scale[scale_index]);
		CheckGLError();
		glDrawBuffer(GL_BACK_RIGHT);
		glMatrixMode(GL_PROJECTION);
		glLoadIdentity();
		glFrustum(-0.53f, 0.47f, -0.5f, 0.5f, 1.0f, 3.0f);
		canvas.DrawImage(myImage, ClientSize.x, ClientSize.y, x, y, scale[scale_index]);
		CheckGLError();
	}
	else
	{
		canvas.DrawImage(myImage, ClientSize.x, ClientSize.y, x, y, scale[scale_index]);
		if (m_useStereo && !m_stereoWarningAlreadyDisplayed)
		{
			m_stereoWarningAlreadyDisplayed = true;
			wxLogError("Stereo not supported by the graphics card.");
		}
	}
	
	SwapBuffers();	// swap the buffers of the OpenGL canvas and thus show current output

	if (rectangle && rec_x > 2 && rec_y > 2)
	{
		wxPaintDC dc(this);
		//dc.Clear();
		/*
		dc.SetBrush(*wxTRANSPARENT_BRUSH); // blue filling
		dc.SetPen( wxPen( wxColor(255,175,175), 0 ) );
		dc.DrawRectangle( mouse_x, mouse_y, rec_x, rec_y );
		*/
		dc.SetPen( wxPen( wxColor(0,0,255), 1 ) ); // black line, 3 pixels thick
		//dc.DrawLine( mouse_x, mouse_y, mouse_x, mouse_y+rec_y ); // draw line across the rectangle
		dc.DrawLine( mouse_x, mouse_y, mouse_x, mouse_y+rec_y ); // draw line across the rectangle
		dc.DrawLine( mouse_x, mouse_y, mouse_x+rec_x, mouse_y ); // draw line across the rectangle
		dc.DrawLine( mouse_x+rec_x, mouse_y, mouse_x+rec_x, mouse_y+rec_y ); // draw line across the rectangle
		dc.DrawLine( mouse_x, mouse_y+rec_y, mouse_x+rec_x, mouse_y+rec_y ); // draw line across the rectangle
	}
}

void TestGLCanvas::OnMouseMotionPanel(wxMouseEvent& event)
{
	if (dragging)
	{
		printf("TestGLCanvas::OnMouseMotionPanel\n");
		
		/*
		const wxPoint pt = wxGetMousePosition();
		int mouseX = pt.x - this->GetScreenPosition().x;
		int mouseY = pt.y - this->GetScreenPosition().y;
		*/
		
		wxPoint mouseOnScreen = wxGetMousePosition();
		rec_x = mouseOnScreen.x - mouse_x - this->GetScreenPosition().x;
		rec_y = mouseOnScreen.y - mouse_y - this->GetScreenPosition().y;
		printf("rec_x = %d, rec_y = %d\n", rec_x, rec_y);
		
		//this->Move( this->ScreenToClient( wxPoint(newx, newy) ) );
		
		Refresh();
	}
	//printf("mouseX = %d, mouseY = %d\n", mouseX, mouseY);
	
}

void TestGLCanvas::OnMouseDown(wxMouseEvent& event)
{	
	printf("isEnableCrop = %d\n", isEnableCrop);

	if (isEnableCrop)
	{
		printf("TestGLCanvas::OnMouseDown\n");
		
		CaptureMouse();
		mouse_x = event.GetX();
		mouse_y = event.GetY();
		printf("mouse_x = %d, mouse_y = %d\n", mouse_x, mouse_y);
		
		dragging = true;
		rectangle = true;
		
		//Refresh();
	}
}

void TestGLCanvas::OnMouseUp(wxMouseEvent& event)
{
	
	if (dragging && rec_x > 2 && rec_y > 2)
	{
		printf("TestGLCanvas::OnMouseUp => Check OK\n");
		
		ReleaseMouse();
		dragging = false;
		rectangle = false;
		isEnableCrop = false;
		
		printf("Cropping: myROI(%d, %d, %d, %d)\n", mouse_x, mouse_y, rec_x, rec_y);
		
		wxMessageDialog dialog( NULL, wxT("Are you sure to crop the image?"), wxT("Crop")
			, wxOK | wxCANCEL | wxICON_QUESTION);
		
		switch ( dialog.ShowModal() )
		{
			case wxID_OK:
			{
				printf("case wxID_OK:\n");
			
				//--- scale
				cv::Rect myROI((int)((mouse_x+x)/scale[scale_index]), (int)((mouse_y+y)/scale[scale_index])
					, (int)((rec_x)/scale[scale_index]), (int)((rec_y)/scale[scale_index]));
				cv::Mat croppedImage = outputImage(myROI);
				croppedImage.copyTo(outputImage);
				
				
				my_frame->SetScrollbarFit();
					
				break;
			}
			case wxID_CANCEL:
				break;
			default:
				break;
		}
		
		Refresh();
		my_frame->SetStatus();
	}
	else if (dragging && (rec_x <= 2 || rec_y <= 2))
	{
		printf("TestGLCanvas::OnMouseUp => Error\n");
		
		ReleaseMouse();
		dragging = false;
		rectangle = false;
		isEnableCrop = false;
		
		my_frame->SetStatus();
	}
	
}

void TestGLCanvas::Spin(float xSpin, float ySpin)
{
	printf("TestGLCanvas::Spin \n");
	m_xangle += xSpin;
	m_yangle += ySpin;

	Refresh(false);
}
/*
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
*/

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
//EVT_MENU(wxID_ABOUT, MyFrame::OnAbout)
//EVT_LEFT_DOWN(MyFrame::OnMouseMotion)
EVT_MENU(wxID_CLOSE, MyFrame::OnClose)
EVT_MENU(wxID_ZOOM_IN, MyFrame::OnScaleZoomIn)
EVT_MENU(wxID_ZOOM_OUT, MyFrame::OnScaleZoomOut)
EVT_MENU(wxID_SAVE, MyFrame::OnSave)
EVT_MENU(wxID_RESIZE, MyFrame::OnResize)
EVT_MENU(wxID_BINARIZE, MyFrame::OnBinarize)
EVT_MENU(wxID_ROTATE, MyFrame::OnRotate)
EVT_MENU(wxID_CROP, MyFrame::OnCrop)
EVT_MENU(wxID_BLUR, MyFrame::OnBlur)
EVT_MENU(wxID_ERODE, MyFrame::OnErode)
EVT_MENU(wxID_DILATE, MyFrame::OnDilate)
EVT_MENU(wxID_RESETIMG, MyFrame::OnReset)
//EVT_MENU(wxID_SCREENSHOT, MyFrame::OnScreenShot)
//EVT_BUTTON(BUTTON_Hello, MyFrame::OnClose)
//EVT_COMMAND_SCROLL_THUMBTRACK(wxID_SCROLL_H, MyFrame::hScroll)
//EVT_COMMAND_SCROLL_THUMBTRACK(wxID_SCROLL_V, MyFrame::vScroll)
EVT_COMMAND_SCROLL(wxID_SCROLL_H, MyFrame::hScroll)
EVT_COMMAND_SCROLL(wxID_SCROLL_V, MyFrame::vScroll)
EVT_KEY_DOWN(MyFrame::OnKeyDown)
wxEND_EVENT_TABLE()

MyFrame::MyFrame(bool stereoWindow)
	: wxFrame(NULL, wxID_ANY, wxT("OpenGL window"))
{
	printf("MyFrame::MyFrame \n");
	
	int stereoAttribList[] = { WX_GL_RGBA, WX_GL_DOUBLEBUFFER, WX_GL_STEREO, 0 };
	//TestGLCanvas* img_panel;
	
	
	//--- support all available image formats
	wxInitAllImageHandlers();
	
	wxBoxSizer* sizer = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer* sizer2 = new wxBoxSizer(wxHORIZONTAL);
	
	img_panel = new TestGLCanvas(this, stereoWindow ? stereoAttribList : NULL, this);
	sbh = new wxScrollBar(this, wxID_SCROLL_H, wxDefaultPosition, wxDefaultSize, wxSB_HORIZONTAL);
	sbv = new wxScrollBar(this, wxID_SCROLL_V, wxDefaultPosition, wxDefaultSize, wxSB_VERTICAL);
	

	//SetIcon(wxICON(sample));		// set icon on window
	
	
	//--- Create a menu
	wxMenu *fileMenu = new wxMenu;
    //wxMenu *imageMenu = new wxMenu;
	//wxMenu *scaleMenu = new wxMenu;
	
	/*
	imageMenu->Append(wxID_RESETIMG, wxT("&Reset"),
                     wxT("Reset the image"));
    imageMenu->Append(wxID_RESIZE, wxT("&Resize"),
                     wxT("Resize the image"));
	imageMenu->Append(wxID_BINARIZE, wxT("&Binarize"),
                     wxT("Binarize the image"));
	imageMenu->Append(wxID_ROTATE, wxT("&Rotate"),
                     wxT("Rotate the image"));
	imageMenu->Append(wxID_CROP, wxT("&Crop"),
                     wxT("Crop the image"));
	imageMenu->Append(wxID_BLUR, wxT("&Blur"),
                     wxT("Blur the image"));
	imageMenu->Append(wxID_ERODE, wxT("&Erode"),
                     wxT("Erode the image"));
	imageMenu->Append(wxID_DILATE, wxT("&Dilate"),
                     wxT("Dilate the image"));
					 
	fileMenu->Append(wxID_SAVE, wxT("&Save As"),
                     wxT("Save the image as..."));
	fileMenu->Append(wxID_SCREENSHOT, wxT("&Screen Shot"),
                     wxT("Screen shot and save as..."));
	*/
    fileMenu->Append(wxID_CLOSE, wxT("&Exit"),
                     wxT("Quit this program"));
	
	/*
	scaleMenu->Append(wxID_ZOOM_IN, wxT("Zoom In"),
                     wxT("Scale the window"));
	scaleMenu->AppendSeparator();
	scaleMenu->Append(wxID_ZOOM_OUT, wxT("Zoom Out"),
                     wxT("Scale the window"));
	*/
					 
	wxMenuBar *menuBar = new wxMenuBar();
    menuBar->Append(fileMenu, wxT("&File"));
    //menuBar->Append(imageMenu, wxT("&Image"));
	//menuBar->Append(scaleMenu, wxT("&Scale"));

    SetMenuBar(menuBar);
		
	/*
	//--- Create a toolbar
	wxBitmap bmpSave(wxT("library/ImageKelvin/save.bmp"), wxBITMAP_TYPE_BMP);
	wxBitmap bmpMinus(wxT("library/ImageKelvin/minus.bmp"), wxBITMAP_TYPE_BMP);
	wxBitmap bmpPlus(wxT("library/ImageKelvin/plus.bmp"), wxBITMAP_TYPE_BMP);
	wxBitmap bmpClose(wxT("library/ImageKelvin/close.bmp"), wxBITMAP_TYPE_BMP);
	
	wxToolBar* toolBar = new wxToolBar(this, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTB_HORIZONTAL|wxNO_BORDER);
	//wxToolBar *toolbar = CreateToolBar();
	
	
	
	toolBar->AddTool(wxID_SAVE, wxT("Save As"), bmpSave, wxT("Save As"));
	toolBar->AddSeparator();
	toolBar->AddTool(wxID_ZOOM_OUT, wxT("Zoom Out"), bmpMinus, wxT("Zoom Out"));
	toolBar->AddTool(wxID_ZOOM_IN, wxT("Zoom In"), bmpPlus, wxT("Zoom In"));
	toolBar->AddSeparator();
	toolBar->AddTool(wxID_CLOSE, wxT("Close"), bmpClose, wxT("Close Window"));
	
	toolBar->Realize();
	SetToolBar(toolBar);
	
	
	toolBar->Connect(
    wxEVT_MOTION,
    wxMouseEventHandler(MyFrame::OnMouseMotionToolbar),
    NULL,
    this );
	*/
					 
	//--- Show a button
	//HelloWorld = new wxButton(this, BUTTON_Hello, _T("Hello World"), wxDefaultPosition, wxDefaultSize, 0);
	
	
    CreateStatusBar(1);
	
	/*
	char status_buf[64];
	int img_w, img_h;
	
	img_w = outputImage.size().width;
	img_h = outputImage.size().height;

	sprintf(status_buf, " %d px x %d px (%d%%)", img_w, img_h, (int)scale*100);
	
    //SetStatusText(wxT("Welcome to wxWidgets!"));
	SetStatusText((status_buf));
	*/

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
	
	/*
	viewable_width = outputImage.size().width;
	viewable_height = outputImage.size().height;
	if (scale_index < 4)
	{
		map_width = outputImage.size().width;
		map_height = outputImage.size().height;
	}
	else
	{
		map_width = outputImage.size().width*scale[scale_index];
		map_height = outputImage.size().height*scale[scale_index];
	}
	
	sbh->SetScrollbar(start_pos, viewable_width, map_width, viewable_width / 4);
	sbv->SetScrollbar(start_pos, viewable_height, map_height, viewable_height / 4);
	*/
	SetScrollbarFit();
	
	sizer2->Add(img_panel, 1, wxEXPAND);
	sizer2->Add(sbv, 0, wxEXPAND);
	sizer->Add(sizer2, 1, wxEXPAND);
	sizer->Add(sbh, 0, wxEXPAND);

	SetSizer(sizer);

	Center();
	
	//--- For keyboard events
	SetFocus();
	
	/*
	//--- Check image size (below 800 x 600)
	while((img_panel->myImage.size().width*scale[scale_index] > 800 ) || (img_panel->myImage.size().height*scale[scale_index] > 600))
	{
		scale_index--;
		
		if (scale_index < 0)
			scale_index = 0;
	}
	*/
	
	//SetStatus();
	
	
	// Important! otherwise, save img will be wrong
	SetClientSize(img_panel->myImage.cols, img_panel->myImage.rows);
	
	
	Show();

	
	//Raise();
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

MyFrame::~MyFrame()
{
	isMyOpenglFrame = false;
}

void MyFrame::OnMouseMotionToolbar(wxMouseEvent& event)
{
	printf("MyFrame::OnMouseMotionToolbar\n");
	
	const wxPoint pt = wxGetMousePosition();
	int mouseX = pt.x - this->GetScreenPosition().x;
	int mouseY = pt.y - this->GetScreenPosition().y;
	
	printf("mouseX = %d, mouseY = %d\n", mouseX, mouseY);
	
	if (mouseY > 60 && mouseY < 70)
	{
		if (mouseX > 10 && mouseX < 30)		//--- the position of Save as
			SetStatusText(wxT("Save As"));
		else if (mouseX > 40 && mouseX < 60)		//--- the position of Zoom Out
			SetStatusText(wxT("Zoom Out"));
		else if (mouseX > 62 && mouseX < 82)		//--- the position of Zoom In
			SetStatusText(wxT("Zoom In"));
		else if (mouseX > 95 && mouseX < 115)		//--- the position of Close Window
			SetStatusText(wxT("Close Window"));
		else
		SetStatus();
	}
	else
		SetStatus();
	
}

void MyFrame::SetScrollbarFit(void)
{
	printf("MyFrame::SetScrollbarFit\n");
	
	viewable_width = img_panel->myImage.size().width;
	viewable_height = img_panel->myImage.size().height;
	
	//const wxSize ClientSize = GetClientSize();
	
	//printf("ClientSize.x = %d\n", ClientSize.x);
	//printf("ClientSize.y = %d\n", ClientSize.y);
	
	//viewable_width = ClientSize.x;
	//viewable_height = ClientSize.y;
	
	if (scale_index < 4)
	{
		map_width = img_panel->myImage.size().width;
		map_height = img_panel->myImage.size().height;
	}
	else
	{
		map_width = img_panel->myImage.size().width*scale[scale_index];
		map_height = img_panel->myImage.size().height*scale[scale_index];
	}
	
	printf("viewable_width = %d\n", viewable_width);
	printf("viewable_height = %d\n", viewable_height);
	printf("map_width = %d\n", map_width);
	printf("map_height = %d\n", map_height);
	
	//sbh->SetScrollbar(start_pos, viewable_width, map_width, viewable_width / 4);
	sbh->SetScrollbar(start_pos, viewable_width, map_width, viewable_width / 4);
	sbv->SetScrollbar(start_pos, viewable_height, map_height, viewable_height / 4);
	
	img_panel->setX(0);
	img_panel->setY(0);
}

void MyFrame::SetStatus(void)
{
	printf("MyFrame::SetStatus\n");
	
	char status_buf[64];
	int img_w, img_h;
	
	img_w = outputImage.size().width;
	img_h = outputImage.size().height;

	sprintf(status_buf, " %d px x %d px (%d%%)", img_w, img_h, (int)(scale[scale_index]*100));
	
    //SetStatusText(wxT("Welcome to wxWidgets!"));
	SetStatusText((status_buf));
}

void MyFrame::OnClose(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnClose \n");
	// true is to force the frame to close
	Close(true);
}
/*
void MyFrame::OnAbout(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnAbout \n");
	
	wxMessageBox("Hello world!");
}
*/
void MyFrame::OnResize(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnResize \n");
	
	ResizeDialog* custom = new ResizeDialog(wxT("Resize"), this);
    custom->Show(true);
}

void MyFrame::OnReset(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnReset \n");
	
	outputImage = inputImage.clone();
	
	scale_index = 4;
	
	while((outputImage.size().width*scale[scale_index] > 800 ) || (outputImage.size().height*scale[scale_index] > 600))
	{
		scale_index--;
		
		if (scale_index < 0)
			scale_index = 0;
	}
	
	Refresh();
	SetClientSize(outputImage.cols*scale[scale_index], outputImage.rows*scale[scale_index]);
	
	SetScrollbarFit();
	SetStatus();
}

void MyFrame::OnKeyDown(wxKeyEvent& event)
{
	printf("MyFrame::OnKeyDown => press <%d> \n", (int)event.GetKeyCode());
    //wxMessageBox(wxString::Format("KeyDown: %i\n", (int)event.GetKeyCode()));
    //event.Skip();
	
}


void MyFrame::OnBinarize(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnBinarize \n");
	
	BinarizeDialog *custom = new BinarizeDialog(wxT("Binarize"), this);
    custom->Show(true);
}

void MyFrame::OnRotate(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnRotate \n");
	/*
	transpose(outputImage, outputImage);  
	flip(outputImage, outputImage,1);		//--- 1 => rotate 90 degree
	
	Refresh();
	*/
	RotateDialog *custom = new RotateDialog(wxT("Rotate"), this);
    custom->Show(true);
	
}

void MyFrame::OnCrop(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnCrop \n");
	
	/*
	cv::Rect myROI(100, 100, 200, 200);
	cv::Mat croppedImage = outputImage(myROI);
	croppedImage.copyTo(outputImage);
	*/
	
	img_panel->isEnableCrop = true;
	
	SetStatusText(wxT("Left click on the mouse, and drag the cropping area"));
	
}

/*
void MyFrame::ShowCropDialog(void)
{
	printf("MyFrame::ShowCropDialog \n");
	
	CropDialog *custom = new CropDialog(wxT("Crop"), this);
    custom->Show(true);
}
*/

void MyFrame::OnBlur(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnBlur \n");
	
	BlurDialog* custom = new BlurDialog(wxT("Blur"), this);
    custom->Show(true);
	
}

void MyFrame::OnErode(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnErode \n");
	
	ErodeDialog* custom = new ErodeDialog(wxT("Erode"), this);
    custom->Show(true);
	
}

void MyFrame::OnDilate(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnDilate \n");
	
	DilateDialog* custom = new DilateDialog(wxT("Dilate"), this);
    custom->Show(true);
	
}

void MyFrame::OnSave(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnSave \n");
	
	wxString filename = wxFileSelector(_T("Save As"), _T(""), _T(""), _T("*.png"), _T("BMP files (*.bmp)|*.bmp|GIF files (*gif)|*.gif|JPEG files (*jpg)|*.jpg|PNG files (*png)|*.png|TIFF files (*tif)|*.tif|XPM files (*xpm)|*.xpm|All files (*.*)|*.*"), (int)NULL);
	
	if (!filename.empty())
		cv::imwrite(filename.ToStdString(), outputImage);

}

void MyFrame::OnScreenShot(void)
{
	printf("MyFrame::OnScreenShot \n");
	
	//wxString filename = wxFileSelector(_T("Save As"), _T(""), _T(""), _T("*.png"), _T("BMP files (*.bmp)|*.bmp|GIF files (*gif)|*.gif|JPEG files (*jpg)|*.jpg|PNG files (*png)|*.png|TIFF files (*tif)|*.tif|XPM files (*xpm)|*.xpm|All files (*.*)|*.*"), (int)NULL);
	
	//if (!filename.empty())
	{
		//--- snapshot
		const wxSize ClientSize = GetClientSize();
		
		//Mat img(outputImage.size().height, outputImage.size().width, CV_8UC3);
		Mat img(ClientSize.y, ClientSize.x, CV_8UC3);
		
		Mat img_flipped;
		
		//--- Read gl front buffer
		glReadBuffer( GL_FRONT );
		//glReadBuffer( GL_BACK_LEFT );
		
		//use fast 4-byte alignment (default anyway) if possible
		glPixelStorei(GL_PACK_ALIGNMENT, (img.step & 3) ? 1 : 4);
		
		//set length of one complete row in destination data (doesn't need to equal img.cols)
		glPixelStorei(GL_PACK_ROW_LENGTH, img.step/img.elemSize());
		
		glReadPixels(0, 0, img.cols, img.rows, GL_BGR_EXT, GL_UNSIGNED_BYTE, img.data);
		
		cv::flip(img, img_flipped, 0);
		
		//cv::imwrite("debug.png", img_flipped);
		//cv::imwrite(filename.ToStdString(), img_flipped);
		
		image_screenshot = img_flipped.clone();
		
		//wxMessageBox("Save successfully!");
	}
}

void MyFrame::OnScaleZoomIn(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnScaleZoomIn \n");
	
	
	scale_index++;
	
	if (scale_index < 0)
	{
		scale_index = 0;
		//wxMessageBox("Zoom limit is from 5% to 1600%");
		return;
	}
	else if (scale_index > 8)
	{
		scale_index = 8;
		//wxMessageBox("Zoom limit is from 5% to 1600%");
		return;
	}
	
	img_panel->setX(0);
	img_panel->setY(0);
	
	//wxMessageBox("MyFrame::OnScaleZoomIn!");
	
	//printf("Before resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	//cv::resize(outputImage, outputImage, cv::Size(outputImage.cols * 2, outputImage.rows * 2), 0, 0, CV_INTER_LINEAR);
	
	//printf("After resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	Update();
    Refresh();
	
	SetScrollbarFit();
	
	SetStatus();
}

void MyFrame::OnScaleZoomOut(wxCommandEvent& WXUNUSED(event))
{
	printf("MyFrame::OnScaleZoomOut \n");
	
	scale_index--;
	
	if (scale_index < 0)
	{
		scale_index = 0;
		//wxMessageBox("Zoom limit is from 5% to 1600%");
		return;
	}
	else if (scale_index > 8)
	{
		scale_index = 8;
		//wxMessageBox("Zoom limit is from 5% to 1600%");
		return;
	}
	
	img_panel->setX(0);
	img_panel->setY(0);
	
	//wxMessageBox("MyFrame::OnScaleZoomOut!");
	
	//printf("Before resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	//cv::resize(outputImage, outputImage, cv::Size(outputImage.cols * 0.5, outputImage.rows * 0.5), 0, 0, CV_INTER_LINEAR);
	
	//printf("After resizing...\noutputImage.row = %d, outputImage.col = %d\n", outputImage.size().width ,outputImage.size().height);
	
	Update();
    Refresh();
	
	SetScrollbarFit();
	
	SetStatus();

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

/*
MyOpenglFrame::MyOpenglFrame(void)
	: wxFrame(NULL, wxID_ANY, wxT("OpenGL window"))
{
	printf("MyOpenglFrame::MyOpenglFrame \n");
	
	int stereoAttribList[] = { WX_GL_RGBA, WX_GL_DOUBLEBUFFER, WX_GL_STEREO, 0 };
	bool stereoWindow = false;
	
	//--- support all available image formats
	wxInitAllImageHandlers();
	
	wxBoxSizer* sizer = new wxBoxSizer(wxVERTICAL);
	wxBoxSizer* sizer2 = new wxBoxSizer(wxHORIZONTAL);
	
	img_panel = new TestGLCanvas(this, stereoWindow ? stereoAttribList : NULL, this);
	sbh = new wxScrollBar(this, wxID_SCROLL_H, wxDefaultPosition, wxDefaultSize, wxSB_HORIZONTAL);
	sbv = new wxScrollBar(this, wxID_SCROLL_V, wxDefaultPosition, wxDefaultSize, wxSB_VERTICAL);
	
	
	wxMenu *fileMenu = new wxMenu;
	
	fileMenu->Append(wxID_EXIT, wxT("&Exit"),
                     wxT("Quit this program"));
					 
	wxMenuBar *menuBar = new wxMenuBar();
    menuBar->Append(fileMenu, wxT("&File"));
	
	SetMenuBar(menuBar);
	
	CreateStatusBar(1);
	
	sizer2->Add(img_panel, 1, wxEXPAND);
	sizer2->Add(sbv, 0, wxEXPAND);
	sizer->Add(sizer2, 1, wxEXPAND);
	sizer->Add(sbh, 0, wxEXPAND);

	SetSizer(sizer);

	Center();
	
	Show();
}

MyOpenglFrame::~MyOpenglFrame(void)
{
	isMyOpenglFrame = false;
	
}
*/

#ifdef __cplusplus
extern "C"
{
#endif

typedef struct {
	Mat image;
	Mat screenShot;
	MyFrame *my_opengl_frame;
	int status;
} OpenGL_t;

typedef struct {
	int r7Sn;
	int functionSn;
} CallBack_t;

static int r7_OpenGL_NewWindow(void *data) {
	
	//R7_GetVariableBool(r7Sn, functionSn, 1, &isEnable); //20171101 改為只能開不能關。
	
	//如果 WxWidgetsEnable 為 true 且 WxWidgets還沒 init ，則在這邊 init

	int r7Sn, functionSn;

	CallBack_t *cbPtr = (CallBack_t *)data;

	r7Sn = cbPtr->r7Sn;
	functionSn = cbPtr->functionSn;

	if (false) {
		int wxArgc = 0;
		char **wxArgv = NULL;
		wxEntryStart(wxArgc, wxArgv);
		wxTheApp->CallOnInit();
		isWxWidgetsInit = true;
	}
	
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
	
	// Initial values of OpenGL_t.
	openglPtr->image = Mat();
	openglPtr->screenShot = Mat();
	openglPtr->status = 0;
	//openglPtr->videoCapture = new VideoCapture();
	//openglPtr->deviceNum = 0;
	//openglPtr->apiID = CAP_ANY;
	//openglPtr->capturedImage = Mat();
	
	isMyOpenglFrame = true;
	
	openglPtr->my_opengl_frame = new MyFrame();
	openglPtr->my_opengl_frame->image_screenshot = Mat();
	
	/*
	//--- wxWidgets
	int testArgc = 0;
	char ** testArgv = NULL;
	wxEntryStart(testArgc, testArgv);
	printf("wxTheApp->CallOnInit();\n");
	wxTheApp->CallOnInit();
	printf("wxTheApp->OnRun();\n");
	wxTheApp->OnRun();
	printf("wxTheApp->OnExit();\n");
	wxTheApp->OnExit();
	printf("wxTheApp->wxEntryCleanup();\n");
	wxEntryCleanup();
	*/
	
	return 1;
}

static int OpenGL_NewWindow(int r7Sn, int functionSn) {

	CallBack_t *cbPtr;

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	R7_QueueWxEvent((R7CallbackHandler)r7_OpenGL_NewWindow((void *)cbPtr));

	return 1;
}

static int r7_OpenGL_ShowWindow(void *data) {
	
	printf("OpenGL_ShowWindow \n");
	
	int r7Sn, functionSn;

	CallBack_t *cbPtr = (CallBack_t *)data;

	r7Sn = cbPtr->r7Sn;
	functionSn = cbPtr->functionSn;

	
	int res;
	void *variableObject = NULL;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	
	printf("Line: %d\n", __LINE__);
	
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	printf("Line: %d\n", __LINE__);
	
	printf("openglPtr->status = %d\n", openglPtr->status);
	
	if (openglPtr->status == 0)
	{
		printf("Line: %d\n", __LINE__);
		
		isWxWidgetsShowAnyImage = true;
	
		openglPtr->my_opengl_frame->Show(true);
		
		//openglPtr->my_opengl_frame->Show(false);
		
		openglPtr->status = 1;
	}
	else
	{
		printf("Status error! \n");
		return 3;
	}
	
	//wxTheApp->OnRun();
	
	/*
	//--- wxWidgets
	int testArgc = 0;
	char ** testArgv = NULL;
	wxEntryStart(testArgc, testArgv);

	wxTheApp->CallOnInit();

	wxTheApp->OnRun();

	wxTheApp->OnExit();

	wxEntryCleanup();
	*/
	return 1;
}

static int OpenGL_ShowWindow(int r7Sn, int functionSn) {

	CallBack_t *cbPtr;

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	R7_QueueWxEvent((R7CallbackHandler)r7_OpenGL_ShowWindow((void *)cbPtr));

	return 1;
}

static int r7_OpenGL_HideWindow(int r7Sn, int functionSn) {
	
	printf("OpenGL_HideWindow \n");

	int res;
	void *variableObject = NULL;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	
	printf("Line: %d\n", __LINE__);
	
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	printf("OpenGL_HideWindow: openglPtr->status = %d\n", openglPtr->status);
	
	if (openglPtr->status == 1)
	{
		printf("Line: %d\n", __LINE__);
		
		openglPtr->my_opengl_frame->Show(false);
		
		openglPtr->status = 0;
	}
	else
	{
		printf("Status error! \n");
		return 3;
	}
	//--- wxWidgets
	/*
	int testArgc = 0;
	char ** testArgv = NULL;
	wxEntryStart(testArgc, testArgv);

	wxTheApp->CallOnInit();

	wxTheApp->OnRun();

	wxTheApp->OnExit();

	wxEntryCleanup();
	*/
	return 1;
}

static int OpenGL_HideWindow(int r7Sn, int functionSn) {
	
	CallBack_t *cbPtr;

	cbPtr->r7Sn = r7Sn;

	cbPtr->functionSn = functionSn;

	R7_QueueWxEvent((R7CallbackHandler)r7_OpenGL_HideWindow((void *)cbPtr));

	return 1;
}

static int OpenGL_ShowImage(int r7Sn, int functionSn) {
	
	printf("OpenGL_ShowImage \n");
	
	int res;
	void *variableObject = NULL;
	Mat getImage;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	res = R7_GetVariableMat(r7Sn, functionSn, 2, &getImage);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! OpenGL_ShowImage get image = %d", res);
		return -2;
	}
	
	if (openglPtr->status == 1)
	{
		openglPtr->my_opengl_frame->img_panel->myImage = getImage.clone();
		
		openglPtr->my_opengl_frame->SetScrollbarFit();
		
		openglPtr->my_opengl_frame->SetClientSize(openglPtr->my_opengl_frame->img_panel->myImage.cols, openglPtr->my_opengl_frame->img_panel->myImage.rows);
		
		openglPtr->my_opengl_frame->img_panel->Refresh();
		
		openglPtr->status = 2;
	}
	else
	{
		printf("Status error! \n");
		return 3;
	}
	
	
	return 1;
}

static int OpenGL_GetImage(int r7Sn, int functionSn) {
	
	printf("OpenGL_GetImage \n");
	
	int res;
	void *variableObject = NULL;
	Mat getImage;
	
	res = R7_GetVariableObject(r7Sn, functionSn, 1, &variableObject);
	if (res <= 0) {
		R7_Printf(r7Sn, "ERROR! R7_GetVariableObject = %d", res);
		return -2;
	}
	OpenGL_t *openglPtr = ((OpenGL_t*)variableObject);
	
	if (openglPtr->status == 2)
	{
		isWxWidgetsInit = false;
		
		wxTheApp->OnRun();
		
		openglPtr->my_opengl_frame->OnScreenShot();
		
		res = R7_SetVariableMat(r7Sn, functionSn, 2, openglPtr->my_opengl_frame->image_screenshot);
		if (res <= 0) {
			R7_Printf(r7Sn, "ERROR! R7_SetVariableMat = %d", res);
			return -4;
		}
		
		wxTheApp->OnExit();

		wxEntryCleanup();
	}
	else
	{
		printf("Status error! \n");
		return 3;
	}
	
	return 1;
}

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
	/*
	int testArgc = 0;
	char ** testArgv = NULL;
	wxEntryStart(testArgc, testArgv);

	wxTheApp->CallOnInit();

	wxTheApp->OnRun();

	wxTheApp->OnExit();

	wxEntryCleanup();

	*/
	printf("ImageKelvin_Debug --\n");
	return 1;
}


R7_API int R7Library_Init(void) {
	// Register your functions in this API.
	
	R7_RegisterFunction("OpenGL_NewWindow", (R7Function_t)&OpenGL_NewWindow);
	R7_RegisterFunction("OpenGL_ShowWindow", (R7Function_t)&OpenGL_ShowWindow);
	R7_RegisterFunction("OpenGL_HideWindow", (R7Function_t)&OpenGL_HideWindow);
	R7_RegisterFunction("OpenGL_ShowImage", (R7Function_t)&OpenGL_ShowImage);
	R7_RegisterFunction("OpenGL_GetImage", (R7Function_t)&OpenGL_GetImage);
	
	R7_RegisterFunction("ImageKelvin_Debug", (R7Function_t)&ImageKelvin_Debug);
	
	
	// Callback function
	
	return 1;
}

R7_API int R7Library_Close(void) {
	// If you have something to do before close R7(ex: free memory), you should handle them in this API.
	
	
	if (false)
	{
		wxTheApp->OnRun();

		wxTheApp->OnExit();

		wxEntryCleanup();
	}

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
