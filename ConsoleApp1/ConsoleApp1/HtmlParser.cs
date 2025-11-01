using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleApp1;

public class HtmlParser
{
    // משתנה סטטי פרטי להכלה של ביטוי רגולרי לזיהוי תגיות
    // הביטוי מחפש את התבנית: <[תוכן התגית]> או את הטקסט שביניהן
    // הערה: יש לשפר את הביטוי הרגולרי בהתאם לצרכים ספציפיים ולטפל בתוכן פנימי (InnerHtml)
    private static readonly Regex TagRegex = new Regex("(<[^>]+>|[^<]+)", RegexOptions.Compiled | RegexOptions.Multiline);

    // פונקציה אסינכרונית לטעינת קוד HTML מכתובת URL
    public async Task<string> Load(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            // בדיקה אם הסטטוס קוד מצליח לפני קריאת התוכן
            //response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }
    }

    // פונקציה ראשית שמבצעת את כל תהליך ה-Parsing
    public HtmlElement Parse(string html)
    {
        // 1. פירוק ה-HTML לרשימת מחרוזות (תגיות וטקסט)
        List<string> rawHtmlParts = SplitHtmlToParts(html);

        // 2. בניית העץ מתוך רשימת המחרוזות
        return BuildTree(rawHtmlParts);
    }

    // פונקציית עזר לפירוק באמצעות Regex
    private List<string> SplitHtmlToParts(string html)
    {
        var matches = TagRegex.Matches(html);
        var parts = new List<string>();

        foreach (Match match in matches)
        {
            string part = match.Value;
            // ניקוי המחרוזת מרווחים מיותרים וירידות שורה
            string trimmedPart = part.Trim();

            if (!string.IsNullOrEmpty(trimmedPart))
            {
                parts.Add(trimmedPart);
            }
        }

        return parts;
    }

    // פונקציית הליבה לבניית עץ ה-HtmlElement
    private HtmlElement BuildTree(List<string> parts)
    {
        // יצירת אלמנט שורש דמה או שימוש ב-"html" כשורש
        HtmlElement root = new HtmlElement { Name = "root" };
        HtmlElement currentElement = root;

        foreach (string part in parts)
        {
            // בדיקה האם זו תגית (מתחילה ב- '<' ומסתיימת ב- '>')
            if (part.StartsWith("<") && part.EndsWith(">"))
            {
                // הסרת הסוגריים < ו- >
                string content = part.Substring(1, part.Length - 2).Trim();

                if (content.StartsWith("/")) // תגית סוגרת (לדוגמה: </div>)
                {
                    // עולים לרמה הקודמת בעץ
                    currentElement = currentElement.Parent ?? root;
                }
                else // תגית פותחת (לדוגמה: <div id="myDiv">)
                {
                    // יוצרים אובייקט חדש ומפרקים את המאפיינים
                    HtmlElement newElement = CreateElementFromTagContent(content);

                    // חיבור לעץ
                    newElement.Parent = currentElement;
                    currentElement.Children.Add(newElement);

                    // בדיקה האם התגית סוגרת את עצמה
                    bool isSelfClosing = content.EndsWith("/") || HtmlHelper.Instance.SelfClosingTags.Contains(newElement.Name);

                    if (!isSelfClosing)
                    {
                        // אם לא סוגרת את עצמה, עוברים לעבוד על האלמנט החדש
                        currentElement = newElement;
                    }
                }
            }
            else // טקסט פנימי (InnerHtml)
            {
                if (currentElement != null && currentElement != root)
                {
                    // הוספת הטקסט הפנימי לאלמנט הנוכחי
                    currentElement.InnerHtml += part;
                }
            }
        }

        // אם התוכן מתחיל ב-<html>, מחזירים את הילד היחיד שלו כשורש אמיתי
        return root.Children.Count > 0 ? root.Children[0] : root;
    }

    // פונקציית עזר לפירוק התוכן של תגית וחילוק למאפיינים
    private HtmlElement CreateElementFromTagContent(string content)
    {
        // 1. חילוץ שם התגית (המילה הראשונה לפני רווח)
        int spaceIndex = content.IndexOf(' ');
        string tagName = spaceIndex == -1 ? content.ToLower() : content.Substring(0, spaceIndex).ToLower();

        HtmlElement element = new HtmlElement { Name = tagName };

        // אם יש מאפיינים להמשך:
        if (spaceIndex != -1)
        {
            string attributesString = content.Substring(spaceIndex + 1).Trim();

            // 2. פירוק המאפיינים באמצעות Regular Expression
            // זהו ביטוי מורכב המחפש את התבנית: [שם]=[ערך במירכאות] או [שם] ללא ערך
            // לדוגמה: <div id="myId" class="c1 c2" disabled>
            Regex attributesRegex = new Regex("(?<name>[^=]+)=\"(?<value>[^\"]*)\"|(?<name>[^\\s\\/]+)", RegexOptions.Compiled);

            var matches = attributesRegex.Matches(attributesString);

            foreach (Match match in matches)
            {
                string name = match.Groups["name"].Value.Trim();
                string value = match.Groups["value"].Success ? match.Groups["value"].Value : string.Empty;

                if (string.IsNullOrEmpty(name) || name.EndsWith("/")) continue; // דלג אם ריק או אם זה סיום תגית סגירה-עצמית

                // עדכון מאפיינים מיוחדים: id ו-class
                if (name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    element.Id = value;
                }
                else if (name.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    // פירוק ה-class לפי רווחים
                    element.Classes.AddRange(value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    // הוספה לרשימת המאפיינים הכללית
                    element.Attributes.Add(new HtmlAttribute { Name = name, Value = value });
                }
            }
        }

        return element;
    }
}
