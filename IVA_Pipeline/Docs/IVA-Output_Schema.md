# Sample for Output Schema:
```json
{
"Tid":"1",
"Did":"DeviceId", 
"Fid":"1160",
"Fs":[
	{
	"Cs":1.0,
	"Lb":"person",
	"Dm":{"X":0.18560473,"Y":0.03351725,"H":0.93880859,"W":0.17927151},
	"Nobj":"",
	"Uid":"",
	"Info":"{}",
	"Kp":{}
	}
	],
"Mtp":[
	{"Etime":"08-02-2023,02:11:33.513PM","Src":"grabber","Stime":"08-02-2023,02:11:22.744PM"},
	{"Etime":"08-02-2023,02:11:33.513PM","Src":"predictor","Stime":"08-02-2023,02:11:22.744PM"},
	{"Etime":"2023-05-04,01:25:58.774PM","Src":"Detecto","Stime":"2023-05-04,01:25:56.974PM"}
	],
"Ts":"",
"Ts_ntp":"",
"Msg_ver":"",
"Inf_ver":"",
"Rc":"200",
"Rm":"Success",
"Ad":"",
"Lfp":"lfp",
"Ffp":"ffp",
"Ltsize":"ltsize",
"Obase_64": [""],                       
"Img_url": [""]
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
   
