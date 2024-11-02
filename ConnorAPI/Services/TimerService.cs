using Timer = System.Timers.Timer;

namespace ConnorAPI.Services
{
    public class TimerService : IHostedService, IDisposable
    {
        private readonly ILogger<TimerService> _logger;
        private readonly Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();

        public TimerService(ILogger<TimerService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TimerService is starting.");

            // Set up timers for specific times each day
            SetDailyTimer("BackupJSON", new TimeSpan(1, 35, 0), BackupJSON);
            SetDailyTimer("GetNewArticles", new TimeSpan(1, 37, 0), GetNewArticles);

            return Task.CompletedTask;
        }

        private void SetDailyTimer(string timerName, TimeSpan triggerTime, Func<Task> method)
        {
            Timer timer = new Timer
            {
                Interval = GetIntervalUntilNextTrigger(triggerTime),
                AutoReset = false // Trigger once and then reset manually
            };

            // Set up the timer to call the async method
            timer.Elapsed += async (sender, e) => await OnTimedEventAsync(timerName, triggerTime, method);

            // Add the timer to the dictionary and start it
            _timers[timerName] = timer;
            timer.Start();
        }

        private double GetIntervalUntilNextTrigger(TimeSpan triggerTime)
        {
            DateTime now = DateTime.Now;
            DateTime nextTrigger = now.Date.Add(triggerTime);

            if (now > nextTrigger)
            {
                nextTrigger = nextTrigger.AddDays(1);
            }

            return (nextTrigger - now).TotalMilliseconds;
        }

        private async Task OnTimedEventAsync(string timerName, TimeSpan triggerTime, Func<Task> asyncMethod)
        {
            _logger.LogInformation("Executing {method} at: {time}", timerName, DateTime.Now);

            // Call the async method
            await asyncMethod.Invoke();

            // Reset the timer for the next day
            _timers[timerName].Interval = GetIntervalUntilNextTrigger(triggerTime);
            _timers[timerName].Start();
        }

        // Example async methods to be triggered
        private async Task BackupJSON()
        {
            _logger.LogInformation("Backed up Galnet Articles at: {time}", DateTime.Now);
            var gitHubService = new GithubService();
            await gitHubService.BackupJSONToGitHub("wwwroot/json/galnetArticles.json");
        }

        private async Task GetNewArticles()
        {
            _logger.LogInformation("Fetched new articles at: {time}", DateTime.Now);
            var galnetService = new GalnetService();
            await galnetService.UpdateJsonAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TimerService is stopping.");

            // Stop and dispose of all timers
            foreach (var timer in _timers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }
        }
    }
}
