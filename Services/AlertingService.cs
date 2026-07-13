using Serilog;

public class AlertingService
{
    private static DateTime _lastAlertTime = DateTime.MinValue;

    public static void CheckForAnomaly(double currentLatency, string endpoint)
    {
        const double P95_Threshold = 500.0;
        
        if (currentLatency > P95_Threshold)
        {
            if ((DateTime.UtcNow - _lastAlertTime).TotalMinutes > 10)
            {
                Log.Error("!!! ANOMALY DETECTED !!! Endpoint: {Endpoint} is slow. Latency: {Val}ms", endpoint, currentLatency);
                _lastAlertTime = DateTime.UtcNow;
            }
        }
    }
}