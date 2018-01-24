#include <r7.hpp>




//Export from C:\Users\leo\Documents\GitHub\OpenR8\workspace\FindBlob\FindBlob.r6
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

    R7_AddVariable(r7h, u8"flie_name", "string", u8"sample.png");

    R7_AddVariable(r7h, u8"SampleImage", "image", u8"");

    R7_AddVariable(r7h, u8"gray_value", "int", u8"110");

    R7_AddVariable(r7h, u8"Binarize_Image", "image", u8"");

    R7_AddVariable(r7h, u8"Output_File_Name", "string", u8"output.png");

    R7_AddVariable(r7h, u8"True", "bool", u8"1");

    R7_AddVariable(r7h, u8"MinArea", "int", u8"1000");

    R7_AddVariable(r7h, u8"Output_Json", "json", u8"");

    R7_AddVariable(r7h, u8"Output_Image", "image", u8"");

    R7_AddVariable(r7h, u8"Type", "string", u8"Defect");


    // Functions

    R7_AddFunction(r7h, u8"Image_Open", u8"flie_name", u8"SampleImage");

    R7_AddFunction(r7h, u8"Debug_Image", u8"SampleImage");

    R7_AddFunction(r7h, u8"Image_Binarize", u8"SampleImage", u8"gray_value", u8"Binarize_Image");

    R7_AddFunction(r7h, u8"Debug_Image", u8"Binarize_Image");

    R7_AddFunction(r7h, u8"Image_FindBlob", u8"Binarize_Image", NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, u8"True", u8"MinArea", NULL, NULL, NULL, NULL, NULL, NULL, u8"Output_Json", u8"SampleImage", u8"Output_Image", u8"Type");

    R7_AddFunction(r7h, u8"Json_Print", u8"Output_Json");

    R7_AddFunction(r7h, u8"Debug_Image", u8"Output_Image");

    R7_AddFunction(r7h, u8"Image_Save", u8"Output_Image", u8"Output_File_Name");


    // Run

    R7_Run(r7h, u8"C:\\Users\\leo\\Documents\\GitHub\\OpenR8\\workspace\\FindBlob\\", 1);

    R7_Release(r7h);

    getchar();

    return 0;
}

