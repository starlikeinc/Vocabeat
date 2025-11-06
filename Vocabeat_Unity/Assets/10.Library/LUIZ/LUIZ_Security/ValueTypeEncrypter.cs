using System;
using System.Text;
using System.Runtime.InteropServices;

// [개요] 대칭키 기반의 간략한 메모리 암호화로 간단한 메모리 핵 방지용
// [주의] 케릭터 문자열의 경우 실제 문자열 길이와 BufferSize 가 같을 경우 해쉬가 같으므로 좀 널널하기 수동조작할것
// [주의] 출력을 할 때마다 메모리가 할당되고 버퍼가 섞이는등 참조에 많은 비용이 발생하므로 캐싱 값을 쓸지 복호화 값을 쓸지 잘 선택할 것
//        Ex) 간단한 유아이 출력등에선 캐싱값을 이용하고 중요 로직 이용시에만 복호화 한다는 등..

namespace LUIZ.Security
{
    public abstract class ValueTypeEncrypterRootBase
    {
        public enum EEncryptValueType
        {
            None,

            Int,
            UInt,
            ULong,

            Bool,

            Float,

            String,
        }

        private EEncryptValueType m_valueType = EEncryptValueType.None;

        //------------------------------------------------------
        protected void SetEncryptValueType(EEncryptValueType encryptValueType) { m_valueType = encryptValueType; }

        //------------------------------------------------------
        public EEncryptValueType ValueType => m_valueType;
    }

    public abstract class ValueTypeEncrypterBase<T> : ValueTypeEncrypterRootBase where T : struct
    {
        private byte[] m_aryBufferValue;
        private byte[] m_aryBufferKey;
        private byte[] m_aryBufferExport;

        private IntPtr m_nativeBuffer; //AllocHGlobal를 통해 할당된 Unmanaged메모리의 값 포인터

        private T m_cachedValue = default(T);     // 복호화한 캐시 값
        private bool m_isCachedValid = false;

        public int BufferTotalSize { get; private set; } = 0;
        public int BufferSizeUse { get; private set; } = 0;
        public bool IsValueLock { get; private set; } = false; //Lock하면  Set을 하지 않는다. 실수로 할당하는 것을 방지 

        //-------------------------------------------------------------------
        protected ValueTypeEncrypterBase(int iSize)
        {
            BufferTotalSize = iSize;
            PrivEncryptAllocBufferSize(BufferTotalSize);
        }

        //-------------------------------------------------------------------
        public void SetValueLock(bool isLock)
        {
            IsValueLock = isLock;
        }

        public void SetValue(T value)
        {
            if (IsValueLock)
                return;

            ProtEncryptDataReset();
            ProtEncryptDataValueAdd(value);
            ProtEncryptDataStart();

            m_cachedValue = value;
            m_isCachedValid = true;
        }

        /// <summary> isDecrypt = true 이면 암호화된 값을 복호화 한 후 가져온다. false 일 경우 캐싱된 값을 이용하므로 복호화 비용X /// </summary>
        public T GetValue(bool isDecrypt = false)
        {
            if (!isDecrypt && m_isCachedValid)
                return m_cachedValue;

            T realValue = ProtDecryptDataStart(0, Marshal.SizeOf(typeof(T)));
            m_cachedValue = realValue;
            m_isCachedValid = true;

            return realValue;
        }

        //--------------------------------------------------------------------
        private void PrivEncryptAllocBufferSize(int size)
        {
            m_aryBufferKey = new byte[size];
            m_aryBufferValue = new byte[size];
            m_aryBufferExport = new byte[size];
            m_nativeBuffer = Marshal.AllocHGlobal(size);

            PrivFillBufferRandom(m_aryBufferKey, 0, size);
        }

        private void PrivEncryptFillFakeData()
        {
            PrivFillBufferRandom(m_aryBufferValue, BufferSizeUse, m_aryBufferValue.Length);
        }

        private void PrivEncryptSymmeticalValue()
        {
            for (int i = 0; i < m_aryBufferValue.Length; i++)
            {
                m_aryBufferValue[i] = (byte)(m_aryBufferValue[i] ^ m_aryBufferKey[i]);
            }
        }

        private void PrivDecryptSymmeticalValue(int start, int end)
        {
            if (end > BufferTotalSize)
            {
                //Error!!
                end = BufferTotalSize;
            }

            int totalLength = start + end;
            for (int i = start; i < totalLength; i++)
            {
                m_aryBufferExport[i] = (byte)(m_aryBufferValue[i] ^ m_aryBufferKey[i]);
            }
        }

        private void PrivFillBufferRandom(byte[] aryBuffer, int startPosition, int end)
        {
            if (end > aryBuffer.Length)
            {
                end = aryBuffer.Length;
            }

            for (int i = startPosition; i < end; i++)
            {
                aryBuffer[i] = (byte)UnityEngine.Random.Range(0, byte.MaxValue);
            }
        }

        //---------------------------------------------------------
        protected void ProtEncryptDataValueAdd(T value)
        {
            int valueSize = Marshal.SizeOf(value);

            if (BufferSizeUse + valueSize > m_aryBufferValue.Length)
            {
                return;
            }

            Marshal.StructureToPtr(value, m_nativeBuffer, false);
            Marshal.Copy(m_nativeBuffer, m_aryBufferValue, BufferSizeUse, valueSize);
            Marshal.WriteByte(m_nativeBuffer, 0);
            BufferSizeUse += valueSize;
        }

