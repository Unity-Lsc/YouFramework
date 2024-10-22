using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 启动流程
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {

        public override void OnEnter() {
            base.OnEnter();
            //GameEntry.Log("OnEnter ProcedureLaunch", LogCategory.Procedure);

            ////访问帐号服务器
            //string url = GameEntry.Http.RealWebAccountUrl + "/api/init";
            //Dictionary<string, object> dict = GameEntry.Pool.DequeueClassObject<Dictionary<string, object>>();
            //dict.Clear();

            //GameEntry.Data.SystemDataManager.CurChannelConfig.ChannelId = 0;
            //GameEntry.Data.SystemDataManager.CurChannelConfig.InnerVersion = 1001;

            //dict["ChannelId"] = 0;
            //dict["InnerVersion"] = 1001;
            //GameEntry.Http.SendData(url, OnWebAccountInit, true, dict);


            var action = GameEntry.Time.CreateTimeAction();
            action.Init(0.5f, () => {
                ToCheckVersion();
            });
            action.Run();
        }

        private void OnWebAccountInit(HttpCallBackArgs args) {
            if (!args.HasError) {
                LitJson.JsonData data = LitJson.JsonMapper.ToObject(args.Value);
                LitJson.JsonData config = LitJson.JsonMapper.ToObject(data["Value"].ToString());

                GameEntry.Data.SystemDataManager.CurChannelConfig.ServerTime = long.Parse(config["ServerTime"].ToString());
                GameEntry.Data.SystemDataManager.CurChannelConfig.SourceVersion = config["SourceVersion"].ToString();
                GameEntry.Data.SystemDataManager.CurChannelConfig.SourceUrl = config["SourceUrl"].ToString();
                GameEntry.Data.SystemDataManager.CurChannelConfig.RechargeUrl = config["RechargeUrl"].ToString();
                GameEntry.Data.SystemDataManager.CurChannelConfig.TDAppId = config["TDAppId"].ToString();
                GameEntry.Data.SystemDataManager.CurChannelConfig.IsOpenTD = int.Parse(config["SourceVersion"].ToString()) == 1;

                Debug.Log("RealSourceUrl" + GameEntry.Data.SystemDataManager.CurChannelConfig.RealSourceUrl);
                GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
            }
        }

        /// <summary>
        /// 切换到检查版本更新的状态(测试使用)
        /// </summary>
        private void ToCheckVersion() {
            GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
        }

        public override void OnUpdate() {
            base.OnUpdate();
        }

        public override void OnLeave() {
            base.OnLeave();
        }

        public override void OnDestroy() {
            base.OnDestroy();
        }

    }
}
