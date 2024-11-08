# Sample for Input Schema:
```json
{ 
"Tid": "1",   
 "Did": "DeviceId",  
 "Fid": "1160",
 "Base_64": "",
 "C_threshold": 0.5,
 "Per": [],
 "Mtp":[
        {
	       "Etime": "2023-04-22,04:56:28.805 AM",
           "Src": "Grabber",
           "Stime": "2023-04-22,04:56:28.805 AM"
        },
        {
           "Etime": "",
           "Src": "Frame Processor",
           "Stime": "2023-04-22,04:56:41.860 AM"
         }
        ],
 "Ts": "",
 "Ts_ntp": "",
 "Inf_ver" :"",
 "Msg_ver": "",
 "Model": "Detecto",
 "Ad": "" ,
 "Ffp": "ffp",
 "Ltsize": "ltsize",
 "Lfp": "lfp",
 "I_fn": " ", 
 "Msk_img": [" "],                         
 "Rep_img": [" "],
  "Prompt":[""]
 } 
 ```

 ## Schema definition
| Key                       | Description                                   | 
|-------------------------------------|-------------------------------------|
| Tid    | Tenant ID              |
| Did    | Device ID              | 
| Fid   | Frame ID              | 
| Base_64    | Base-64 encoding string of an image              |
| C_threshold          | Confidence threshold value. Data type is float         |
| Per            | Previous frame's metadata                   |
| Mtp                            | Message Travel Path                             | 
| Ts    | Time Stamp              |
| Ts_ntp    | NTP Time Stamp              | 
| Inf_ver   | Infosys Version              | 
| Msg_ver   | Message Version              |
| Model         | Name of the Model that is being triggered          |
| Ad            | Additional Parameters                   |
| Ffp                            | First Frame passing                             |   
| Ltsize    | Lot size              |
| Lfp    | Last Frame Passing              | 
| Fs   | It contains all the predicted output of the model              | 
| Rc    | Response Code. Denotes success or failure code              |
| Rm          | Response Message. Denotes success or failure message         |
| I_fn            | Input file name                   |
| Msk_img                            | List of mask images                             |   
| Rep_img    | Replaces images as a list             |
| Prompt    | List of text prompts             | 
| Img_url   | List of urls of output images             | 
| Obase_64    | List of output images in base64 format              |
   


