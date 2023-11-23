using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IltscheduleMailReminderSchedule.Model
{
    [Table("MailServerConfiguration", Schema = "Masters")]
    public class MailServerConfiguration
    {
        public int Id { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string FromEmailAddress { get; set; }
        public string FromEmailName { get; set; }
        public bool UseSecureConnection { get; set; }
        public string CustomerCode { get; set; }
        public bool IsActive { get; set; }
        public string ReplyEmailName { get; set; } 
        public string ConnectionMode { get; set; } 
        public int Port { get; set; }
        //public int CreatedBy { get; set; }
        //public int ModifiedBy { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
