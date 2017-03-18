在我的[学习Unity(5)](http://www.jianshu.com/p/07028b3da573)和[学习Unity(6)](http://www.jianshu.com/p/5a572a61f809)中，我们已经完成了一个简单的*牧师与恶魔游戏*并改进了它的场景。在这一篇文章中，我想从代码的架构来改进它——实现一个动作管理器来管理场景中的动作。
> 美化场景、控制摄像机和光源这些技巧，可能只适用于游戏编程，甚至只限于Unity3D，但是掌握**代码组织架构**和**面向对象的思想**对任何方面的编程都有巨大的提升。

****
# 下载我的项目在本地查看！
从[我的github](https://github.com/csr632/Priests-and-devils/tree/Improvement2)下载项目资源，将Assets文件夹覆盖你的项目中的Assets文件夹，然后在U3D中双击“ass”，就可以运行了！
****
# 为什么要引入动作管理器
在一个场景中肯定有很多“会动”的物体，它们的运动是有很多共性的，如果我们为游戏角色实现一个运动方法，为船实现一个运动方法，为**将来出现的所有会动的物体**都实现一个运动方法，势必是一种资源的浪费。我们可以将运动的共性提取出来，用一个管理器统一管理，这样，代码的复用性和可读性都会提高。

****
# 什么是动作管理器
* 动作管理器就是一个对象，管理**整个场景**中**所有的动作**。
* **一个SceneController（场景管理器）**只配备一个动作管理器对象。
* 不管是游戏角色的移动还是船的移动，都归这个对象管；
* 动作管理器可以添加动作（添加的时候要指定动作所**作用的GameObject**），监测已经完成的动作并清除。

![UML类图](http://upload-images.jianshu.io/upload_images/4888929-eff20e4691fa67d8.gif?imageMogr2/auto-orient/strip)
我下面对重要的类做出解释。
****
# ActionCallback 
这个接口很简单，就一个方法。实现了这个接口的类，就可以知道到“某个动作已完成”（动作一完成actionDone方法就会被调用），并对这个事件做出反应。

    public interface ActionCallback {
        void actionDone(ObjAction source);
    }

****
# ObjAction
ObjAction是所有动作的基类。ActionManager就是通过ObjAction这个接口来管理动作的。

    public class ObjAction : ScriptableObject
    {

		public bool enable = true;
		public bool destroy = false;

		public GameObject gameObject;
        public Transform transform;
        public ActionCallback whoToNotify;

        public virtual void Start()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Update()
        {
            throw new System.NotImplementedException();
        }
    }

ObjAction保存了这个动作作用的对象和这个对象的Transform组件（这里有一点多余了）。ObjAction只是一个动作的抽象，具体如何实现动作要让它的子类来实现Update方法。**注意ObjAction并不是MonoBehaviour的子类，它的Start和Update方法不会主动调用，我们会在后面一个MonoBehaviour类的Update中调用这个对象的Update方法。**

这个类还定义了一个ActionCallback的成员，用来保存动作完成时要通知的对象。

为什么要继承ScriptableObject呢？因为ScriptableObject有一些生命周期方法，等一下它的子类就要用到OnDestroy。

****
# MoveToAction
MoveToAction是ObjAction的一个实现，它代表一个直线移动的动作。

    public class MoveToAction : ObjAction
    {
        public Vector3 target;
        public float speed;

       private MoveToAction(){}
       public static MoveToAction getAction(Vector3 target, float speed) {
            MoveToAction action = ScriptableObject.CreateInstance<MoveToAction>();
            action.target = target;
            action.speed = speed;
            return action;
       }

       public override void Update() {
           this.transform.position = Vector3.MoveTowards(this.transform.position, target, speed*Time.deltaTime);
           if (this.transform.position == target) {
               this.destroy = true;
               this.whoToNotify.actionDone(this);
           }
       }

       public override void Start() {
           //
       }

    }

MoveToAction不能直接通过new来得到对象，只能通过它的静态方法getAction()来新建实例。

当MoveToAction发现自己的动作完成的时候，它会将自己标识为“要被销毁”，并通过`this.whoToNotify.actionDone(this);`告知whoToNotify动作已完成。
****
# SequenceAction
SequenceAction是ObjAction的另一个子类，它代表一连串MoveToAction组成的动作，也就是折线移动。

    public class SequenceAction: ObjAction, ActionCallback {
        public List<ObjAction> sequence;
        public int repeat = 1; // 1->only do it for once, -1->repeat forever
        public int currentActionIndex = 0;

        public static SequenceAction getAction(int repeat, int currentActionIndex, List<ObjAction> sequence) {
            SequenceAction action = ScriptableObject.CreateInstance<SequenceAction>();
            action.sequence = sequence;
            action.repeat = repeat;
            action.currentActionIndex = currentActionIndex;
            return action;
        }

        public override void Update() {
            if (sequence.Count == 0)return;
            if (currentActionIndex < sequence.Count) {
                sequence[currentActionIndex].Update();
            }
        }

        public void actionDone(ObjAction source) {
            source.destroy = false;
            this.currentActionIndex++;
            if (this.currentActionIndex >= sequence.Count) {
                this.currentActionIndex = 0;
                if (repeat > 0) repeat--;
                if (repeat == 0) {
                    this.destroy = true;
                    this.whoToNotify.actionDone(this);
                }
            }
        }

        public override void Start() {
            foreach(ObjAction action in sequence) {
                action.gameObject = this.gameObject;
                action.transform = this.transform;
                action.whoToNotify = this;
                action.Start();
            }
        }

        void OnDestroy() {
            foreach(ObjAction action in sequence) {
                DestroyObject(action);
            }
        }
    }

按照sequence的动作顺序，一个一个地执行下来，如果repeat大于0则从头再执行一次。

注意actionDone中有一句话`source.destroy = false;`因为MoveToAction到达指定地点以后会自动将自己标识为destroy，这一句话阻止它被销毁，因为如果有repeat，后面还要执行它。

SequenceAction是怎么知道某一个子动作已执行完呢？它实现了ActionCallback，并将子动作的whoToNotify指向自己，当子动作完成的时候就会调用自己的actionDone，它就会进入下一个动作。

这里的SequenceAction和MoveToAction看似是包含与被包含的关系，实际上它们都是ObjAction的子类。这就是一种**组合模式**，这样做的好处是它们的管理者ActionManager不需要区分谁是组合动作谁是单一动作，统统当作“动作（也就是ObjAction）”来处理。

> 回忆我们在[以前的文章](http://www.jianshu.com/p/c987b9259896#)说过，GameObject的父子关系也是一种组合模式。不管是组合的GameObject还是单一的GameObject，我们都可以当作普通的GameObject来处理（一样地操作Transform，一样地操作其他Component……），是不是与这里的组合模式有一些相似之处？

****
# ActionManager
ActionManager就是管理动作的类，它负责让动作“真正执行起来”，并销毁标记为destroy的动作。

    public class ActionManager: MonoBehaviour, ActionCallback {
        private Dictionary<int, ObjAction> actions = new Dictionary<int, ObjAction>();
        private List<ObjAction> waitingToAdd = new List<ObjAction>();
        private List<int> watingToDelete = new List<int>();

        protected void Update() {
            foreach(ObjAction ac in waitingToAdd) {
                actions[ac.GetInstanceID()] = ac;
            }
            waitingToAdd.Clear();

            foreach(KeyValuePair<int, ObjAction> kv in actions) {
                ObjAction ac = kv.Value;
                if (ac.destroy) {
                    watingToDelete.Add(ac.GetInstanceID());
                } else if (ac.enable) {
                    ac.Update();
                }
            }

            foreach(int key in watingToDelete) {
                ObjAction ac = actions[key];
                actions.Remove(key);
                DestroyObject(ac);
            }
            watingToDelete.Clear();
        }

        public void addAction(GameObject gameObject, ObjAction action, ActionCallback whoToNotify) {
            action.gameObject = gameObject;
            action.transform = gameObject.transform;
            action.whoToNotify = whoToNotify;
            waitingToAdd.Add(action);
            action.Start();
        }

        public void actionDone(ObjAction source) {
            
        }

    }

ActionManager实现了MonoBehaviour，因此它的Update方法在每一帧都会自动被调用，而在它的Update方法中又调用了**所有已添加的动作的Update**，这就是为什么**一个动作只有添加到了ActionManager才会真正执行起来**的原因！我们说过**ObjAction本身的Update不会自动被调用（不是MonoBehaviour的子类）**，它们需要靠ActionManager来“带动”。

****
# FirstSceneActionManager
FirstSceneActionManager是ActionManager的子类，FirstController就是通过它来管理所有动作的。本来有ActionManager似乎已经足够管理动作了，为什么还要实现一个子类FirstSceneActionManager来管理动作呢？FirstSceneActionManager针对具体的需求做了封装，让FirstController调用起来更简洁。

    public class FirstSceneActionManager:ActionManager {
        public void moveBoat(BoatController boat) {
		    MoveToAction action = MoveToAction.getAction(boat.getDestination(), boat.movingSpeed);
		    this.addAction(boat.getGameobj(), action, this);
        }

        public void moveCharacter(MyCharacterController characterCtrl, Vector3 destination) {
			Vector3 currentPos = characterCtrl.getPos();
			Vector3 middlePos = currentPos;
			if (destination.y > currentPos.y) {		//from low(boat) to high(coast)
				middlePos.y = destination.y;
			} else {	//from high(coast) to low(boat)
				middlePos.x = destination.x;
			}
			ObjAction action1 = MoveToAction.getAction(middlePos, characterCtrl.movingSpeed);
			ObjAction action2 = MoveToAction.getAction(destination, characterCtrl.movingSpeed);
			ObjAction seqAction = SequenceAction.getAction(1, 0, new List<ObjAction>{action1, action2});
			this.addAction(characterCtrl.getGameobj(), seqAction, this);
        }
    }

可以想象，如果我们不封装一个FirstSceneActionManager，而是直接使用ActionManager来管理场景中的动作，那么FirstController中的移动代码将会有多么臃肿！
****
# 修改后的FirstController 
最后让我们看看修改后的FirstController是怎么使用动作管理器的：
```
public class FirstController : MonoBehaviour, SceneController, UserAction {

	UserGUI userGUI;

	public CoastController fromCoast;
	public CoastController toCoast;
	public BoatController boat;
	private MyCharacterController[] characters;

	private FirstSceneActionManager actionManager;

	void Awake() {
		Director director = Director.getInstance ();
		director.currentSceneController = this;
		userGUI = gameObject.AddComponent <UserGUI>() as UserGUI;
		characters = new MyCharacterController[6];
		loadResources ();
	}

	void Start() {
		actionManager = GetComponent<FirstSceneActionManager>();
	}

	public void loadResources() {
		//GameObject water = Instantiate (Resources.Load ("Perfabs/Water", typeof(GameObject)), water_pos, Quaternion.identity, null) as GameObject;
		//water.name = "water";

		fromCoast = new CoastController ("from");
		toCoast = new CoastController ("to");
		boat = new BoatController ();

		loadCharacter ();
	}

	private void loadCharacter() {
		for (int i = 0; i < 3; i++) {
			MyCharacterController cha = new MyCharacterController ("priest");
			cha.setName("priest" + i);
			cha.setPosition (fromCoast.getEmptyPosition ());
			cha.getOnCoast (fromCoast);
			fromCoast.getOnCoast (cha);

			characters [i] = cha;
		}

		for (int i = 0; i < 3; i++) {
			MyCharacterController cha = new MyCharacterController ("devil");
			cha.setName("devil" + i);
			cha.setPosition (fromCoast.getEmptyPosition ());
			cha.getOnCoast (fromCoast);
			fromCoast.getOnCoast (cha);

			characters [i+3] = cha;
		}
	}


	public void moveBoat() {
		if (boat.isEmpty ())
			return;
		/*	old way to move boat
		boat.Move ();
		*/
		actionManager.moveBoat(boat);
		boat.move();
		userGUI.status = check_game_over ();
	}

	public void characterIsClicked(MyCharacterController characterCtrl) {
		if (characterCtrl.isOnBoat ()) {
			CoastController whichCoast;
			if (boat.get_to_or_from () == -1) { // to->-1; from->1
				whichCoast = toCoast;
			} else {
				whichCoast = fromCoast;
			}

			boat.GetOffBoat (characterCtrl.getName());
			//characterCtrl.moveToPosition (whichCoast.getEmptyPosition ());
			actionManager.moveCharacter(characterCtrl, whichCoast.getEmptyPosition ());
			characterCtrl.getOnCoast (whichCoast);
			whichCoast.getOnCoast (characterCtrl);

		} else {									// character on coast
			CoastController whichCoast = characterCtrl.getCoastController ();

			if (boat.getEmptyIndex () == -1) {		// boat is full
				return;
			}

			if (whichCoast.get_to_or_from () != boat.get_to_or_from ())	// boat is not on the side of character
				return;

			whichCoast.getOffCoast(characterCtrl.getName());
			//characterCtrl.moveToPosition (boat.getEmptyPosition());
			actionManager.moveCharacter(characterCtrl, boat.getEmptyPosition());
			characterCtrl.getOnBoat (boat);
			boat.GetOnBoat (characterCtrl);
		}
		userGUI.status = check_game_over ();
	}

	int check_game_over() {	// 0->not finish, 1->lose, 2->win
		int from_priest = 0;
		int from_devil = 0;
		int to_priest = 0;
		int to_devil = 0;

		int[] fromCount = fromCoast.getCharacterNum ();
		from_priest += fromCount[0];
		from_devil += fromCount[1];

		int[] toCount = toCoast.getCharacterNum ();
		to_priest += toCount[0];
		to_devil += toCount[1];

		if (to_priest + to_devil == 6)		// win
			return 2;

		int[] boatCount = boat.getCharacterNum ();
		if (boat.get_to_or_from () == -1) {	// boat at toCoast
			to_priest += boatCount[0];
			to_devil += boatCount[1];
		} else {	// boat at fromCoast
			from_priest += boatCount[0];
			from_devil += boatCount[1];
		}
		if (from_priest < from_devil && from_priest > 0) {		// lose
			return 1;
		}
		if (to_priest < to_devil && to_priest > 0) {
			return 1;
		}
		return 0;			// not finish
	}

	public void restart() {
		boat.reset ();
		fromCoast.reset ();
		toCoast.reset ();
		for (int i = 0; i < characters.Length; i++) {
			characters [i].reset ();
		}
	}
}
```
****
谢谢阅读！
