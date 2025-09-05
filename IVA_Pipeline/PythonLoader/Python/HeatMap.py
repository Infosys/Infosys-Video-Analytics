import numpy as np
import math
import argparse
import datetime
import random
import time
from pathlib import Path
import torch
import torchvision.transforms as standard_transforms
import pandas as pd
from PIL import Image
from scipy import stats
import matplotlib.pyplot as plt
import seaborn as sns
from matplotlib.backends.backend_agg import FigureCanvasAgg as FigureCanvas
import math
import json
import base64
import io
import cv2
class HeatMap:
    def add(self, req):
        x=[] 
        y=[]
        ret=json.loads(req)
        x=ret['x']
        y=ret['y']
        base64_image=ret['Base_64'] 
        decoded_data=base64.b64decode((base64_image))
        image = Image.open(io.BytesIO(base64.b64decode(base64_image)))
        #image = image.convert('RGB') if image.mode != 'RGB' else image
        width, height = image.size
        img_rgb = cv2.cvtColor(np.array(image), cv2.COLOR_RGB2BGR)
        #img_file = open('image.JPEG', 'wb')        
        #img_file.write(decoded_data)
        #height=img_file.shape[0]
        #width=img_file.shape[1]
        #images = np.array(Image.open('image.JPEG'))
        #height=images.shape[0]
        #width=images.shape[1]
        #height=360
        #width=640
        #img_file.close()
        fig = plt.figure(frameon=False)   
        #plt.axis('off')
        #txt_path = "C:\\Users\\yoges.govindaraj\\Desktop\\coords.txt"
        #coords = pd.read_csv(txt_path)
       #df = pd.DataFrame(coords, columns =['x_coords', 'y_coords'])
        #ax = sns.kdeplot(data = coords, x="x_coords", y="y_coords", fill=True,cmap="Reds") #rocket

        ax = sns.kdeplot(x=x, y=y, fill=True,cmap="Reds") #rocket

        #sns.despine(bottom = True, left = True)
        
        sns.despine(left=True, bottom=True)
        ax.set(yticklabels=[])  
        ax.set(xticklabels=[])

        #ax.set(title='Penguins: Body Mass by Species for Gender')
        
        ax.set(ylabel=None)  # remove the y-axis label
        ax.set(xlabel=None)
        ax.tick_params(bottom=False)  # remove the ticks
        ax.tick_params(left=False)
        
        ax.tick_params(left=False)  # remove the ticks
        ax.axis('off')
        #ax.set(ylabel=None)
        
        #sns.despine(bottom = True, left = True)
        #fig = plt.figure(frameon=False)   
       # plt.axis('off')
        canvas = FigureCanvas(fig)

        canvas.draw()
        # convert canvas to image
        graph_image = np.array(fig.canvas.get_renderer()._renderer)
        # it still is rgb, convert to opencv's default bgr
        graph_image = cv2.cvtColor(graph_image,cv2.COLOR_RGB2BGR)
        image_resize = cv2.resize(graph_image,(width,height))
        #image_resize = cv2.resize(image_resize, (width,height))
        
        #print(image_resize.shape)
        #status=cv2.imwrite("C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\sampleplot.jpg",image_resize)
        #status=cv2.imwrite("C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\samplergb.jpg",img_rgb)
        #print("Image written to file-system : ",status)
        #images = np.array(Image.open('image.JPEG'))
        #img_rgb = cv2.cvtColor(images, cv2.COLOR_BGR2RGB)
        #images1 = np.array(Image.open('C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\sample1.jpg'))



        dst = cv2.addWeighted(img_rgb, 0.7, image_resize, 0.3, 0);
        #plt.imshow(dst)
      #  final_data = json.loads(dst)
        frame = cv2.imencode(".JPEG", dst)
        encoded_string = base64.b64encode(frame[1])
        b64_string = encoded_string.decode("utf-8")
        #b64_bytes=base64.b64encode(dst)
        #b64_string = Base_64.decode()
        #base64path='C:/Users/yoges.govindaraj/Desktop/base64.txt'
        #with open(base64path, "w") as text_file:
        #    text_file.write(b64_string)
        rval=b64_string
        #cv2.addWeighted(img,0.5,images,0.4,0)
        #image_rgb1 = cv2.cvtColor(dst, cv2.COLOR_BGR2RGB)
        #status1=cv2.imwrite("C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\sample2.jpg",image_rgb1)
        #status1=cv2.imwrite("C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\dst.jpg",dst)
        #print("Image written to file-system : ",status1)

        #image_resize = cv2.resize(image_resize, (width,height))[41:319,80:560]
        #status3=cv2.imwrite("C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\sampleimageresize.jpg",image_resize)
        #print("Image written to file-system : ",status3)
        ##dst = cv2.addWeighted(img_file,0.5,image_resize,0.4,0)
        #img = cv2.imread("C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\sample1.jpg")
        ##gray = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        #images = np.array(Image.open('image.jpeg'))
        #image_rgb = cv2.cvtColor(images, cv2.COLOR_BGR2RGB)
        #status1=cv2.imwrite("C:\\Users\\yoges.govindaraj\\IVA\\RenderImage\\sample2.jpg",image_rgb)
        #print("Image written to file-system : ",status1)
  
        #image_resize = cv2.resize(image_resize, (width,height))
        #plt.show()

        #ax = sns.kdeplot(data = coords, x="x_coords", y="y_coords", fill=True, thresh=0, levels=100, cmap="mako")
        #plt.show()
        #fig = ax.get_figure()
        #fig.savefig('kde.png', transparent=True, bbox_inches='tight', pad_inches=0)
        #rval=base64.b64encode(ax.read())
        #img1 = img_file #.imread(img_file)
        # Get the dimensions of the original image
        #x_dim, y_dim, z_dim = np.shape(img1)
        # Create heatmap
        #heatmap = np.zeros((x_dim, y_dim), dtype=float)
        #x1 = np.int(x)
        #y1= np.int(y)
        #heatmap[x1,y1] = p
        # Plot images
        #fig, axes = plt.subplots(1, 2, figsize=(8, 4))
        #ax = axes.ravel()
        #ax[0].imshow(img)
        #ax[0].set_title("Original")
        #fig.colorbar(ax[0].imshow(img1), ax=ax[0])

        #ax[1].imshow(img_file, vmin=0, vmax=1)
        #ax[1].imshow(heatmap, alpha=.5, cmap='jet')
        #ax[1].set_title("Original + heatmap")

# Specific colorbar
        #norm = mpl.colors.Normalize(vmin=0,vmax=2)
        #N = 11
        #cmap = plt.get_cmap('jet',N)
        #sm = plt.cm.ScalarMappable(cmap=cmap, norm=norm)
        #sm.set_array([])
        #plt.colorbar(sm, ticks=np.linspace(0,1,N), 
        #     boundaries=np.arange(0,1.1,0.1)) 

        #fig.tight_layout()
        #plt.show()
        #points=[[x[0],1080-point[1]] for point in points]
         #df = pd.DataFrame(points, columns =['x_coords', 'y_coords'])
        #x = np.random.rayleigh(50, size=5000)
        #y = np.random.rayleigh(50, size=5000)

        #plt.hist2d(x,y, bins=[np.arange(0,400,5),np.arange(0,300,5)])

        #plt.show()
        return rval
