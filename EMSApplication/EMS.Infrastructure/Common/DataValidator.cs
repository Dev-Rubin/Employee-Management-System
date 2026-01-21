using System;
using System.Collections.Generic;
using System.Text;

namespace EMS.Infrastructure.Common
{
    public static class DataValidator
    {
        public static ValidationResult Validate<T>(T entity, ValidationRule<T> rule)
        {
            return Validate(entity, new[] { rule });
        }
        public static async Task<ValidationResult> ValidateAsync<T>(T entity, ValidationRule<T> rule)
        {
            return Validate(entity, new[] { rule });
        }

        public static ValidationResult Validate<T>(T entity, IEnumerable<ValidationRule<T>> rules)
        {
            var result = new ValidationResult(typeof(T).Name);

            foreach (var rule in rules)
            {
                var shouldContinue = CheckRule(entity, result, rule);

                if (!shouldContinue)
                    return result;
            }
            return result;
        }
        public static async Task<ValidationResult> ValidateAsync<T>(T entity, IEnumerable<ValidationRule<T>> rules)
        {
            var result = new ValidationResult(typeof(T).Name);

            foreach (var rule in rules)
            {
                var shouldContinue = await CheckRuleAsync(entity, result, rule).ConfigureAwait(false);

                if (!shouldContinue)
                    return result;
            }
            return result;
        }

        private static bool CheckRule<T>(T entity, ValidationResult finalResult, ValidationRule<T> rule)
        {
            var checkResult = rule.Check(entity);

            if (checkResult.IsSuccessful)
            {
                foreach (var dependentRule in rule.DependentRules)
                {
                    CheckRule(entity, finalResult, dependentRule);
                }
            }
            else
            {
                finalResult.Errors.Add(checkResult.Message);

                if (rule.IsCritical) return false;
            }
            return true;
        }
        private static async Task<bool> CheckRuleAsync<T>(T entity, ValidationResult finalResult, ValidationRule<T> rule)
        {
            var checkResult = await rule.CheckAsync(entity).ConfigureAwait(false);

            if (checkResult.IsSuccessful)
            {
                foreach (var dependentRule in rule.DependentRules)
                {
                    await CheckRuleAsync(entity, finalResult, dependentRule).ConfigureAwait(false);
                }
            }
            else
            {
                finalResult.Errors.Add(checkResult.Message);

                if (rule.IsCritical) return false;
            }
            return true;
        }
    }
}
