using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IltscheduleMailReminderSchedule.Model
{
    public class IltscheduleMailReminder
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int ScheduleId { get; set; }
        public int ModuleId { get; set; }

        public int? FirstRemDays { get; set; }

        public string FirstRemTemplate { get; set; }

        public int? SecondRemDays { get; set; }

        public string SecondRemTemplate { get; set; }

        public int? ThirdRemDays { get; set; }

        public string? ThirdRemTemplate { get; set; }

        public int? FourthRemDays { get; set; }

        public string? FourthRemTemplate { get; set; }

        public int? FifthRemDays { get; set; }

        public string? FifthRemTemplate { get; set; }
        public bool FirstManagerInCC { get; set; }
        public bool SecondManagerInCC { get; set; }
        public bool ThirdManagerInCC { get; set; }
        public bool FourthManagerInCC { get; set; }
        public bool FifthManagerInCC { get; set; }
        public string? CourseImage { get; set; }
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

    }
}
