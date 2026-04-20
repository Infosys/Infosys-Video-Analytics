# Output Schema (Response)

## Sample
```json
{
  "Did": "DeviceId",
  "Fid": "1160",
  "Tid": "1",
  "Ts": "2023-05-04,01:25:58.774 PM",
  "Ts_ntp": "",
  "Msg_ver": "",
  "Inf_ver": "",
  "Ad": "",
  "Fs": [
    {
      "Dm": { "X": "0.18560473", "Y": "0.03351725", "H": "0.93880859", "W": "0.17927151" },
      "Pid": "",
      "Np": "",
      "Cs": "1.0",
      "Lb": "person",
      "Uid": "",
      "Nobj": "",
      "Info": "{}",
      "Kp": {
        "0": [0.0, 0.0, 0.0]
      },
      "Tpc": [
        [0.0, 0.0]
      ],
      "Bpc": [
        [0.0, 0.0]
      ],
      "TaskType": "ObjectDetection"
    }
  ],
  "Mtp": [
    { "Stime": "2023-05-04,01:25:56.974 PM", "Etime": "2023-05-04,01:25:58.774 PM", "Src": "Detecto" }
  ],
  "Ffp": "ffp",
  "Ltsize": "10",
  "Lfp": "lfp",
  "I_fn": "input_video.mp4",
  "Rc": "200",
  "Rm": "Success",
  "Obase_64": [],
  "Img_url": [],
  "Prompt": [["describe this image"]],
  "Hp": ""
}
```

## Typed Schema

See [ResponseSchema.jsonc](ResponseSchema.jsonc) for the full typed schema with data types.

## Schema Definition

| Key | Type | Required | Description |
|-----|------|----------|-------------|
| Did | string | Yes | Device ID |
| Fid | string | Yes | Frame ID, unique for each frame |
| Tid | string | Yes | Tenant ID |
| Ts | string | Optional | Timestamp at which prediction is done |
| Ts_ntp | string | Optional | Timestamp in string format |
| Msg_ver | string | Optional | Message version |
| Inf_ver | string | Optional | Inference version |
| Ad | string | Optional | Additional details at frame level |
| Fs | list[object] | Optional | Array of prediction results (see Fs Object Fields below) |
| Mtp | list[object] | Yes | Message Travel Path — timing metadata with Stime, Etime (format: `yyyy-MM-dd,HH:mm:ss.fff tt`), and Src (source) |
| Ffp | string | Yes | First frame flag |
| Ltsize | string | Yes | Frame to predict size |
| Lfp | string | Yes | Last frame flag |
| I_fn | string | Yes | Input file name |
| Rc | int | Yes | Response code (e.g., 200 for success, 500 for failure) |
| Rm | string | Yes | Response message for success or failure |
| Obase_64 | list[string] | Optional | List of base64 encoded rendered output images after applying predictions |
| Img_url | list[string] | Optional | List of URLs where output images are saved (used when output image is too large to send in response) |
| Prompt | list[list[string]] | Optional | Prompts for the model, array of array of strings |
| Hp | string | Optional | Hyper parameters as string for generative AI models |

### Fs Object Fields

| Key | Type | Description |
|-----|------|-------------|
| Dm | object | Bounding box coordinates — X, Y, H (height), W (width) |
| Pid | string | Person ID if assigned |
| Np | string | New person flag |
| Cs | string | Confidence score of prediction |
| Lb | string | Label of prediction |
| Uid | string | Unique ID if assigned |
| Nobj | string | Nested object |
| Info | string | Extra information at object level (string field, any information can be sent) |
| Kp | dict or null | Key points for pose models — dict of int to list of floats (e.g., `{"0": [0.0, 0.0, 0.0]}`) |
| Tpc | list or null | Object pixel coordinates for segmentation (list of [x, y] pairs) |
| Bpc | list or null | Boundary pixel coordinates for segmentation (list of [x, y] pairs) |
| TaskType | string | Type of rendering task (e.g., ObjectDetection, Classification, Segmentation) |
| Ad | Additional Parameters |
| Lfp | Last Frame Passing |
| Ffp | First Frame passing |
| Ltsize | Lot size |
| Obase_64 | List of output images in base64 format |
| Img_url | List of URLs of output images |
