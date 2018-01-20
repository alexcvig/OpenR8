Requirement

運行 tensorflow library 需要以下項目：
1. python (建議 3.5.2 版本)
2. tensorflow 的 python 版(有安裝 python 的話可以用 pip install tensorflow 指令安裝)
3. OpenCV 的 python 版 (有安裝 python 的話可以用 pip install opencv-python 指令安裝)
4. tensorflow object_detection 模組安裝，詳細安裝方法可參照以下網頁
https://github.com/tensorflow/models/blob/master/research/object_detection/g3doc/installation.md
該模組還需要
pip install pillow
pip install lxml
pip install jupyter
pip install matplotlib
安裝後要設定環境變數
export PYTHONPATH=$PYTHONPATH:`pwd`:`pwd`/slim :
環境變數增加 PYTHONPATH，設定 tensorflow\models 的路徑，與tensorflow\models\slim 的路徑
(例如 F:\Downloads\models-master;F:\Downloads\models-master\slim)


以上用 pip 安裝時， cmd 需要有管理權限(滑鼠右鍵點 cmd ，選擇[以管理員身分執行])