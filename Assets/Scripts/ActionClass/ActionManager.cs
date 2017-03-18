using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;


namespace Com.Mygame
{
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

    
}