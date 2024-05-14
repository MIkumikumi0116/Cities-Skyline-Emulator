import io
import json
import base64
import requests
from PIL import Image


class GameApiProvider:
    def __init__(self)->None:pass

    def send(self,data):
        # 发送数据
        print(data)
        json_data = json.dumps(data)
        response = requests.post('http://localhost:11451/', data=json_data,
                                 headers={'Content-Type': 'application/json'})
        response_json = json.loads(response.text.replace('\ufeff', ''))

        # 如果是获取截图的请求
        if data['action'] == 'Get_Screen_Shot':
            base64_str = response_json['screen_shot_base64']
            image_data = base64.b64decode(base64_str)
            image_file = io.BytesIO(image_data)
            image = Image.open(image_file)
            image.save('screen_shot.png')
        else:
            # 其他请求的响应
            print(response_json)

