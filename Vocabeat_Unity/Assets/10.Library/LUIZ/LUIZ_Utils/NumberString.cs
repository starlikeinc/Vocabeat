using System.Runtime.CompilerServices;
using System;

//숫자의 출력이 매우 빈번한 Update나 슬라이더 조정 등에서 텍스트를 출력해야할때 이용함.
//TMP에서 사용할 경우 Set을 통해 값을 세팅한 후 TMP컴포넌트.text.SetCharArray(NumberString.Buffer)를 이용한다면 TMP 내부에서의 불필요한 할당까지 피할 수 있음
//tmp 관련 : http://digitalnativestudios.com/textmeshpro/docs/ScriptReference/TextMeshPro-SetCharArray.html
//팁 : TMP의 SetCharArray는 실제 플레이시 GC를 일으키지 않는다. Editor 상에서만 에디터 인스펙터 표기를 위해 GC가 발생한다.
namespace LUIZ.Utils
{
    public class NumberString
    {
        public enum EAlignmentType
        {
            Left = 0,
            Right = 1
        }

        public NumberString(int size, EAlignmentType alignment = EAlignmentType.Left, char fillCharacter = '\0')
        {
            m_aryCharBuffer = new char[size];

            Capacity = size;
            AlignmentType = alignment;
            FillCharacter = fillCharacter;
        }

        //--------------------------------------------------------
        private const char c_charZero = '0';
        private const char c_charMinus = '-';

        private readonly char[] m_aryCharBuffer;

        //--------------------------------------------------------
        /// <summary> size가 fillCharacter보다 클때 나머지를 FillCharacter로 채움 </summary>
        public char          FillCharacter  { get; set; }
        public EAlignmentType AlignmentType  { get; set; }
        public int           Capacity       { get; private set; }

        public string Value => new string(m_aryCharBuffer);
        public char[] Buffer => m_aryCharBuffer;

        //--------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int value)
        {
            bool isNegativeValue = value < 0;
            int capacity = Capacity;

            int log10 = (int)Math.Floor(Math.Log10(value));
            int charCount = log10 + (isNegativeValue ? 2 : 1);
            int blankCount = capacity - charCount;

            int curCharIndex = 0;

            if (AlignmentType == EAlignmentType.Left)
            {
                if (value == 0)
                {
                    for (int i = 0; i < m_aryCharBuffer.Length; i++)
                        m_aryCharBuffer[i] = FillCharacter;
                    
                    m_aryCharBuffer[0] = c_charZero;
                    return;
                }

                if (isNegativeValue)
                {
                    m_aryCharBuffer[0] = c_charMinus;
                    curCharIndex++;
                }

                int min = Math.Max(charCount - capacity, 0);
                for (int i = log10; i >= min; i--)
                {
                    int pow = (int)Math.Pow(10, i);
                    int digit = value / pow % 10;
                    m_aryCharBuffer[curCharIndex++] = (char)(digit + c_charZero);
                }

                for (int i = 0; i < blankCount; i++)
                    m_aryCharBuffer[curCharIndex++] = FillCharacter;
            }
            else
            {
                for (int i = 0; i < blankCount; i++)
                    m_aryCharBuffer[curCharIndex++] = FillCharacter;

                if (isNegativeValue)
                {
                    m_aryCharBuffer[curCharIndex] = c_charMinus;
                    curCharIndex++;
                }

                int min = Math.Max(charCount - capacity, 0);
                for (int i = log10; i >= min; i--)
                {
                    int pow = (int)Math.Pow(10, i);
                    int digit = value / pow % 10;
                    m_aryCharBuffer[curCharIndex++] = (char)(digit + c_charZero);
                }
            }
        }
    }
}
