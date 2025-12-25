using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.Core
{
    /// <summary>
    /// A minimal service locator (DI container) used by this template.
    ///
    /// Why it exists:
    /// - Keep Unity scenes/prefabs lightweight (avoid long inspector wiring).
    /// - Centralize access to runtime services (UI, Audio, Input, Save, Settings, etc.).
    ///
    /// Notes / Rules:
    /// - Services are stored by their interface/type (typeof(T)).
    /// - By default, Register() rejects duplicate registrations (dev-friendly).
    ///   This prevents subtle bugs where a correct service is overwritten by a broken one.
    /// - If you really want to override an existing service (rare), use RegisterOrReplace().
    ///
    /// Limitations:
    /// - Not thread-safe (intended for main thread use).
    /// - Global state; call Clear() only during shutdown / full reset (e.g., leaving play mode).
    /// </summary>
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// Registers a service instance for type T.
        /// Duplicate registrations are rejected to avoid accidental overrides.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
        public static void Register<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var type = typeof(T);

            if (_services.TryGetValue(type, out var existing) && existing != null)
            {
                // StackTrace is expensive; keep it in Editor / Development builds only.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError(
                    $"[ServiceContainer] DUPLICATE REGISTER: {type.Name}\n" +
                    $"Existing: {existing.GetType().Name}\n" +
                    $"New: {service.GetType().Name}\n" +
                    $"Stack:\n{Environment.StackTrace}"
                );
#else
                Debug.LogError(
                    $"[ServiceContainer] DUPLICATE REGISTER: {type.Name} " +
                    $"(Existing: {existing.GetType().Name}, New: {service.GetType().Name})"
                );
#endif
                return;
            }

            _services[type] = service;
        }

        /// <summary>
        /// Registers a service instance for type T and allows overriding an existing service.
        /// Use with caution: overriding can easily hide bugs.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
        public static void RegisterOrReplace<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var type = typeof(T);

            if (_services.TryGetValue(type, out var existing) && existing != null)
            {
                Debug.LogWarning(
                    $"[ServiceContainer] REPLACE: {type.Name}\n" +
                    $"Existing: {existing.GetType().Name}\n" +
                    $"New: {service.GetType().Name}"
                );
            }

            _services[type] = service;
        }

        /// <summary>
        /// Gets a registered service. Throws if not found.
        /// Prefer TryGet() if you want to handle missing services gracefully.
        /// </summary>
        /// <exception cref="Exception">Thrown when the service is not registered.</exception>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
                return (T)obj;

            throw new Exception($"Service not found: {typeof(T).Name}");
        }

        /// <summary>
        /// Tries to get a registered service.
        /// Returns true if found; otherwise returns false and sets service to null.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Clears all registered services.
        /// Warning: This wipes global state. Call only when you are sure the app is resetting.
        /// </summary>
        public static void Clear() => _services.Clear();
    }
}
