using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

namespace Com.Mygame {
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
}