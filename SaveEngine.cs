using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Reflection;

/*!
 * \brief Save engine core function
 * 
 * SaveEngine class
 * * Create empty GameObject and add SaveEngine.cs on property.
 */


[System.Serializable]
public class SaveEngine : Singleton<SaveEngine>{
    [System.Serializable]
    class SaveContent
    {
        public enum ContentType { properties, field}
        public Dictionary<string, string> Contents;
        public ContentType type;
        
        public SaveContent()
        {
            Contents = new Dictionary<string, string>();
        }
    }
    [System.Serializable]
    class SaveTemplate
    {
        public Dictionary<string, SaveContent> SaveContents;
    }

    public string SaveName;//!< Name for your save file

    private string savePath;
    private List<object> ListObjectToSave;
    private Dictionary<string, SaveContent> ListContents;

    private void Awake()
    {
        base.Awake();
        savePath = Application.persistentDataPath + "/" + SaveName + ".sav";
        ListObjectToSave = new List<object>();
        ListContents = new Dictionary<string, SaveContent>();
        LoadGame(); //!< Load the game when Awake
    }

    private void Start()
    {
        
    }

    /*! 
     * * Register which property or field that must be save. 
     * * This function also load data from saved file. 
     * 
     * usage: 
     *>    SaveEngine.Instance.RegisterSaveClass(this);
     * 
     */
    public void RegisterSaveClass<T> (T thisClass)
    {
        
        int i = 0;
        foreach(object obj in ListObjectToSave)
        {
            if(obj == null)
            {
                ListObjectToSave.RemoveAt(i);
            }
            i++;
        }
        ListObjectToSave.Add(thisClass);
        foreach(object obj in ListObjectToSave)
        {
            Debug.Log("object exist : " + obj);
        }
        syncCurrentDataWithLoadedData(thisClass);
    }

    /*!
     * * This function for save game data 
     * * Data is from all registered properties and field (see SaveManager)
     * 
     * 
     * usage: 
     *>     SaveEngine.Instance.SaveGame();
     * 
     */

    public void SaveGame()
    {
        // 1. load all properties data 
        ListContents = GetSaveContent();
        // 2. replace save data on file
        saveDataToBinnary(ListContents);

        //LoadGame();
    }

    private void LoadGame()
    {
        ListContents = LoadDataFromBinnary();
    }

    private void syncCurrentDataWithLoadedData(object thisClass)
    {
        SaveContent dataFromList = ListContents[thisClass.GetType().ToString()];
        if(dataFromList != null)
        {
            Dictionary<string, string> contents = dataFromList.Contents;
            foreach (PropertyInfo o in thisClass.GetType().GetProperties())
            {
                try
                {
                    if (o.GetCustomAttributes(typeof(SaveManager), false)[0] != null)
                    {
                        SaveManager saveAttr = (SaveManager)o.GetCustomAttributes(true)[0];
                        if(contents[saveAttr.NameSave] != null)
                        {
                            setValue(thisClass, o, contents[saveAttr.NameSave]);
                        }
                    }
                }
                catch (Exception) { }
            }

            foreach (FieldInfo o in thisClass.GetType().GetFields())
            {
                try
                {
                    if (o.GetCustomAttributes(typeof(SaveManager), false)[0] != null)
                    {
                        SaveManager saveAttr = (SaveManager)o.GetCustomAttributes(true)[0];
                        if (contents[saveAttr.NameSave] != null)
                        {
                            setValue(thisClass, o, contents[saveAttr.NameSave]);
                        }
                    }
                }
                catch (Exception) { }
            }
        }
    }

