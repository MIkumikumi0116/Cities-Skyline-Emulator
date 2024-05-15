import base64
import json
import os

import numpy as np
from citybrain.VLM.openai import OpenAIProvider


embedding_provider = OpenAIProvider()  # 实例化OpenAIProvider对象，用于提供语言模型服务
embedding_provider.init_provider("./conf/openai_config.json")  # 初始化语言模型提供者配置

def get_embedding(skill_name, skill_doc):
    return np.array(embedding_provider.embed_query('{}: {}'.format(skill_name, skill_doc)))
def save_json(file_path, json_dict, indent=-1):
    with open(file_path, mode='w', encoding='utf8') as fp:
        if indent == -1:
            json.dump(json_dict, fp, ensure_ascii=False)
        else:
            json.dump(json_dict, fp, ensure_ascii=False, indent=indent)

skill_library_filename="skill_library.json"
file_path = os.path.join( './res/skills', skill_library_filename)

store_file = {}

with open("./code.json", "r") as file:
    data = json.load(file)

# 使用 for 循环遍历每个字段
for item in data:
    store_file[item["name"]] = {'skill_code': item["code"],
                              "skill_emb": base64.b64encode(get_embedding(item["name"],item["code_description"]).tobytes()).decode('utf-8'),
                              "skill_code_base64": base64.b64encode(item["code"].encode('utf-8')).decode('utf-8')}
    save_json(file_path=file_path, json_dict=store_file, indent=4)
