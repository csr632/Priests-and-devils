# 游戏规则：
* 你要运用智慧帮助3个牧师（方块）和3个魔鬼（圆球）渡河。
* 船最多可以载2名游戏角色。
* 船上有游戏角色时，你才可以点击这个船，让船移动到对岸。
* 当有一侧岸的魔鬼数多余牧师数时（包括船上的魔鬼和牧师），魔鬼就会失去控制，吃掉牧师（如果这一侧没有牧师则不会失败），游戏失败。
* 当所有游戏角色都上到对岸时，游戏胜利。
****
# 项目资源
https://github.com/csr632/Priests-and-devils
# 游戏截图：
![开始游戏](http://upload-images.jianshu.io/upload_images/4888929-b91221deeb85c0ad.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

![游戏失败](http://upload-images.jianshu.io/upload_images/4888929-f2c28caa984232e5.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

![游戏胜利](http://upload-images.jianshu.io/upload_images/4888929-a35353d419752977.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

****
# 在Unity中体验
从Github中下载[我的项目](https://github.com/csr632/Priests-and-devils)。
将我的Asserts文件夹覆盖你的Unity项目中的Asserts文件夹。在你的Assets窗口中双击“ass”，然后就可以点击运行按钮了！

****
# 游戏架构
使用了MVC架构。
* 场景中的所有GameObject就是Model，它们受到Controller的控制，比如说牧师和魔鬼受到MyCharacterController类的控制，船受到BoatController类的控制，河岸受到CoastController类的控制。
* View就是UserGUI和ClickGUI，它们展示游戏结果，并提供用户交互的渠道（点击物体和按钮）。
* Controller：除了刚才说的MyCharacterController、BoatController、CoastController以外，还有更高一层的Controller：**FirstController（场景控制器）**，FirstController控制着这个场景中的所有对象，包括其加载、通信、用户输入。
**最高层的Controller是Director类**，一个游戏中只能有一个实例，它控制着场景的创建、切换、销毁、游戏暂停、游戏退出等等最高层次的功能。

****
## Director
Director的定义：

	public class Director : System.Object {
		private static Director _instance;
		public SceneController currentSceneController { get; set; }

		public static Director getInstance() {
			if (_instance == null) {
				_instance = new Director ();
			}
			return _instance;
		}
	}
Director是最高层的控制器，运行游戏时始终只有一个实例，它掌控着场景的加载、切换等，也可以控制游戏暂停、结束等等。
> 虽然Director控制着场景，但是它并不控制场景中的具体对象，控制场景对象的任务交给了SceneController（场景控制器），我们等一下会谈到。

Director类使用了单例模式。第一次调用Director.getInstance()时，会创建一个新的Director对象，保存在_instance，此后每次调用getInstance，都回返回_instance。也就是说Director最多只有一个实例。这样，我们在任何Script中的任何地方通过`Director.getInstance()`都能得到同一个Director对象，也就可以获得同一个currentSceneController，这样我们就可以轻易实现类与类之间的通信，比如说我在其他控制器中就可以使用`Director.getInstance().somethingHappen()`来告诉导演某一件事情发生了，导演就可以在`somethingHappen()`方法中做出对应的反应。

****
## SceneController接口
SceneController接口定义：

	public interface SceneController {
		void loadResources ();
	}

interface（接口）不能直接用来创建对象！必须先有一个类实现（继承）它，在我的这个游戏中就是FirstController类。
SceneController 是用来干什么的呢？它是导演控制场景控制器的渠道。在上面的Director 类中，currentSceneController （FirstController类）就是SceneController的实现，所以Director可以调用SceneController接口中的方法，来实现对场景的生杀予夺。

> 在这个游戏中SceneController的定义非常简单，因为这个游戏做得并不完整。我们刚才说过导演可以加载、切换、销毁场景、暂停游戏，所以SceneController 还可以规定`void switchScene()`、`void destroyScene()`、`void pause()`这些方法，供给导演来调用。

****
## Moveable
Moveable是一个可以挂载在GameObject上的类：

	public class Moveable: MonoBehaviour {
		
		readonly float move_speed = 20;

		// change frequently
		int moving_status;	// 0->not moving, 1->moving to middle, 2->moving to dest
		Vector3 dest;
		Vector3 middle;

		void Update() {
			if (moving_status == 1) {
				transform.position = Vector3.MoveTowards (transform.position, middle, move_speed * Time.deltaTime);
				if (transform.position == middle) {
					moving_status = 2;
				}
			} else if (moving_status == 2) {
				transform.position = Vector3.MoveTowards (transform.position, dest, move_speed * Time.deltaTime);
				if (transform.position == dest) {
					moving_status = 0;
				}
			}
		}
		public void setDestination(Vector3 _dest) {
			dest = _dest;
			middle = _dest;
			if (_dest.y == transform.position.y) {	// boat moving
				moving_status = 2;
			}
			else if (_dest.y < transform.position.y) {	// character from coast to boat
				middle.y = transform.position.y;
			} else {								// character from boat to coast
				middle.x = transform.position.x;
			}
			moving_status = 1;
		}

		public void reset() {
			moving_status = 0;
		}
	}
GameObject挂载上Moveable以后，Controller就可以通过`setDestination()`方法轻松地让GameObject移动起来。
> 在这里我没有让物体直接移动到目的地dest，因为那样可能会直接穿过河岸物体。我用middle来保存一个中间位置，让物体先移动到middle，再移动到dest，这就实现了一个折线的移动，不会穿越河岸。moving_status记录着目前该物体处于哪种移动状态。

****
## MyCharacterController
MyCharacterController封装了一个GameObject，表示游戏角色（牧师或恶魔）。

	public class MyCharacterController {
		readonly GameObject character;
		readonly Moveable moveableScript;
		readonly ClickGUI clickGUI;
		readonly int characterType;	// 0->priest, 1->devil

		// change frequently
		bool _isOnBoat;
		CoastController coastController;


		public MyCharacterController(string which_character) {
			
			if (which_character == "priest") {
				character = Object.Instantiate (Resources.Load ("Perfabs/Priest", typeof(GameObject)), Vector3.zero, Quaternion.identity, null) as GameObject;
				characterType = 0;
			} else {
				character = Object.Instantiate (Resources.Load ("Perfabs/Devil", typeof(GameObject)), Vector3.zero, Quaternion.identity, null) as GameObject;
				characterType = 1;
			}
			moveableScript = character.AddComponent (typeof(Moveable)) as Moveable;

			clickGUI = character.AddComponent (typeof(ClickGUI)) as ClickGUI;
			clickGUI.setController (this);
		}

		public void setName(string name) {
			character.name = name;
		}

		public void setPosition(Vector3 pos) {
			character.transform.position = pos;
		}

		public void moveToPosition(Vector3 destination) {
			moveableScript.setDestination(destination);
		}

		public int getType() {	// 0->priest, 1->devil
			return characterType;
		}

		public string getName() {
			return character.name;
		}

		public void getOnBoat(BoatController boatCtrl) {
			coastController = null;
			character.transform.parent = boatCtrl.getGameobj().transform;
			_isOnBoat = true;
		}

		public void getOnCoast(CoastController coastCtrl) {
			coastController = coastCtrl;
			character.transform.parent = null;
			_isOnBoat = false;
		}

		public bool isOnBoat() {
			return _isOnBoat;
		}

		public CoastController getCoastController() {
			return coastController;
		}

		public void reset() {
			moveableScript.reset ();
			coastController = (Director.getInstance ().currentSceneController as FirstController).fromCoast;
			getOnCoast (coastController);
			setPosition (coastController.getEmptyPosition ());
			coastController.getOnCoast (this);
		}
	}
在构造函数中实例化了一个perfab，创建GameObject，因此我们每`new MyCharacterController()`一次，场景中就会多一个游戏角色。
构造函数还将clickGUI挂载到了这个角色上，以监测“鼠标点击角色”的事件。

MyCharacterController还定义了一些方法提供给场景控制器来调用，方法名已经能够表明这个方法是做什么的了。

****
## BoatController和CoastController
BoatController和CoastController也类似MyCharacterController，封装了船GameObject和河岸GameObject。实现这两个类的难度主要在于它们是一种“容器”，游戏角色要进入它们的空位中。因此它们要提供`getEmptyPosition()`方法，给出自己的空位，让游戏角色能够移动到合适的位置。

	/*-----------------------------------CoastController------------------------------------------*/
	public class CoastController {
		readonly GameObject coast;
		readonly Vector3 from_pos = new Vector3(9,1,0);
		readonly Vector3 to_pos = new Vector3(-9,1,0);
		readonly Vector3[] positions;
		readonly int to_or_from;	// to->-1, from->1

		// change frequently
		MyCharacterController[] passengerPlaner;

		public CoastController(string _to_or_from) {
			positions = new Vector3[] {new Vector3(6.5F,2.25F,0), new Vector3(7.5F,2.25F,0), new Vector3(8.5F,2.25F,0), 
				new Vector3(9.5F,2.25F,0), new Vector3(10.5F,2.25F,0), new Vector3(11.5F,2.25F,0)};

			passengerPlaner = new MyCharacterController[6];

			if (_to_or_from == "from") {
				coast = Object.Instantiate (Resources.Load ("Perfabs/Stone", typeof(GameObject)), from_pos, Quaternion.identity, null) as GameObject;
				coast.name = "from";
				to_or_from = 1;
			} else {
				coast = Object.Instantiate (Resources.Load ("Perfabs/Stone", typeof(GameObject)), to_pos, Quaternion.identity, null) as GameObject;
				coast.name = "to";
				to_or_from = -1;
			}
		}

		public int getEmptyIndex() {
			for (int i = 0; i < passengerPlaner.Length; i++) {
				if (passengerPlaner [i] == null) {
					return i;
				}
			}
			return -1;
		}

		public Vector3 getEmptyPosition() {
			Vector3 pos = positions [getEmptyIndex ()];
			pos.x *= to_or_from;
			return pos;
		}

		public void getOnCoast(MyCharacterController characterCtrl) {
			int index = getEmptyIndex ();
			passengerPlaner [index] = characterCtrl;
		}

		public MyCharacterController getOffCoast(string passenger_name) {	// 0->priest, 1->devil
			for (int i = 0; i < passengerPlaner.Length; i++) {
				if (passengerPlaner [i] != null && passengerPlaner [i].getName () == passenger_name) {
					MyCharacterController charactorCtrl = passengerPlaner [i];
					passengerPlaner [i] = null;
					return charactorCtrl;
				}
			}
			Debug.Log ("cant find passenger on coast: " + passenger_name);
			return null;
		}

		public int get_to_or_from() {
			return to_or_from;
		}

		public int[] getCharacterNum() {
			int[] count = {0, 0};
			for (int i = 0; i < passengerPlaner.Length; i++) {
				if (passengerPlaner [i] == null)
					continue;
				if (passengerPlaner [i].getType () == 0) {	// 0->priest, 1->devil
					count[0]++;
				} else {
					count[1]++;
				}
			}
			return count;
		}

		public void reset() {
			passengerPlaner = new MyCharacterController[6];
		}
	}

	/*-----------------------------------BoatController------------------------------------------*/
	public class BoatController {
		readonly GameObject boat;
		readonly Moveable moveableScript;
		readonly ClickGUI clickGUI;
		readonly Vector3 fromPosition = new Vector3 (5, 1, 0);
		readonly Vector3 toPosition = new Vector3 (-5, 1, 0);
		readonly Vector3[] from_positions;
		readonly Vector3[] to_positions;

		// change frequently
		int to_or_from; // to->-1; from->1
		MyCharacterController[] passenger = new MyCharacterController[2];

		public BoatController() {
			to_or_from = 1;

			from_positions = new Vector3[] { new Vector3 (4.5F, 1.5F, 0), new Vector3 (5.5F, 1.5F, 0) };
			to_positions = new Vector3[] { new Vector3 (-5.5F, 1.5F, 0), new Vector3 (-4.5F, 1.5F, 0) };

			boat = Object.Instantiate (Resources.Load ("Perfabs/Boat", typeof(GameObject)), fromPosition, Quaternion.identity, null) as GameObject;
			boat.name = "boat";

			moveableScript = boat.AddComponent (typeof(Moveable)) as Moveable;
			clickGUI = boat.AddComponent (typeof(ClickGUI)) as ClickGUI;
		}


		public void Move() {
			if (to_or_from == -1) {
				moveableScript.setDestination(fromPosition);
				to_or_from = 1;
			} else {
				moveableScript.setDestination(toPosition);
				to_or_from = -1;
			}
		}

		public int getEmptyIndex() {
			for (int i = 0; i < passenger.Length; i++) {
				if (passenger [i] == null) {
					return i;
				}
			}
			return -1;
		}

		public bool isEmpty() {
			for (int i = 0; i < passenger.Length; i++) {
				if (passenger [i] != null) {
					return false;
				}
			}
			return true;
		}

		public Vector3 getEmptyPosition() {
			Vector3 pos;
			int emptyIndex = getEmptyIndex ();
			if (to_or_from == -1) {
				pos = to_positions[emptyIndex];
			} else {
				pos = from_positions[emptyIndex];
			}
			return pos;
		}

		public void GetOnBoat(MyCharacterController characterCtrl) {
			int index = getEmptyIndex ();
			passenger [index] = characterCtrl;
		}

		public MyCharacterController GetOffBoat(string passenger_name) {
			for (int i = 0; i < passenger.Length; i++) {
				if (passenger [i] != null && passenger [i].getName () == passenger_name) {
					MyCharacterController charactorCtrl = passenger [i];
					passenger [i] = null;
					return charactorCtrl;
				}
			}
			Debug.Log ("Cant find passenger in boat: " + passenger_name);
			return null;
		}

		public GameObject getGameobj() {
			return boat;
		}

		public int get_to_or_from() { // to->-1; from->1
			return to_or_from;
		}

		public int[] getCharacterNum() {
			int[] count = {0, 0};
			for (int i = 0; i < passenger.Length; i++) {
				if (passenger [i] == null)
					continue;
				if (passenger [i].getType () == 0) {	// 0->priest, 1->devil
					count[0]++;
				} else {
					count[1]++;
				}
			}
			return count;
		}

		public void reset() {
			moveableScript.reset ();
			if (to_or_from == -1) {
				Move ();
			}
			passenger = new MyCharacterController[2];
		}
	}




另外一个需要注意的是MyCharacterController、BoatController、CoastController有一些方法名是重复的，比如说getOnBoat在MyCharacterController和BoatController中都有（BoatController中的GetOnBoat是我当时手抖了，第一个字母应该小写）。看起来似乎功能有点重复，为什么不只用一个函数操控游戏角色的上船呢？原因是**不要在一个类中操作另一个类，那会加强两个类之间的耦合性**。MyCharacterController中的`getOnBoat()`只应该操作MyCharacterController中的成员，BoatController中的`GetOnBoat()`只应该操作BoatController中的成员。
我们在FirstController中想让游戏角色上船的时候，两个类的getOnBoat都要调用：
```
whichCoast.getOffCoast(characterCtrl.getName());
characterCtrl.moveToPosition (boat.getEmptyPosition());
characterCtrl.getOnBoat (boat);
boat.GetOnBoat (characterCtrl);
```
****
## UserAction
这个接口实际上使用了门面模式。
FirstController必须要实现这个接口才能对用户的输入做出反应。
```
public interface UserAction {
	void moveBoat();
	void characterIsClicked(MyCharacterController characterCtrl);
	void restart();
}
```
在这个游戏中，对用户输入做出反应，有这三个方法就够了。
UserAction是如何得到用户的输入的呢？原来，在ClickGUI和UserGUI这两个类中，都保存了一个UserAction的引用。当ClickGUI监测到用户点击GameObject的时候，就会调用这个引用的characterIsClicked方法，这样FirstController就知道哪一个游戏角色被点击了。UserGUI同理，只不过它监测的是“用户点击Restart按钮”的事件。

门面模式的好处：通过一套接口（UserAction）来定义Controller与GUI交互的渠道，这样实现Controller类的程序员只需要实现UserAction接口，他的代码就可以被任何**支持这个接口的GUI类**所使用；实现GUI类的程序员也不需要知道Controller的实现方式，它只需要调用接口中的方法，后面的事情就交给Controller吧！
****
## ClickGUI
ClickGUI类是用来监测用户点击，并调用SceneController进行响应的。

```
public class ClickGUI : MonoBehaviour {
	UserAction action;
	MyCharacterController characterController;

	public void setController(MyCharacterController characterCtrl) {
		characterController = characterCtrl;
	}

	void Start() {
		action = Director.getInstance ().currentSceneController as UserAction;
	}

	void OnMouseDown() {
		if (gameObject.name == "boat") {
			action.moveBoat ();
		} else {
			action.characterIsClicked (characterController);
		}
	}
}
```

我们可以看到`UserAction action`实际上是FirstController的对象，它实现了UserAction接口。ClickGUI与FirstController打交道，就是通过UserAction接口的API。ClickGUI不知道这些API是怎么被实现的，但它知道FirstController类一定有这些方法。
****
# 可以做的扩展：
* 游戏失败以后不能再响应用户点击的事件，用户只能点击Restart。
* 增加计时的功能（这应该由SceneController来控制）。
* 增加暂停/恢复游戏的功能（这应该由Director来控制）。
* 在开始游戏之前做一个欢迎界面，与用户进行交互（这就是另一个场景了）。
* 让用户可以在游戏中切换到欢迎界面，再切换回游戏界面的时候，游戏状态要和之前一样（场景的切换）。用户可以在游戏中放弃游戏，回到欢迎页面（场景的销毁）。
* 让用户能够在欢迎界面指定有几个牧师几个恶魔，然后开始游戏。（运行时决定场景的创建）
* 增加一种更难的模式，开始3秒以后牧师和恶魔外观相同，玩家需要凭借记忆来操作。
* 美化游戏对象！
