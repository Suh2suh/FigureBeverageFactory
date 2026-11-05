using UnityEngine;

namespace FBFactory
{
    public static class StringExtension
    {
        public static string Color(this string message, Color color)
        {
            string colorCode = ColorUtility.ToHtmlStringRGBA(color);
            return $"<color=#{colorCode}>{message}</color>";
        }
    }
}
