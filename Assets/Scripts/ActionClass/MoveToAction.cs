using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

namespace Com.Mygame
{
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
}