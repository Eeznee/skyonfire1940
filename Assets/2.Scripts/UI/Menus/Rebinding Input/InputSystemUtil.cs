using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.Layouts;

public static class InputSystemUtil
{
    public static string DisplayCamelCaseString(string camelCase)
    {
        List<char> chars = new List<char>();
        for (int i = 0; i < camelCase.Length; i++)
        {
            char c = camelCase[i];
            if (i == 0)
            {
                chars.Add(char.ToUpper(c));
            }
            else if (char.IsUpper(c))
            {
                chars.Add(' ');
                chars.Add(char.ToUpper(c));
            }
            else
                chars.Add(c);
        }

        return new string(chars.ToArray());
    }
    public static string FindSubStringInBrackets(string text, char leftBracket, char rightBracket)
    {
        int leftBracketId = text.IndexOf(leftBracket) + 1;
        int rightBracketId = text.IndexOf(rightBracket);
        if (leftBracketId == -1 || rightBracketId == -1) return text;
        if (leftBracketId > rightBracketId) return text;

        int length = rightBracketId - leftBracketId;
        return text.Substring(leftBracketId, length);
    }
}
