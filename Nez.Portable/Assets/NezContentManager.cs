using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;

namespace Nez.Systems;

/// <summary>
///     ContentManager subclass that also manages Effects from ogl files. Adds asynchronous loading of assets as well.
/// </summary>
public class NezContentManager : ContentManager
{
    private readonly Dictionary<string, Effect> _loadedEffects = new();
    private List<IDisposable> _disposableAssets;

    private List<IDisposable> DisposableAssets
    {
        get
        {
            if (_disposableAssets == null)
            {
                var fieldInfo = ReflectionUtils.GetFieldInfo(typeof(ContentManager), "disposableAssets");
                _disposableAssets = fieldInfo.GetValue(this) as List<IDisposable>;
            }

            return _disposableAssets;
        }
    }

#if FNA
		Dictionary<string, object> _loadedAssets;
		Dictionary<string, object> LoadedAssets
		{
			get
			{
				if (_loadedAssets == null)
				{
					var fieldInfo = ReflectionUtils.GetFieldInfo(typeof(ContentManager), "loadedAssets");
					_loadedAssets = fieldInfo.GetValue(this) as Dictionary<string, object>;
				}
				return _loadedAssets;
			}
		}
#endif


    public NezContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider,
        rootDirectory)
    {
    }

    public NezContentManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public NezContentManager() : base(((Game)Core._instance).Services, ((Game)Core._instance).Content.RootDirectory)
    {
    }

    #region Strongly Typed Loaders

    /// <summary>
    ///     loads a Texture2D either from xnb or directly from a png/jpg. Note that xnb files should not contain the .xnb file
    ///     extension or be preceded by "Content" in the path. png/jpg files should have the file extension and have an
    ///     absolute
    ///     path or a path starting with "Content".
    /// </summary>
    public Texture2D LoadTexture(string name, bool premultiplyAlpha = false)
    {
        // no file extension. Assumed to be an xnb so let ContentManager load it
        if (string.IsNullOrEmpty(Path.GetExtension(name)))
            return Load<Texture2D>(name);

        if (LoadedAssets.TryGetValue(name, out var asset) && asset is Texture2D { IsDisposed: false } tex)
                return tex;

        using var stream = Path.IsPathRooted(name) ? File.OpenRead(name) : TitleContainer.OpenStream(name);
        var texture = premultiplyAlpha
            ? TextureUtils.TextureFromStreamPreMultiplied(stream)
            : Texture2D.FromStream(Core.GraphicsDevice, stream);
        texture.Name = name;
        LoadedAssets[name] = texture;
        DisposableAssets.Add(texture);

        return texture;
    }

    /// <summary>
    ///     Get a 3d model.
    /// </summary>
    /// <param name="path">Asset path / name.</param>
    /// <returns>Model instance.</returns>
    public Model LoadModel(string path, Func<Effect, object> tagProcessor)
    {
        // model to return

        // try to get from cache of processed models (means we already processed this one)
        if (LoadedAssets.TryGetValue(path, out var a)) return (Model)a;

        // if we got here it means its a model we didn't load and process yet. load it.
        var ret = base.Load<Model>(path);

        // create GeonBit material per effect and set it as the tag property
        foreach (var mesh in ret.Meshes)
        {
            foreach (var effect in mesh.Effects) effect.Tag = tagProcessor(effect);
            foreach (var part in mesh.MeshParts) part.Tag = part.Effect;
        }

        // return the model
        return ret;
    }

    /// <summary>
    ///     loads a SoundEffect either from xnb or directly from a wav. Note that xnb files should not contain the .xnb file
    ///     extension or be preceded by "Content" in the path. wav files should have the file extension and have an absolute
    ///     path or a path starting with "Content".
    /// </summary>
    public SoundEffect LoadSoundEffect(string name)
    {
        // no file extension. Assumed to be an xnb so let ContentManager load it
        if (string.IsNullOrEmpty(Path.GetExtension(name)))
            return Load<SoundEffect>(name);

        if (LoadedAssets.TryGetValue(name, out var asset))
            if (asset is SoundEffect sfx)
                return sfx;

        using var stream = Path.IsPathRooted(name) ? File.OpenRead(name) : TitleContainer.OpenStream(name);
        var streamSfx = SoundEffect.FromStream(stream);
        LoadedAssets[name] = streamSfx;
        DisposableAssets.Add(streamSfx);
        return streamSfx;
    }

    /// <summary>
    ///     loads a Tiled map
    /// </summary>
    public TmxMap LoadTiledMap(string name)
    {
        if (LoadedAssets.TryGetValue(name, out var asset))
            if (asset is TmxMap map)
                return map;

        var tiledMap = new TmxMap().LoadCompressedTmxMap(name, this);

        LoadedAssets[name] = tiledMap;
        DisposableAssets.Add(tiledMap);

        return tiledMap;
    }

    /// <summary>
    ///     Loads a SpriteAtlas created with the Sprite Atlas Packer tool
    /// </summary>
    public SpriteAtlas LoadSpriteAtlas(string name, bool generateOutline, bool premultiplyAlpha = false)
    {
        return SpriteAtlasLoader.ParseSpriteAtlas(name, this, generateOutline, premultiplyAlpha);
    }

    /// <summary>
    ///     Loads a BitmapFont
    /// </summary>
    public BitmapFont LoadBitmapFont(string name, bool premultiplyAlpha = false)
    {
        if (LoadedAssets.TryGetValue(name, out var asset))
            if (asset is BitmapFont bmFont)
                return bmFont;

        var font = BitmapFontLoader.LoadFontFromFile(name, premultiplyAlpha);

        LoadedAssets.Add(name, font);
        DisposableAssets.Add(font);

        return font;
    }

    /// <summary>
    ///     loads an ogl effect directly from file and handles disposing of it when the ContentManager is disposed. Name should
    ///     be the path
    ///     relative to the Content folder or including the Content folder.
    /// </summary>
    /// <returns>The effect.</returns>
    /// <param name="name">Name.</param>
    public Effect LoadEffect(string name)
    {
        return LoadEffect<Effect>(name);
    }

    /// <summary>
    ///     loads an embedded Nez effect. These are any of the Effect subclasses in the Nez/Graphics/Effects folder.
    ///     Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect
    ///     duplication.
    /// </summary>
    /// <returns>The nez effect.</returns>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public T LoadNezEffect<T>() where T : Effect, new()
    {
        var cacheKey = typeof(T).Name + "-" + Utils.RandomString(5);
        var effect = new T
        {
            Name = cacheKey
        };
        _loadedEffects[cacheKey] = effect;

        return effect;
    }

    /// <summary>
    ///     loads an ogl effect directly from file and handles disposing of it when the ContentManager is disposed. Name should
    ///     the the path
    ///     relative to the Content folder or including the Content folder. Effects must have a constructor that accepts
    ///     GraphicsDevice and
    ///     byte[]. Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect
    ///     duplication.
    /// </summary>
    /// <returns>The effect.</returns>
    /// <param name="name">Name.</param>
    public T LoadEffect<T>(string name) where T : Effect
    {
        // make sure the effect has the proper root directory
        if (!name.StartsWith(RootDirectory))
            name = RootDirectory + "/" + name;

        var bytes = EffectResource.GetFileResourceBytes(name);

        return LoadEffect<T>(name, bytes);
    }

    /// <summary>
    ///     loads an ogl effect directly from its bytes and handles disposing of it when the ContentManager is disposed. Name
    ///     should the the path
    ///     relative to the Content folder or including the Content folder. Effects must have a constructor that accepts
    ///     GraphicsDevice and
    ///     byte[]. Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect
    ///     duplication.
    /// </summary>
    /// <returns>The effect.</returns>
    /// <param name="name">Name.</param>
    public T LoadEffect<T>(string name, byte[] effectCode) where T : Effect
    {
        var effect = (T)Activator.CreateInstance(typeof(T), Core.GraphicsDevice, effectCode);
        effect!.Name = name + "-" + Utils.RandomString(5);
        _loadedEffects[effect.Name] = effect;

        return effect;
    }

    /// <summary>
    ///     loads and manages any Effect that is built-in to MonoGame such as BasicEffect, AlphaTestEffect, etc. Note that this
    ///     will
    ///     return a unique instance if you attempt to load the same Effect twice. If you intend to use the same Effect in
    ///     multiple locations
    ///     keep a reference to it and use it directly.
    /// </summary>
    /// <returns>The mono game effect.</returns>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public T LoadMonoGameEffect<T>() where T : Effect
    {
        var effect = (T)Activator.CreateInstance(typeof(T), Core.GraphicsDevice);
        effect!.Name = typeof(T).Name + "-" + Utils.RandomString(5);
        _loadedEffects[effect.Name] = effect;

        return effect;
    }

    #endregion

    /// <summary>
    ///     loads an asset on a background thread with optional callback for when it is loaded. The callback will occur on the
    ///     main thread.
    /// </summary>
    /// <param name="assetName">Asset name.</param>
    /// <param name="onLoaded">On loaded.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public void LoadAsync<T>(string assetName, Action<T> onLoaded = null)
    {
        var syncContext = SynchronizationContext.Current;
        Task.Run(() =>
        {
            var asset = Load<T>(assetName);

            // if we have a callback do it on the main thread
            if (onLoaded != null) syncContext!.Post(_ => { onLoaded(asset); }, null);
        });
    }

    /// <summary>
    ///     loads an asset on a background thread with optional callback that includes a context parameter for when it is
    ///     loaded.
    ///     The callback will occur on the main thread.
    /// </summary>
    /// <param name="assetName">Asset name.</param>
    /// <param name="onLoaded">On loaded.</param>
    /// <param name="context">Context.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public void LoadAsync<T>(string assetName, Action<object, T> onLoaded = null, object context = null)
    {
        var syncContext = SynchronizationContext.Current;
        Task.Run(() =>
        {
            var asset = Load<T>(assetName);

            if (onLoaded != null) syncContext!.Post(_ => { onLoaded(context, asset); }, null);
        });
    }

    /// <summary>
    ///     loads a group of assets on a background thread with optional callback for when it is loaded
    /// </summary>
    /// <param name="assetNames">Asset names.</param>
    /// <param name="onLoaded">On loaded.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public void LoadAsync<T>(string[] assetNames, Action onLoaded = null)
    {
        var syncContext = SynchronizationContext.Current;
        Task.Run(() =>
        {
            for (var i = 0; i < assetNames.Length; i++)
                Load<T>(assetNames[i]);

            // if we have a callback do it on the main thread
            if (onLoaded != null) syncContext!.Post(_ => { onLoaded(); }, null);
        });
    }

    /// <summary>
    ///     removes assetName from LoadedAssets and Disposes of it
    ///     disposeableAssets List.
    /// </summary>
    /// <param name="assetName">Asset name.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public void UnloadAsset<T>(string assetName) where T : class, IDisposable
    {
        if (IsAssetLoaded(assetName))
            try
            {
                // first fetch the actual asset. we already know its loaded so we'll grab it directly
                var assetToRemove = LoadedAssets[assetName];
                for (var i = 0; i < DisposableAssets.Count; i++)
                {
                    // see if the asset is disposeable. If so, find and dispose of it.
                    var typedAsset = DisposableAssets[i] as T;
                    if (typedAsset != null && typedAsset == assetToRemove)
                    {
                        typedAsset.Dispose();
                        DisposableAssets.RemoveAt(i);
                        break;
                    }
                }

                LoadedAssets.Remove(assetName);
            }
            catch (Exception e)
            {
                Debug.Error("Could not unload asset {0}. {1}", assetName, e);
            }
    }

    /// <summary>
    ///     unloads an Effect that was loaded via loadEffect, loadNezEffect or loadMonoGameEffect
    /// </summary>
    /// <param name="effectName">Effect.name</param>
    public bool UnloadEffect(string effectName)
    {
        if (_loadedEffects.ContainsKey(effectName))
        {
            _loadedEffects[effectName].Dispose();
            _loadedEffects.Remove(effectName);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     unloads an Effect that was loaded via loadEffect, loadNezEffect or loadMonoGameEffect
    /// </summary>
    public bool UnloadEffect(Effect effect)
    {
        return UnloadEffect(effect.Name);
    }

    /// <summary>
    ///     checks to see if an asset with assetName is loaded
    /// </summary>
    /// <returns><c>true</c> if this instance is asset loaded the specified assetName; otherwise, <c>false</c>.</returns>
    /// <param name="assetName">Asset name.</param>
    public bool IsAssetLoaded(string assetName)
    {
        return LoadedAssets.ContainsKey(assetName);
    }

    /// <summary>
    ///     provides a string suitable for logging with all the currently loaded assets and effects
    /// </summary>
    /// <returns>The loaded assets.</returns>
    internal string LogLoadedAssets()
    {
        var builder = new StringBuilder();
        foreach (var asset in LoadedAssets.Keys)
            builder.AppendFormat("{0}: ({1})\n", asset, LoadedAssets[asset].GetType().Name);

        foreach (var asset in _loadedEffects.Keys)
            builder.AppendFormat("{0}: ({1})\n", asset, _loadedEffects[asset].GetType().Name);

        return builder.ToString();
    }

    /// <summary>
    ///     reverse lookup. Gets the asset path given the asset. This is useful for making editor and non-runtime stuff.
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public string GetPathForLoadedAsset(object asset)
    {
        if (LoadedAssets.ContainsValue(asset))
            foreach (var kv in LoadedAssets)
                if (kv.Value == asset)
                    return kv.Key;

        return null;
    }

    /// <summary>
    ///     override that disposes of all loaded Effects
    /// </summary>
    public override void Unload()
    {
        base.Unload();

        foreach (var key in _loadedEffects.Keys)
            _loadedEffects[key].Dispose();

        _loadedEffects.Clear();
    }
}

/// <summary>
///     the only difference between this class and NezContentManager is that this one can load embedded resources from the
///     Nez.dll
/// </summary>
internal sealed class NezGlobalContentManager : NezContentManager
{
    public NezGlobalContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider,
        rootDirectory)
    {
    }

    /// <summary>
    ///     override that will load embedded resources if they have the "nez://" prefix
    /// </summary>
    /// <returns>The stream.</returns>
    /// <param name="assetName">Asset name.</param>
    protected override Stream OpenStream(string assetName)
    {
        if (assetName.StartsWith("nez://"))
        {
            var assembly = GetType().Assembly;

#if FNA
				// for FNA, we will just search for the file by name since the assembly name will not be known at runtime
				foreach (var item in assembly.GetManifestResourceNames())
				{
					if (item.EndsWith(assetName.Substring(assetName.Length - 20)))
					{
						assetName = "nez://" + item;
						break;
					}
				}
#endif
            return assembly.GetManifestResourceStream(assetName.Substring(6));
        }

        return base.OpenStream(assetName);
    }
}