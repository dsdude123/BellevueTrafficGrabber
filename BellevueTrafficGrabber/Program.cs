using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using BellevueTrafficGrabber.models;
using Newtonsoft.Json;
using System.IO;
using System.Timers;

namespace BellevueTrafficGrabber
{
    class Program
    {
        static CameraListing cameras;

        static void Main(string[] args)
        {
            WriteConsole("Bellevue Traffic Camera Grabber\n");
            // Get camera listing from City of Bellevue
            try
            {
                WebClient web = new WebClient();
                string cameraRawData = web.DownloadString("http://trafficmap.bellevuewa.gov/Services/CameraInfoService.ashx");
                cameras = JsonConvert.DeserializeObject<CameraListing>(cameraRawData);
            } catch (Exception ex)
            {
                WriteConsole("Failed to get camera listing. Unable to continue.");
                WriteConsole(ex.Message);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // Initalize file structure
            string outputFolder = Path.Combine(Environment.CurrentDirectory, "output");
            if (!Directory.Exists(outputFolder)){
                WriteConsole("Creating output directory...");
                Directory.CreateDirectory(outputFolder);
            }
            foreach(CAMERA c in cameras.CAMERAS)
            {
                if (c.CHANNEL != 9999) // Ignore external cameras
                {
                    WriteConsole("Detected camera: " + c.DISP_ADDR);
                    string cameraFolder = Path.Combine(Environment.CurrentDirectory, "output", c.DISP_ADDR);
                    if (!Directory.Exists(cameraFolder))
                    {
                        Directory.CreateDirectory(cameraFolder);
                    }
                }
            }

            // Setup timers and wait forever
            Timer cameraQuery = new Timer();
            cameraQuery.Interval = 60000;
            cameraQuery.Elapsed += new ElapsedEventHandler(GrabCameras);
            cameraQuery.Enabled = true;
            WriteConsole("Camera grabber is ready.");
            while (true) ;
        }

        static void GrabCameras(object source, ElapsedEventArgs e)
        {
            WriteConsole("Grabbing camera images...");
            string jobTitle = DateTime.Now.ToString("yyyyMMMdd-HH-mm");
            WebClient w = new WebClient();
            foreach(CAMERA c in cameras.CAMERAS)
            {
                try
                {
                    string outputFolder = Path.Combine(Environment.CurrentDirectory, "output", c.DISP_ADDR,jobTitle);
                    Directory.CreateDirectory(outputFolder);
                    switch (c.CHANNEL)
                    {
                        case 360:
                        case 2:
                        case 43:// Multi direction camera
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "n.jpg", Path.Combine(outputFolder, c.ID + "n.jpg"));
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "s.jpg", Path.Combine(outputFolder, c.ID + "s.jpg"));
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "w.jpg", Path.Combine(outputFolder, c.ID + "w.jpg"));
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "e.jpg", Path.Combine(outputFolder, c.ID + "e.jpg"));
                            break;
                        case 41: // Multi direction + additional live
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "n.jpg", Path.Combine(outputFolder, c.ID + "n.jpg"));
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "s.jpg", Path.Combine(outputFolder, c.ID + "s.jpg"));
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "w.jpg", Path.Combine(outputFolder, c.ID + "w.jpg"));
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + "e.jpg", Path.Combine(outputFolder, c.ID + "e.jpg"));
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + ".jpg", Path.Combine(outputFolder, c.ID + ".jpg"));
                            break;
                        case 8:
                        case 40:
                        case 36:
                        case 32:
                        case 7:
                        case 11:// Pull only main camera, these cameras have inactive directional cameras
                            w.DownloadFile("http://trafficmap.bellevuewa.gov/TrafficCamImages/" + c.ID + ".jpg", Path.Combine(outputFolder, c.ID + ".jpg"));
                            break;
                        case 9999: // Unwanted camera, do nothing
                            break;
                    }
                } catch (Exception ex)
                {
                    WriteConsole("Failed to grab camera image(s) for: " + c.DISP_ADDR);
                    WriteConsole(ex.Message);
                }
            }
        }

        static void WriteConsole(string text)
        {
            Console.WriteLine("[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text);
        }
    }
}
