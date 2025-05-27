using System.Text;

namespace ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;

internal sealed class GeneratedVariableNameConverter
{
    private const string Prefix = "__ChronoTrace";

    internal string ToGeneratedVariableName(string variableName)
    {
        var resultBuilder = new StringBuilder(Prefix).Append('_');

        resultBuilder.Append(char.ToUpper(variableName[0]));

        for (var i = 1; i < variableName.Length; i++)
        {
            var currentChar = variableName[i];

            if (char.IsUpper(currentChar))
            {
                if (resultBuilder[^1] != '_')
                {
                    var previousChar = variableName[i-1];
                    var nextCharIsLowerOrEnd = (i + 1 < variableName.Length && char.IsLower(variableName[i+1]))
                        || i + 1 == variableName.Length;

                    if (char.IsLower(previousChar)
                        || (char.IsUpper(previousChar) && nextCharIsLowerOrEnd && i > 1)
                        || char.IsUpper(previousChar) && !nextCharIsLowerOrEnd && resultBuilder[^1] != '_')
                    {
                         resultBuilder.Append('_');
                    }
                }
            }

            resultBuilder.Append(currentChar);
        }

        return resultBuilder.ToString();
    }
}