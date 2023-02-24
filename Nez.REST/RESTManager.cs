using Microsoft.Xna.Framework.Graphics;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace Nez.REST;
public class RESTManager : GlobalManager
{
    public RestClient Client { get; private set; }

    private Queue<Action> _callbacks = new();

    public delegate void RestDataCallback<T>(T? Data, HttpStatusCode status);
    public delegate void RestStatusCallback(HttpStatusCode status);

    public RESTManager(string URL, IAuthenticator? authenticator = null)
    {
        Client = new RestClient(URL);
        Client.Authenticator = authenticator;
    }


    public void Get<T>(string resource, IEnumerable<Parameter>? parameters = null, RestDataCallback<T>? callback = null, bool synchronize = false) =>
        RESTDataSend(resource, Method.Get, parameters, callback, synchronize);

    public void Post(string resource, IEnumerable<Parameter>? parameters = null, RestStatusCallback? callback = null, bool synchronize = false) =>
        RESTStatusSend(resource, Method.Post, parameters, callback, synchronize);
    public void Post<T>(string resource, IEnumerable<Parameter>? parameters = null, RestDataCallback<T>? callback = null, bool synchronize = false) => 
        RESTDataSend(resource, Method.Post, parameters, callback, synchronize);


    public void Put(string resource, IEnumerable<Parameter>? parameters = null, RestStatusCallback? callback = null, bool synchronize = false) =>
        RESTStatusSend(resource, Method.Put, parameters, callback, synchronize);
    public void Put<T>(string resource, IEnumerable<Parameter>? parameters = null, RestDataCallback<T>? callback = null, bool synchronize = false) =>
        RESTDataSend(resource, Method.Put, parameters, callback, synchronize);


    public void Delete(string resource, IEnumerable<Parameter>? parameters = null, RestStatusCallback? callback = null, bool synchronize = false) =>
        RESTStatusSend(resource, Method.Delete, parameters, callback, synchronize);
    public void Delete<T>(string resource, IEnumerable<Parameter>? parameters = null, RestDataCallback<T>? callback = null, bool synchronize = false) =>
        RESTDataSend(resource, Method.Delete, parameters, callback, synchronize);


    private void RESTDataSend<T>(string resource, Method method, IEnumerable<Parameter>? parameters, RestDataCallback<T>? callback, bool synchronize)
    {
        var req = new RestRequest(resource, method);
        if (parameters is not null) req.Parameters.AddParameters(parameters);
        _ = ExecuteRESTAsync(req, callback, synchronize);
    }

    private void RESTStatusSend(string resource, Method method, IEnumerable<Parameter>? parameters, RestStatusCallback? callback, bool synchronize)
    {
        var req = new RestRequest(resource, method);
        if (parameters is not null) req.Parameters.AddParameters(parameters);
        _ = ExecuteRESTAsync(req, callback, synchronize);
    }

    public void Execute(RestRequest request, RestStatusCallback? callback = null, bool synchronize = false) =>
        _ = ExecuteRESTAsync(request, callback, synchronize);
    public void Execute<T>(RestRequest request, RestDataCallback<T>? callback = null, bool synchronize = false) =>
        _ = ExecuteRESTAsync(request, callback, synchronize);



    private async Task ExecuteRESTAsync(RestRequest request, RestStatusCallback? callback, bool synchronize)
    {
        var result = await Client.ExecuteAsync(request);
        if (callback == null) return;

        if (synchronize)
            _callbacks.Enqueue(() => callback(result.StatusCode));
        else
            callback(result.StatusCode);
    }

    private async Task ExecuteRESTAsync<T>(RestRequest request, RestDataCallback<T>? callback, bool synchronize)
    {
        var result = await Client.ExecuteAsync<T>(request);
        if (callback == null) return;
        
        if (synchronize)
            _callbacks.Enqueue(() => callback(result.Data, result.StatusCode));
        else
            callback(result.Data, result.StatusCode);
    }

    public void StreamTexture(string resource, Action<Texture2D?> callback) =>
        _ = StreamTextureAsync(new RestRequest(resource, Method.Get), callback);

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
