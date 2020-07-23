using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPT
{
    public class Entidades
    {

        public class ObjetoPosiciones 
        {
            public string ID { get; set; }
            public string Onu { get; set; }
            public string OperStatus { get; set; }
            public string ConfigState { get; set; }
            public string DownloadState { get; set; }
            public string Tx { get; set; }
            public string Rx { get; set; }
            public string KM { get; set; }
            public string OnuStatus { get; set; }
            public string State { get; set; }
        }
        public class ObjScaner 
        {
            public string Onu { get; set; }
            public string ConfiguredAutoDetection { get; set; }
            public string AdministrativeState { get; set; }
            public string OperationalState { get; set; }
            public string ConnectionType { get; set; }
        }

    }
}
