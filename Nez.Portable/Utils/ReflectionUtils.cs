﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Nez.Utils;

/// <summary>
///     helper class to fetch property delegates
/// </summary>
[PublicAPI]
public static class ReflectionUtils
{
    public static T CreateDelegate<T>(object targetObject, MethodInfo methodInfo)
    {
        return (T)(object)Delegate.CreateDelegate(typeof(T), targetObject, methodInfo);
    }

    /// <summary>
    ///     gets all subclasses of
    ///     <paramref name="baseClassType">
    ///         optionally filtering only for those with
    ///         a parameterless constructor. Abstract Types will not be returned.
    /// </paramref>
    /// </summary>
    /// <param name="baseClassType"></param>
    /// <param name="onlyIncludeParameterlessConstructors"></param>
    /// <returns></returns>
    public static List<Type> GetAllSubclasses(Type baseClassType, bool onlyIncludeParameterlessConstructors = false)
    {
        var typeList = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        foreach (var type in assembly.GetTypes())
            if (type.IsSubclassOf(baseClassType) && !type.IsAbstract)
            {
                if (onlyIncludeParameterlessConstructors)
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                    {
                        Debug.Log("no go: " + type.Name);
                        continue;
                    }

                typeList.Add(type);
            }

        return typeList;
    }

    /// <summary>
    ///     gets all Types assignable from
    ///     <paramref name="baseClassType">
    ///         optionally filtering only for those with
    ///         a parameterless constructor. Abstract Types will not be returned.
    /// </paramref>
    /// </summary>
    /// <param name="baseClassType"></param>
    /// <param name="onlyIncludeParameterlessConstructors"></param>
    /// <returns></returns>
    public static List<Type> GetAllTypesAssignableFrom(Type baseClassType,
        bool onlyIncludeParameterlessConstructors = false)
    {
        var typeList = new List<Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        foreach (var type in assembly.GetTypes())
            if (baseClassType.IsAssignableFrom(type) && !type.IsAbstract)
            {
                if (onlyIncludeParameterlessConstructors)
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                typeList.Add(type);
            }

        return typeList;
    }

    /// <summary>
    ///     checks <paramref name="type" /> to see if it or any base class in the chain IsGenericType
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsGenericTypeOrSubclassOfGenericType(Type type)
    {
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType)
                return true;

            currentType = currentType.BaseType;
        }

        return false;
    }

    public static List<Type> GetAllTypesWithAttribute<T>() where T : Attribute
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).ToList();
    }

    #region Fields

    public static FieldInfo GetFieldInfo(object targetObject, string fieldName)
    {
        return GetFieldInfo(targetObject.GetType(), fieldName);
    }

    public static FieldInfo GetFieldInfo(Type type, string fieldName)
    {
        FieldInfo? fieldInfo;
        var nullableType = type;
        do
        {
            fieldInfo = nullableType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            nullableType = type.BaseType;
        } while (fieldInfo == null && nullableType != null);

        return fieldInfo!;
    }

    public static IEnumerable<FieldInfo> GetFields(Type type)
    {
        return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    public static object? GetFieldValue(object targetObject, string fieldName)
    {
        return GetFieldInfo(targetObject, fieldName).GetValue(targetObject);
    }

    #endregion

    #region Properties

    public static PropertyInfo? GetPropertyInfo(object targetObject, string propertyName)
    {
        return GetPropertyInfo(targetObject.GetType(), propertyName);
    }

    public static PropertyInfo? GetPropertyInfo(Type type, string propertyName)
    {
        return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    public static IEnumerable<PropertyInfo> GetProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    public static MethodInfo? GetPropertyGetter(PropertyInfo prop)
    {
        return prop.GetGetMethod(true);
    }

    public static MethodInfo? GetPropertySetter(PropertyInfo prop)
    {
        return prop.GetSetMethod(true);
    }

    public static object? GetPropertyValue(object targetObject, string propertyName)
    {
        var propInfo = GetPropertyInfo(targetObject, propertyName);

        if (propInfo is null)
        {
            return null;
        }
        
        var methodInfo = GetPropertyGetter(propInfo);
        return methodInfo?.Invoke(targetObject, []);
    }

    /// <summary>
    ///     either returns a super fast Delegate to set the given property or null if it couldn't be found
    ///     via reflection
    /// </summary>
    public static T? SetterForProperty<T>(object targetObject, string propertyName) where T : class
    {
        // first get the property
        var propInfo = GetPropertyInfo(targetObject, propertyName);
        return propInfo?.SetMethod != null ? CreateDelegate<T>(targetObject, propInfo.SetMethod) : null;
    }

    /// <summary>
    ///     either returns a super fast Delegate to get the given property or null if it couldn't be found
    ///     via reflection
    /// </summary>
    public static T? GetterForProperty<T>(object targetObject, string propertyName) where T : class
    {
        // first get the property
        var propInfo = GetPropertyInfo(targetObject, propertyName);
        return propInfo?.GetMethod != null ? CreateDelegate<T>(targetObject, propInfo.GetMethod) : null;
    }

    #endregion

    #region Methods

    public static IEnumerable<MethodInfo> GetMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    public static MethodInfo? GetMethodInfo(object targetObject, string methodName)
    {
        return GetMethodInfo(targetObject.GetType(), methodName);
    }

    public static MethodInfo? GetMethodInfo(object targetObject, string methodName, Type[] parameters)
    {
        return GetMethodInfo(targetObject.GetType(), methodName, parameters);
    }

    public static MethodInfo? GetMethodInfo(Type type, string methodName, Type[]? parameters = null)
    {
        if (parameters is null)
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            Type.DefaultBinder, parameters, null);
    }

    #endregion
}