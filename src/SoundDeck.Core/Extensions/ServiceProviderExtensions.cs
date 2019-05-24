namespace SoundDeck.Core.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Provides extension methods for <see cref="IServiceProvider"/>.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="provider">The type to activate.</param>
        /// <param name="parameters">Constructor arguments not provided by the provider..</param>
        /// <returns>An activated object of type <typeparamref name="T"/>.</returns>
        public static T GetInstance<T>(this IServiceProvider provider, params object[] parameters)
            => ActivatorUtilities.CreateInstance<T>(provider, parameters);
    }
}
