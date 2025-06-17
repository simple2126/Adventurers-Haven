
/*     ===== Do not touch this. Auto Generated Code. =====    */
/*     If you want custom code generation modify this => 'CodeGeneratorUnityEngine.cs'  */
using GoogleSheet.Protocol.v2.Res;
using GoogleSheet.Protocol.v2.Req;
using UGS;
using System;
using UGS.IO;
using GoogleSheet;
using System.Collections.Generic;
using System.IO;
using GoogleSheet.Type;
using System.Reflection;
using UnityEngine;


namespace AdventurersHaven
{
    [GoogleSheet.Attribute.TableStruct]
    public partial class RestaurantCon_Data : ITable, IConstructionSubData
    { 

        public delegate void OnLoadedFromGoogleSheets(List<RestaurantCon_Data> loadedList, Dictionary<string, RestaurantCon_Data> loadedDictionary);

        static bool isLoaded = false;
        static string spreadSheetID = "1DE69qF4w2rp6ZoH9e1cs3t_OK4SO1F7KoRRX16lpkog"; // it is file id
        static string sheetID = "1647072879"; // it is sheet id
        static UnityFileReader reader = new UnityFileReader();

/* Your Loaded Data Storage. */
    
        public static Dictionary<string, RestaurantCon_Data> RestaurantCon_DataMap = new Dictionary<string, RestaurantCon_Data>();  
        public static List<RestaurantCon_Data> RestaurantCon_DataList = new List<RestaurantCon_Data>();   

        /// <summary>
        /// Get RestaurantCon_Data List 
        /// Auto Load
        /// </summary>
        public static List<RestaurantCon_Data> GetList()
        {{
           if (isLoaded == false) Load();
           return RestaurantCon_DataList;
        }}

        /// <summary>
        /// Get RestaurantCon_Data Dictionary, keyType is your sheet A1 field type.
        /// - Auto Load
        /// </summary>
        public static Dictionary<string, RestaurantCon_Data>  GetDictionary()
        {{
           if (isLoaded == false) Load();
           return RestaurantCon_DataMap;
        }}

    

/* Fields. */

		public System.String id;
		public System.String tag;
		public System.String name;
		public System.Int32 buildCost;
		public System.Collections.Generic.List<Int32> blockSize;
		public System.Int32 sales;
		public System.Int32 salesIncrement;
		public System.Int32 upgradeCost;
		public System.Int32 costIncrement;
		public System.Int32 maxLevel;

        public string ID => id;
        public string Tag => tag;
        public string Name => name;
        public int[] BlockSize => blockSize?.ToArray() ?? new int[0];
        public int BuildCost => buildCost;
        public int Sales => sales;
        public int SalesIncrement => salesIncrement;
        public int UpgradeCost => upgradeCost;
        public int CostIncrement => costIncrement;
        public int MaxLevel => maxLevel;

        #region fuctions


        public static void Load(bool forceReload = false)
        {
            if(isLoaded && forceReload == false)
            {
#if UGS_DEBUG
                 Debug.Log("RestaurantCon_Data is already loaded! if you want reload then, forceReload parameter set true");
#endif
                 return;
            }

            string text = reader.ReadData("AdventurersHaven"); 
            if (text != null)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ReadSpreadSheetResult>(text);
                CommonLoad(result.jsonObject, forceReload); 
                if(!isLoaded)isLoaded = true;
            }
      
        }
 

