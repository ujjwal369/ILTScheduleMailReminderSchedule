using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IltscheduleMailReminderSchedule.Helper
{
    internal class SQLMapping
    {
        #region "EmailNotification"
        public DataTable GetEmailNotificationTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("CustomerCode");
            dataTable.Columns.Add("Description");
            dataTable.Columns.Add("Message");
            dataTable.Columns.Add("Status");
            dataTable.Columns.Add("Subject");
            dataTable.Columns.Add("ToEmail");
            dataTable.Columns.Add("CreatedDate");
            dataTable.Columns.Add("CC");
            dataTable.Columns.Add("CourseId");
            dataTable.Columns.Add("UserId");
            dataTable.Columns.Add("EventTitle");
            dataTable.Columns.Add("Ids");
            return dataTable;
        }

        public Dictionary<string, string> GetEmailNotificationColumnMapping()
        {
            Dictionary<string, string> columnMapping = new Dictionary<string, string>();
            columnMapping.Add("CustomerCode", "CustomerCode");
            columnMapping.Add("Description", "Description");
            columnMapping.Add("Message", "Message");
            columnMapping.Add("Status", "Status");
            columnMapping.Add("Subject", "Subject");
            columnMapping.Add("ToEmail", "ToEmail");
            columnMapping.Add("CreatedDate", "CreatedDate");
            columnMapping.Add("CC", "CC");
            columnMapping.Add("CourseId", "CourseId");
            columnMapping.Add("UserId", "UserId");
            columnMapping.Add("EventTitle", "EventTitle");

            return columnMapping;
        }

        #endregion
    }
}
