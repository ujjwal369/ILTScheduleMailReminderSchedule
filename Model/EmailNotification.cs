using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IltscheduleMailReminderSchedule.Model
{
    [Table("EmailNotification", Schema = "Notification")]
    public class EmailNotification
    {
        public int Id { get; set; }
            
        public string ToEmail {get;set;}

        [MaxLength(1000)]
        public string Subject { get; set; }

        public string Message { get; set; }

        public string CustomerCode { get; set; }        

        public bool Status { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CC { get; set; }

        public int? CourseId { get; set; }

        public int? UserId { get; set; }

        [MaxLength(500)]
        public string EventTitle { get; set; }


    }
}
