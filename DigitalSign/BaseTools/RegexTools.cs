using System.Text.RegularExpressions;

namespace DigitalSign.BaseTools;

public static class RegexTools
{
    public static bool IsMatch(this string inputStr, string pattern)
    {
        return Regex.IsMatch(inputStr, pattern); // 检查是否存在匹配
    }
}