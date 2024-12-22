namespace MassConvertFromModel.Loggers
{
    public interface ILogger
    {
        public void Write(string value);
        public void WriteLine(string value);
    }
}
