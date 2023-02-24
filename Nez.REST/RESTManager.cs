using Microsoft.Xna.Framework.Graphics;
using RestSharp;
using RestSharp.Authenticators;

namespace Nez.REST;
public class RESTManager : GlobalManager
{
    public RestClient Client { get; private set; }

    private Queue<Action> _callbacks = new();

    public RESTManager(string URL, IAuthenticator? authenticator = null)
    {
        Client = new RestClient(URL);
        Client.Authenticator = authenticator;
    }
    
    public void Get<T>(string resource, Action<T?> callback, bool synchronize = false)
    {
        _ = ExecuteRESTAsync(new RestRequest(resource, Method.Get), callback, synchronize);
    }

    public void StreamTexture(string resource, Action<Texture2D?> callback) =>
        _ = StreamTextureAsync(new RestRequest(resource, Method.Get), callback);

    private async Task ExecuteRESTAsync<T>(RestRequest request, Action<T?> callback, bool synchronize)
    {
        var result = await Client.ExecuteGetAsync<T>(request);
        if (synchronize)
            _callbacks.Enqueue(() => callback(result.Data));
        else
            callback(result.Data);
    }
    private async Task StreamTextureAsync(RestRequest request, Action<Texture2D?> callback)
    {

        var canc = new CancellationToken();
        var result = await Client.DownloadStreamAsync(request, canc);
        callback(result != null ? Texture2D.FromStream(Core.GraphicsDevice, result) : null);
    }

    public override void Update()
    {
        while (_callbacks.TryDequeue(out var result)) result();
    }
}
