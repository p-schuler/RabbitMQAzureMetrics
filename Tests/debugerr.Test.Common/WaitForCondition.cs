using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;

namespace debugerr.Test.Common
{
    public static class WaitForCondition
    {
        const int Seconds = 1000;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "We want to override the caller info when calling into the overload, as it reflects the original caller in the test.")]

        public static Task<TimeSpan> AssertAsync(Func<bool> condition,
                                       string description,
                                       double timeout = 60 * Seconds,
                                       int pollingInterval = 5 * Seconds,
                                       bool throwWhenDebugging = true,
                                       [CallerFilePath] string callerFilePath = "",
                                       [CallerLineNumber] int callerLineNumber = 0,
                                       [CallerMemberName] string callerMember = "")
        {
            return AssertAsync(() => Task.FromResult(condition()), () => description, timeout, pollingInterval, throwWhenDebugging, callerFilePath, callerLineNumber, callerMember);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "We want to override the caller info when calling into the overload, as it reflects the original caller in the test.")]

        public static Task<TimeSpan> AssertAsync(Func<bool> condition,
                                       Func<string> descriptionFactory,
                                       double timeout = 60 * Seconds,
                                       int pollingInterval = 5 * Seconds,
                                       bool throwWhenDebugging = true,
                                       [CallerFilePath] string callerFilePath = "",
                                       [CallerLineNumber] int callerLineNumber = 0,
                                       [CallerMemberName] string callerMember = "")
        {
            return AssertAsync(() => Task.FromResult(condition()), descriptionFactory, timeout, pollingInterval, throwWhenDebugging, callerFilePath, callerLineNumber, callerMember);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "We want to override the caller info when calling into the overload, as it reflects the original caller in the test.")]

        public static Task<TimeSpan> AssertAsync(Func<Task<bool>> condition,
                                       string description,
                                       double timeout = 60 * Seconds,
                                       int pollingInterval = 5 * Seconds,
                                       bool throwWhenDebugging = true,
                                       [CallerFilePath] string callerFilePath = "",
                                       [CallerLineNumber] int callerLineNumber = 0,
                                       [CallerMemberName] string callerMember = "")
        {
            return AssertAsync(() => condition(), () => description, timeout, pollingInterval, throwWhenDebugging, callerFilePath, callerLineNumber, callerMember);
        }

        public static async Task<TimeSpan> AssertAsync(Func<Task<bool>> condition,
                                             Func<string> descriptionFactory,
                                             double timeout = 60 * Seconds,
                                             int pollingInterval = 5 * Seconds,
                                             bool throwWhenDebugging = true,
                                             [CallerFilePath] string callerFilePath = "",
                                             [CallerLineNumber] int callerLineNumber = 0,
                                             [CallerMemberName] string callerMember = "")
        {
            DateTime start = DateTime.UtcNow;

            Debug.WriteLine(callerFilePath);
            Debug.WriteLine(callerLineNumber);
            Debug.WriteLine(callerMember);

            while (!await condition())
            {
                await Task.Delay(pollingInterval);

                bool shouldThrow = !Debugger.IsAttached || (Debugger.IsAttached && throwWhenDebugging);
                if (shouldThrow && (DateTime.UtcNow - start).TotalMilliseconds > timeout)
                {
                    Assert.Fail($"Condition not reached within timeout ({timeout}ms): {descriptionFactory()}. File: {callerFilePath}, Line {callerLineNumber}, Test: '{callerMember}'");
                }
            }

            return DateTime.UtcNow - start;
        }
    }
}
