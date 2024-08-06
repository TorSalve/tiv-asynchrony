using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[System.Serializable]
public class UserData
{
    public QuestionAnswer[] answers;
    
    public bool test;

    public string device;
    public string IP;
    
    public long start;
    public long end;

    public UserData(List<QuestionAnswer> answers, bool isTest)
    {
        this.test = isTest;

        this.answers = answers.ToArray();
        
        this.setDeviceInfo();
        this.setIP();
        this.setStart();
    }

    private long timestamp()
    {
        return new System.DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
    }

    public void setIP()
    {
        if(!this.test)
        {
            this.IP = new WebClient().DownloadString("http://icanhazip.com").Trim();
        }
        else {
            this.IP = "localhost";
        }
    }

    public void setDeviceInfo()
    {
        this.device = SystemInfo.deviceModel;
    }

    public void setStart()
    {
        this.start = this.timestamp();
    }

    public void setEnd()
    {
        this.end = this.timestamp();
    }
}

[System.Serializable]
public class QuestionAnswer
{
    public string key;
    public int value;

    public QuestionAnswer(string key, int value)
    {
        this.key = key;
        this.value = value;
    }
}

