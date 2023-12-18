using IltscheduleMailReminderSchedule.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IltscheduleMailReminderSchedule.Interface
{
    public interface IMailMessageAlert
    {
        Task<int> SendEmailAsyncNew(List<EmailNotification> lstemailNotifications, string orgCode, MailServerConfiguration mailConfiguration);

        Task<MailServerConfiguration> GetMailConfiguration(string orgCode);
        EmpoweredTLSContext GetDbContext();
        string GetRegardsName(string OrganizationCode);
        Task<List<ConfigurableParameter>> GetUserConfigurationValueAsync(string[] str_arr, string orgCode, string defaultValue = "0");
        
    }
}
