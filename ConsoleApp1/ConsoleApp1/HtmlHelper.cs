using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;


namespace ConsoleApp1
{
    public class HtmlHelper
    {
        // משתנה סטטי פרטי לשמירת המופע היחיד
        private static HtmlHelper _instance;

        // הגדרת קבועים עבור שמות קבצי ה-JSON
        private const string ALL_TAGS_FILE = "HtmlTags.json";
        private const string SELF_CLOSING_FILE = "HtmlVoidTags.json";

        // מאפיינים ציבוריים להחזקת רשימות התגיות
        public string[] AllHtmlTags { get; private set; }
        public string[] SelfClosingTags { get; private set; }

        // בנאי פרטי (מנוע יצירת מופעים מבחוץ - רק Singleton יכול להפעיל אותו)
        private HtmlHelper()
        {
            try
            {
                // 1. קריאת וטעינת כל התגיות
                string allTagsJsonContent = File.ReadAllText(ALL_TAGS_FILE);
                this.AllHtmlTags = JsonSerializer.Deserialize<string[]>(allTagsJsonContent);

                // 2. קריאת וטעינת תגיות סגירה-עצמית
                string selfClosingJsonContent = File.ReadAllText(SELF_CLOSING_FILE);
                this.SelfClosingTags = JsonSerializer.Deserialize<string[]>(selfClosingJsonContent);
            }
            catch (Exception ex)
            {
                // טיפול בשגיאות קריאה/דה-סריאליזציה, למשל אם הקבצים לא נמצאו בתיקיית הפלט
                Console.WriteLine($"שגיאה בטעינת קבצי תגיות HTML: {ex.Message}");
                // במקרה של כשל, מאתחל מערכים ריקים
                this.AllHtmlTags = Array.Empty<string>();
                this.SelfClosingTags = Array.Empty<string>();
            }
        }

        // מאפיין סטטי ציבורי לקבלת המופע היחיד (ה-Singleton)
        public static HtmlHelper Instance
        {
            get
            {
                // אם המופע לא נוצר עדיין, ניצור אותו כעת
                if (_instance == null)
                {
                    _instance = new HtmlHelper();
                }
                // החזרת המופע היחיד
                return _instance;
            }
        }
    }
}
