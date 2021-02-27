using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.AnomalyDetector;
using Azure.AI.AnomalyDetector.Models;

//This sample assumes you have created an environment variable for your key and endpoint
string endpoint = "https://timeseriesanomalydetectionwithdotnet.cognitiveservices.azure.com/";
string key = "";
string datapath = "power-export_min.csv";

var client = new AnomalyDetectorClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));

// get series from file

var list = File.ReadAllLines(datapath, Encoding.UTF8)
    .Skip(1)
    .Where(e => e.Trim().Length != 0)
    .Select(e => e.Split(','))
    .Where(e => e.Length == 3)
    .Select(e => new TimeSeriesPoint(DateTime.Parse(e[1]).ToUniversalTime().Date, float.Parse(e[2]))).ToList();

await EntireDetectSampleAsync(client, list); 
await LastDetectSampleAsync(client, list); 
await DetectChangePoint(client, list); 

Console.WriteLine("\nPress ENTER to exit.");
Console.ReadLine();

async Task EntireDetectSampleAsync(AnomalyDetectorClient client, IList<TimeSeriesPoint> points)
{
    Console.WriteLine("Detecting anomalies in the entire time series.");

    var request = new DetectRequest(points, TimeGranularity.Daily);

    var result = await client.DetectEntireSeriesAsync(request).ConfigureAwait(false);

    if (result.Value.IsAnomaly.Contains(true))
    {
        Console.WriteLine("An anomaly was detected at index:");
        for (int i = 0; i < request.Series.Count; ++i)
        {
            if (result.Value.IsAnomaly[i])
            {
                Console.Write(i);
                Console.Write(" ");
            }
        }
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine(" No anomalies detected in the series.");
    }
}

async Task LastDetectSampleAsync(AnomalyDetectorClient client, IList<TimeSeriesPoint> points)
{
    Console.WriteLine("Detecting the anomaly status of the latest point in the series.");

    var request = new DetectRequest(points, TimeGranularity.Daily);

    LastDetectResponse result = await client.DetectLastPointAsync(request).ConfigureAwait(false);

    if (result.IsAnomaly)
    {
        Console.WriteLine("The latest point was detected as an anomaly.");
    }
    else
    {
        Console.WriteLine("The latest point was not detected as an anomaly.");
    }
}

async Task DetectChangePoint(AnomalyDetectorClient client, IList<TimeSeriesPoint> points)
{
    Console.WriteLine("Detecting the change points in the series.");

    var request = new ChangePointDetectRequest(points, TimeGranularity.Daily);

    ChangePointDetectResponse result = await client.DetectChangePointAsync(request).ConfigureAwait(false);

    if (result.IsChangePoint.Contains(true))
    {
        Console.WriteLine("A change point was detected at index:");
        for (int i = 0; i < request.Series.Count; ++i)
        {
            if (result.IsChangePoint[i])
            {
                Console.Write(i);
                Console.Write(" ");
            }
        }
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("No change point detected in the series.");
    }
}