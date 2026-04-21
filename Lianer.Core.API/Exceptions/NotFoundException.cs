public class NotFoundException : Exception
{
    public NotFoundException(string message, object id) : base(message.Replace("{Id}", id.ToString()))
    {
    }
}
