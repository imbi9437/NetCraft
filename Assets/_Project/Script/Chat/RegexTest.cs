using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegexTest : MonoBehaviour
{
    private void Start()
    {
        print("안녕 ? 반가워".ToValidString());
        
        print("ㅗ ㅑ ㅊㅊ ㅈㅈㅈㅂㄷㅂ".NicknameValidate());
        print("AB마마".NicknameValidate());
    }
}
