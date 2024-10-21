using System;

/// <summary>
/// 系统相关数据
/// </summary>
public class SystemDataManager : IDisposable
{
    /// <summary>
    /// 当前的服务器时间
    /// </summary>
    public long CurServerTime;

    /// <summary>
    /// 当前渠道设置
    /// </summary>
    public ChannelConfigEntity CurChannelConfig {
        get;
        private set;
    }


    public SystemDataManager() {
        CurChannelConfig = new ChannelConfigEntity();
    }

    /// <summary>
    /// 清空数据(游戏周期内可以不清空)
    /// </summary>
    public void Clear() {

    }

    public void Dispose() {

    }

}
