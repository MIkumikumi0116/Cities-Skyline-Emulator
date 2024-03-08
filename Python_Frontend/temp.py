import json
import requests



data = {
    "action": "build_road",
    "start_x": 0.0,
    "start_z": 0.0,
    "end_x": 90.0,
    "end_z": 90.0,
    "prefab_id": 101
}

json_data = json.dumps(data)
response = requests.post("http://localhost:11451/", data = json_data, headers = {"Content-Type": "application/json"})
response_json = json.loads(response.text.replace('\ufeff', ''))

print(response_json)
