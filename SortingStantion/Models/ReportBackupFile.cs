using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingStantion.Models
{

    public class ReportBackupFile
    {
        public string id
        {
            get;
            set;
        }

        public List<UserAuthorizationHistotyItem> operators
        {
            get;
            set;
        }

        public string startTime
        {
            get;
            set;
        }

        public string endTime
        {
            get;
            set;
        }


        public List<string> defectiveCodes
        {
            get;
            set;
        }

        public List<string> Packs
        {
            get;
            set;
        }

        public List<RepeatPack> repeatPacks
        {
            get;
            set;
        }

        public ReportBackupFile()
        {
            
        }
    }
}
