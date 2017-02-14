using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Globalization;
using System.Linq;

/*
 * 大学帰宅時刻表 
 * やんないといけない実装
 * 　休業中ダイヤ分岐実装
 * 　コメント挿入
 * 　
 * やってる実装 
 * 　平日・土曜時刻分け
 * 　時刻をjsonで管理
 * 　次回、次々回のバス到着時刻表示
 * 　
 * 
 * 
 * 
 */

public class MyJsonData
{
    public int id;
    public float val;
    public string text;
    public string[] takasaka;
    public string[] takadoyo;
    public string[] kitasakado;
    public string[] kitadoyo;
    public string[] kumagaya;
    public string[] kumadoyo;

}

public class JsonTest : MonoBehaviour {

    private string filepath = "";
    public string jsonName="jsonTest.json";
    public Text nowTime;
    public Text takasakaZikai;
    public Text kitasakaZikai;
    public Text kumagayaZikai;
    public Text hitokoto;

    MyJsonData data;

    private float time =0.0f;
    private CultureInfo ci;
    private DateTime dt;
    private DayOfWeek week;

	// Use this for initialization
    void Start()
    {
        ci = new CultureInfo("ja-JP", false);
        StartCoroutine("JsonYobi");
    }
	
	 //Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        Debug.Log(time.ToString("f2"));
        if (time >= 10.0f)
        {
            StartCoroutine("JsonYobi");
            time = 0.0f;
        }
        nowTime.text =DateTime.Now.ToString(ci) + "\n" + DateTime.Now.ToString("dddd" ,ci);
	}


    IEnumerator JsonYobi()
    {
        Debug.Log("JsonYobi Reload.");
#if UNITY_ANDROID
        filepath = "file://" + Application.persistentDataPath +"/"+ jsonName;
#else
        filepath = "file://" + Application.streamingAssetsPath +"/"+ jsonName;
#endif
        WWW www = new WWW(filepath);
        yield return www;

        if (www.bytesDownloaded == 0)
        {
            hitokoto.text = "jsonが無いか、名前が間違ってて読み込めない:;(∩´﹏`∩);:";
            hitokoto.color = Color.red;
            yield break;
        }

        data = JsonUtility.FromJson<MyJsonData>(www.text);

        hitokoto.text = data.text;

        Debug.Log(www.text);
        Debug.Log(data.takasaka[0] + data.id + data.val + data.text);

        TakasakaTimes();
        KitasakaTimes();
        KumagayaTimes();

    }

    public void TakasakaTimes()
    {
        takasakaZikai.text = "";
        DisplayTimes(takasakaZikai,data.takasaka,data.takadoyo);
    }

    public void KitasakaTimes()
    {
        kitasakaZikai.text = "";
        DisplayTimes(kitasakaZikai,data.kitasakado,data.kitadoyo);

    }

    public void KumagayaTimes()
    {
        kumagayaZikai.text = "";
        DisplayTimes(kumagayaZikai,data.kumagaya,data.kumadoyo);
    }

    public void DisplayTimes(Text zikai,string[] heizitu,string[] doyo)
    {
        dt = DateTime.Now;
        week = dt.DayOfWeek;

        if (week != DayOfWeek.Saturday && week != DayOfWeek.Sunday)
        {
            SetTimes(zikai, heizitu, doyo);
        }
        else if (week == DayOfWeek.Saturday)
        {
            SetTimes(zikai, heizitu, doyo);
        }
        else
        {
            zikai.text += "本日の便はありません";
            zikai.color = Color.black;
        }
    }

    public void SetTimes(Text zikai, string[] heizitu, string[] doyo)
    {

        List<string> zikoku = new List<string>();
        for (int i = 0; i < heizitu.Length; i++)
        {
            DateTime dt2 = DateTime.Parse(heizitu[i]);
            Debug.Log(dt + "////" + dt2);

            if (dt <= dt2)
            {
                //この内部処理内、dt2で扱わないと1つ前が通らなかった
                zikoku.Add(dt2.ToString("HH:mm"));
                Debug.Log(dt2.ToString("HH:mm"));
            }

        }
        if (zikoku.Count >= 2)
        {
            zikai.text += "次回　　：" + zikoku.FirstOrDefault() + "\n";
            zikai.text += "次々回　：" + zikoku[1] + "\n";
            zikai.color = Color.black;
        }
        else if (zikoku.Count == 1)
        {
            zikai.text += "次回　　：" + zikoku.FirstOrDefault() + "\n";
            zikai.text += "上記便が最終です\n";
            zikai.color = Color.red;
        }
        else
        {
            zikai.text += "本日の便はありません";
        }


    }

}
