import cv2
import numpy as np
from sklearn.cluster import KMeans

def extract_dominant_colors(image, k=5):
    # 将图像数据转换为一维数组
    data = np.reshape(image, (-1, 3))
    # 使用K-means算法提取颜色
    kmeans = KMeans(n_clusters=k)
    kmeans.fit(data)
    dominant_colors = kmeans.cluster_centers_.astype(int)
    return dominant_colors

def find_similar_regions(target_image, dominant_color):
    # 转换颜色格式
    target_hsv = cv2.cvtColor(target_image, cv2.COLOR_BGR2HSV)
    dominant_color_hsv = cv2.cvtColor(np.uint8([[dominant_color]]), cv2.COLOR_BGR2HSV)[0][0]

    # 定义颜色范围
    lower_bound = np.array([dominant_color_hsv[0] - 10, 50, 50])
    upper_bound = np.array([dominant_color_hsv[0] + 10, 255, 255])

    # 创建掩码
    mask = cv2.inRange(target_hsv, lower_bound, upper_bound)

    # 使用形态学操作清理噪声
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)

    # 找到轮廓
    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    marked_image = target_image.copy()
    color_regions = []

    for contour in contours:
        # 计算每个轮廓的外接圆
        (x, y), radius = cv2.minEnclosingCircle(contour)
        center = (int(x), int(y))
        radius = int(radius)
        color_regions.append((center, radius))
        # 在目标图像上绘制外接圆
        cv2.circle(marked_image, center, radius, (0, 255, 0), 3)

    return marked_image, mask, color_regions

# 读取两张图片
src_image_path = 'source_image_path.jpg'
target_image_path = 'target_image_path.jpg'

src_image = cv2.imread(src_image_path)
target_image = cv2.imread(target_image_path)

# 提取源图像的前5种主体颜色
dominant_colors = extract_dominant_colors(src_image, k=5)

# 初始化结果图像
result_images = []
color_regions_list = []

# 在目标图像中查找每个主体颜色的相似区域
for color in dominant_colors:
    result_image, mask, color_regions = find_similar_regions(target_image, color)
    result_images.append(result_image)
    color_regions_list.append(color_regions)

# 显示结果
for i, result_image in enumerate(result_images):
    cv2.imshow(f'Result {i+1}', result_image)
cv2.waitKey(0)
cv2.destroyAllWindows()

# 输出每个颜色匹配到的区域
for i, color_regions in enumerate(color_regions_list):
    print(f'Color {i+1} regions:')
    for center, radius in color_regions:
        print(f'Center: {center}, Radius: {radius}')
