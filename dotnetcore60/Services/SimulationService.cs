using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Net;

namespace dotnetcore60.Services
{
    public interface ISimulator
    {
        void SyncHighSleep();
        void SyncLowSleep();
        void SlowDatabaseConnection();
        void SlowOutboundService();
        void LeakMemoryLow();
        void LeakMemoryHigh();
        void ThrowException();

        Task SleepAsync();
        Task SlowOutboundHttpServiceAsync();
        Task SlowDatabaseConnectionAsync();

        

    }
    public class SimulationService : ISimulator
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly WebClient _webClient;

        private static readonly ConcurrentDictionary<string, List<UserTrackingModel>> _applicationDictionary = new();   
        private static readonly List<List<byte[]>> _staticList = new();

        public SimulationService()
        {
            _random = new Random();

            _webClient = new WebClient();
            _webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.162 Safari/537.36");
            _webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.162 Safari/537.36");
            _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

        }
        public void LeakMemoryHigh()
        {
            var rnd = new Random();
            var list = new List<byte[]>();
            int i = 0;
            while (i < 16100)
            {
                i++;
                byte[] b = new byte[1024];
                b[rnd.Next(0, b.Length)] = byte.MaxValue;
                list.Add(b);
            }

            _staticList.Add(list);
        }

        public void LeakMemoryLow()
        {
            var userTrackingModel = new UserTrackingModel(Guid.NewGuid().ToString());

            if (_applicationDictionary.ContainsKey("StaticEntry"))
            {
                _applicationDictionary["StaticEntry"].Add(userTrackingModel);
            }
            else
            {
                _applicationDictionary.TryAdd("StaticEntry", new List<UserTrackingModel> { userTrackingModel });
            }
        }

        public void ThrowException()
        {
            throw new ApplicationException("Find this in trace file");
        }

        public async Task SleepAsync()
        {
            await Task.Delay(8000);
        }

        public void SlowDatabaseConnection()
        {
            SqlConnection conn = new SqlConnection("Server=tcp:itsqobarp5.database.windows.net,1433;Database=demomvp;User ID=kaushal@itsqobarp5;Password=LS1setup!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;");
            conn.Open();
            var cmd = new SqlCommand("dbo.sleepingproc", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            
            cmd.Parameters.Add(new SqlParameter("@sleeptime", "00:00:05"));
            
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public async Task SlowDatabaseConnectionAsync()
        {
            SqlConnection conn = new SqlConnection("Server=tcp:itsqobarp5.database.windows.net,1433;Database=demomvp;User ID=kaushal@itsqobarp5;Password=LS1setup!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;");
            await conn.OpenAsync();

            var cmd = new SqlCommand("dbo.sleepingproc", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@sleeptime", "00:00:05"));
            
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }

        public void SlowOutboundService()
        {
            int delay = _random.Next(15, 25);
            _webClient.DownloadString("https://httpbin.org/delay/" + delay);
        }

        public async Task SlowOutboundHttpServiceAsync()
        {
            int delay = _random.Next(3,9);
            await _client.GetStringAsync("https://httpbin.org/delay/" + delay);
        }

        public void SyncHighSleep()
        {
            Thread.Sleep(215000);
        }

        public void SyncLowSleep()
        {
            Thread.Sleep(9000);
        }
    }

    public class UserTrackingModel
    {
        const int BYTES_LEAK = 1024 * 1024 * 3;
        public DateTime PageVisited { get; set; }
        public string SessionId { get; set; }
        public byte[] EncryptedBlob { get; set; }

        public UserTrackingModel(string uniqueId)
        {
            SessionId = uniqueId;
            PageVisited = DateTime.Now;
            EncryptedBlob = new byte[BYTES_LEAK];
        }
    }
}
