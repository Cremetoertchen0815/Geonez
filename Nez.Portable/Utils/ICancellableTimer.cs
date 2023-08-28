namespace Nez;

public interface ICancellableTimer
{
    public void Cancel(bool completeFinalAction);
}