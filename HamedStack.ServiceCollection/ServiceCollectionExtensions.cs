// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

namespace HamedStack.ServiceCollection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

/// <summary>
/// Provides extension methods for IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Checks if the specified service type is registered in the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to check.</param>
    /// <param name="serviceType">The type of the service to check for registration.</param>
    /// <returns>true if the service type is registered; otherwise, false.</returns>
    /// <remarks>
    /// This method iterates over the service collection to check if a service type is registered.
    /// It does not consider the lifetime or implementation of the service.
    /// </remarks>
    public static bool IsRegistered(this IServiceCollection services, Type serviceType)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

        return services.Any(serviceDescriptor => serviceDescriptor.ServiceType == serviceType);
    }

    /// <summary>
    /// Tries to get the service of type T from the service provider built from the service collection.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <param name="services">The IServiceCollection to build the ServiceProvider from.</param>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>true if the service of type T is found; otherwise, false.</returns>
    /// <remarks>
    /// This method builds a new ServiceProvider to attempt to retrieve the service.
    /// Be aware that building the ServiceProvider can be costly, and caching strategies should be considered
    /// for performance-critical applications.
    /// </remarks>
    public static bool TryGetService<T>(this IServiceCollection services, out T? service) where T : class
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var serviceProvider = services.BuildServiceProvider();
        service = serviceProvider.GetService<T>();
        return service != null;
    }

    /// <summary>
    /// Attempts to find a service of the specified type in the service collection.
    /// </summary>
    /// <typeparam name="T">The type of the service to look for.</typeparam>
    /// <param name="services">The IServiceCollection to search.</param>
    /// <param name="descriptor">The found service descriptor, if any.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    public static bool TryGetService<T>(
        this IServiceCollection services,
        out ServiceDescriptor? descriptor)
    {
        descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(T));
        return descriptor != null;
    }

    /// <summary>
    /// Attempts to find a service of the specified type in the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to search.</param>
    /// <param name="serviceType">The type of the service to look for.</param>
    /// <param name="descriptor">The found service descriptor, if any.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    public static bool TryGetService(
        this IServiceCollection services,
        Type serviceType,
        out ServiceDescriptor? descriptor)
    {
        descriptor = services.FirstOrDefault(sd => sd.ServiceType == serviceType);
        return descriptor != null;
    }


    /// <summary>
    /// Removes the first occurrence of a service of the specified type from the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service to remove.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to modify.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="services"/> is read-only.</exception>
    public static IServiceCollection Remove<T>(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (services.IsReadOnly)
        {
            throw new InvalidOperationException($"{nameof(services)} is read only");
        }

        var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
        if (serviceDescriptor != null) services.Remove(serviceDescriptor);

        return services;
    }

    /// <summary>
    /// Removes all occurrences of services of the specified types from the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to modify.</param>
    /// <param name="types">An array of service types to remove.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="services"/> is read-only.</exception>
    public static IServiceCollection RemoveAll(this IServiceCollection services, params Type[] types)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (types == null) throw new ArgumentNullException(nameof(types));

        if (services.IsReadOnly)
        {
            throw new InvalidOperationException($"{nameof(services)} is read only");
        }

        foreach (var type in types)
        {
            var serviceDescriptor = services.Where(descriptor => descriptor.ServiceType == type).ToList();
            foreach (var descriptor in serviceDescriptor)
            {
                services.Remove(descriptor);
            }
        }

        return services;
    }
    /// <summary>
    /// Adds a singleton service of the type specified in <typeparamref name="TService"/> to the
    /// <see cref="IServiceCollection"/> if the service type has not already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSingletonIfNotExists<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        if (services.All(sd => sd.ServiceType != typeof(TService)))
        {
            services.AddSingleton<TService, TImplementation>();
        }
        return services;
    }

    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TService"/> to the
    /// <see cref="IServiceCollection"/> if the service type has not already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddScopedIfNotExists<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        if (services.All(sd => sd.ServiceType != typeof(TService)))
        {
            services.AddScoped<TService, TImplementation>();
        }
        return services;
    }

    /// <summary>
    /// Adds a transient service of the type specified in <typeparamref name="TService"/> to the
    /// <see cref="IServiceCollection"/> if the service type has not already been registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddTransientIfNotExists<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        if (services.All(sd => sd.ServiceType != typeof(TService)))
        {
            services.AddTransient<TService, TImplementation>();
        }
        return services;
    }

    /// <summary>
    /// Removes all services that match the given predicate.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to remove the services from.</param>
    /// <param name="predicate">The predicate to match services against.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection RemoveIf(this IServiceCollection services, Func<ServiceDescriptor, bool> predicate)
    {
        var toRemove = services.Where(predicate).ToList();
        foreach (var service in toRemove)
        {
            services.Remove(service);
        }
        return services;
    }
    /// <summary>
    /// Adds a service of the specified type to the service collection or replaces an existing registration.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add or replace.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to modify.</param>
    /// <param name="lifetime">The lifecycle of the service.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddOrReplace<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
        where TService : class
        where TImplementation : class, TService
    {
        var existing = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
        if (existing != null)
        {
            services.Remove(existing);
        }

        services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
        return services;
    }
    /// <summary>
    /// Conditionally adds a service to the collection based on a predicate.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="predicate">The condition under which to add the service.</param>
    /// <param name="implementationFactory">The factory that creates the service instance.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIf<TService>(this IServiceCollection services, Func<bool> predicate, Func<IServiceProvider, TService> implementationFactory, ServiceLifetime lifetime)
        where TService : class
    {
        if (predicate())
        {
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, lifetime);
            services.Add(descriptor);
        }
        return services;
    }

    /// <summary> 
    /// Replace the services identified by <typeparamref name="TService"/> with the implementation provided by <typeparamref name="TImplementation"/>. 
    /// </summary> 
    /// <param name="serviceCollection">The design-time service collection.</param> 
    /// <typeparam name="TService">The type of the service to replace.</typeparam> 
    /// <typeparam name="TImplementation">The implementation type for the replaced service.</typeparam> 
    public static void Replace<TService, TImplementation>(this IServiceCollection serviceCollection)
    {
        serviceCollection.Replace(typeof(TService), typeof(TImplementation));
    }

    /// <summary> 
    /// Replace the services identified by <paramref name="serviceType"/> with the implementation provided by <paramref name="implementationType"/>. 
    /// </summary> 
    /// <param name="serviceCollection">The design-time service collection.</param> 
    /// <param name="serviceType">The type of the service to replace.</param> 
    /// <param name="implementationType">The implementation type for the replaced service.</param> 
    public static void Replace(this IServiceCollection serviceCollection, Type serviceType, Type implementationType)
    {
        foreach (var originalServiceDescriptor in serviceCollection.Where(e => e.ServiceType == serviceType).ToList())
        {
            var serviceDescriptor = new ServiceDescriptor(serviceType, implementationType, originalServiceDescriptor.Lifetime);
            serviceCollection.Replace(serviceDescriptor);
        }
    }

    /// <summary>
    /// Tries to add a service to the collection using a factory if it is not already registered.
    /// </summary>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="implementationFactory">The factory to create the service.</param>
    /// <param name="lifetime">The lifetime of the service.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAdd<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory, ServiceLifetime lifetime)
        where TService : class
    {
        if (services.All(sd => sd.ServiceType != typeof(TService)))
        {
            var descriptor = new ServiceDescriptor(typeof(TService), implementationFactory, lifetime);
            services.Add(descriptor);
        }
        return services;
    }

    /// <summary>
    /// Removes all registrations of the specified service type.
    /// </summary>
    /// <typeparam name="TService">The type of the service to remove.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection RemoveAll<TService>(this IServiceCollection services)
    {
        var serviceDescriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (var descriptor in serviceDescriptors)
        {
            services.Remove(descriptor);
        }
        return services;
    }

    /// <summary>
    /// Checks if any implementation of a given interface is registered.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>True if any implementation is registered; otherwise, false.</returns>
    public static bool HasImplementationOf<TService>(this IServiceCollection services)
    {
        return services.Any(sd => sd.ImplementationType != null && typeof(TService).IsAssignableFrom(sd.ImplementationType));
    }

}
