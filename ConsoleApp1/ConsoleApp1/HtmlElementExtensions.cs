using System.Collections.Generic;
using System.Linq;

public static class HtmlElementExtensions
{
    
    public static IEnumerable<HtmlElement> Descendants(this HtmlElement element)
    {
        Queue<HtmlElement> queue = new Queue<HtmlElement>();
        foreach (var child in element.Children)
        {
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            HtmlElement current = queue.Dequeue();
            yield return current;
            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    public static IEnumerable<HtmlElement> Ancestors(this HtmlElement element)
    {
        HtmlElement parent = element.Parent;
        while (parent != null)
        {
            yield return parent;
            parent = parent.Parent;
        }
    }
    public static HashSet<HtmlElement> QuerySelector(this HtmlElement element, Selector selector)
    {
        var results = new HashSet<HtmlElement>();
        FindMatchesRecursive(element, selector, results);
        return results;
    }
    private static void FindMatchesRecursive(HtmlElement currentElement, Selector currentSelector, HashSet<HtmlElement> results)
    {
        if (currentSelector == null)
        {
            return; 
        }
        var descendantsAndSelf = new List<HtmlElement> { currentElement };
        descendantsAndSelf.AddRange(currentElement.Descendants());
        var matchingElements = descendantsAndSelf
            .Where(el => IsMatch(el, currentSelector))
            .ToList();
        if (currentSelector.Child == null)
        {
            foreach (var match in matchingElements)
            {
                results.Add(match);
            }
        }
        else
        {
            foreach (var match in matchingElements)
            {
                FindMatchesRecursive(match, currentSelector.Child, results);
            }
        }
    }
    private static bool IsMatch(HtmlElement element, Selector selector)
    {
        if (!string.IsNullOrEmpty(selector.TagName) &&
            !element.Name.Equals(selector.TagName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(selector.Id) &&
            !element.Id.Equals(selector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (selector.Classes.Any())
        {
            if (!selector.Classes.All(c => element.Classes.Contains(c)))
            {
                return false;
            }
        }
        return true;
    }
}
