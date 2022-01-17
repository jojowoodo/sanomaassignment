using websiteMonitoring.models;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;

namespace websiteMonitoring
{
    public class configReader
    {
        public static List<webpageModel>? readConfig(string configFile)
        {
            //configFile with webpages to check statuses of is read here.
            var configList = new List<webpageModel>();
            string destinationPath = Path.GetFullPath(@"..\..\..\configFiles\") + configFile;
            using (StreamReader r = new StreamReader(destinationPath))
            {
                string json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<webpageModel>>(json);
                configList = items;
            }

            return configList;
        }
        public static void httpRequest(List<webpageModel>? httpList)
        {
            
            if (httpList != null)
            {

                foreach (var item in httpList)
                {

                    var request = (HttpWebRequest)WebRequest.Create(item.url); 

                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start(); //start the countdown for how long each request takes.
                    HttpWebResponse response = null;

                    try
                    {
                        response = (HttpWebResponse)request.GetResponse();
                        stopwatch.Stop();
                        if (response != null)
                        {
                            //If the website isn't down and/or does exist.
                            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                            item.responseTime = stopwatch.ElapsedMilliseconds.ToString();
                            if (response.StatusCode.ToString() == "OK")
                            {
                                if (responseString.Contains(item.contentRequirement))
                                {
                                    item.responseStatus = "server response is OK(200), and content is OK";
                                }
                                else
                                {
                                    //If the website is reachable, but the content requirements aren't met
                                    item.responseStatus = "server response is OK(200), and content is NOT OK";
                                }
                            }
                            else
                            {
                                item.responseStatus = "server response is " + response.StatusCode.ToString();
                            }
                            string destinationPath = Path.GetFullPath(@"..\..\..\logFiles\") + "logFile.txt";
                            string modelObjectToJson = JsonConvert.SerializeObject(item);

                            File.AppendAllText(destinationPath, modelObjectToJson + Environment.NewLine);
                        }
                    }
                    catch
                    {
                        //If the website is down/doesn't exist
                        stopwatch.Stop();
                        item.responseStatus = "server doesn't respond"; 
                        item.responseTime = "null";
                        string destinationPath = Path.GetFullPath(@"..\..\..\logFiles\") + "logFile.txt";
                        string modelObjectToJson = JsonConvert.SerializeObject(item);

                        File.AppendAllText(destinationPath, modelObjectToJson + Environment.NewLine);
                    }
                }
            }

        }
        //This method runs the http request
        public static void requestLoop()
        {

            var requestMeasure = 0;
            string destinationPath = Path.GetFullPath(@"..\..\..\configFiles\") + "timerConfig.txt"; //Reads the timer config file to execute the loop every 3 seconds
            using (TextReader reader = File.OpenText(destinationPath))
            {
                requestMeasure = int.Parse(reader.ReadLine());
            }
            while (true)
            {
                configReader.httpRequest(configReader.readConfig("webpageModelConfig.json"));
                System.Threading.Thread.Sleep(requestMeasure);
            }
        }
    }
}