    private void setValue(object thisClass, PropertyInfo info, string value)
    {

        Type type = info.GetType();

        switch (type.ToString())
        {
            case "System.Int32" :
                int val = 0;
                Int32.TryParse(value, out val);
                info.SetValue(thisClass, val, null);
                break;
            case "System.String":
                info.SetValue(thisClass, value, null);
                break;
            case "System.Single":
                info.SetValue(thisClass, float.Parse(value), null);
                break;
            case "System.Double":
                info.SetValue(thisClass, double.Parse(value), null);
                break;
            default:
                break;
        }
        
    }
    private void setValue(object thisClass, FieldInfo info, string value)
    {

        Type type = info.GetType();

        switch (type.ToString())
        {
            case "System.Int32":
                int val = 0;
                Int32.TryParse(value, out val);
                info.SetValue(thisClass, val);
                break;
            case "System.String":
                info.SetValue(thisClass, value);
                break;
            case "System.Single":
                info.SetValue(thisClass, float.Parse(value));
                break;
            case "System.Double":
                info.SetValue(thisClass, double.Parse(value));
                break;
            default:
                break;
        }

    }

    private void setValueField(object thisClass, FieldInfo info)
    {

    }

    private Dictionary<string, SaveContent> GetSaveContent()
    {
        Dictionary<string, SaveContent> t_contents = new Dictionary<string, SaveContent>();
        foreach (object obj in ListObjectToSave)
        {
            SaveContent contents = getSaveProperty(obj);
            t_contents[obj.GetType().ToString()] = contents;
            Debug.Log("Save " + obj.GetType());
            foreach (KeyValuePair<string, string> entry in contents.Contents)
            {
                Debug.Log("      " + entry.Key + " : " + entry.Value);
            }
        }

        return t_contents;
    }

    private Dictionary<string, SaveContent> LoadDataFromBinnary()
    {
        SaveTemplate result = new SaveTemplate();
        if (File.Exists(savePath))
        {
            FileStream file = File.Open(savePath, FileMode.Open);
            BinaryFormatter bin = new BinaryFormatter();
            result = (SaveTemplate)bin.Deserialize(file);
            file.Close();

            foreach (KeyValuePair<string, SaveContent> e in result.SaveContents)
            {
                Debug.Log("Load data key : " + e.Key);

            }
        }

        return result.SaveContents;
    }

    private void saveDataToBinnary(Dictionary<string, SaveContent> contents)
    {
        FileStream file = File.Open(savePath, FileMode.Create);
        Debug.Log("Save to path " + savePath);
        BinaryFormatter bin = new BinaryFormatter();
        SaveTemplate template = new SaveTemplate();
        template.SaveContents = contents;
        bin.Serialize(file, template);
        file.Close();
    }

    private SaveContent getSaveProperty<T>(T thisClass)
    {
        //1. get data from class
        Dictionary<string, SaveContent> contents = new Dictionary<string, SaveContent>();
        Dictionary<string, string> contentContents = new Dictionary<string, string>();

        object[] test = thisClass.GetType().GetProperties();
        Debug.Log("Length attribute " + thisClass.GetType() + " is " + test.Length);
        foreach (PropertyInfo o in thisClass.GetType().GetProperties())
        {
            try
            {
                if (o.GetCustomAttributes(typeof(SaveManager), false)[0] != null)
                {
                    object[] saveAttr = o.GetCustomAttributes(true);
                    if(saveAttr.Length > 0)
                    {
                        SaveManager sman = (SaveManager)saveAttr[0];
                        contentContents[sman.NameSave] = o.GetValue(thisClass, null).ToString();
                        Debug.Log("Property player " + sman.NameSave + " = " + contentContents[sman.NameSave] + " sata type " + o.GetValue(thisClass, null).GetType());
                    }
                }
            }
            catch (Exception) { }
        }

        foreach (FieldInfo o in thisClass.GetType().GetFields())
        {
            try
            {
                if (o.GetCustomAttributes(typeof(SaveManager), false)[0] != null)
                {
                    object[] saveAttr = o.GetCustomAttributes(true);
                    if (saveAttr.Length > 0)
                    {
                        SaveManager sman = (SaveManager)saveAttr[0];
                        contentContents[sman.NameSave] = o.GetValue(thisClass).ToString();
                        Debug.Log("Property player " + sman.NameSave + " = " + contentContents[sman.NameSave] + " sata type " + o.GetValue(thisClass).GetType());
                    }
                }
            }
            catch (Exception) { }
        }

        SaveContent savec = new SaveContent();
        savec.Contents = contentContents;

        return savec;
    }

    protected override void Init()
    {
    }
}
