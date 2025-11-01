// כתובת URL לדוגמה לבדיקה (החליפי אותה בכתובת רצויה)
const string TARGET_URL = "http://www.ynet.co.il/home/0,7340,L-2,00.html";

// 1. אתחול והפעלת ה-Parser
Console.WriteLine($"--- 1. טעינת דף HTML מ: {TARGET_URL} ---");
HtmlParser parser = new HtmlParser();

string htmlContent = await parser.Load(TARGET_URL);

// בדיקה לוודא שהתוכן נטען
if (string.IsNullOrEmpty(htmlContent))
{
    Console.WriteLine("שגיאה: לא נטען תוכן HTML.");
    return;
}

// 2. בניית עץ האלמנטים
HtmlElement rootElement = parser.Parse(htmlContent);
Console.WriteLine("בניית עץ ה-HTML הסתיימה בהצלחה.");

if (rootElement == null)
{
    Console.WriteLine("שגיאה: שורש העץ הוא Null.");
    return;
}

// =======================================================
// 3. בדיקת Html Query (חיפוש אלמנטים)
// =======================================================

// דוגמה 1: חיפוש אלמנטים מסוג 'div' עם class 'main-body'
string selector1String = "div.main-body";
Console.WriteLine($"\n--- 3.1. חיפוש לפי סלקטור: {selector1String} ---");

// המרת המחרוזת לאובייקט Selector
Selector selector1 = Selector.Parse(selector1String);

// ביצוע החיפוש על שורש העץ
HashSet<HtmlElement> results1 = rootElement.QuerySelector(selector1);

Console.WriteLine($"נמצאו {results1.Count} אלמנטים תואמים.");
foreach (var element in results1.Take(5)) // מציג 5 תוצאות ראשונות
{
    Console.WriteLine($"- התאמה: <{element.Name}>, ID: {element.Id}, Classes: {string.Join(", ", element.Classes)}");
}

// דוגמה 2: חיפוש היררכי (בעומק)
string selector2String = "body div #someId p.text-item";
Console.WriteLine($"\n--- 3.2. חיפוש היררכי: {selector2String} ---");

Selector selector2 = Selector.Parse(selector2String);
HashSet<HtmlElement> results2 = rootElement.QuerySelector(selector2);

Console.WriteLine($"נמצאו {results2.Count} אלמנטים תואמים.");
// ניתן להדפיס תוצאות נוספות כאן...

// =======================================================
// 4. בדיקת פונקציות עזר (Descendants ו-Ancestors)
// =======================================================

// בדיקת Descendants
int totalDescendants = rootElement.Descendants().Count();
Console.WriteLine($"\n--- 4.1. בדיקת Descendants ---");
Console.WriteLine($"סה\"כ צאצאים בעץ: {totalDescendants}");

// בדיקת Ancestors (אם יש תוצאות מדוגמה 1)
if (results1.Any())
{
    HtmlElement testElement = results1.First();
    Console.WriteLine($"\n--- 4.2. בדיקת Ancestors לאלמנט <{testElement.Name}> ---");
    int ancestorsCount = testElement.Ancestors().Count();
    Console.WriteLine($"סה\"כ אבות לאלמנט זה: {ancestorsCount}");
    Console.Write("שרשרת אבות (Name): ");
    Console.WriteLine(string.Join(" -> ", testElement.Ancestors().Select(el => el.Name)));
}

