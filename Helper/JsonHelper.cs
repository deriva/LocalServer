using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;

namespace Bc.LocalServer
{
    /// <summary>
    /// JSON帮助类
    /// </summary>
    public static class JsonHelper
    {
        //private static readonly JavaScriptSerializer _jss = new JavaScriptSerializer();

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
        }

        public static object ResponseJson(bool success, string message = "", object data = null)
        {
            return new { success = success, message = message, data = data };
        }

        public static object JsonObject(bool success, string message = "", object data = null)
        {
            return new { success = success, message = message, attr = data };
        }


        /// <summary>
        /// 返回JSON，object对象字符串
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="message">消息</param>
        /// <param name="data">附加信息，是一个object对象</param>
        /// <returns></returns>
        public static string ToObjectStr(bool success, string message = "", object data = null)
        {
            var obj = new { success = success, message = message, attr = data };
            return Serialize(obj);
        }

        /// <summary>
        /// 返回验证错误响应信息
        /// 错误格式：ErrorMessage ， PropertyName
        /// 前后台需格式统一
        /// </summary>
        /// <param name="errors">错误消息</param>
        /// <returns>JSON</returns>
        public static object ValidateErrorResponse(Dictionary<string, string> errors)
        {
            var list = new List<object>();
            foreach (var error in errors)
            {
                list.Add(new { PropertyName = error.Key, ErrorMessage = error.Value });
            }
            return JsonObject(false, "", list);
        }

        /// <summary>
        /// 返回验证错误响应信息
        /// 错误格式：ErrorMessage ， PropertyName
        /// 前后台需格式统一
        /// </summary>
        /// <param name="errorMessage">错误小心</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>JSON</returns>
        public static object ValidateErrorResponse(string propertyName, string errorMessage)
        {
            var list = new List<object> { new { ErrorMessage = errorMessage, PropertyName = propertyName } };
            return JsonObject(false, "", list);
        }
        public static string Serialize(bool success, string message = "", object data = null)
        {
            return JsonConvert.SerializeObject(new { success = success, message = message, attr = data });
        }
        /// <summary>
        /// 反序列化json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Json"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string Json)
        {
            return JsonConvert.DeserializeObject<T>(Json);
        }
        /// <summary>
        /// 反序列化json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Json"></param>
        /// <returns></returns>
        public static T DtToObj<T>(this DataTable dt) where T : class
        {
            if (dt == null) return null;
            var json = ToJson(dt);
            return JsonConvert.DeserializeObject<T>(json);
        }
        #region Json与DataTable互转 add by  2017-11-17

        #region DataTable 转换为Json 字符串
        /// <summary>
        /// DataTable 对象 转换为Json 字符串
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToJson(DataTable dt)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  //实例化一个参数集合
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    dictionary.Add(dataColumn.ColumnName, dataRow[dataColumn.ColumnName].ToStr());
                }
                arrayList.Add(dictionary); //ArrayList集合中添加键值
            }

            return javaScriptSerializer.Serialize(arrayList);  //返回一个json字符串
        }
        private static void DataColumnToLower(DataTable dt)
        {
            foreach (DataColumn item in dt.Columns)
            {
                item.ColumnName = item.ColumnName;
            }
        }

        public static string DataTableToJson(DataTable dt, int records)
        {

            DataColumnToLower(dt);
            if (records <= 0) records = dt.Rows.Count;

            string json = "{\"total\":\"" + records + "\",\"rows\":" + ToJson(dt) + "}";
            return json;
        }
        #endregion

        #region Json 字符串 转换为 DataTable数据集合
        /// <summary>
        /// Json 字符串 转换为 DataTable数据集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(string json)
        {
            DataTable dataTable = new DataTable();  //实例化
            DataTable result;
            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);
                if (arrayList.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in arrayList)
                    {
                        if (dictionary.Keys.Count<string>() == 0)
                        {
                            result = dataTable;
                            return result;
                        }
                        if (dataTable.Columns.Count == 0)
                        {
                            foreach (string current in dictionary.Keys)
                            {
                                dataTable.Columns.Add(current, dictionary[current].GetType());
                            }
                        }
                        DataRow dataRow = dataTable.NewRow();
                        foreach (string current in dictionary.Keys)
                        {
                            dataRow[current] = dictionary[current];
                        }

                        dataTable.Rows.Add(dataRow); //循环添加行到DataTable中
                    }
                }
            }
            catch
            {
            }
            result = dataTable;
            return result;
        }
        #endregion

        #region 转换为string字符串类型
        /// <summary>
        ///  转换为string字符串类型
        /// </summary>
        /// <param name="s">获取需要转换的值</param>
        /// <param name="format">需要格式化的位数</param>
        /// <returns>返回一个新的字符串</returns>
        public static string ToStr(this object s, string format = "")
        {
            string result = "";
            try
            {
                if (s == null) return "";
                if (string.IsNullOrWhiteSpace(format))
                {
                    result = s.ToString();
                }
                else
                {
                    result = string.Format("{0:" + format + "}", s);
                }
            }
            catch
            {
            }
            return result == "null" ? "" : result;

        }
        #endregion


        #endregion

        public static T Deserialize<T>(string strJson)
        {
            if (strJson == null)
                strJson = "";
            return JsonConvert.DeserializeObject<T>(strJson);
        }

        /// <summary>
        /// 通过JSON序列化，复制实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T CloneJson<T>(T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        /// <summary>
        /// Assign parameters to specified objects
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="dic">Fields/values</param>
        /// <returns></returns>
        public static T Assign<T>(Dictionary<string, string> dic) where T : new()
        {
            Type myType = typeof(T);
            T entity = new T();
            var fields = myType.GetProperties();
            string val = string.Empty;
            object obj = null;

            foreach (var field in fields)
            {
                if (!dic.ContainsKey(field.Name))
                    continue;
                val = dic[field.Name];

                object defaultVal;
                if (field.PropertyType.Name.Equals("String"))
                    defaultVal = "";
                else if (field.PropertyType.Name.Equals("Boolean"))
                {
                    defaultVal = false;
                    val = (val.Equals("1") || val.Equals("on")).ToString();
                }
                else if (field.PropertyType.Name.Equals("Decimal"))
                    defaultVal = 0M;
                else
                    defaultVal = 0;

                if (!field.PropertyType.IsGenericType)
                    obj = string.IsNullOrEmpty(val) ? defaultVal : Convert.ChangeType(val, field.PropertyType);
                else
                {
                    Type genericTypeDefinition = field.PropertyType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Nullable<>))
                        obj = string.IsNullOrEmpty(val) ? defaultVal : Convert.ChangeType(val, Nullable.GetUnderlyingType(field.PropertyType));
                }

                field.SetValue(entity, obj, null);
            }

            return entity;
        }
    }

}
