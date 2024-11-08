import tensorflow as tf
import requests
import json
import ast
import time
import os
import base64
import numpy as np
from PIL import Image
import os
import io
import configparser
from object_detection import ObjectDetection
from datetime import datetime
from flask import jsonify
from HeatMap import HeatMap

model_filename = 'saved_model.pb'
labels_filename = 'labels.txt'
filter_threshold =0.4
x=[]
y=[]
#filter_threshold = config['Prediction']['minimumFilterThreshold']
OUTPUT_TENSOR_NAMES = ['detected_boxes', 'detected_scores', 'detected_classes']
#model_filename= config['ModelDetails']['azure_modelFile_i6']
#labels_filename= config['ModelDetails']['azure_labels_File_i6']
#MODEL_FILENAME_NEW = config['ModelDetails']['azure_modelFile_tflite_t5']


#sess.close()
sess = tf.compat.v1.InteractiveSession()
model = tf.saved_model.load(os.path.dirname(model_filename))
serve = model.signatures['serving_default']
input_shape = serve.inputs[0].shape[1:3]

#print(input_shape)
class TFObjectDetection(ObjectDetection):
    """Object Detection class for TensorFlow SavedModel"""

    def __init__(self, model_filename, labels):
        super(TFObjectDetection, self).__init__(labels)
        model = tf.saved_model.load(os.path.dirname(model_filename))
        self.serve = model.signatures['serving_default']

    def predict(self, preprocessed_image):
        inputs = np.array(preprocessed_image, dtype=np.float32)[np.newaxis, :, :, (2, 1, 0)]  # RGB -> BGR
        inputs = tf.convert_to_tensor(inputs)
        outputs = self.serve(inputs)
        return np.array(outputs['outputs'][0])

class TFLiteObjectDetection(ObjectDetection):
    """Object Detection class for TensorFlow Lite"""
    def __init__(self, model_filename, labels):
        super(TFLiteObjectDetection, self).__init__(labels)
        self.interpreter = tf.lite.Interpreter(model_path=model_filename)
        self.interpreter.allocate_tensors()
        self.input_index = self.interpreter.get_input_details()[0]['index']
        self.output_index = self.interpreter.get_output_details()[0]['index']

    def predict(self, preprocessed_image):
        inputs = np.array(preprocessed_image, dtype=np.float32)[np.newaxis, :, :, (2, 1, 0)]  # RGB -> BGR and add 1 dimension.

        # Resize input tensor and re-allocate the tensors.
        self.interpreter.resize_tensor_input(self.input_index, inputs.shape)
        self.interpreter.allocate_tensors()
        
        self.interpreter.set_tensor(self.input_index, inputs)
        self.interpreter.invoke()
        return self.interpreter.get_tensor(self.output_index)[0]

with open(labels_filename) as f:
        labels = [l.strip() for l in f.readlines()]
od_model = TFObjectDetection(model_filename, labels)

def getDetectMask_t5(req):
    l=[]
    req_data = json.loads(req)
    #base64_image=req_data['base64_image']
    #confidence_threshold=float(req_data['confidence_threshold'])
    base64_image=req_data['Base_64']
    confidence_threshold=float(req_data['C_threshold'])
    #print('getDetectMask_t5')
    #print(type(base64_image))
    #print(base64_image)
    
    #api_latency = open(r"C:/WorkArea/Services/Python/logs/api_latency.txt", "a")
    
    curr_time = datetime.now().time() # time object
    #api_latency.write("Before Pre-processing:"+ str(curr_time))
    #api_latency.write("\n")
    
    image = Image.open(io.BytesIO(base64.b64decode(base64_image)))
    #image.show()
    image = image.convert('RGB') if image.mode != 'RGB' else image
    ## Added by Ananth 
    #api_latency.write("input shape:"+ str(input_shape))
    #api_latency.write("\n")
    #input_shape1 = (416, 416)
    image = image.resize(input_shape) 
    width, height = image.size
    #print(width)

    #with open(labels_filename) as f:
    #    labels = [l.strip() for l in f.readlines()]
            
    curr_time = datetime.now().time() # time object
    #api_latency.write("After Pre-processing:"+ str(curr_time))
    #api_latency.write("\n")
    
    #od_model = TFObjectDetection(model_filename, labels)
    #od_model_new = TFLiteObjectDetection(MODEL_FILENAME_NEW, labels)
    curr_time = datetime.now().time()
    #api_latency.write("After Model Loading:"+ str(curr_time))
    #api_latency.write("\n")
    predictions = od_model.predict_image(image, confidence_threshold)
    #predictions = od_model_new.predict_image(image, confidence_threshold)
        
    curr_time = datetime.now().time() # time object
    #api_latency.write("After Model Execution:"+ str(curr_time))
    #api_latency.write("\n")
    
    standardized_predictions = []
    for i in predictions:
        j = {}
        X = float(i['Dm']['X'])
        Y = float(i['Dm']['Y'])
        H = float(i['Dm']['H'])
        W = float(i['Dm']['W'])
        j['Cs'] = i['Cs']
        j['Lb'] = i['Lb']
        j['Dm'] = {'X':X,
                   'Y':Y,
                   'H':W,
                   'W':H
                    }
        standardized_predictions.append(j)
        
    curr_time = datetime.now().time() # time object
    #api_latency.write("After Post-processing:"+ str(curr_time))
    #api_latency.write("\n")
    #api_latency.close()
    
    return standardized_predictions

def getDetectMask_t5Mock(req):
    p = getDetectMask_t5(req)
    if(p or p==[]):
        result = {"Objects" : p,"RC":200,"RM":"Success"}
        json_string = json.dumps(result)
        return json_string
heatmap_obj=HeatMap()
def getHeatMap(req):
    #heatmap_obj=HeatMap()
  #  req_data = json.loads(req)
    a=heatmap_obj.add(req)
    #json_string = json.dumps(req)
    return a

 

