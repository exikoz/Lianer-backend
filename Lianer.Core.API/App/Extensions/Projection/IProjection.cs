using System.Linq.Expressions;
namespace Lianer.Core.API.Projection;
public interface IProjection<Entity, Dto>
where Entity: class
where Dto: class
{
    static abstract Expression<Func<Entity, Dto>> Selector {get;}
}
