namespace AppplicationTask.Utils
{
    public interface IDbSerializer
    {
        public Task<byte[]> Serialize();
        public object? Deserialize(byte[] obj);
    }
}
