# C_Sharp_Backend
后端项目，将编译为一个天际线的mod，用visual studio打开，编译前需要创建环境变量CITIES_SKYLINE_PATH，指向天际线本地文件的路径，例如：G:\SteamLibrary\steamapps\common\Cities_Skylines，编译时引入天际线提供的dll会用到

用vs编译，菜单栏-生成-生成解决方案，编译后mod会自动复制天际线的mod目录下，进游戏后在内容管理-模组里启用

后端监听localhost的11451端口，前端与后端通过该端口通信，前端和agent对接，根据agent的输出构造http请求，后端接受请求，执行相应动作，给出http回复。前后端通信的接口定义见Api_definition.md

建议订阅mod https://steamcommunity.com/sharedfiles/filedetails/?id=450877484，以后会用到，订阅后steam会自动下载

# Python_Frontend
以后会和llm agent对接，现在只有一个最简单的例子，用来验证流程跑通了

# Api_definition.md
前后端通信接口，定义了后端可以执行的动作，每种动作的参数和返回值的字段和类型

# Tips
目前的mod还有bug，需要开启解锁所有里程碑，位置在内容管理-模组-解锁所有里程碑

关于提交的版本号

采用三位版本号，比如0.1.0，当有不兼容上个版本的更改，也就是Api_definition.md改了已有动作的接口定义时，第一位版本号+1，后两位归零；当增加了功能，也就是Api_definition.md里加了新的动作时，第二位+1，第三位归零；其他更改，第三位+1

当前需要做的：

大致想几个task，构思这些task可以分解为哪些基本动作，确定这些动作的请求和返回参数，加到Api_definition.md里，然后前后端分别照着md继续开发，以后可以慢慢迭代更多task，更对基本动作