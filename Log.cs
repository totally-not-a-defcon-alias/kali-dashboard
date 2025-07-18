namespace KaliDashboard
{
    public static class Logger
    {
       private static readonly DateTime _start = DateTime.Now;

     public static void Log(string msg)
        {
            var now = DateTime.Now;
            var diff = now - _start;
            if (diff.TotalMicroseconds < 0) diff = TimeSpan.Zero;
            
            Console.WriteLine($"{diff.Minutes:00}:{diff.Seconds:00}.{diff.Microseconds:0000} -=> {msg}");
        }
    }
}