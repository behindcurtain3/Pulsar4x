using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides a human readable identifier for data.
///
/// Because it is a record it is not mutable and provides equality
/// and hash code functionality by default.
/// Example: var id = new StringIdentifer("base.rp1-engine");
/// Example: var id = new StringIdentifier("base.nestedScope.rp1-engine");
/// </summary>
/// <param name="Scopes"></param>
/// <param name="Name"></param>
public record StringIdentifier(List<string> Scopes, string Name)
{
    public StringIdentifier(string singleScope, string Name) : this(new List<string> { singleScope }, Name)
    {
        if (string.IsNullOrEmpty(singleScope) || string.IsNullOrEmpty(Name))
        {
            throw new ArgumentException("Both scope and itemName must be provided.");
        }
    }

    public StringIdentifier(string fullIdentifier) : this(
        fullIdentifier.Split('.', StringSplitOptions.RemoveEmptyEntries)
                      .TakeWhile((part, index) => index < fullIdentifier.Count(c => c == '.'))
                      .ToList(),
        fullIdentifier.Split('.').Last())
    {
        if (fullIdentifier.Count(c => c == '.') < 1)
        {
            throw new ArgumentException("Invalid identifier format. Must be in the form 'scope1.scope2...Name'.");
        }
    }

    public override string ToString() => $"{string.Join('.', Scopes)}.{Name}";
}
