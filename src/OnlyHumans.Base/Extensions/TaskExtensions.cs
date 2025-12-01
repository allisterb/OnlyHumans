using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyHumans.Base.Extensions
{
    public static class TaskExtensions
    {
        public static Task<T> NotImplementedException<T>(this Task<T> task) => Task.FromException<T>(new NotImplementedException());
    }
}
