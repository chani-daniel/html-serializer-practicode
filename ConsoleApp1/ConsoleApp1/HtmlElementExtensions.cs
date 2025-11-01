using System.Collections.Generic;
using System.Linq;

// מחלקת ההרחבה חייבת להיות סטטית
public static class HtmlElementExtensions
{
    // ====================================================================
    // 1. פונקציית Descendants (צאצאים) - שימוש בתור (Queue) למניעת Stack Overflow
    // ====================================================================

    // מחזירה את כל צאצאי האלמנט (מכל הדורות)
    public static IEnumerable<HtmlElement> Descendants(this HtmlElement element)
    {
        // שימוש בתור (Queue) במקום ריקורסיה לעומק, כדי למנוע Stack Overflow בעצים גדולים
        Queue<HtmlElement> queue = new Queue<HtmlElement>();

        // מוסיפים את הילדים הישירים של האלמנט הנוכחי
        foreach (var child in element.Children)
        {
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            // שולפים את האלמנט מהתור
            HtmlElement current = queue.Dequeue();

            // מחזירים את האלמנט (באמצעות yield return)
            yield return current;

            // מוסיפים את ילדיו של האלמנט הנוכחי לתור
            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    // ====================================================================
    // 2. פונקציית Ancestors (אבות) - שימוש בלולאה פשוטה
    // ====================================================================

    // מחזירה את כל אבות האלמנט (עד השורש)
    public static IEnumerable<HtmlElement> Ancestors(this HtmlElement element)
    {
        HtmlElement parent = element.Parent;

        // רצים בלולאה כל עוד יש אב
        while (parent != null)
        {
            // מחזירים את האב הנוכחי
            yield return parent;

            // עוברים לאב הבא
            parent = parent.Parent;
        }
    }

    // ====================================================================
    // 3. פונקציית החיפוש הראשית (Selector)
    // ====================================================================

    // הפונקציה המרכזית שמקבלת Selector ומחזירה רשימת אלמנטים תואמים
    public static HashSet<HtmlElement> QuerySelector(this HtmlElement element, Selector selector)
    {
        // שימוש ב-HashSet כדי להבטיח שכל אלמנט יופיע פעם אחת בלבד בתוצאה הסופית
        var results = new HashSet<HtmlElement>();

        // הפעלת הפונקציה הריקורסיבית הראשית
        FindMatchesRecursive(element, selector, results);

        return results;
    }

    // פונקציית העזר הריקורסיבית
    private static void FindMatchesRecursive(HtmlElement currentElement, Selector currentSelector, HashSet<HtmlElement> results)
    {
        if (currentSelector == null)
        {
            return; // תנאי עצירה ריקורסיבי
        }

        // 1. קבלת כל הצאצאים של האלמנט הנוכחי (כולל האלמנט עצמו)
        // מכיוון ש-Descendants לא כוללת את האלמנט המפעיל, נוסיף אותו ידנית
        var descendantsAndSelf = new List<HtmlElement> { currentElement };
        descendantsAndSelf.AddRange(currentElement.Descendants());

        // 2. סינון רשימת הצאצאים לפי קריטריוני הסלקטור הנוכחי
        var matchingElements = descendantsAndSelf
            .Where(el => IsMatch(el, currentSelector))
            .ToList();

        // 3. תנאי עצירה (אם הגענו לסלקטור האחרון בשרשרת)
        if (currentSelector.Child == null)
        {
            // מוסיפים את האלמנטים התואמים ל-HashSet (מונע כפילויות אוטומטית)
            foreach (var match in matchingElements)
            {
                results.Add(match);
            }
        }
        else
        {
            // 4. קריאה ריקורסיבית: ממשיכים לחפש את הסלקטור הבא (הבן) בתוך הצאצאים התואמים
            foreach (var match in matchingElements)
            {
                FindMatchesRecursive(match, currentSelector.Child, results);
            }
        }
    }

    // ====================================================================
    // 4. פונקציית עזר לבדיקת התאמה בודדת
    // ====================================================================

    // בודקת האם אלמנט HtmlElement מתאים לקריטריונים של Selector בודד
    private static bool IsMatch(HtmlElement element, Selector selector)
    {
        // בדיקת TagName (אם הסלקטור דורש TagName מסוים)
        if (!string.IsNullOrEmpty(selector.TagName) &&
            !element.Name.Equals(selector.TagName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // בדיקת ID (אם הסלקטור דורש ID מסוים)
        if (!string.IsNullOrEmpty(selector.Id) &&
            !element.Id.Equals(selector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // בדיקת Classes (אם הסלקטור דורש Class מסוים)
        if (selector.Classes.Any())
        {
            // חייב להכיל את כל ה-Classes הנדרשים בסלקטור
            if (!selector.Classes.All(c => element.Classes.Contains(c)))
            {
                return false;
            }
        }

        // אם עבר את כל הבדיקות - יש התאמה
        return true;
    }
}