        protected void ProtEncryptDataReset()
        {
            BufferSizeUse = 0;
        }

        protected void ProtEncryptDataStart()
        {
            //남은 버퍼 공간을 페이크 데이터로 채운다.
            PrivEncryptFillFakeData();
            PrivEncryptSymmeticalValue();
        }

        protected T ProtDecryptDataStart(int startPosition, int decryptSize)
        {
            PrivDecryptSymmeticalValue(startPosition, decryptSize);
            Marshal.Copy(m_aryBufferExport, startPosition, m_nativeBuffer, decryptSize);
            T Data = (T)Marshal.PtrToStructure(m_nativeBuffer, typeof(T));
            Marshal.WriteByte(m_nativeBuffer, 0);
            // 출력한 이후에 원본을 변조하여 값을 노출 시키지 않는다.
            PrivFillBufferRandom(m_aryBufferExport, startPosition, startPosition + decryptSize);
            return Data;
        }
        //---------------------------------------------------------
    }

    public class EnInt : ValueTypeEncrypterBase<int>
    {
        public const byte DefaultCryptionBufferSize = 16; // 4바이트  int 4 개 규모

        public EnInt() : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.Int);
        }

        public EnInt(int iValue) : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.Int);
            SetValue(iValue);
        }

        static public implicit operator int(EnInt pEnInt)
        {
            return pEnInt.GetValue(true);
        }
    }

    public class EnUInt : ValueTypeEncrypterBase<uint>
    {
        public const byte DefaultCryptionBufferSize = 16; // 4바이트  int 4 개 규모

        public EnUInt() : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.UInt);
        }

        public EnUInt(uint value) : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.UInt);
            SetValue(value);
        }

        static public implicit operator uint(EnUInt pEnUInt)
        {
            return pEnUInt.GetValue(true);
        }
    }

    public class EnULong : ValueTypeEncrypterBase<ulong>
    {
        public const byte DefaultCryptionBufferSize = 32; // 32바이트  uint64  4개 규모

        public EnULong() : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.ULong);
        }

        public EnULong(ulong value) : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.ULong);
            SetValue(value);
        }

        static public implicit operator ulong(EnULong pEnULong)
        {
            return pEnULong.GetValue(true);
        }
    }

    public class EnBool : ValueTypeEncrypterBase<bool>
    {
        public const byte DefaultCryptionBufferSize = 8; // 1바이트 bool × 8 (페이크 데이터 포함)

        public EnBool() : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.Bool);
        }

        public EnBool(bool value) : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.Bool);
            SetValue(value);
        }

        static public implicit operator bool(EnBool pEnBool)
        {
            return pEnBool.GetValue(true);
        }
    }

    public class EnFloat : ValueTypeEncrypterBase<float>
    {
        public const byte DefaultCryptionBufferSize = 16; // 4바이트 float × 4 (페이크 포함)

        public EnFloat() : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.Float);
        }

        public EnFloat(float value) : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.Float);
            SetValue(value);
        }

        static public implicit operator float(EnFloat pEnFloat)
        {
            return pEnFloat.GetValue(true);
        }
    }

    public class EnString : ValueTypeEncrypterBase<char>
    {
        public const byte DefaultCryptionBufferSize = 128; // 64개 문자갯수 
        private StringBuilder m_pStringNote;
        private string m_sCachedValue;
        private bool m_bCachedValid = false;

        public EnString() : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.String);
            m_pStringNote = new StringBuilder(DefaultCryptionBufferSize / 2);
        }
        public EnString(int iBufferSize) : base(iBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.String);
            m_pStringNote = new StringBuilder(iBufferSize / 2);
        }

        public EnString(string strText) : base(DefaultCryptionBufferSize)
        {
            SetEncryptValueType(EEncryptValueType.String);
            m_pStringNote = new StringBuilder(DefaultCryptionBufferSize / 2);
            SetValue(strText);
        }

        public void SetValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (IsValueLock) return;

            ProtEncryptDataReset();

            for (int i = 0; i < value.Length; i++)
            {
                ProtEncryptDataValueAdd(value[i]);
            }

            ProtEncryptDataStart();

            m_sCachedValue = value;
            m_bCachedValid = true;
        }

        public new string GetValue(bool decrypt = true)
        {
            if (!decrypt && m_bCachedValid)
                return m_sCachedValue;

            string decrypted = PrivDecryptDataString();
            m_sCachedValue = decrypted;
            m_bCachedValid = true;
            return decrypted;
        }

        public static implicit operator string(EnString pEnString)
        {
            return pEnString.GetValue();
        }

        //-----------------------------------------------------
        private string PrivDecryptDataString()
        {
            int iCharSize = Marshal.SizeOf(typeof(char));
            for (int i = 0; i < BufferSizeUse; i += iCharSize)
            {
                char C = ProtDecryptDataStart(i, iCharSize);
                m_pStringNote.Append(C);
            }

            string strDecryptString = m_pStringNote.ToString();
            m_pStringNote.Length = 0; // 초기화
            return strDecryptString;
        }
    }
}