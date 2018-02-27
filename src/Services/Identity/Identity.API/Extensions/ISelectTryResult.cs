using System;

namespace Microsoft.eShopOnContainers.Services.Identity.API.Extensions {
    public interface ISelectTryResult<out TSource, out TResult>
    {
        Exception CaughtException { get; }
        TResult Result { get; }
        TSource Source { get; }
    }
}