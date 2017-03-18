using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

namespace Com.Mygame {
    public interface ActionCallback {
        void actionDone(ObjAction source);
    }
}