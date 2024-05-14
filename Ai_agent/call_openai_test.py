# Reference: https://openai.com/blog/function-calling-and-other-api-updates

from loguru import logger
from openai import OpenAI
import torch
import requests
import base64
from PIL import Image
from io import BytesIO


openai_api_key = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJ1c2VyLWNlbnRlciIsImV4cCI6MTcyMjIyMjMxOCwiaWF0IjoxNzE0NDQ2MzE4LCJqdGkiOiJjb281dnJpdG5uMHF0MzE0bTFuZyIsInR5cCI6InJlZnJlc2giLCJzdWIiOiJjb281dnJpdG5uMHF0MzE0bTFtMCIsInNwYWNlX2lkIjoiY29vNXZyaXRubjBxdDMxNG0xbGciLCJhYnN0cmFjdF91c2VyX2lkIjoiY29vNXZyaXRubjBxdDMxNG0xbDAifQ.yqfOQjju68Pfg1TQjzrHJvy8v79AwTZKv061OGtIAPD_6XvuY3hz-NTvoLGdcyC5rlgexG8uXUfbJtRzwEpqow"
openai_api_base = "http://10.0.168.248:8018/v1"
client = OpenAI(
    api_key=openai_api_key,
    base_url=openai_api_base,
)

def call_qwen(messages, functions=None):
    logger.info(messages)
    if functions:
        response = client.chat.completions.create(
            model="Qwen", messages=messages, functions=functions ,
            temperature = 0.5,
            seed=1234,
            max_tokens=1024
        )
    else:
        response = client.chat.completions.create(model="Qwen", messages=messages)
    logger.info(response.model_dump())
    logger.info("this is response content:"+response.choices[0].message.content)
    return response

def run_test_5():
    chat_response = client.chat.completions.create(
        model="kimi",
        messages=[
            {"role": "system", "content": "你是游戏大模型"},
            {"role": "user", "content": "你好"}
        ],
        temperature = 0.5,
        seed=1234,
        max_tokens=1024
    )
    connect_content = chat_response.choices[0].message.content
    # print("question:", question)
    print("response:", connect_content)
    print("\n")

def run_test_1():
    messages = [ {"role": "system", "content": "你是游戏大模型"},{"role": "user", "content": "你好,你是什么模型"}]
    call_qwen(messages)
    messages.append({"role": "assistant", "content": "你好，我是游戏大模型"})

    messages.append({"role": "user", "content": "给我讲一个年轻人奋斗创业最终取得成功的故事。故事只能有一句话。"})
    call_qwen(messages)
    messages.append(
        {
            "role": "assistant",
            "content": "故事的主人公叫李明，他来自一个普通的家庭，父母都是普通的工人。李明想要成为一名成功的企业家。……",
        }
    )

    messages.append({"role": "user", "content": "给这个故事起一个标题"})
    call_qwen(messages)


def run_test_2():
    functions = [
        {
            "name_for_human": "谷歌搜索",
            "name_for_model": "google_search",
            "description_for_model": "谷歌搜索是一个通用搜索引擎，可用于访问互联网、查询百科知识、了解时事新闻等。"
            + " Format the arguments as a JSON object.",
            "parameters": [
                {
                    "name": "search_query",
                    "description": "搜索关键词或短语",
                    "required": True,
                    "schema": {"type": "string"},
                }
            ],
        },
        {
            "name_for_human": "文生图",
            "name_for_model": "image_gen",
            "description_for_model": "文生图是一个AI绘画（图像生成）服务，输入文本描述，返回根据文本作画得到的图片的URL。"
            + " Format the arguments as a JSON object.",
            "parameters": [
                {
                    "name": "prompt",
                    "description": "英文关键词，描述了希望图像具有什么内容",
                    "required": True,
                    "schema": {"type": "string"},
                }
            ],
        },
    ]

    messages = [{"role": "user", "content": "你好"}]
    call_qwen(messages, functions)
    messages.append(
        {"role": "assistant", "content": "你好！很高兴见到你。有什么我可以帮忙的吗？"},
    )

    messages.append({"role": "user", "content": "谁是周杰伦"})
    call_qwen(messages, functions)
    messages.append(
        {
            "role": "assistant",
            "content": "Thought: 我应该使用Google搜索查找相关信息。",
            "function_call": {
                "name": "google_search",
                "arguments": '{"search_query": "周杰伦"}',
            },
        }
    )

    messages.append(
        {
            "role": "function",
            "name": "google_search",
            "content": "Jay Chou is a Taiwanese singer.",
        }
    )
    call_qwen(messages, functions)
    messages.append(
        {
            "role": "assistant",
            "content": "周杰伦（Jay Chou）是一位来自台湾的歌手。",
        },
    )

    messages.append({"role": "user", "content": "他老婆是谁"})
    call_qwen(messages, functions)
    messages.append(
        {
            "role": "assistant",
            "content": "Thought: 我应该使用Google搜索查找相关信息。",
            "function_call": {
                "name": "google_search",
                "arguments": '{"search_query": "周杰伦 老婆"}',
            },
        }
    )

    messages.append(
        {"role": "function", "name": "google_search", "content": "Hannah Quinlivan"}
    )
    call_qwen(messages, functions)
    messages.append(
        {
            "role": "assistant",
            "content": "周杰伦的老婆是Hannah Quinlivan。",
        },
    )

    messages.append({"role": "user", "content": "给我画个可爱的小猫吧，最好是黑猫"})
    call_qwen(messages, functions)
    messages.append(
        {
            "role": "assistant",
            "content": "Thought: 我应该使用文生图API来生成一张可爱的小猫图片。",
            "function_call": {
                "name": "image_gen",
                "arguments": '{"prompt": "cute black cat"}',
            },
        }
    )

    messages.append(
        {
            "role": "function",
            "name": "image_gen",
            "content": '{"image_url": "https://image.pollinations.ai/prompt/cute%20black%20cat"}',
        }
    )
    call_qwen(messages, functions)


