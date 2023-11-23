using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IltscheduleMailReminderSchedule.Helper
{
    public class TemplateTitles
    {
        public const string FirstReminderEmail = "First Reminder Mail For Completion";
        public const string SecondReminderEmail = "Second Reminder Mail For Completion";
        public const string ThirdReminderEmail = "Third Reminder Mail For Completion";
        public const string ForthReminderEmail = "Forth Reminder Mail For Completion";
        public const string FifthReminderEmail = "Fifth Reminder Mail For Completion";
    }
    public class Constants
    {
        public const int SEND_BATCH_SIZE = 5000;
    }
    public static class Token
    {
        public static string UserName = "[UserName]";       
        public static string AppUrl = "[AppUrl]";
        public static string RegardName = "[RegardName]";        
        public static string CourseTitle = "[CourseTitle]";
        public static string CourseDueDate = "[CourseDueDate]";
        public static string Reason = "[Reason]";
        public static string CourseName = "[CourseName]";
        public static string CourseUrl = "[CourseUrl]";
        public static string CourseImage = "[CourseImage]";


    }
    public static class ConfigCode
    {
        public static string firstRemCode = "ESCALATION_FIRST";
        public static string secondRemCode = "ESCALATION_SECOND";
        public static string thirdRemCode = "ESCALATION_THIRD";
       
    }
}
