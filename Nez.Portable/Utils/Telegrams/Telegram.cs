using Newtonsoft.Json;

namespace Nez;

public record struct Telegram
{
    public string Sender;
    public string Receiver;
    public string Head;
    public object Body;

    public Telegram(string Sender, string Receiver, string Head, object Body)
    {
        this.Sender = Sender;
        this.Receiver = Receiver;
        this.Head = Head;
        this.Body = Body;
    }

    public static void ClearTelegrams()
    {
        TelegramService.DeregisterAll();
    }

    public static Telegram Empty => new();

    public static Telegram? Deserialize(string s)
    {
        return JsonConvert.DeserializeObject<Telegram?>(s);
    }
}