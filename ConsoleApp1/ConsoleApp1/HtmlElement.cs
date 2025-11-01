using System.Collections.Generic;
public class HtmlAttribute
{
    public string Name { get; set; }
    public string Value { get; set; }
}
public class HtmlElement
{
    // מאפיינים בסיסיים
    public string Id { get; set; }
    public string Name { get; set; }
    public string InnerHtml { get; set; }

    // מאפיינים מורכבים
    public List<HtmlAttribute> Attributes { get; set; }
    public List<string> Classes { get; set; }

    // מאפיינים היררכיים (עץ)
    public HtmlElement Parent { get; set; }
    public List<HtmlElement> Children { get; set; }

    // בנאי לאתחול רשימות
    public HtmlElement()
    {
        Attributes = new List<HtmlAttribute>();
        Classes = new List<string>();
        Children = new List<HtmlElement>();
    }
}