using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a service provider that always returns <see langword="null"/> for any requested service.
    /// </summary>
    /// <remarks>This class implements the <see cref="IServiceProvider"/> interface and serves as a
    /// placeholder or default implementation when no actual service provider is required. It is a singleton, and the
    /// <see cref="Instance"/> property provides the single instance of this class.</remarks>
    public sealed class NullServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Gets an instance of the <see cref="NullServiceProvider"/>.
        /// </summary>
        public static NullServiceProvider Instance { get; } = new NullServiceProvider();

        private NullServiceProvider()
        {
        }

        /// <inheritdoc />
        public object? GetService(Type serviceType) => null;
    }
}
