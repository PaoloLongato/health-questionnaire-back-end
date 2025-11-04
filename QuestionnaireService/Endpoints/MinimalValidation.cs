using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuestionnaireService.Endpoints;

internal static class MinimalValidation
{
    public static bool TryValidate<T>(T instance, out IDictionary<string, string[]> errors)
    {
        var validationContext = new ValidationContext(instance!);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(instance!, validationContext, validationResults, validateAllProperties: true);

        if (isValid)
        {
            errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            return true;
        }

        var errorDictionary = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var result in validationResults)
        {
            if (result.MemberNames.Any())
            {
                foreach (var memberName in result.MemberNames)
                {
                    if (!errorDictionary.TryGetValue(memberName, out var messages))
                    {
                        messages = new List<string>();
                        errorDictionary[memberName] = messages;
                    }

                    messages.Add(result.ErrorMessage ?? "Validation error");
                }
            }
            else
            {
                const string key = "";
                if (!errorDictionary.TryGetValue(key, out var messages))
                {
                    messages = new List<string>();
                    errorDictionary[key] = messages;
                }

                messages.Add(result.ErrorMessage ?? "Validation error");
            }
        }

        errors = errorDictionary.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);

        return false;
    }
}
