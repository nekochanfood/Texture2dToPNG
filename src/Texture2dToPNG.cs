using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class Texture2dToPNG : EditorWindow
{
    [MenuItem("Tools/Convert Texture2D to PNG")]
    [MenuItem("Assets/Create PNG from Texture2D")]
    private static void ConvertTexture2d() {
        var window = GetWindow<Texture2dToPNG>();
        window.minSize = new Vector2(360,540);
        window.titleContent = new GUIContent("Texture2dToPNG");
    }

    public Texture2D[] texture2D;
    public string path;
    public string FileName;
    int Format;
    string[] Format_str = new string[] { "PNG", "JPEG", "TGA" };
    int[] Format_int = new int[] { 0, 1, 2 };
    private Vector2 pos = Vector2.zero;
    bool foldout1 = true;

    void OnGUI ()
    {
            pos = EditorGUILayout.BeginScrollView(pos);
            ScriptableObject target = this;
            SerializedObject a = new SerializedObject(target);
            SerializedProperty texture2dProperty = a.FindProperty("texture2D");
            EditorGUILayout.PropertyField(texture2dProperty, true);
            a.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            foldout1 = EditorGUILayout.Foldout( foldout1, "設定" );
            if ( foldout1 == true ) {
            Format = EditorGUILayout.IntPopup("フォーマット", Format, Format_str, Format_int); 
            path = EditorGUILayout.TextField("保存先 (任意)", path);
            FileName = EditorGUILayout.TextField("ファイル名 (任意)", FileName);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }
            if (GUILayout.Button("実行"))
            {
                Convert(Format,FileName,path,texture2D);
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Texture2dToPNG v1.0 by nekochanfood");
                GUILayout.FlexibleSpace();
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            
    }
    
    Texture2D createReadabeTexture2D(Texture2D texture2d)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
                    texture2d.width,
                    texture2d.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);
        Graphics.Blit(texture2d, renderTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D readableTextur2D = new Texture2D(texture2d.width, texture2d.height);
        readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTextur2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readableTextur2D;
    }

    public void Convert(int Format,string FileName,string _path,Texture2D[] texture)
    {
        int i,o;
        string[] _missing = new string[texture.Length];
        string missing = "0, ";
        string notmissing = "";
        bool firstmissing = false;
        bool UseTexture2dName = false;
        byte[] bytes = null;
        string _format = "";
        for(i=0;i!=texture.Length;i++)
        {
            if(texture[i] == null)
            {
                for(o=0;o!=_missing.Length;o++)
                {
                    if(_missing[o] == "")
                    {
                        _missing[o] = i.ToString();
                        break;
                    }
                }
                if(!firstmissing)
                {
                    firstmissing = true;
                    missing = "";
                }
                missing = missing + i.ToString() + ", ";
            }else
            {
                notmissing = notmissing + i.ToString() + ", ";
            }
        }
        if(missing != "")
        {
            missing = missing.Remove((missing.Length - 2));
            if(missing == "0" && notmissing != ""){
            }else
            {
                if(notmissing != "")
                {
                    notmissing = notmissing.Remove((notmissing.Length - 2));
                    bool ConvertOnlyNotMissing = EditorUtility.DisplayDialog("確認","Texture[" + missing + "]にTexture2dが設定されていませんが、\nTexture[" + notmissing + "]にはTexture2dが設定されています。このまま実行しますか？", "はい","いいえ");
                    if(!ConvertOnlyNotMissing)return;
                }else
                {
                    EditorUtility.DisplayDialog("エラー","Texture[" + missing + "]にTexture2dが設定されていません。", "了解");
                    Debug.LogError("Texture[" + missing + "]にTexture2dが設定されていません。");
                    return;
                }
            } 
        }
        string DateAndTime = System.DateTime.Now.ToString().Replace("/","_").Replace(":",".").Replace(" ","-");
        _path = _path.Replace("<date>",DateAndTime);
        if(_path == "")
        {
            _path = EditorUtility.OpenFolderPanel("フォルダを指定してください","","");
        }else if(!Directory.Exists(_path))
        {
            bool CreateFolder = EditorUtility.DisplayDialog("確認","選択されたパスは存在しません。フォルダを作成しますか?","はい","いいえ");
            if(CreateFolder)
            {
                Directory.CreateDirectory(_path);
            }else
            {
                return;
            }
        }
        if( _path.EndsWith("/") || _path.EndsWith("\\") ) _path = _path.Remove((_path.Length - 1));
        bool skipConvert = false;
        int success = 0;
        int passed = 0;
        for(i=0;i!=texture.Length;i++)
        {
            if(FileName == "") UseTexture2dName = true;
            if(texture[i] == null) skipConvert = true;
            if(!skipConvert)
            {
                switch(Format)
                {
                case 0:
                    bytes = createReadabeTexture2D(texture[i]).EncodeToPNG();
                    _format = ".png";
                    break;
                case 1:
                    bytes = createReadabeTexture2D(texture[i]).EncodeToJPG();
                    _format = ".jpeg";
                    break;
                case 2:
                    bytes = createReadabeTexture2D(texture[i]).EncodeToTGA();
                    _format = ".tga";
                    break;
                default:
                    _format = "";
                    break;
                }
                string size = createReadabeTexture2D(texture[i]).height + "x" + createReadabeTexture2D(texture[i]).width;
                string path = _path + "/";
                if(UseTexture2dName) FileName = texture[i].name;
                success++;
                File.WriteAllBytes(path + size + "_" + DateAndTime + "." + FileName + i.ToString() + _format, bytes);
                Debug.Log("\"" + texture[i].name + "\" の" + _format.Replace(".","").ToUpper() + "ファイルの生成に成功しました。");
            }else if(skipConvert)
            {
                passed++;
                skipConvert = false;
            } 
        }
        if(UseTexture2dName) FileName = null;
        Debug.Log("変換完了"+"( 成功: "+success.ToString()+", パス: "+passed+" )"+"\n指定したパス: " + _path);
        System.Diagnostics.Process.Start(_path);
    }
}