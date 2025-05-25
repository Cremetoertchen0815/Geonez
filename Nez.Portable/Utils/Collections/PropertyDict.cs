using System;
using System.Collections.Generic;

namespace Nez;

public class PropertyDict : Dictionary<string, string>
{
    public new string this[string index]
    {
        get => ContainsKey(index) ? base[index] : null;
        set
        {
            if (ContainsKey(index)) base[index] = value;
            else Add(index, value);
        }
    }

    public T FetchType<T>(string index, Func<string, T> conversionFunc, T errVal = default)
    {
        return ContainsKey(index) ? conversionFunc(base[index]) : errVal;
    }

    public int FetchInteger(string index, int errVal = default)
    {
        return ContainsKey(index) && int.TryParse(base[index], out var i) ? i : errVal;
    }

    public bool FetchBoolean(string index, bool errVal = default)
    {
        return ContainsKey(index) && bool.TryParse(base[index], out var b) ? b : errVal;
    }

    public float FetchFloat(string index, float errVal = default)
    {
        return ContainsKey(index) && float.TryParse(base[index], out var f) ? f : errVal;
    }

    public Telegram FetchTelegram(string index, Telegram errVal = default)
    {
        return ContainsKey(index) ? Telegram.Deserialize(base[index]) ?? errVal : errVal;
    }


    public int? FetchIntegerNullable(string index)
    {
        return ContainsKey(index) && int.TryParse(base[index], out var i) ? i : null;
    }

    public bool? FetchBooleanNullable(string index)
    {
        return ContainsKey(index) && bool.TryParse(base[index], out var b) ? b : null;
    }

    public float? FetchFloatNullable(string index)
    {
        return ContainsKey(index) && float.TryParse(base[index], out var f) ? f : null;
    }

    public Telegram? FetchTelegramNullable(string index)
    {
        return ContainsKey(index) ? Telegram.Deserialize(base[index]) : null;
    }
}