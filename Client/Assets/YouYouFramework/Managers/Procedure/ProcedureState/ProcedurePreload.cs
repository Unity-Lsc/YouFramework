using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 预加载流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        public override void OnEnter() {
            base.OnEnter();
            GameEntry.Resource.InitAssetInfo();
            GameEntry.Event.CommonEvent.AddListener(SystemEventId.LoadOneDataTableComplete, OnLoadOneDataTableComplete);
            GameEntry.Event.CommonEvent.AddListener(SystemEventId.LoadDataTableComplete, OnLoadDataTableComplete);
            GameEntry.DataTable.LoadDataTableAsync();
        }

        public override void OnUpdate() {
            base.OnUpdate();
        }

        public override void OnLeave() {
            base.OnLeave();
            GameEntry.Event.CommonEvent.RemoveListener(SystemEventId.LoadDataTableComplete, OnLoadDataTableComplete);
            GameEntry.Event.CommonEvent.RemoveListener(SystemEventId.LoadOneDataTableComplete, OnLoadOneDataTableComplete);
        }

        
        public override void OnDestroy() {
            base.OnDestroy();
        }

        /// <summary>
        /// 加载单一表完毕
        /// </summary>
        private void OnLoadOneDataTableComplete(object param) {
            GameEntry.DataTable.DataTableManager.CurLoadCount++;
            if(GameEntry.DataTable.DataTableManager.CurLoadCount == GameEntry.DataTable.DataTableManager.TotalLoadCount) {
                GameEntry.Event.CommonEvent.Dispatch(SystemEventId.LoadDataTableComplete);
            }
        }

        /// <summary>
        /// 加载全部表完毕
        /// </summary>
        private void OnLoadDataTableComplete(object param) {
            GameEntry.Log("加载所有表完毕");
        }

    }
}
