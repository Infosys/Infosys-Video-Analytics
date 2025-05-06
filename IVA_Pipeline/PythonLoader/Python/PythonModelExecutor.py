'''
Created on March 16, 2023

@author: Dr Nirbhay Mathur
'''
import json
import cv2
from PIL.Image import Image
from PIL import Image
import base64
from flask import Flask
from flask import jsonify, request
from pickle import NONE
import numpy as np
from types import SimpleNamespace
from configparser import ConfigParser
from modelinference import Request  # whl file for model inference
from yoloinference import  Yolo5,Yolo8     # whl file for yolo inference
from tensorflowinference import  Tensor, TFObjectDetection  # whl file for tensor inference
from custominference import  Custom  # whl file for custom
#from safecountinference import Safecount #whl file for sfecount
from inceptionv3inference import Inception 
from mplugimginference import Mplug

app = Flask("IVA Inference APIs", static_folder='../static')
config = ConfigParser()
config.read('config.ini')

def readb64(encoded_data):       
    nparr = np.fromstring(base64.b64decode(encoded_data), np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    return img
    
def preloadModel(request):
    print("preloadModel {}",request.toJSON )

def unloadModel(request):
    print("unloadModel{}",request.toJSON)

@app.route('/api/<framework>/<category>/<model>/<version>', methods=['GET','POST'])
def executeModelRestAPI(framework,category,model,version):
    req_data = request.get_json(True, False, False)   
    base64_image = req_data["Base_64"]
    if base64_image is None:
        print("No image found in the request JSON. Please check your input once.")
    a = executeModel(Request(Did=req_data["Did"],Fid=req_data["Fid"],Ffp=req_data["Ffp"],Lfp=req_data["Lfp"],Ltsize=req_data["Ltsize"],Mtp=req_data["Mtp"],Tid=req_data["Tid"],Ts=req_data["Ts"],Ts_ntp=req_data["Ts_ntp"],Msg_ver=req_data["Msg_ver"],Inf_ver=req_data["Inf_ver"],Base_64=req_data["Base_64"],C_threshold=req_data["C_threshold"],Model=req_data["Model"],Per=req_data["Per"],Ad=req_data["Ad"]).toJSON())
    return a

def executeModel(request):
    #req_data = json.loads(request, object_hook=lambda d: SimpleNamespace(**d))
    req_data = json.loads(request)
    modelName=req_data["Model"]
    framework =config.get(modelName, 'framework')
    modelPath =json.loads(config.get(modelName, 'modelPaths'))
    className= None if config.get(modelName, 'classNames')== "None" else json.loads(config.get(modelName, 'classNames')) # -> "value3"
    if framework=='TensorFlow':
        #print("I am working on {} model...".format(modelName))          
        reqData ={}
        reqData["modelPath"]=modelPath        
        reqData["Model"]=req_data["Model"]
        reqData["classNames"]=className
        reqData["C_threshold"]=req_data["C_threshold"]
        reqData["Base_64"]=req_data["Base_64"]
        with open(reqData["classNames"][0]) as f:
            labels = [l.strip() for l in f.readlines()]
            od_model = TFObjectDetection(reqData["modelPath"][0], labels)
            tensor=Tensor()
            modelOutput = tensor.executeModel(reqData, od_model);
    elif framework=="Yolov5":           # framework for Yolov5
        print("I am working on {} model...".format(modelName))
        reqData ={}
        reqData["modelPaths"]=modelPath
        reqData["classNames"]=className
        reqData["Model"]=req_data["Model"]
        reqData["C_threshold"]=req_data["C_threshold"]
        reqData["Base_64"]=req_data["Base_64"]
        yolo=Yolo5()
        modelOutput = yolo.executeModel(reqData);
    elif framework=="Yolov8":        # framework for Yolov8
        print("I am working on {} model...".format(modelName))
        reqData ={}
        reqData["modelPaths"]=modelPath
        reqData["classNames"]=className
        reqData["Model"]=req_data["Model"]
        reqData["C_threshold"]=req_data["C_threshold"]
        reqData["Base_64"]=req_data["Base_64"]
        yolo=Yolo8()
        modelOutput = yolo.executeModel(reqData);        
    elif framework =="Custom":          # framework for custom
        print("I am working on {} model...".format(modelName))
        IMG_SIZE = 64
        img1 = []
        img2 = []
        class1 = []
        img_1 = Image.open("C:/Users/nirbhay.mathur/Desktop/CustomMOdel/1.png")
        img_1 = img_1.convert('RGB')
        img_1 = img_1.resize((IMG_SIZE, IMG_SIZE))
        img1.append(np.array(img_1)/255)
        img_2 = Image.open("C:/Users/nirbhay.mathur/Desktop/CustomMOdel/1.png")
        img_2 = img_2.convert('RGB')
        img_2 = img_2.resize((IMG_SIZE, IMG_SIZE))
        img2.append(np.array(img_2)/255)
        custom=Custom()
        modelOutput = custom.executeModel(req_data, img1, img2)
    elif framework =="InceptionV3":
        print("I am working on {} model...".format(modelName))
        reqData ={}
        inception=Inception()            
        reqData["modelPaths"]=modelPath
        reqData["framework"]=framework
        reqData["C_threshold"]=req_data["C_threshold"]
        reqData["Base_64"]=req_data["Base_64"]
        modelOutput = inception.executeModel(reqData);
    elif framework =="Mplug":
        print("I am working on {} model...".format(modelName))
        reqData ={}
        reqData["modelPaths"]=modelPath
        reqData["Model"]=req_data["Model"]
        reqData["C_threshold"]=req_data["C_threshold"]
        reqData["Base_64"]=req_data["Base_64"]
        imgCaption_model=Mplug()
        modelOutput = imgCaption_model.executeModel(reqData);     
    
    p = modelOutput  #Storing results from model
    finalResult = {}
    if(p or p==[]):
        finalResult["Did"]=req_data["Did"]
        finalResult["Fid"]=req_data["Fid"]
        finalResult["Ffp"]=req_data["Ffp"]
        finalResult["Lfp"]=req_data["Lfp"]
        finalResult["Ltsize"]=req_data["Ltsize"]
        finalResult["Fs"]=p
        finalResult["Mtp"]=[x for x in req_data["Mtp"]]
        finalResult["Tid"]=req_data["Tid"]
        finalResult["Ts"]=req_data["Ts"]
        finalResult["Ts_ntp"]=req_data["Ts_ntp"]
        finalResult["Msg_ver"]=req_data["Msg_ver"]
        finalResult["Inf_ver"]=req_data["Inf_ver"]
        finalResult["RC"]=200    
        finalResult["RM"]="Success"
        finalResult["Ad"]=req_data["Ad"]
        #print("Model Response: ", json.dumps(finalResult, default=lambda obj: obj.__dict__))
        return json.dumps(finalResult, default=lambda obj: obj.__dict__)
    
if __name__ == '__main__':
    
    app.run(host="localhost", port=8080, debug=True)

    
