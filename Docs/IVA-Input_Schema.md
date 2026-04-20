# Input Schema (Request)

## Sample
```json
{
  "Did": "DeviceId",
  "Fid": "1160",
  "Tid": "1",
  "Ts": "2023-04-22,04:56:28.805 AM",
  "Ts_ntp": "",
  "Msg_ver": "",
  "Inf_ver": "",
  "Model": "Detecto",
  "Base_64": ["<base64_encoded_image>"],
  "Roi_c": {
    "region_1": [
      [
        { "X": 100, "Y": 200 },
        { "X": 300, "Y": 200 },
        { "X": 300, "Y": 400 },
        { "X": 100, "Y": 400 }
      ]
    ]
  },
  "C_threshold": 0.5,
  "Mtp": [
    {
      "Stime": "2023-04-22,04:56:28.805 AM",
      "Etime": "2023-04-22,04:56:28.805 AM",
      "Src": "Grabber"
    }
  ],
  "Per": [
    {
      "Fid": "1159",
      "Fs": [
        {
          "Dm": { "X": "0.185", "Y": "0.033", "H": "0.938", "W": "0.179" },
          "Cs": "0.95",
          "Lb": "person",
          "Pid": "",
          "Uid": "",
          "Nobj": "",
          "Np": "",
          "Info": "",
          "Kp": null,
          "Tpc": null,
          "Bpc": null,
          "TaskType": "ObjectDetection"
        }
      ]
    }
  ],
  "Fs": [
    {
      "Dm": { "X": "0.185", "Y": "0.033", "H": "0.938", "W": "0.179" },
      "Cs": "0.95",
      "Lb": "person",
      "Pid": "",
      "Uid": "",
      "Nobj": "",
      "Np": "",
      "Info": "",
      "Kp": null,
      "Tpc": null,
      "Bpc": null,
      "TaskType": "ObjectDetection"
    }
  ],
  "Ad": "",
  "Ffp": "ffp",
  "Ltsize": "10",
  "Lfp": "lfp",
  "I_fn": "input_video.mp4",
  "Msk_img": [],
  "Rep_img": [],
  "Prompt": [["describe this image"]],
  "Text": "",
  "Hp": "",
  "Exp_api_ver": "",
  "Explainers_to_run": [],
  "Explainer_url": "",
  "FeedId": null
}
```

## Typed Schema

See [RequestSchema.jsonc](RequestSchema.jsonc) for the full typed schema with data types.

## Schema Definition

| Key | Type | Required | Description |
|-----|------|----------|-------------|
| Did | string | Yes | Device ID |
| Fid | string | Yes | Frame ID, unique for each frame |
| Tid | string | Yes | Tenant ID |
| Ts | string | Optional | Timestamp at which frame is sent for prediction |
| Ts_ntp | string | Optional | Timestamp in string format |
| Msg_ver | string | Optional | Message version |
| Inf_ver | string | Optional | Inference version |
| Model | string | Yes | Inference model being used for prediction |
| Base_64 | list[string] | Yes | List of base64 encoded images on which predictions should be done |
| Roi_c | object | Optional | Region of interest coordinates. Keys are region names, values are arrays of point arrays. Each point has X, Y, and optional R (radius for circular regions) |
| C_threshold | float | Yes | Confidence threshold for predictions, if applicable for the model |
| Mtp | list[object] | Yes | Message Travel Path — timing metadata with Stime, Etime (format: `yyyy-MM-dd,HH:mm:ss.fff tt`), and Src |
| Per | list[object] | Optional | Previous frame's metadata including Fid and Fs (prediction results) |
| Fs | list[object] | Optional | Array of previous prediction results (used in model chaining to forward earlier model outputs) |
| Ad | string | Optional | Additional details at frame level |
| Ffp | string | Yes | First frame flag |
| Ltsize | string | Yes | Frame to predict size |
| Lfp | string | Optional | Last frame flag |
| I_fn | string | Yes | Input file name |
| Msk_img | list[string] | Optional | List of mask image data in base64 format |
| Rep_img | list[string] | Optional | List of replace image data in base64 format |
| Prompt | list[list[string]] | Optional | Input prompts for the model, array of array of strings |
| Text | string | Optional | Text input for text to speech models |
| Hp | string | Optional | Hyper parameters as string for generative AI models |
| Exp_api_ver | string | Optional | Explainer API version |
| Explainers_to_run | list[string] | Optional | List of explainers to run for the current frame |
| Explainer_url | string | Optional | URL for calling explainer API |
| FeedId | string/null | Optional | Execution feed master ID |

### Fs / Per[].Fs Object Fields

| Key | Type | Description |
|-----|------|-------------|
| Dm | object | Bounding box coordinates — X, Y, H (height), W (width) |
| Cs | string | Confidence score of prediction |
| Lb | string | Label of prediction |
| Pid | string | Person ID if assigned |
| Uid | string | Unique ID if assigned |
| Nobj | string | Nested object |
| Np | string | New person flag |
| Info | string | Extra information at object level (string field) |
| Kp | dict or null | Key points for pose models — dict of int to list of floats |
| Tpc | list or null | Object pixel coordinates for segmentation |
| Bpc | list or null | Boundary pixel coordinates for segmentation |
| TaskType | string | Type of rendering task |

### Roi_c Point Object Fields

| Key | Type | Description |
|-----|------|-------------|
| X | int | X coordinate of the point |
| Y | int | Y coordinate of the point |
| R | int (optional) | Radius of the point if region is circular |