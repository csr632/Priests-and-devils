# 学习Unity(6)小游戏场景改进——摄像机和光源

在我的[上一篇文章](http://www.jianshu.com/p/07028b3da573)中，我们制作了一个简单的游戏，现在让我们从场景、摄像机、光源方面来改进它。
# 改进结果图
为了适应这个游戏的剧情，我们要将画面改得阴森一些：
![](http://upload-images.jianshu.io/upload_images/4888929-a160fa7eaa208d8b.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
![是不是很恐怖](http://upload-images.jianshu.io/upload_images/4888929-83cc01b94d4aef73.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)


你可以下载运行[我的项目](https://github.com/csr632/Priests-and-devils/tree/Improvement1)，将我的项目中的Assets文件夹覆盖你的项目中的Assets文件夹，然后在U3D中双击“ass”，就可以运行了！

****
# 地形
选中一个Terrain（地形）以后，在Inspector中有很多调整地形的工具：
![地形调整工具](http://upload-images.jianshu.io/upload_images/4888929-918fac3cb3842575.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
其中前三个分别调整地形的隆起、凹陷、平滑过渡。选中工具以后要先在Inspector中设置好刷子的形状、大小和效果强度：
![地形工具的设置](http://upload-images.jianshu.io/upload_images/4888929-656570d4a7bee9c2.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

> 使用隆起工具时，按住shift再点击可以让选中区域的地形**变回原本的平面**。


第4~6个工具分别是地面纹理、数目、草。
地面纹理比较容易使用，点击Edit texture->add texture。然后在Diffuse中选择一张**你提前放在Assert中的图片**，你就会发现整个地形的纹理都变成了你选择的图片（第一个Texture默认影响整个地形），后面你可以添加其他的texture，可以在原本的纹理上用刷子“刷出”第二纹理、第三纹理（比如在草地上有一些光秃的土壤）。
****
# 水域
这里的水域我使用的是Standard Asset的水，它有动态水波效果，还有倒影效果，非常漂亮。
首先将示例资源添加到我们的Asset：
![添加示例资源](http://upload-images.jianshu.io/upload_images/4888929-247fe4e85a285fc6.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
然后选择我们需要的Water文件夹，添加好以后你可以在你的Asset中搜索到`WaterProNighttime`Perfab，将它拖到你的场景中，调整好大小和位置就好了。

![找到需要的预制](http://upload-images.jianshu.io/upload_images/4888929-0a975a25cf8447ac.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)
****
# 摄像机
在Unity中，[摄像机](https://docs.unity3d.com/ScriptReference/Camera.html)就是一个Component（部件），任何一个GameOnject只要挂载上它，都可以变成一个“摄像师”的角色。

![摄像机的继承关系](http://upload-images.jianshu.io/upload_images/4888929-3f3b631d861ddb97.gif?imageMogr2/auto-orient/strip)
> 你可以去[Unity文档](https://docs.unity3d.com/ScriptReference/index.html)分别查找这些类的作用是什么（一般在第一句话就会讲，比如说[Behaviour](https://docs.unity3d.com/ScriptReference/Behaviour.html)的作用是控制部件是否enable），然后你就知道为什么Unity要这样划分继承关系了。

一个画面不一定是由一个摄像机渲染出来的，你可以通过使用多个摄像机达到一些特别的效果，比如说一个摄像机只渲染某一个层的对象，另一个摄像机渲染另一个层的对象（用Culling Mask控制）。
## 摄像机可设置的属性
* ClearFlags ：像机渲染每一帧包含两个部分，一个就是我们能看到的颜色缓冲，另一个是深度缓冲（它决定画面中物体之间的前后关系）。ClearFlags决定此摄像机在渲染下一帧之前如何清除当前画面的渲染。
  * ClearFlags默认是Skybox，它会将颜色缓冲和深度缓冲都清除，然后渲染一个天空盒，再在其上渲染场景中的物体。
  * Solid Color：同样会将颜色缓冲和深度缓冲都清除，只不过它不渲染天空盒，而是简单地用一个颜色填充，再渲染场景中的物体。
  * Depth only：仅清除深度缓冲，这样此摄像机的颜色渲染会叠加在当前画面，在多摄像头的时候有时用到。
  * Don't Clear：都不清除，这样摄像机的渲染都会叠加在当前画面，但是深度要与遗留的画面进行比较，如果某个要渲染像素的深度比遗留像素的深度要浅，则覆盖这个像素，否则还是显示原来的像素。

> 有关多摄像机的使用和深度缓冲，可以看看[这篇文章](http://www.manew.com/thread-47076-1-1.html)和[它的原文](http://blog.theknightsofunity.com/using-multiple-unity-cameras-why-this-may-be-important/)。

* Culling Mask：用这个属性来设置摄像机只渲染哪些物体。
* Projection：投影方式，分为透视（模拟人眼观察人眼）和正交（物体大小不受距离影响）。
* Field of view：视野范围，只适用于透视模式。
* Clipping Planes：摄像机观察的深度范围。Near为最近可观察的点，Far为最远可观察的点。
* Normalized View Port Rect：标准视图矩形，用四个数值来控制摄像机的渲染画面在屏幕中的位置及大小，该项使用屏幕坐标系（屏幕左下角为原点），数值在0~1之间。
X 水平位置起点
Y 垂直位置起点
W 宽度
H 高度
* Depth：深度 ，用于控制摄像机的渲染顺序，决定有多个摄像机时的渲染顺序，**深度大的摄像机**在**深度小的摄像机**之后渲染。
这个参数可同Normalized View Port Rect做小地图，类似CS右上角的地图。

***
## 改进游戏摄像机
为了让游戏更有代入感，我们想要**让摄像机随着船运动**。做到这一点很简单，只需要让摄像机成为船的子对象就好了。

因为我们的船是在运行时通过脚本来加载的，所以我们的摄像机也应该通过脚本来加载。
首先我们删掉原本的摄像机。然后在BoatController中加入一个方法：
```
private void attachCamera() {
	cameraObj = new GameObject("Camera_follow");	// 新建一个空对象叫做Camera_follow
	cameraObj.transform.parent = boat.transform;	// 先让这个对象成为boat的子对象
	
	 // 调整一下Camera_follow与boat的相对方位
	cameraObj.transform.localPosition = new Vector3(0, 7, -8);
	cameraObj.transform.localRotation = Quaternion.Euler(10, 0, 0);

	cameraObj.AddComponent<Camera>();	// 添加Camera组件，让这个空对象成为一个摄像机
	Camera cameraComp = cameraObj.GetComponent<Camera>();	// 获取摄像机组件
	cameraComp.fieldOfView = 40;

	// 将Resources/skybox添加为这个摄像机的天空盒
	cameraObj.AddComponent<Skybox>().material = Resources.Load("skybox") as Material;
}
```
然后在BoatController的构造函数最后调用这个方法`attachCamera();`。

> 你在Inspector窗口可以调整的属性，绝大部分也可以在运行时通过脚本来调整，比如说在上面我用脚本调整了fieldOfView（摄像机的视角宽度）。要查看其他你可以调整的属性，可以看[摄像机官方文档](https://docs.unity3d.com/ScriptReference/Camera.html)。

****

# 光源
[Light](https://docs.unity3d.com/ScriptReference/Light.html)也是一种Component，且其继承关系与Camera一模一样！
有四种光源：
* Directional light： 方向光，类似太阳的日照效果，它的位置不会影响光照情况，只有它的rotation起作用。
* Point light： 点光源。
* Spotlight： 聚光灯，从一个点发出锥形区域的光。
* Area Light：区域光，一般用于光照贴图烘培。

光源的设置属性比较简单，自己在Inspector中玩一玩就知道了。
****
## 改进游戏光源
为了营造一个阴森的效果，我们希望只有一个聚光灯从船的上方照下来，再没有其他的光源。首先先删除原本的Directional Light。
> 你可能会发现，即使你将所有光源都删除，画面还是亮的，这是因为Skybox本身会发光，这时你需要找到Window->Lighting->Scene->Ambient Intensity 将强度调整为0。

实现的方式与Camera类似，在BoatController中再加入一个方法：
``` C#
private void attachLight() {
	lightObj = new GameObject("Light_follow");	
	lightObj.transform.parent = boat.transform;
	lightObj.transform.localPosition = new Vector3(0, 5, -2);
	lightObj.transform.localRotation = Quaternion.Euler(60, 0, 0);

	lightObj.AddComponent<Light>();
	Light lightComp =  lightObj.GetComponent<Light>();
	lightComp.type = LightType.Spot;		// 调整光源的类型为聚光灯
	lightComp.intensity = 4;		// 调整光源的强度
	lightComp.spotAngle = 170;		// 调整光源的照射张角
}
```
同样要在BoatController构造函数的最后一句话调用这个函数。
> 可能有同学不知道，我的这些相对位置、角度是怎么得到的呢？因为游戏没有运行的时候所有物体都没有加载，也就不知道这些参数到底要怎么确定。
其实你可以在运行游戏的时候点击Scene标签（就在运行按钮的下面一行，注意不要全屏游戏，否则你看不到这个标签），然后在里面调整。将合适的数据写在脚本中。
注意**你在运行过程中的调整会在游戏结束后全部复原！**

****
# 总结：
U3d 设计师 没有将 Camera,Light 设计为 GameObject ，这样我们只需要在一个物体上挂载上相应的部件，就可以将这个物体变成“摄影师”、光源，甚至同时成为两者。否则如果摄像机、光源原生就是GameObject，我们还要去调整它们的位置、父子关系，比较麻烦。

只有Component来可以改变物体特性，只有Component可以在Inspoector中暴露出属性以供设置，GameObject本身不应该具备任何可设置的属性（除了它必须挂载Transform部件可以设置）。即使是我们直接在Hierarchy右键创建一个Camera，也只不过是Unity帮我们在一个空对象上挂载了几个部件而已。

> 不需要一次性将Camera和Light的所有属性都背下来，只要先学会在Inspector中调整这个部件，将来需要在脚本中调整的时候去官方文档查找对应的属性或方法就好了！
