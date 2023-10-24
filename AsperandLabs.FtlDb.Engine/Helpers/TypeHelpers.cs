namespace AsperandLabs.FtlDb.Engine.Helpers;

public static class TypeHelpers
{
    public static unsafe int GetSizeOf<T>() where T : unmanaged
    {
        return sizeof(T);
    }
}