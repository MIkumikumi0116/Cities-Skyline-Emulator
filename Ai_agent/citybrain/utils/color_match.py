'''1.
提取主体颜色：使用K-means聚类算法来确定图片中的主要颜色。这个算法会将所有像素点的颜色分成k个类别（或“簇”），每个类别都有一个中心颜色，这些中心颜色代表了图片中的主体颜色。
2.
创建颜色范围：对于每个主体颜色，代码会计算一个HSV颜色空间中的颜色范围。这个范围是围绕主体颜色的，由radius_threshold参数动态调整。
3.
生成颜色掩码：使用cv2.inRange函数，根据上一步计算的颜色范围，在目标图片上生成一个二值掩码。这个掩码会标记出所有落在颜色范围内的像素点。
4.
查找轮廓：使用cv2.findContours函数在掩码上查找连续的区域（即轮廓）。这些轮廓代表了与主体颜色相匹配的区域。
5.
计算圆形区域：对于每个轮廓，代码会计算最小包围圆，并检查这个圆的半径是否大于或等于radius_threshold。
6.
验证颜色一致性：对于符合半径条件的圆形区域，代码会进一步检查圆内至少95%的像素是否是主体颜色。
7.
标记和排序：满足所有条件的圆形区域会被添加到一个列表中，并按照半径大小排序。最后，代码会返回最大的k个区域。'''


import cv2
import numpy as np
from sklearn.cluster import KMeans
import os

def color_match(src_image_path, target_image_path, k1=1, k2=1, radius_threshold=5):
    # 创建输出目录
    output_dir = "./image_tmp"
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)

    # 读取图像
    src_image = cv2.imread(src_image_path)
    target_image = cv2.imread(target_image_path)

    # 检查图像是否成功读取
    if src_image is None:
        raise ValueError(f"Error: Unable to read source image from path {src_image_path}")
    if target_image is None:
        raise ValueError(f"Error: Unable to read target image from path {target_image_path}")

    # 提取主体颜色
    def extract_dominant_colors(image, k):
        data = np.reshape(image, (-1, 3))
        kmeans = KMeans(n_clusters=k, random_state=0)
        kmeans.fit(data)
        dominant_colors = kmeans.cluster_centers_.astype(int)
        return dominant_colors

    dominant_colors = extract_dominant_colors(src_image, k1)

    # 查找相似区域
    def find_similar_regions(target_image, dominant_color, k, radius_threshold):
        target_hsv = cv2.cvtColor(target_image, cv2.COLOR_BGR2HSV)
        dominant_color_hsv = cv2.cvtColor(np.uint8([[dominant_color]]), cv2.COLOR_BGR2HSV)[0][0]

        # 根据主体颜色动态调整阈值
        lower_bound = np.array([max(dominant_color_hsv[0] - radius_threshold, 0), 50, 50])
        upper_bound = np.array([min(dominant_color_hsv[0] + radius_threshold, 179), 255, 255])

        mask = cv2.inRange(target_hsv, lower_bound, upper_bound)
        kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
        mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)

        contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        marked_image = target_image.copy()
        color_regions = []

        for contour in contours:
            mask_circle = np.zeros_like(mask)
            (x, y), radius = cv2.minEnclosingCircle(contour)
            center = (int(x), int(y))
            cv2.circle(mask_circle, center, int(radius), 255, -1)
            circle_area = cv2.bitwise_and(mask, mask, mask=mask_circle)

            # 计算圆形区域内的颜色分布
            circle_pixels = target_image[mask_circle > 0]
            main_color_pixels = circle_pixels[np.all(circle_pixels == dominant_color, axis=1)]
            if len(main_color_pixels) / len(circle_pixels) >= 0.95:  # 确保至少95%的像素是主体颜色
                if radius >= radius_threshold:  # 确保半径等于或大于radius_threshold
                    color_regions.append((center, radius))
                    cv2.circle(marked_image, center, int(radius), (0, 255, 0), 3)

        # 根据半径大小排序并返回最大的k个区域
        color_regions = sorted(color_regions, key=lambda x: x[1], reverse=True)[:k]
        return marked_image, color_regions

    result_images = []
    color_regions_list = []

    # 对每种主体颜色执行相似区域查找
    for color in dominant_colors:
        result_image, color_regions = find_similar_regions(target_image, color, k2, radius_threshold)
        result_images.append(result_image)
        color_regions_list.append(color_regions)

    output_strings = []

    # 生成描述颜色区域的字符串
    for i, color_regions in enumerate(color_regions_list):
        for j, (center, radius) in enumerate(color_regions):
            output_strings.append(f"Color {i+1} region {j+1}: Center: {center}, Radius: {radius}")

    # 保存结果图像
    for i, result_image in enumerate(result_images):
        output_image_path = os.path.join(output_dir, f"result_{i+1}.png")
        cv2.imwrite(output_image_path, result_image)

    return "\n".join(output_strings)

# 调用示例
src_image_path = r'D:\Users\10424\Desktop\game\3-match.png'
target_image_path = r'D:\Users\10424\Desktop\game\3.png'
#k1 选取src_image_path中几个颜色
#k2 选取target_image_path中几个范围
#radius_threshold 圆半径

output = color_match(src_image_path, target_image_path, k1=1, k2=1, radius_threshold=10)
print(output)
#输出:Color {i+1} region {j+1}: Center: {center}, Radius: {radius}