def run_test_3():
    functions = [
        {
            "name": "get_current_weather",
            "description": "Get the current weather in a given location.",
            "parameters": {
                "type": "object",
                "properties": {
                    "location": {
                        "type": "string",
                        "description": "The city and state, e.g. San Francisco, CA",
                    },
                    "unit": {"type": "string", "enum": ["celsius", "fahrenheit"]},
                },
                "required": ["location"],
            },
        }
    ]

    messages = [
        {
            "role": "user",
            # Note: The current version of Qwen-7B-Chat (as of 2023.08) performs okay with Chinese tool-use prompts,
            # but performs terribly when it comes to English tool-use prompts, due to a mistake in data collecting.
            "content": "波士顿天气如何？",
        }
    ]
    call_qwen(messages, functions)
    messages.append(
        {
            "role": "assistant",
            "content": None,
            "function_call": {
                "name": "get_current_weather",
                "arguments": '{"location": "Boston, MA"}',
            },
        },
    )

    messages.append(
        {
            "role": "function",
            "name": "get_current_weather",
            "content": '{"temperature": "22", "unit": "celsius", "description": "Sunny"}',
        }
    )
    call_qwen(messages, functions)

def run_test_4():
    image_url = "https://qianwen-res.oss-cn-beijing.aliyuncs.com/Qwen-VL/assets/demo.jpeg"
    response = requests.get(image_url)
    image_data = response.content

    # 将图片编码为base64格式
    image_base64 = base64.b64encode(image_data).decode('utf-8')

    messages = []
    messages.append({"role": "user", "content": [
          {
            "type": "text",
            "text": "图片里有什么"
          },
          {
            "type": "image_url",
            "image_url": {
            "url": f"data:image/jpeg;base64,{ image_base64}"
            # "url": f"data:image/jpeg;base64,1"
            }
          }]
                     })
    call_qwen(messages)
    messages.append({"role": "assistant", "content": "图片中主要包含了两个主体，一个年轻的女性和一只黄色的狗。女性面带微笑坐在沙滩上，狗则坐在她的前方，两者都面向着海平线。女性的右腿弯曲，左手扶着地面，右手做出往高处托起的动作，而狗的右前腿也向上弯曲，两只手看起来像是要做出往高处抓的动作。 "})
    messages.append({"role": "user", "content": "请再详细点"})
    call_qwen(messages)

    # messages.append(
    #     {
    #         "role": "assistant",
    #         "content": "故事的主人公叫李明，他来自一个普通的家庭，父母都是普通的工人。李明想要成为一名成功的企业家。……",
    #     }
    # )
    #
    # messages.append({"role": "user", "content": "给这个故事起一个标题"})
    # call_qwen(messages)

if __name__ == "__main__":
    logger.info("### Test Case 1 - No Function Calling (普通问答、无函数调用) ###")
    run_test_1()
    logger.info("### Test Case 2 - Use Qwen-Style Functions (函数调用，千问格式) ###")
    #run_test_2()
    logger.info("### Test Case 3 - Use GPT-Style Functions (函数调用，GPT格式) ###")
    #run_test_3()
    logger.info("### Test Case 4 - Use GPT-Style images (vl，GPT格式) ###")
    #run_test_4()
