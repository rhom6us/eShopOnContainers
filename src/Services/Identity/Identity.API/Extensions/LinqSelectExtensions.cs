using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Identity.API.Extensions {
    public static class LinqSelectExtensions {
        [NotNull]
        public static IEnumerable<ISelectTryResult<TSource, TResult>> SelectTry<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {

            return source.Select(
                element => {
                    try {
                        return new SelectTryResult<TSource, TResult>(element, selector(element));
                    }
                    catch (Exception ex) {
                        return new SelectTryResult<TSource, TResult>(element, ex);
                    }
                });

        }

        [NotNull]
        public static IEnumerable<TResult> Switch<TSource, TOperand, TResult>([NotNull]this IEnumerable<TSource> source, Func<TSource, TOperand> operandSelector, IDictionary<TOperand, Func<TSource,TResult>> branches) {
            return source.Select(item => branches[operandSelector(item)](item));
            
        }

        [NotNull]
        public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<ISelectTryResult<TSource, TResult>> source, Func<Exception, TResult> exceptionHandler) {
            return LinqSelectExtensions.OnCaughtException(source, (s, ex) => exceptionHandler(ex));
        }

        [NotNull]
        public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<ISelectTryResult<TSource, TResult>> source, Func<TSource, Exception,  TResult> exceptionHandler) {
            return source.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.Source, x.CaughtException));
        }

        private class SelectTryResult<TSource, TResult> : ISelectTryResult<TSource, TResult>
        {
            public TSource Source { get; }
            public TResult Result { get; } = default(TResult);
            public Exception CaughtException { get; } = null;

            private SelectTryResult(TSource source) {
                this.Source = source;

            }

            internal SelectTryResult(TSource source, Exception exception) : this(source) {
                this.CaughtException = exception;
            }
            internal SelectTryResult(TSource source, TResult result): this(source) {
                this.Result = result;
            }
        }
    }
}