        public static void LoadFromGoogle(System.Action<List<RestaurantCon_Data>, Dictionary<string, RestaurantCon_Data>> onLoaded, bool updateCurrentData = false)
        {      
                IHttpProtcol webInstance = null;
    #if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    webInstance = UnityEditorWebRequest.Instance as IHttpProtcol;
                }
                else 
                {
                    webInstance = UnityPlayerWebRequest.Instance as IHttpProtcol;
                }
    #endif
    #if !UNITY_EDITOR
                     webInstance = UnityPlayerWebRequest.Instance as IHttpProtcol;
    #endif
          
 
                var mdl = new ReadSpreadSheetReqModel(spreadSheetID);
                webInstance.ReadSpreadSheet(mdl, OnError, (data) => {
                    var loaded = CommonLoad(data.jsonObject, updateCurrentData); 
                    onLoaded?.Invoke(loaded.list, loaded.map);
                });
        }

               


    public static (List<RestaurantCon_Data> list, Dictionary<string, RestaurantCon_Data> map) CommonLoad(Dictionary<string, Dictionary<string, List<string>>> jsonObject, bool forceReload){
            Dictionary<string, RestaurantCon_Data> Map = new Dictionary<string, RestaurantCon_Data>();
            List<RestaurantCon_Data> List = new List<RestaurantCon_Data>();     
            TypeMap.Init();
            FieldInfo[] fields = typeof(RestaurantCon_Data).GetFields(BindingFlags.Public | BindingFlags.Instance);
            List<(string original, string propertyName, string type)> typeInfos = new List<(string, string, string)>(); 
            List<List<string>> rows = new List<List<string>>();
            var sheet = jsonObject["RestaurantCon_Data"];

            foreach (var column in sheet.Keys)
            {
                string[] split = column.Replace(" ", null).Split(':');
                         string column_field = split[0];
                string   column_type = split[1];

                typeInfos.Add((column, column_field, column_type));
                          List<string> typeValues = sheet[column];
                rows.Add(typeValues);
            }

          // 실제 데이터 로드
                    if (rows.Count != 0)
                    {
                        int rowCount = rows[0].Count;
                        for (int i = 0; i < rowCount; i++)
                        {
                            RestaurantCon_Data instance = new RestaurantCon_Data();
                            for (int j = 0; j < typeInfos.Count; j++)
                            {
                                try
                                {
                                    var typeInfo = TypeMap.StrMap[typeInfos[j].type];
                                    //int, float, List<..> etc
                                    string type = typeInfos[j].type;
                                    if (type.StartsWith(" < ") && type.Substring(1, 4) == "Enum" && type.EndsWith(">"))
                                    {
                                         Debug.Log("It's Enum");
                                    }

                                    var readedValue = TypeMap.Map[typeInfo].Read(rows[j][i]);
                                    fields[j].SetValue(instance, readedValue);

                                }
                                catch (Exception e)
                                {
                                    if (e is UGSValueParseException)
                                    {
                                        Debug.LogError("<color=red> UGS Value Parse Failed! </color>");
                                        Debug.LogError(e);
                                        return (null, null);
                                    }

                                    //enum parse
                                    var type = typeInfos[j].type;
                                    type = type.Replace("Enum<", null);
                                    type = type.Replace(">", null);

                                    var readedValue = TypeMap.EnumMap[type].Read(rows[j][i]);
                                    fields[j].SetValue(instance, readedValue); 
                                }
                              
                            }
                            List.Add(instance); 
                            Map.Add(instance.id, instance);
                        }
                        if(isLoaded == false || forceReload)
                        { 
                            RestaurantCon_DataList = List;
                            RestaurantCon_DataMap = Map;
                            isLoaded = true;
                        }
                    } 
                    return (List, Map); 
}


 

        public static void Write(RestaurantCon_Data data, System.Action<WriteObjectResult> onWriteCallback = null)
        { 
            TypeMap.Init();
            FieldInfo[] fields = typeof(RestaurantCon_Data).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var datas = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                var type = fields[i].FieldType;
                string writeRule = null;
                if(type.IsEnum)
                {
                    writeRule = TypeMap.EnumMap[type.Name].Write(fields[i].GetValue(data));
                }
                else
                {
                    writeRule = TypeMap.Map[type].Write(fields[i].GetValue(data));
                } 
                datas[i] = writeRule; 
            }  
           
#if UNITY_EDITOR
if(Application.isPlaying == false)
{
                UnityPlayerWebRequest.Instance.WriteObject(new WriteObjectReqModel(spreadSheetID, sheetID, datas[0], datas), OnError, onWriteCallback);

}
else
{
            UnityPlayerWebRequest.Instance.WriteObject(new  WriteObjectReqModel(spreadSheetID, sheetID, datas[0], datas), OnError, onWriteCallback);

}
#endif

#if !UNITY_EDITOR
   UnityPlayerWebRequest.Instance.WriteObject(new  WriteObjectReqModel(spreadSheetID, sheetID, datas[0], datas), OnError, onWriteCallback);

#endif
        } 
          

 


#endregion

#region OdinInsepctorExtentions
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Button("UploadToSheet")]
    public void Upload()
    {
        Write(this);
    }
 
    
#endif


 
#endregion
    public static void OnError(System.Exception e){
         UnityGoogleSheet.OnTableError(e);
    }
 
    }
}
        