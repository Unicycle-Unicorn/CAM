﻿using AuthProvider.AuthModelBinder;
using Microsoft.AspNetCore.Http;

namespace AuthProvider.Utils;
public static class ItemUtils
{
    public static void Add<T>(HttpContext context, object value) where T : IFromAuthModelBinder => context.Items.Add(typeof(T).Name, value);

    public static bool TryGet<T>(HttpContext context, out object? value)
    {
        return TryGet(context, typeof(T), out value);
    }

    public static bool TryGet<T, A>(HttpContext context, out A value) where T : FromAuthModelBinder<A>
    {

        bool t = TryGet(context, typeof(T), out object? s);
        value = t ? (A)s : default;
        return t;
    }

    public static bool TryGet(HttpContext context, Type type, out object? value) => context.Items.TryGetValue(type.Name, out value) && value != null;
}
