
namespace Caliburn.Light
{
    /// <summary>
    /// Denotes an instance which implements <see cref="IActivatable"/>, <see cref="ICloseGuard"/> and <see cref="IBindableObject"/>
    /// </summary>
    public interface IScreen : IActivatable, ICloseGuard, IBindableObject
    {
    }
}
