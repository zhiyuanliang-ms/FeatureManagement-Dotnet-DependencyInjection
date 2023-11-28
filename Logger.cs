namespace DIContainers
{
    public interface IMyLogger
    {
        public void Info(string message);
    }

    public class MyLogger : IMyLogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"INFO: {message}");
        }
    }
}
