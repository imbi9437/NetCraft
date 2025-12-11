using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

//정규 표현식 관련 기능을 모아놓은 확장 클래스 
public static class RegexExtensions 
{   //Regex(Regular Expression) : 정규 표현식 
    
    //닉네임으로 0~9, a~z , A~Z , 가~힣 안에 포함되는 완성형 한글 및 영문, 숫자만 포함하는 정규식 

    static string nicknameRegexIncludeText = @"[0-9a-zA-Z가-힣]"; //위의 범위 안에 있는 문자를 필터링 하겠다 

    private static string nicknameRegexExcludeText = @"[^0-9a-zA-Z가-힣]"; //위에 범위를 벗어나는 문자를 필터링 

    //Regex 클래스 활용법 1번 : 필터링 할 정규표현식을 파라미터로 새 Regex 객체 생성.
    private static Regex nicknameRegex = new Regex(@"[^0-9a-zA-Z가-힣]");
    //닉네임에 적합한 문자열인지 확인하는 확장함수 
    public static bool NicknameValidate(this string nickname){
        //Regex.IsMatch : 이 정규표현식에 해당하는 문자가 하나라도 있으면 True를 변환 
        return false ==  nicknameRegex.IsMatch(nickname); 
        //완성형 문자에 해당하지 않는 문자가 하나라도 있으면 false를 반환해야 하므로 nicknameExcludeText에 IsMatch 여부를 반대로 반환 
        
    }
    
    //Regex 클래스 활용법2번 : Regex의 static 함수를 이용항 포맷 문자열로 변환
    //문자열 입력중에 미완성형 한글을 허용하되 특수만자 입력은 제외하도록 필터링 하는 정규 표현식 
    private static string inputForm = @"[^0-9a-zA-Z가-힣ㄱ-ㅎㅏ-ㅣㆍᆞᆢ]";

    public static string ToValidString(this string param)
    {   //Regex.Replace : 첫번쨰 파라미터 문자열을 정규식으로 하여 ,해당하는 문자는 모두 교체하는 함수 
        return Regex.Replace(param , inputForm ,"",RegexOptions.Singleline);
        //예를 들어 파라미터로 넘어온 문자에 !나 ?가 포함되어 있을 경우 , inputForm을 정규식패턴으로 활용할 떄 포함되는 문자가 되므로 
        //replacement로 대체됨("" 빈문자열)
    }

    private static string completeHangel = @"[^가-힝]"; //완성형 한글이 아닌 문자는 모두 걷어낼 예정 

    //일반적인 완성형 한글로 이루어진 비속어 
    private static List<string> fword = new List<string>()
    {
        "씨발", "개새끼", "너희어머니", "너희아버지"
    }; 
    
    //변형 비속어
    private static List<string> irregularFword = new List<string>()
    {
        "ㅅㅐ771"
    };

    public static bool ContainsFword(this string param)
    {
        if (string.IsNullOrEmpty(param)) return false; 
        //빈 문자열에 비속어가 포함되어 있을리 없으니 false리턴 
        
        //변형 비속어를 먼저 검사
        if (irregularFword.Exists(param.Contains))
        {
            return true;
        }
        
        //완성형 한글만 남김 . 예 : 예1쁜1말 => 예쁜말 
        param = Regex.Replace(param, completeHangel,"",RegexOptions.Singleline);
        
        //완성형 한글만 남긴 문자열에서 fword에 포함된 단어가 있는지 여무를 return
        return fword.Exists(param.Contains);
    }



}
