using Newtonsoft.Json;

namespace Nez;

public struct Telegram
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

    public string Serialize(Telegram t)
    {
        return JsonConvert.SerializeObject(t);
    }

    public static Telegram? Deserialize(string s)
    {
        return JsonConvert.DeserializeObject<Telegram?>(s);
    }


    public static bool operator ==(Telegram a, Telegram b)
    {
        return a.Sender == b.Sender && a.Receiver == b.Receiver && a.Head == b.Head && a.Body == b.Body;
    }

    public static bool operator !=(Telegram a, Telegram b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}