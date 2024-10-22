using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class ProcedureBase : FsmState<ProcedureManager>
    {

        public override void OnEnter() {
            GameEntry.Log("OnEnter " + GetType().Name, LogCategory.Procedure);
        }

        public override void OnUpdate() {
            
        }

        public override void OnLeave() {
            GameEntry.Log("OnLeave " + GetType().Name, LogCategory.Procedure);
        }

        public override void OnDestroy() {
            GameEntry.Log("OnDestroy " + GetType().Name, LogCategory.Procedure);
        }



    }
}
