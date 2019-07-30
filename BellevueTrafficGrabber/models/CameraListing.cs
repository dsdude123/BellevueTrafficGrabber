using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BellevueTrafficGrabber.models
{
    public class CameraListing
    {
        public CAMERA[] CAMERAS { get; set; }
    }

    public class CAMERA
    {
        public int CHANNEL { get; set; }
        public string TYPE { get; set; }
        public string ID { get; set; }
        public string ADDRESS { get; set; }
        public string DISP_ADDR { get; set; }
        public string LATITUDE { get; set; }
        public string LONGITUDE { get; set; }
    }
}
