using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IltscheduleMailReminderSchedule.Model
{
    [Table("MailTemplateDesigner", Schema = "Masters")]
    public class MailTemplateDesigner 
    {
        public int Id { get; set; }
        public String MailSubject { get; set; }
        [Required]
        public String TemplateContent { get; set; }
        [MaxLength(500)]
        public string EventTitle { get; set; }
        [MaxLength(500)]
        public string AdditionalInformation { get; set; }
        public bool Status { get; set; }

        public object Shallowcopy()
        {
            return this.MemberwiseClone();
        }
    }
}
