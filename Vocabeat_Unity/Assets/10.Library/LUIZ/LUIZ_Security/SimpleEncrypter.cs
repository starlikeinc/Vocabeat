using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

namespace LUIZ.Security
{
    public class SimpleEncrypter
    {
        private static string m_saltForKey;

        private static byte[] m_saltBytes;
        private static byte[] m_keys;

        private const int m_keySize = 256;
        private const int m_blockSize = 128;
        private const int m_hashLen = 32;

        static SimpleEncrypter()
        {
            //aes기반
            //8 바이트로 하고, 변경해서 쓸것
            m_saltBytes = new byte[] { 25, 36, 8, 44, 43, 14, 60, 39 };

            //길이 상관 없음, 키를 만들기 위한 용도로 씀
            string randomHashSeed = "5b6fcb4aaa0a42acae649eba45a506ga";

            //길이 상관 없음, aes에 쓸 key 용도
            string part1 = "2e32772578";
            string part2 = "9841b5bb5c";
            string part3 = "706d6b2ad8";
            string part4 = "070g8hqgas";

            string randomEncryptSeed = part1 + part2 + part3 + part4;

            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomHashSeed, m_saltBytes, 1000);
                m_saltForKey = System.Convert.ToBase64String(key.GetBytes(m_blockSize / 8));
            }

            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(randomEncryptSeed, m_saltBytes, 1000);
                m_keys = key.GetBytes(m_keySize / 8);
            }
        }

        //----------------------------------------------------------
        //키가 유출되면 뚫릴 수 있기 때문에
        //디바이스 고유 정보, 유저 정보 등의 접속마다 일정한 정보로 동적 생성하면 더 안전할 수 있음.
        //물론 같은 데이터 파일을 다른 기계에서도 재사용 가능하게 하고 싶을수도 있으니... 필요할 경우 적절히 이용
        public static void SetCustomSeed(string customHashSeed, string customEncryptSeed)
        {
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(customHashSeed, m_saltBytes, 1000);
                m_saltForKey = System.Convert.ToBase64String(key.GetBytes(m_blockSize / 8));
            }

            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(customEncryptSeed, m_saltBytes, 1000);
                m_keys = key.GetBytes(m_keySize / 8);
            }
        }

        //----------------------------------------------------------
        /// <summary> string이나 특정 key를 간단하게 난독화 할때 이용 (단방향임) </summary>
        public static string DoMakeHashSimple(string stringOrigin)
        {
            string encryptValue = MakeHash(stringOrigin + m_saltForKey);
            return encryptValue;
        }

        //------------------------------------------------------------------------
        //간단한 암호화
        public static string DoEncryptSimple(string input)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted);

            return System.Convert.ToBase64String(bytesEncrypted);
        }
        
        public static string DoDecryptSimple(string input)
        {
            byte[] bytesToBeDecrypted = System.Convert.FromBase64String(input);
            byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        //------------------------------------------------------------------------
        //데이터가 좀 길어지고 처리가 좀더 걸리지만 Simple보다 좀 더 안전한 암호화
        public static string DoEncryptAdv(string stringOrigin)
        {
            string encryptValue = DoEncryptSimple(stringOrigin + MakeHash(stringOrigin));
            return encryptValue;
        }

        public static string DoDecryptAdv(string stringOrigin)
        {
            if (string.IsNullOrEmpty(stringOrigin))
                return string.Empty;

            string valueAndHash = DoDecryptSimple(stringOrigin);

            if (m_hashLen > valueAndHash.Length)
                return string.Empty;

            string savedValue = valueAndHash.Substring(0, valueAndHash.Length - m_hashLen);
            string savedHash = valueAndHash.Substring(valueAndHash.Length - m_hashLen);

            if (MakeHash(savedValue) != savedHash)
                return string.Empty;

            return savedValue;
        }

        //----------------------------------------------------------------------------
        private static string MakeHash(string original)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(original);
                byte[] hashBytes = md5.ComputeHash(bytes);

                string hashToString = "";
                for (int i = 0; i < hashBytes.Length; ++i)
                    hashToString += hashBytes[i].ToString("x2");

                return hashToString;
            }
        }

        private static byte[] Encrypt(byte[] bytesToBeEncrypted)
        {
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.KeySize = m_keySize;
                aes.BlockSize = m_blockSize;

                aes.Key = m_keys;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (ICryptoTransform ct = aes.CreateEncryptor())
                {
                    byte[] encrypted = ct.TransformFinalBlock(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);

                    byte[] combined = new byte[iv.Length + encrypted.Length];
                    Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
                    Buffer.BlockCopy(encrypted, 0, combined, iv.Length, encrypted.Length);

                    return combined;
                }
            }
        }

        private static byte[] Decrypt(byte[] bytesToBeDecrypted)
        {
            using (RijndaelManaged aes = new RijndaelManaged())
            {
                aes.KeySize = m_keySize;
                aes.BlockSize = m_blockSize;

                aes.Key = m_keys;

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                int ivSize = m_blockSize / 8;
                byte[] iv = new byte[ivSize];
                byte[] cipherText = new byte[bytesToBeDecrypted.Length - ivSize];

                Buffer.BlockCopy(bytesToBeDecrypted, 0, iv, 0, ivSize);
                Buffer.BlockCopy(bytesToBeDecrypted, ivSize, cipherText, 0, cipherText.Length);

                aes.IV = iv;

                using (ICryptoTransform ct = aes.CreateDecryptor())
                {
                    return ct.TransformFinalBlock(cipherText, 0, cipherText.Length);
                }
            }
        }
    }
}
