namespace OnlyHumans;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TaskExtensions
{
    public static Task<Result<U>> Then<T, U>(this Task<Result<T>> resultTask, Func<T, U> func)
    {
        return resultTask.ContinueWith(t =>
        {
            if (resultTask.IsCompletedSuccessfully)
            {
                if (t.Result.IsSuccess)
                {
                    return Result<U>.Success(func(t.Result.Value));
                }
                else
                {
                    return Result<U>.Failure(t.Result.Message, t.Result.Exception);
                }
            }
            else if (t.IsFaulted)     
            {
                return Result<U>.Failure(null, t.Exception);
            }
            else
            {
                return Result<U>.Failure("The operation was canceled or otherwise failed to complete.");
            }
        });
    }

    public static Task<Result<T>> ThenWhen<T>(this Task<Result<T>> resultTask, Func<T, bool> condition, Func<T, T> func)    
        =>  resultTask.Then(r => condition(r) ? func(r) : r);

    public static Task<Result<T>> ThenWhen<T>(this Task<Result<T>> resultTask, Func<T, bool> condition, Action<T> func)
        => resultTask.Then(r =>
        { 
            if (condition(r))
            {
                func(r);
                
            }
            return r;
        });

    public static Task<bool> IsSuccess<T>(this Task<Result<T>> resultTask) =>
        resultTask.ContinueWith(t => t.IsCompletedSuccessfully && t.Result.IsSuccess);

    public static Task<T> Succeeded<T>(this Task<Result<T>> resultTask) =>
       resultTask.ContinueWith(t =>
       {
           if (t.IsCompletedSuccessfully)
           {
               if (t.Result.IsSuccess)
               {
                   return t.Result.Value;
               }
               else if (t.Result.Exception is not null)
               {
                   throw t.Result.Exception;
               }
               else throw new InvalidOperationException();
           }
           else if (t.Exception is not null)
           {
               throw t.Exception;
           }
           else
           {
               throw new InvalidOperationException();
           }
       });
}

