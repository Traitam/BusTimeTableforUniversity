using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
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

public struct ViewInfo
{
    public string[] _viewTakasaka;
    public string[] _viewKitasakado;
    public string[] _viewKumagaya;
    public string _hitokoto;
}

public enum BusPlace
{
    Takasaka = 0,
    Kitasakado = 1,
    Kumagaya = 2,
    other = 99
}


public class JsonTest : MonoBehaviour
{

    private string _filepath = "";
    [SerializeField]
    private string jsonName = "jsonTest.json";
    [SerializeField]
    private Text nowTime;
    [SerializeField]
    private Text takasakaZikai;
    [SerializeField]
    private Text kitasakaZikai;
    [SerializeField]
    private Text kumagayaZikai;
    [SerializeField]
    private Text hitokoto;

    MyJsonData data;

    private float time = 0.0f;
    private CultureInfo _cultureInfo;
    private DateTime _dateTime;
    private DayOfWeek _week;

    private ViewInfo _viewInfo;



    // Use this for initialization
    private void Start()
    {
        Initialize();
    }

    //Update is called once per frame
    private void Update()
    {
        time += Time.deltaTime;
        View(time);
    }

    #region Initialize
    private void Initialize()
    {
#if UNITY_ANDROID
        _filepath = "file://" + Application.persistentDataPath +"/"+ jsonName;
#else
        _filepath = "file://" + Application.streamingAssetsPath + "/" + jsonName;
#endif
        _cultureInfo = new CultureInfo("ja-JP", false);
        data = new MyJsonData();
        _viewInfo = new ViewInfo();
        ViewInitialize();   
        StartCoroutine(LoadJson(_viewInfo));

    }
    #endregion

    public void View(float time)
    {
        if (time >= 10.0f)
        {
            ViewInitialize();
            StartCoroutine(LoadJson(_viewInfo));
            time = 0.0f;

            //各種テキスト記述

        }
        Debug.Log(time.ToString("f2"));
        nowTime.text = DateTime.Now.ToString(_cultureInfo) + "\n" + DateTime.Now.ToString("dddd", _cultureInfo);

    }

    public void ViewInitialize()
    {
        takasakaZikai.text = "";
        kitasakaZikai.text = "";
        kumagayaZikai.text = "";
    }



    IEnumerator LoadJson(ViewInfo viewInfo)
    {
        Debug.Log("JsonYobi Reload.");
        WWW www = new WWW(_filepath);
        yield return www;

        if (www.bytesDownloaded == 0)
        {
            ErrorMessage();
            yield break;
        }

        PerseJson(www.text);
    }

    public void ErrorMessage()
    {
        hitokoto.text = "jsonが無いか、名前が間違ってて読み込めない:;(∩´﹏`∩);:";
        hitokoto.color = Color.red;
    }

    public void PerseJson(string wwwText)
    {
        data = JsonUtility.FromJson<MyJsonData>(wwwText);

        hitokoto.text = data.text;

        Debug.Log(data.takasaka[0] + data.id + data.val + data.text);

        MiddleViewtime();

    }

    public void MiddleViewtime()
    {
        PlaceTimes(BusPlace.Takasaka);
        PlaceTimes(BusPlace.Kitasakado);
        PlaceTimes(BusPlace.Kumagaya);
    }

    public void PlaceTimes(BusPlace busPlace)
    {
        switch (busPlace)
        {
            case BusPlace.Takasaka:
                CheeseTimes(takasakaZikai, data.takasaka, data.takadoyo);
                break;
            case BusPlace.Kitasakado:
                CheeseTimes(kitasakaZikai, data.kitasakado, data.kitadoyo);
                break;
            case BusPlace.Kumagaya:
                CheeseTimes(kumagayaZikai, data.kumagaya, data.kumadoyo);
                break;
            case BusPlace.other:
                break;
        }

    }

    public void CheeseTimes(Text zikai, string[] heizitu, string[] doyo)
    {
        _dateTime = DateTime.Now;
        _week = _dateTime.DayOfWeek;

        if (_week != DayOfWeek.Saturday && _week != DayOfWeek.Sunday)
        {
            SetTimes(heizitu);
        }
        else if (_week == DayOfWeek.Saturday)
        {
            SetTimes(doyo);
        }
        else
        {
        }
    }

    public void SetTimes(string[] timeTable)
    {
        //Listに2つ表示用時刻追加
        List<string> zikoku = new List<string>();
        for (int i = 0; i < timeTable.Length; i++)
        {
            DateTime dt2 = DateTime.Parse(timeTable[i]);
            //Debug.Log(dt + "////" + dt2);

            if (_dateTime <= dt2)
            {
                //この内部処理内、dt2で扱わないと1つ前が通らなかった
                zikoku.Add(dt2.ToString("HH:mm"));
                //Debug.Log(dt2.ToString("HH:mm"));
            }

        }
        InputInfo(BusPlace.Takasaka, zikoku);
        InputInfo(BusPlace.Kitasakado, zikoku);
        InputInfo(BusPlace.Kumagaya, zikoku);

    }

    public void InputInfo(BusPlace place ,List<string> viewList)
    {
        switch (place)
        {
            case BusPlace.Takasaka:
                InputDatas(place, viewList, takasakaZikai);
                break;
            case BusPlace.Kitasakado:
                InputDatas(place, viewList, kitasakaZikai);
                break;
            case BusPlace.Kumagaya:
                InputDatas(place, viewList, kumagayaZikai);
                break;
            case BusPlace.other:
                break;
        }

    }

    public void InputDatas(BusPlace place, List<string> viewList,Text ViewText)
    {
        if (viewList.Count >= 2)
        {
            ViewText.text += "次回　　：" + viewList.FirstOrDefault() + "\n";
            ViewText.text += "次々回　：" + viewList[1] + "\n";
            ViewText.color = Color.black;
        }
        else if (viewList.Count == 1)
        {
            ViewText.text += "次回　　：" + viewList.FirstOrDefault() + "\n";
            ViewText.text += "上記便が最終です\n";
            ViewText.color = Color.red;
        }
        else
        {
            ViewText.text += "本日の便はありません";
            ViewText.color = Color.black;
        }

    }

}
