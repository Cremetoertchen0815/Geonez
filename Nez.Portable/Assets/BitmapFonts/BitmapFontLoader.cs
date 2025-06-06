﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Nez.BitmapFonts;

/// <summary>
///     Parsing class for bitmap fonts generated by AngelCode BMFont
/// </summary>
public static class BitmapFontLoader
{
	/// <summary>
	///     Loads a bitmap font from a file, attempting nto auto detect the file type
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
	/// <exception cref="FileNotFoundException">Thrown when the requested file is not present.</exception>
	/// <exception cref="InvalidDataException">Thrown when an Invalid Data error condition occurs.</exception>
	/// <param name="filename">Name of the file to load.</param>
	/// <returns>
	///     A <see cref="BitmapFont" /> containing the loaded data.
	/// </returns>
	public static BitmapFont LoadFontFromFile(string filename, bool premultiplyAlpha = false)
    {
        using (var file = TitleContainer.OpenStream(filename))
        {
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                if (line.StartsWith("info "))
                    return LoadFontFromTextFile(filename, premultiplyAlpha);
                if (line.StartsWith("<?xml") || line.StartsWith("<font"))
                    return LoadFontFromXmlFile(filename, premultiplyAlpha);
                throw new InvalidDataException("Unknown file format.");
            }
        }
    }

	/// <summary>
	///     Loads a bitmap font from a file containing font data in text format.
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
	/// <exception cref="FileNotFoundException">Thrown when the requested file is not present.</exception>
	/// <param name="filename">Name of the file to load.</param>
	/// <returns>
	///     A <see cref="BitmapFont" /> containing the loaded data.
	/// </returns>
	public static BitmapFont LoadFontFromTextFile(string filename, bool premultiplyAlpha = false)
    {
        var font = new BitmapFont();
        using (var stream = TitleContainer.OpenStream(filename))
        {
            font.LoadText(stream);
        }

        QualifyResourcePaths(font, Path.GetDirectoryName(filename));
        font.Initialize(premultiplyAlpha);

        return font;
    }

	/// <summary>
	///     Loads a bitmap font from a file containing font data in XML format.
	/// </summary>
	/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
	/// <exception cref="FileNotFoundException">Thrown when the requested file is not present.</exception>
	/// <param name="filename">Name of the file to load.</param>
	/// <returns>
	///     A <see cref="BitmapFont" /> containing the loaded data.
	/// </returns>
	public static BitmapFont LoadFontFromXmlFile(string filename, bool premultiplyAlpha = false)
    {
        var font = new BitmapFont();
        using (var stream = TitleContainer.OpenStream(filename))
        {
            font.LoadXml(stream);
        }

        QualifyResourcePaths(font, Path.GetDirectoryName(filename));
        font.Initialize(premultiplyAlpha);

        return font;
    }

	/// <summary>
	///     Returns a boolean from an array of name/value pairs.
	/// </summary>
	/// <param name="parts">The array of parts.</param>
	/// <param name="name">The name of the value to return.</param>
	/// <param name="defaultValue">Default value(if the key doesnt exist or can't be parsed)</param>
	/// <returns></returns>
	internal static bool GetNamedBool(string[] parts, string name, bool defaultValue = false)
    {
        var s = GetNamedString(parts, name);
        if (int.TryParse(s, out var v))
            return v > 0;

        return defaultValue;
    }

	/// <summary>
	///     Returns an integer from an array of name/value pairs.
	/// </summary>
	/// <param name="parts">The array of parts.</param>
	/// <param name="name">The name of the value to return.</param>
	/// <param name="defaultValue">Default value(if the key doesnt exist or can't be parsed)</param>
	/// <returns></returns>
	internal static int GetNamedInt(string[] parts, string name, int defaultValue = 0)
    {
        var s = GetNamedString(parts, name);
        if (!int.TryParse(s, out var result))
            return defaultValue;

        return result;
    }

	/// <summary>
	///     Returns a string from an array of name/value pairs.
	/// </summary>
	/// <param name="parts">The array of parts.</param>
	/// <param name="name">The name of the value to return.</param>
	/// <returns></returns>
	internal static string GetNamedString(string[] parts, string name)
    {
        var result = string.Empty;
        foreach (var part in parts)
        {
            var nameEndIndex = part.IndexOf('=');
            if (nameEndIndex != -1)
            {
                var namePart = part.Substring(0, nameEndIndex);
                var valuePart = part.Substring(nameEndIndex + 1);

                if (string.Equals(name, namePart, StringComparison.OrdinalIgnoreCase))
                {
                    var length = valuePart.Length;
                    if (length > 1 && valuePart[0] == '"' && valuePart[length - 1] == '"')
                        valuePart = valuePart.Substring(1, length - 2);

                    result = valuePart;
                    break;
                }
            }
        }

        return result;
    }

	/// <summary>
	///     Creates a Padding object from a string representation
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns></returns>
	internal static Padding ParsePadding(string s)
    {
        var parts = s.Split(',');
        return new Padding
        {
            Left = Convert.ToInt32(parts[3].Trim()),
            Top = Convert.ToInt32(parts[0].Trim()),
            Right = Convert.ToInt32(parts[1].Trim()),
            Bottom = Convert.ToInt32(parts[2].Trim())
        };
    }

	/// <summary>
	///     Creates a Point object from a string representation
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns></returns>
	internal static Point ParseInt2(string s)
    {
        var parts = s.Split(',');
        return new Point
        {
            X = Convert.ToInt32(parts[0].Trim()),
            Y = Convert.ToInt32(parts[1].Trim())
        };
    }

	/// <summary>
	///     Updates <see cref="Page" /> data with a fully qualified path
	/// </summary>
	/// <param name="font">The <see cref="BitmapFont" /> to update.</param>
	/// <param name="resourcePath">The path where texture resources are located.</param>
	internal static void QualifyResourcePaths(BitmapFont font, string resourcePath)
    {
        var pages = font.Pages;
        for (var i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            page.Filename = Path.Combine(resourcePath, page.Filename);
            pages[i] = page;
        }

        font.Pages = pages;
    }

	/// <summary>
	///     Splits the specified string using a given delimiter, ignoring any instances of the delimiter as part of a quoted
	///     string.
	/// </summary>
	/// <param name="s">The string to split.</param>
	/// <param name="delimiter">The delimiter.</param>
	/// <returns></returns>
	internal static string[] Split(string s, char delimiter)
    {
        if (s.IndexOf('"') != -1)
        {
            var partStart = -1;
            var parts = new List<string>();

            do
            {
                var quoteStart = s.IndexOf('"', partStart + 1);
                var quoteEnd = s.IndexOf('"', quoteStart + 1);
                var partEnd = s.IndexOf(delimiter, partStart + 1);

                if (partEnd == -1)
                    partEnd = s.Length;

                var hasQuotes = quoteStart != -1 && partEnd > quoteStart && partEnd < quoteEnd;
                if (hasQuotes)
                    partEnd = s.IndexOf(delimiter, quoteEnd + 1);

                parts.Add(s.Substring(partStart + 1, partEnd - partStart - 1));
                if (hasQuotes)
                    partStart = partEnd - 1;

                partStart = s.IndexOf(delimiter, partStart + 1);
            } while (partStart != -1);

            return parts.ToArray();
        }

        return s.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
    }

	/// <summary>
	///     Converts the given collection into an array
	/// </summary>
	/// <typeparam name="T">Type of the items in the array</typeparam>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	internal static T[] ToArray<T>(ICollection<T> values)
    {
        var result = new T[values.Count];
        values.CopyTo(result, 0);

        return result;
    }
}