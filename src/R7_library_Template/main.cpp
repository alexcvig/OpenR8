#include <r7.hpp>




//Export from D:\wx_test.r6
int main(void) {

    SetConsoleOutputCP(65001);
    setlocale(LC_ALL, "en_US.UTF-8");

    int res;
    int r7h;

    res = R7_InitLib();
    if (res <= 0) {
        return 1;
    }

    r7h = R7_New();


    // Variables

    R7_AddVariable(r7h, u8"Enable", "bool", u8"1");

    R7_AddVariable(r7h, u8"Image_テスト_utf8測試", "image", u8"");

    R7_AddVariable(r7h, u8"ImageBinarize", "image", u8"");

    R7_AddVariable(r7h, u8"ImageFileName", "string", u8"テスト.png");

    R7_AddVariable(r7h, u8"IsHide", "bool", u8"1");

    R7_AddVariable(r7h, u8"Threshold", "int", u8"100");


    // Functions

    R7_AddFunction(r7h, u8"R7_EnableWxWidgets");

    R7_AddFunction(r7h, u8"Image_Open", u8"ImageFileName", u8"Image_テスト_utf8測試");

    R7_AddFunction(r7h, u8"Debug_Image", u8"Image_テスト_utf8測試");

    R7_AddFunction(r7h, u8"Image_Binarize", u8"Image_テスト_utf8測試", u8"Threshold", u8"ImageBinarize");

    R7_AddFunction(r7h, u8"Debug_Image", u8"ImageBinarize");


    // Run

    R7_Run(r7h, u8"D:\\", 1);

    R7_Release(r7h);

    getchar();

    return 0;
}

