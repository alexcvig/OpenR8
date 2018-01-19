#ifndef LIBRARY_H_PP
#define LIBRARY_H_PP

using namespace cv;

class MyFrame;



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
class TestGLCanvas : public wxGLCanvas
{
public:
	TestGLCanvas(wxWindow *parent, int *attribList = NULL, MyFrame *frame = NULL);
	void setX(int x) { TestGLCanvas::x = x; printf("scroll_x = %d\n", x);}
	void setY(int y) { TestGLCanvas::y = y; printf("scroll_y = %d\n", y);}
	void paintEvent(wxPaintEvent& evt);
	void OnMouseMotionPanel(wxMouseEvent& event);
	void OnMouseDown(wxMouseEvent& event);
	void OnMouseUp(wxMouseEvent& event);

	TestGLContext& GetContext(wxGLCanvas *canvas);
	
	MyFrame *my_frame;
	Mat myImage;

	bool dragging = false, rectangle = false;
	bool isEnableCrop = false;
	int mouse_x, mouse_y;
	int rec_x = 1, rec_y = 1;		//--- mouse dragging rectangle x and y
	

private:
	void OnPaint(wxPaintEvent& event);
	void Spin(float xSpin, float ySpin);
	//void OnKeyDown(wxKeyEvent& event);
	void OnSpinTimer(wxTimerEvent& WXUNUSED(event));
	TestGLContext *m_glContext;
	
	int x=0, y=0;		//--- shift
	// angles of rotation around x- and y- axis
	float m_xangle,
		m_yangle;

	wxTimer m_spinTimer;
	bool m_useStereo,
		m_stereoWarningAlreadyDisplayed;

	wxDECLARE_EVENT_TABLE();
};

#endif
