import os
import requests
from bs4 import BeautifulSoup
import re

# 网页URL
url = 'https://wiki.biligame.com/csl/Notifications'
path = 'D:/Users/10424/Desktop/Notifications'  # 图片保存路径

# 创建一个文件夹来保存下载的图片
if not os.path.exists(path):  # 如果路径不存在，则创建文件夹
    os.makedirs(path)

# 获取网页内容
response = requests.get(url)  # 发送HTTP请求获取网页内容
soup = BeautifulSoup(response.text, 'html.parser')  # 使用BeautifulSoup解析网页内容

# 找到所有图片标签
images = soup.find_all('img')  # 找到所有img标签

# 下载并保存图片
for i, image in enumerate(images):
    # 获取图片的URL
    image_url = image['src']
    # 清理URL中的特殊字符（去除查询字符串部分）
    clean_url = re.sub(r'\?.*$', '', image_url)
    # 发送HTTP请求获取图片数据
    img_data = requests.get(clean_url).content
    # 获取图片格式（如.jpg、.png）
    extension = os.path.splitext(clean_url)[1]
    # 创建图片文件名
    filename = os.path.join(path, f'image_{i+1}{extension}')
    # 保存图片到本地
    with open(filename, 'wb') as file:
        file.write(img_data)
    print(f'图片 {filename} 已下载.')

print('所有图片下载完成。')
