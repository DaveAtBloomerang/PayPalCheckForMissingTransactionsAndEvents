// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Text;

/*
SELECT pi.Token FROM ProcessingInfo pi join TransactionProcessorAccount tpa on pi.TransactionProcessorAccountId = tpa.Id Where tpa.Name = 'PayPal'
## Run the following command to export the PayPalTransactionId for the EventTransaction Feeder File
.\Command.exe Export.SelectStatement "SELECT PayPalTransactionId FROM PayPalPayoutTransactions" -databaseFlavor Customer
## Run the following command to export the PayoutId from the PayPalPayoutEvent table for the PayoutEvent Feeder File
.\Command.exe Export.SelectStatement "SELECT PayoutId FROM PayPalPayoutEvents" -databaseFlavor Customer
*/
Console.WriteLine("Hello, World!");

Dictionary<string, Boolean> eventTransactionIds = new Dictionary<string, Boolean>();
List<string> missingTransactionIds = new List<string>();
List<string> missingPayoutEventIds = new List<string>();
Dictionary<string, int> missingCounts = new Dictionary<string, int>();

Dictionary<string, Boolean> paypalPayoutIds = new Dictionary<string, Boolean>();

var isFirst = true;
foreach (var line in File.ReadLines("../../../EventTransactionFeeder.txt").Distinct())
{
    if(isFirst)
        isFirst = false;
    else
        eventTransactionIds.Add(line.Replace(",",""), true);
}

isFirst = true;
foreach (var line in File.ReadLines("../../../PayoutEventFeeder.txt").Distinct())
{
    if(isFirst)
        isFirst = false;
    else
        paypalPayoutIds.Add(line.Replace(",",""), true);
}

//I want to read through each file in a folder and parse it as a csv
var allFiles = Directory.GetFiles("../../../../PSR Files");

decimal missingAmount = 0;
allFiles.ToList().ForEach(file =>
{
    var csv = File.ReadLines(file);
    foreach (var line in csv)
    {
        var split = line.Split(',');
        //Working with Payments
        if(split[0] == "\"SB\""){
            if(split[1] != "WITHDRAWAL") //To compare ids
            {
                if (!eventTransactionIds.ContainsKey(split[4]))
                {
                    missingTransactionIds.Add(line);
                    missingAmount += decimal.Parse(split[9]);
                    if(missingCounts.ContainsKey(file))
                    {
                        missingCounts[file]++;
                    }
                    else
                    {
                        missingCounts.Add(file, 1);
                    }
                }
            }
            //Working with Payouts
            if(split[1] == "WITHDRAWAL") //To compare ids
            {
                if (!paypalPayoutIds.ContainsKey(split[4]))
                {
                    missingPayoutEventIds.Add(line);
                }
            }
        }
    }
});
Console.WriteLine($"Missing amount: {missingAmount}");

File.WriteAllLines("../../../missingEventTransactionIds.csv", missingTransactionIds);
File.WriteAllLines("../../../missingPayoutEventIds.csv", missingPayoutEventIds);
File.WriteAllLines("../../../fileCounts.csv", missingCounts.Select(x => $"{x.Key},{x.Value}"));