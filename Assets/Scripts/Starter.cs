using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//using System.IO;
using System;
using System.Text;

public class Starter : MonoBehaviour
{
    public Text Result;
    public void Load()
    {

        string url = "http://37.200.64.12:1620";

        WWWForm form = new WWWForm();
        var headers = form.headers;
        headers["Content-Type"] = "application/json";
        string json = "{\"header\":{\"method\":\"load\",\"login\":1125498,\"auth\":\"fd3f5a0620a8e2793c42f490200abdac\"}}";

        byte[] bytes = Encoding.UTF8.GetBytes(json);
        WWW www = new WWW(url, bytes, headers);

        //WWW www = new WWW(url, bytes);

        //while (!www.isDone) { }

        //if (www.error == null)
        //{
        //    Result.text = www.text;
        //}
        //else
        //{
        //    Result.text = www.error;
        //}

        StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWW www)
    {
        while (!www.isDone)
            yield return www;

        // check for errors
        if (www.error == null)
        {
            Result.text = www.text;
        }
        else
        {
            Result.text = www.error;
        }
    }

    //{
    //    var request = (HttpWebRequest)WebRequest.Create("http://37.200.64.12:1620");

    //    var postData = "{\"header\":{\"method\":\"load\",\"login\":1125498,\"auth\":\"fd3f5a0620a8e2793c42f490200abdac\"}}";
    //    var data = Encoding.ASCII.GetBytes(postData);

    //    request.Method = "POST";
    //    request.ContentType = "application/x-www-form-urlencoded";
    //    request.ContentLength = data.Length;

    //    using (var stream = request.GetRequestStream())
    //    {
    //        stream.Write(data, 0, data.Length);
    //    }

    //    var response = (HttpWebResponse)request.GetResponse();

    //    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

    //    Result.text = responseString;
    //}

    void Start()
    {

    }

    void Update()
    {

    }
}
