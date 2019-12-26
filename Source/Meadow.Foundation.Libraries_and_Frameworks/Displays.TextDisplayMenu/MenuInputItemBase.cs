using System;


namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public class MenuInputItemBase
    {
        public char[] Items { get; private set; }
        
        public void Init(bool addUpperCaseLetters, bool addLowerCaseLetters, bool addNumbers, char[] punctuation)
        {
            int size = 0;
            int index = 0;

            if (addUpperCaseLetters) size += 26;
            if (addLowerCaseLetters) size += 26;
            if (addNumbers) size += 10;
            if (punctuation != null) size += punctuation.Length;

            this.Items = new char[size];

            if (addUpperCaseLetters)
            {
                index += AddUpperCaseLetters(index);
            }
            if (addLowerCaseLetters)
            {
                index += AddLowerCaseLetters(index);
            }
            if (addNumbers)
            {
                index += AddNumbers(index);
            }
            if (punctuation != null)
            {
                Array.Copy(punctuation, 0, this.Items, index, punctuation.Length);
            }
        }

        private int AddUpperCaseLetters(int index)
        {
            var uppers = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            Array.Copy(uppers, 0, this.Items, index, uppers.Length);
            return uppers.Length;
        }

        private int AddLowerCaseLetters(int index)
        {
            var lowers = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            Array.Copy(lowers, 0, this.Items, index, lowers.Length);
            return lowers.Length;
        }

        private int AddNumbers(int index)
        {
            var numbers = new char[] { '0','1','2','3','4','5','6','7','8','9' };
            Array.Copy(numbers, 0, this.Items, index, numbers.Length);
            return numbers.Length;

        }
    }
}
