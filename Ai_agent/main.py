# import openai
# openai.api_base = "http://10.0.168.248:7888/v1"
# openai.api_key = "none"


import google.generativeai as genai
import PIL.Image

img = PIL.Image.open(r'D:\Users\10424\Desktop\demo.jpeg')


genai.configure(api_key='AIzaSyDnCWe-TkqF2Y9dScxrugxCIX6DFMsS0ME' , transport="rest")

model = genai.GenerativeModel('gemini-pro-vision')


response = model.generate_content(["这个图片里有什么?", img])

model = genai.GenerativeModel('gemini-pro')
# response = model.generate_content("What is the meaning of life?")

print(response.text)
# client = OpenAI(api_key=api_key)
# 不使用流式回复的请求

# response = openai.ChatCompletion.create(
#     model="1",
#     # messages=[
#     #     {"role": "user", "content":"Picture 1: <img>/tmp/gradio/8864c5095af996c4918f14d570a852c997e6c620/demo.jpeg</img>\n你好，这个图片有什么\n"}
#     # ],
#     messages="你好",
#     stream=False,
#     stop=[] # 在此处添加自定义的stop words 例如ReAct prompting时需要增加： stop=["Observation:"]。
# )
# print(response.choices[0].message.content)
response = openai.create_completion(
    model="1",
    # messages=[
    #     {"role": "user", "content":"Picture 1: <img>/tmp/gradio/8864c5095af996c4918f14d570a852c997e6c620/demo.jpeg</img>\n你好，这个图片有什么\n"}
    # ],
    messages="你好",
    stream=False,
    stop=[] # 在此处添加自定义的stop words 例如ReAct prompting时需要增加： stop=["Observation:"]。
)
print(response.choices[0].message.content)
# system_promt = "你是交通领域大模型，要求专业有耐心的回答用户的问题"
# system_his = {'role': 'system', 'content': system_promt}
# messages=[]
# messages.insert(0,system_his)
# messages.insert(0,system_his)
# messages.insert(0,system_his)
# print(messages)
"""Create a chat completion using the OpenAI API

Supports both GPT-4 and GPT-4V).

Example Usage:
image_path = "path_to_your_image.jpg"
base64_image = encode_image(image_path)
response, info = self.create_completion(
    model="gpt-4-vision-preview",
    messages=[
      {
        "role": "user",
        "content": [
          {
            "type": "text",
            "text": "What’s in this image?"
          },
          {
            "type": "image_url",
            "image_url": {
              "url": f"data:image/jpeg;base64,{base64_image}"
            }
          }
        ]
      }
    ],
)
"""