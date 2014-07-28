
namespace Caliburn.Light
{
    /// <summary>
    /// Denotes an instance which implements <see cref="IActivate"/>,  <see cref="IDeactivate"/>, <see cref="ICloseGuard"/> and <see cref="IBindableObject"/>
    /// </summary>
    public interface IScreen : IActivate, IDeactivate, ICloseGuard, IBindableObject
    {
    }
}
