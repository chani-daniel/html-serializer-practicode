using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Selector
{
    // מאפייני חיפוש
    public string TagName { get; set; }
    public string Id { get; set; }
    public List<string> Classes { get; set; }

    // מאפיינים היררכיים (עץ בינארי מנוון)
    public Selector Parent { get; set; }
    public Selector Child { get; set; }

    // בנאי
    public Selector()
    {
        Classes = new List<string>();
    }

    // ... המשך המחלקה Selector ...

    // פונקציה סטטית שממירה מחרוזת שאילתה (CSS Selector) לשרשרת של אובייקטי Selector
    public static Selector Parse(string selectorString)
    {
        // 1. פירוק המחרוזת לפי רווחים (מפריד בין רמות היררכיה - עומק)
        string[] parts = selectorString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return null; // שאילתה ריקה
        }

        Selector root = null;
        Selector currentSelector = null;

        // 2. לולאה על חלקי השאילתה
        foreach (string part in parts)
        {
            Selector newSelector = new Selector();

            // Regex לפירוק החלק הנוכחי לפי מפרידים: # (ID) ו- . (Class)
            // הביטוי מפצל את המחרוזת לפי המפרידים, אך שומר עליהם ברשימה
            string[] subParts = Regex.Split(part, "(?=[#\\.])");

            // 3. לולאה על חלקי הסלקטור הבודד (tag#id.class)
            foreach (string subPart in subParts)
            {
                if (string.IsNullOrEmpty(subPart)) continue;

                if (subPart.StartsWith("#"))
                {
                    // מפריד ID
                    newSelector.Id = subPart.Substring(1);
                }
                else if (subPart.StartsWith("."))
                {
                    // מפריד Class
                    newSelector.Classes.Add(subPart.Substring(1));
                }
                else
                {
                    // שם תגית (אם זה לא מתחיל ב-# או ב-.)
                    // יש לוודא שזהו שם תגית HTML תקני (באמצעות HtmlHelper)
                    if (HtmlHelper.Instance.AllHtmlTags.Contains(subPart))
                    {
                        newSelector.TagName = subPart;
                    }
                    else
                    {
                        // אם זה לא שם תגית תקני, ייתכן שזו טעות בשאילתה.
                        // בפרויקט זה נניח שאם זה לא ID או Class זהו שם תגית (אבל עדיף לוודא).
                        newSelector.TagName = subPart;
                    }
                }
            } // סיום פירוק subPart

            // 4. בניית שרשרת הסלקטורים
            if (root == null)
            {
                root = newSelector;
            }
            else
            {
                // מחברים את הסלקטור החדש כבן של הקודם
                currentSelector.Child = newSelector;
                newSelector.Parent = currentSelector;
            }

            // מעבירים את המצביע לסלקטור החדש
            currentSelector = newSelector;

        } // סיום לולאת part

        return root;
    }

}