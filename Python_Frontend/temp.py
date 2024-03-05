import json
import requests



data = {
    "action": "build_road",
    "start_x": 200.0,
    "start_z": 100.0,
    "end_x": 300.0,
    "end_z": 300.0,
    "prefab_id": 144
}

json_data = json.dumps(data)
response = requests.post("http://localhost:11451/", data = json_data, headers = {"Content-Type": "application/json"})
response_json = json.loads(response.text.replace('\ufeff', ''))

print(response_json)
