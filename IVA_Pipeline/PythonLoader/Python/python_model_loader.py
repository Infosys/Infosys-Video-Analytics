import json
import base64
from io import BytesIO


class DetectoModelLoader:
    def __init__(self,config,model_name):
        from detectoinference import Detecto
        model_path=None if config.get(model_name,'modelPaths').lower()=='none' \
            else json.loads(config.get(model_name,'modelPaths'))
        self.model_obj=Detecto(model_path if model_path is None else model_path[0])

    def predict(self,req_data):
        return_dict={}
        ad=req_data.get("Ad")
        modelOutput=self.model_obj.executeModel(req_data["Base_64"],req_data["C_threshold"])
        return_dict["Fs"]=modelOutput
        return_dict["Ad"]=ad
        return return_dict


class FireSmokeClassifierResnetModelLoader:
    def __init__(self,config,model_name):
        from firesmokeclassifierresnetinference import FireSmokeClassifierResnet
        model_path=None if config.get(model_name,'modelPaths').lower()=='none' \
            else json.loads(config.get(model_name,'modelPaths'))
        device=config.get(model_name,'device')
        self.model_obj=FireSmokeClassifierResnet(model_path=model_path if model_path is None else model_path[0],map_location=device)

    def predict(self,req_data):
        return_dict={}
        ad=req_data.get("Ad")
        modelOutput=self.model_obj.executeModel(req_data["Base_64"],req_data["C_threshold"])
        return_dict["Fs"]=modelOutput
        return_dict["Ad"]=ad
        return return_dict


class CustomModelLoader:
    def __init__(self, config, model_name):
        from custommodelinference import CustomModel
        model_path = None if config.get(model_name, 'modelPaths').lower() == 'none' \
            else json.loads(config.get(model_name, 'modelPaths'))
        device = config.get(model_name, 'device')
        self.model_obj = CustomModel(model_path=model_path, device=device)

    def predict(self, req_data):
        ad = req_data.get("Ad")
        modelOutput = self.model_obj.executeModel(req_data["Base_64"], req_data["C_threshold"])
        return modelOutput, ad

    def preprocessing(self, req_data):
        pass

    def postprocessing(self, req_data):
        pass


class BaseModelLoader:
    def __init__(self, config, model_name):
        pass

    def preprocessing(self, req_data):
        pass

    def postprocessing(self, req_data):
        pass

    def predict(self, req_data):
        pass


class Yolov5ModelLoader:
    def __init__(self, config, model_name):
        import Yolo5
        self.model_obj = Yolo5()

    def preprocessing(self, req_data):
        pass

    def postprocessing(self, req_data):
        pass

    def predict(self, req_data):
        modelOutput = self.model_obj.executeModel(req_data)
        return modelOutput


class PyTorchModelLoader:
    def __init__(self, config, model_name):
        import torch
        self.torch = torch
        self.model_path = model_path
        self.device = device

    def load_model(self):
        model = self.torch.load(self.model_path, map_location=self.device)
        return model

    def load_state_dict(self, model):
        state_dict = self.torch.load(self.model_path, map_location=self.device)
        model.load_state_dict(state_dict)
        return model


class TensorflowModelLoader:
    def __init__(self, model_path):
        import tensorflow as tf
        self.tf = tf
        self.model_path = model_path

    def load_model(self):
        return self.tf.keras.models.load_model(self.model_path)

    def load(self):
        return self.tf.saved_model.load(self.model_path)


class DetectMask_t5_OfflineModelLoader:
    def __init__(self,config,model_name):
        from tensorflowinference import  Tensor,TFObjectDetection  
        with open(json.loads(config.get(model_name,"classNames"))[0]) as f:
            labels=[l.strip() for l in f.readlines()]
            self.od_model=TFObjectDetection(json.loads(config.get(model_name,"modelPaths"))[0],labels)
            self.model_obj=Tensor()

    def predict(self,req_data):
        return_dict={}
        ad=req_data.get("Ad")
        modelOutput=self.model_obj.executeModel(req_data,self.od_model)
        return_dict["Fs"]=modelOutput
        return_dict["Ad"]=ad
        return return_dict


class PointNetBatch_OfflineModelLoader:
    def __init__(self,config,model_name):
        from pointnetinference import PointNetClassifier
        import matplotlib.pyplot as plt  
        self.plt=plt
        self.model_path=json.loads(config.get(model_name,"modelPaths"))[0]
        self.num_points=int(json.loads(config.get(model_name,"numPoints")))
        self.k=int(json.loads(config.get(model_name,"k")))
        self.model_obj=PointNetClassifier(model_path=self.model_path,num_points=self.num_points,k=self.k)

    def predict(self,req_data):
        return_dict={}
        ad=req_data.get("Ad")
        decoded_bytes=base64.b64decode(req_data["Base_64"])
        decoded_str=decoded_bytes.decode('utf-8')
        point_cloud=[list(map(float,line.split())) for line in decoded_str.split('\n') if line.strip()]
        x=[point[0] for point in point_cloud]
        y=[point[1] for point in point_cloud]
        modelOutput=self.model_obj.predict(point_cloud,req_data["C_threshold"])
        self.plt.figure(figsize=(8,6))
        self.plt.scatter(x, y, color='blue')
        buffer=BytesIO()
        self.plt.savefig(buffer,format='jpg')
        buffer.seek(0)
        image_base64=base64.b64encode(buffer.read()).decode('utf-8')
        self.plt.close()
        return_dict["Fs"]=modelOutput
        return_dict["Ad"]=ad
        return_dict["Obase_64"]=[image_base64]
        return return_dict
