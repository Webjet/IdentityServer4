//The following libraries were added to this sample.
//using System.IO;
//using System.Web.Hosting;

namespace AdminPortal.Models
{
    public class Task
    {
        //Every Task entry has a Task, a Status, and a TaskID
        public int TaskID { get; set; }
        public string TaskText { get; set; }
        public string Status { get; set; }
    }
}