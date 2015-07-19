using System;

namespace UberDeployer.Common.SyntaxSugar
{
  public static class SyntaxSugarExtensions
  {
    public static TResult With<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator)
      where TResult : class
      where TInput : class
    {
      return o == null ? null : evaluator(o);
    }

    public static TResult Return<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator, TResult failureValue)
      where TInput : class
    {
      return o == null ? failureValue : evaluator(o);
    }

    public static bool Check<TInput>(this TInput o, Func<TInput, bool> evaluator)
      where TInput : class
    {
      return Return(o, evaluator, false);
    }

    public static TInput If<TInput>(this TInput o, Func<TInput, bool> evaluator)
      where TInput : class
    {
      if (o == null)
      {
        return null;
      }

      return evaluator(o) ? o : null;
    }

    public static TInput Unless<TInput>(this TInput o, Func<TInput, bool> evaluator)
      where TInput : class
    {
      if (o == null)
      {
        return null;
      }

      return evaluator(o) ? null : o;
    }

    public static TInput Do<TInput>(this TInput o, Action<TInput> action)
      where TInput : class
    {
      if (o == null)
      {
        return null;
      }
      action(o);
      return o;
    }
  }
}