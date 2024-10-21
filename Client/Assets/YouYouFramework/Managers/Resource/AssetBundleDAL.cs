using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace YouYou
{
    /// <summary>
    /// AssetBundle管理类
    /// </summary>
    public class AssetBundleDAL
    {
        /// <summary>
        /// XML路径
        /// </summary>
        private string mXmlPath;

        private XDocument mXDoc;

        /// <summary>
        /// 返回的数据集合
        /// </summary>
        private List<AssetBundleEntity> mDataList = null;

        public AssetBundleDAL(string path) {
            mXmlPath = path;
            mDataList = new List<AssetBundleEntity>();
            //读取XML,把数据添加到mDataList里面
            mXDoc = XDocument.Load(mXmlPath);
        }

        /// <summary>
        /// 获取版本号
        /// </summary>
        public string GetVersion() {
            XElement root = mXDoc.Root;
            XElement assetBundleNode = root.Element("AssetBundle");
            XAttribute attribute = assetBundleNode.Attribute("ResourceVersion");
            return attribute.Value;
        }

        /// <summary>
        /// 升级版本号
        /// </summary>
        public void UpdateVersion() {
            XElement root = mXDoc.Root;
            XElement assetBundleNode = root.Element("AssetBundle");
            XAttribute attribute = assetBundleNode.Attribute("ResourceVersion");
            string version = attribute.Value;
            string[] arr = version.Split('.');

            int shortVersion = int.Parse(arr[2]);
            version = string.Format("{0}.{1}.{2}", arr[0], arr[1], ++shortVersion);
            attribute.SetValue(version);
            mXDoc.Save(mXmlPath);
        }

        /// <summary>
        /// 返回XML数据
        /// </summary>
        /// <returns></returns>
        public List<AssetBundleEntity> GetList() {
            mDataList.Clear();

            XElement root = mXDoc.Root;
            XElement assetBundleNode = root.Element("AssetBundle");
            IEnumerable<XElement> lst = assetBundleNode.Elements("Item");
            int index = 0;
            foreach (XElement item in lst) {
                AssetBundleEntity entity = new AssetBundleEntity();
                entity.Key = "key" + ++index;
                entity.Name = item.Attribute("Name").Value;
                entity.Tag = item.Attribute("Tag").Value;
                entity.Overall = item.Attribute("Overall").Value.Equals("True", StringComparison.CurrentCultureIgnoreCase);
                entity.IsFirstData = item.Attribute("IsFirstData").Value.Equals("True", StringComparison.CurrentCultureIgnoreCase);
                entity.IsEncrypt = item.Attribute("IsEncrypt").Value.Equals("True", StringComparison.CurrentCultureIgnoreCase);

                IEnumerable<XElement> pathList = item.Elements("Path");
                foreach (XElement path in pathList) {
                    entity.PathList.Add(path.Attribute("Value").Value);
                }
                mDataList.Add(entity);
            }

            return mDataList;
        }

    }
}
