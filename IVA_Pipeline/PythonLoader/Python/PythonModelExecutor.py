import importlib
import json
import cv2
import base64
import numpy as np
from configparser import ConfigParser
import time
import datetime
from HeatMap import HeatMap

model_obj = None
config = ConfigParser()
config.read('C:\Configurations\config.ini')
heatmap_obj = HeatMap()

def getHeatMap(req):
    a = heatmap_obj.add(req)
    return a


def readb64(encoded_data):
    nparr = np.fromstring(base64.b64decode(encoded_data), np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    return img

def get_mtp(mtp, start_time, end_time, model_name):
    # code logic needs to be writted here
    start_1 = str(start_time.strftime("%Y-%m-%d,%I:%M:%S"))
    start_2 = str(start_time.strftime("%f"))[:3]
    start_3 = str(start_time.strftime("%p"))
    start_time = start_1 + "." + start_2 + " " + start_3

    end_1 = str(end_time.strftime("%Y-%m-%d,%I:%M:%S"))
    end_2 = str(end_time.strftime("%f"))[:3]
    end_3 = str(end_time.strftime("%p"))
    end_time = end_1 + "." + end_2 + " " + end_3

    print("end_time : ", end_time)
    print("type end_time : ", type(end_time))
    mtp_dict = {
        "Etime": end_time,
        "Src": model_name,
        "Stime": start_time
    }
    mtp.append(mtp_dict)
    return mtp

def executeModel(request):
    start_time = datetime.datetime.now(datetime.UTC)
    global model_obj
    function_call_start = time.time()
    # return_output=[]
    # req_data = json.loads(request, object_hook=lambda d: SimpleNamespace(**d))
    # req_data_batch = json.loads(request)
    req_data=json.loads(request)
    # for req_data in req_data_batch:
    modelName = req_data["Model"]
    # framework = config.get(modelName, 'framework')
    # modelPath = json.loads(config.get(modelName, 'modelPaths'))
    # className = None if config.get(modelName, 'classNames') == "None" else json.loads(
    #     config.get(modelName, 'classNames'))  # -> "value3"

    if model_obj is None:
        model_loader = getattr(importlib.import_module("python_model_loader"), modelName + "ModelLoader")
        model_obj = model_loader(config, modelName)
    finalResult= model_obj.predict(req_data)
        
    # p = modelOutput  # Storing results from model
    # if (p or p == []):
    finalResult["Did"] = req_data["Did"]
    # finalResult["Fid"]=str(round(time.time()*1000))
    finalResult["Fid"]=req_data["Fid"]
    finalResult["Ffp"] = req_data["Ffp"]
    finalResult["Lfp"] = req_data["Lfp"]
    finalResult["Ltsize"] = req_data["Ltsize"]
    # finalResult["Fs"] = p
    # finalResult["Mtp"] = [x for x in req_data["Mtp"]]
    finalResult["Tid"] = req_data["Tid"]
    finalResult["Ts"] = req_data["Ts"]
    finalResult["Ts_ntp"] = req_data["Ts_ntp"]
    finalResult["Msg_ver"] = req_data["Msg_ver"]
    finalResult["Inf_ver"] = req_data["Inf_ver"]
    finalResult["Rc"] = 200
    finalResult["Rm"] = "Success"
    end_time = datetime.datetime.now(datetime.UTC)
    mtp = get_mtp(req_data["Mtp"], start_time, end_time, modelName)
    finalResult["Mtp"] = mtp

    # finalResult["Ad"] = ad
    # print("Model Response: ", json.dumps(finalResult, default=lambda obj: obj.__dict__))
    print("Time taken for one method call: ", time.time() - function_call_start)
    # return_output.append(finalResult)
    return json.dumps(finalResult)


if __name__ == '__main__':
    pass
