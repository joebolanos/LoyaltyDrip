using Core.Shared.Common;
using MediatR;

namespace Core.Shared.Abstractions
{
    public interface ICacheable
    {
        bool BypassCache { get; }
        string CacheKey { get; }
    }

    public interface ICacheableRequest : ICacheable { }

    public interface ICacheableRequest<T> : ICacheableRequest, IRequest<Result<T>> { }
}
