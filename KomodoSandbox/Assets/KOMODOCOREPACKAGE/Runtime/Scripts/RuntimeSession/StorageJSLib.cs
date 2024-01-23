using UnityEngine;
using System.Runtime.InteropServices;

public class StorageJSLib : MonoBehaviour
{
    //[DllImport("__Internal")]
    //public static extern void config();

    [DllImport("__Internal")]
    public static extern string getItem(string key);

    [DllImport("__Internal")]
    public static extern void setItem(string key, string value);

    [DllImport("__Internal")]
    public static extern void removeItem(string key);

    [DllImport("__Internal")]
    public static extern void keys();

    //void Start()
    //{
    //    //  config("{\"name\":\"myApp\",\"version\":1.0,\"storeName\":\"myStore\"}");
    //   // config();
      

    //    //setItem("myKey", "myValue");

    //    //string value = getItem("myKey");

    //    //Debug.Log(value);
    //   // removeItem("myKey");
    //}

   
}