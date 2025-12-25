using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.Core
{
    public static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// Register a service. By default, duplicate registration is NOT allowed (dev-friendly).
        /// If you really want override, call RegisterOrReplace instead.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var type = typeof(T);

            if (_services.TryGetValue(type, out var existing) && existing != null)
            {
                Debug.LogError(
                    $"[ServiceContainer] DUPLICATE REGISTER: {type.Name}\n" +
                    $"Existing: {existing.GetType().Name}\n" +
                    $"New: {service.GetType().Name}\n" +
                    $"Stack:\n{Environment.StackTrace}"
                );
                return;
            }

            _services[type] = service;
            //Debug.Log($"[ServiceContainer] Register {type.Name} -> {service.GetType().Name}");
        }

        /// <summary>
        /// Register a service and allow override (use with caution).
        /// </summary>
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

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
                return (T)obj;

            throw new Exception($"Service not found: {typeof(T).Name}");
        }

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

        public static void Clear() => _services.Clear();
    }
}
