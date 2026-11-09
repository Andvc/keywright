using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keywright.Core;

namespace CurioTest;

/// <summary>
/// Runs every curio check (any static class here with <c>public static IEnumerable&lt;Check&gt; Checks()</c>), prints
/// each one ok or FAIL, then names any curio nothing checks. Exits 1 on a failure. <c>--only=key,key</c> runs just those
/// curios' checks.
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        var only = args.FirstOrDefault(a => a.StartsWith("--only="))?["--only=".Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
        var checks = typeof(Program).Assembly.GetTypes()
            .Where(t => t.IsAbstract && t.IsSealed && t.Namespace == typeof(Program).Namespace)
            .Select(t => t.GetMethod("Checks", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes))
            .Where(m => m != null && m.ReturnType == typeof(IEnumerable<Check>))
            .SelectMany(m => (IEnumerable<Check>)m.Invoke(null, null))
            .Where(c => only == null || only.Contains(c.Curio))
            .ToList();
        int failed = 0;
        foreach (var check in checks)
        {
            try
            {
                check.Run();
                Console.WriteLine($"ok    {check.Curio,-12} {check.What}");
            }
            catch (Exception e)
            {
                failed++;
                var inner = e is TargetInvocationException { InnerException: { } cause } ? cause : e;
                Console.WriteLine($"FAIL  {check.Curio,-12} {check.What}\n      {inner.GetType().Name}: {inner.Message}");
                if (inner is not Failed) Console.WriteLine(inner.StackTrace?.Split('\n').FirstOrDefault()?.Trim());
            }
        }
        var unchecked_ = Curios.All.Select(c => c.Key).Where(k => checks.All(c => c.Curio != k)).ToList();
        if (only == null && unchecked_.Count > 0) Console.WriteLine($"\nNo check yet for {unchecked_.Count}: {string.Join(", ", unchecked_)}");
        Console.WriteLine($"\n{checks.Count - failed} of {checks.Count} checks passed{(failed > 0 ? $", {failed} failed" : "")}.");
        return failed > 0 ? 1 : 0;
    }
}
