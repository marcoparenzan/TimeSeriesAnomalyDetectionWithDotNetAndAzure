using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.AnomalyDetector;
using Microsoft.Azure.CognitiveServices.AnomalyDetector.Models;

//This sample assumes you have created an environment variable for your key and endpoint
string endpoint = "https://timeseriesanomalydetectionwithdotnet.cognitiveservices.azure.com/";
string key = "";
string datapath = "power-export_min.csv";

var client = new AnomalyDetectorClient(new ApiKeyServiceClientCredentials(key))
{
    Endpoint = endpoint
};

// get series from file

var list = File.ReadAllLines(datapath, Encoding.UTF8)
    .Skip(1)
    .Where(e => e.Trim().Length != 0)
    .Select(e => e.Split(','))
    .Where(e => e.Length == 3)
    .Select(e => new Point(DateTime.Parse(e[1]).ToUniversalTime().Date, float.Parse(e[2]))).ToList();

await EntireDetectSampleAsync(client, list); 
await LastDetectSampleAsync(client, list); 
await DetectChangePoint(client, list); 

Console.WriteLine("\nPress ENTER to exit.");
Console.ReadLine();

async Task EntireDetectSampleAsync(AnomalyDetectorClient client, IList<Point> points)
{
    Console.WriteLine("Detecting anomalies in the entire time series.");

    var request = new Request(points, Granularity.Daily);

    var result = await client.EntireDetectAsync(request).ConfigureAwait(false);

    if (result.IsAnomaly.Contains(true))
    {
        Console.WriteLine("An anomaly was detected at index:");
        for (int i = 0; i < request.Series.Count; ++i)
        {
            if (result.IsAnomaly[i])
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

async Task LastDetectSampleAsync(AnomalyDetectorClient client, IList<Point> points)
{
    Console.WriteLine("Detecting the anomaly status of the latest point in the series.");

    var request = new Request(points, Granularity.Daily);

    LastDetectResponse result = await client.LastDetectAsync(request).ConfigureAwait(false);

    if (result.IsAnomaly)
    {
        Console.WriteLine("The latest point was detected as an anomaly.");
    }
    else
    {
        Console.WriteLine("The latest point was not detected as an anomaly.");
    }
}

async Task DetectChangePoint(AnomalyDetectorClient client, IList<Point> points)
{
    Console.WriteLine("Detecting the change points in the series.");

    var request = new ChangePointDetectRequest(points, Granularity.Daily);

    ChangePointDetectResponse result = await client.ChangePointDetectAsync(request).ConfigureAwait(false);

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