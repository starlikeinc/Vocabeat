using UnityEngine;

public static class SecurePrefs
{
    public static void SetString(string key, string value)
    {
        // 무결성 체크까지 포함한 고급 암호화 사용
        string enc = LUIZ.Security.SimpleEncrypter.DoEncryptAdv(value ?? string.Empty);
        PlayerPrefs.SetString(key, enc);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        if (!PlayerPrefs.HasKey(key))
            return defaultValue;

        string enc = PlayerPrefs.GetString(key);
        try
        {
            string dec = LUIZ.Security.SimpleEncrypter.DoDecryptAdv(enc);
            // DoDecryptAdv가 ""을 리턴하면 "깨진 데이터"로 간주
            return string.IsNullOrEmpty(dec) ? defaultValue : dec;
        }
        catch
        {
            // Base64 깨짐 / 암호화 방식 변경 등
            return defaultValue;
        }
    }

    public static void SetInt(string key, int value)
    {
        SetString(key, value.ToString());
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        string s = GetString(key, null);
        if (string.IsNullOrEmpty(s))
            return defaultValue;

        return int.TryParse(s, out int result) ? result : defaultValue;
    }

    public static void Save() => PlayerPrefs.Save();
}
