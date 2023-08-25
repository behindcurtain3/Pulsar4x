using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Pulsar4X.ECSLib
{
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

        public virtual bool Equals(StringIdentifier other)
        {
            return Scopes.SequenceEqual(other.Scopes) && Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = -2045290805;
            foreach (var scope in Scopes)
            {
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(scope);
            }
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public override string ToString() => $"{string.Join('.', Scopes)}.{Name}";
    }

    public class StringIdentifierConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StringIdentifier);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return new StringIdentifier(reader.Value.ToString());
            }
            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing StringIdentifier.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identifier = (StringIdentifier)value;
            writer.WriteValue(identifier.ToString());
        }
    }
}
