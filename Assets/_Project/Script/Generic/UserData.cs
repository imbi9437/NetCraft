using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Generic;
using UnityEngine;

[Serializable]
public class UserData
{
    public string nickname;
    public string uid;

    public WorldData worldData;

    public UserData(string nickname, string uid)
    {
        this.nickname = nickname;
        this.uid = uid;
    }
}
