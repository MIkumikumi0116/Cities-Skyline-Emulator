import os

# 指定目标文件夹路径
# folder_path = r'D:\PycharmPJ\mutil-agent\Cities-Skyline-Emulator\Ai_agent\res\icons\Notifications'
folder_path=r'D:\Users\10424\Desktop\Notifications'
name=[]
# 遍历文件夹中的所有文件
for filename in os.listdir(folder_path):
    # 检查文件名是否以 Notification_icon_ 开头
    name.append(filename[:-4])
    if filename.startswith('Building'):
        # 新文件名，去掉 Notification_icon_ 前缀
        new_filename = filename[len('Building'):]
        # 获取旧文件和新文件的完整路径
        old_file = os.path.join(folder_path, filename)
        new_file = os.path.join(folder_path, new_filename)
        # 重命名文件
        os.rename(old_file, new_file)

print("文件名已成功更新")
print(name)