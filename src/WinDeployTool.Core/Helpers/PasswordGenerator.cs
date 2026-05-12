namespace WinDeployTool.Core.Helpers
{
    /// <summary>
    /// 密码生成器
    /// </summary>
    public static class PasswordGenerator
    {
        private static readonly Random Random = new();
        private const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const string SpecialChars = "!@#$%^&*";

        /// <summary>
        /// 生成随机密码
        /// </summary>
        /// <param name="length">密码长度</param>
        /// <param name="includeSpecial">是否包含特殊字符</param>
        public static string Generate(int length = 12, bool includeSpecial = false)
        {
            if (length < 8)
                length = 8;

            var chars = LowerChars + UpperChars + Numbers;
            if (includeSpecial)
                chars += SpecialChars;

            var password = new char[length];

            // 确保至少包含一个大写字母、一个小写字母和一个数字
            password[0] = UpperChars[Random.Next(UpperChars.Length)];
            password[1] = LowerChars[Random.Next(LowerChars.Length)];
            password[2] = Numbers[Random.Next(Numbers.Length)];

            // 填充剩余字符
            for (int i = 3; i < length; i++)
            {
                password[i] = chars[Random.Next(chars.Length)];
            }

            // 打乱顺序
            return new string(password.OrderBy(x => Random.Next()).ToArray());
        }

        /// <summary>
        /// 生成易记的密码（单词+数字组合）
        /// </summary>
        public static string GenerateMemorable()
        {
            var words = new[] { "Blue", "Star", "Moon", "Sun", "Sky", "Cloud", "Wind", "Rain", "Snow", "Fire" };
            var word = words[Random.Next(words.Length)];
            var number = Random.Next(1000, 9999);
            return $"{word}{number}";
        }
    }
}
