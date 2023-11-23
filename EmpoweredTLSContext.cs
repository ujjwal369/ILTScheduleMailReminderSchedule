using IltscheduleMailReminderSchedule.Helper;
using IltscheduleMailReminderSchedule.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IltscheduleMailReminderSchedule
{
    public class EmpoweredTLSContext : DbContext
    {
        public EmpoweredTLSContext(DbContextOptions<EmpoweredTLSContext> options):base(options) 
        {
                
        }
        public virtual DbSet<MailServerConfiguration> MailServerConfiguration { get; set; }
        public virtual DbSet<IltscheduleMailReminder> IltscheduleMailReminder { get; set; }
        public virtual DbSet<ConfigurableParameter> ConfigurableParameter { get; set; }
        public virtual DbSet<MailTemplateDesigner> MailTemplateDesigner { get; set; }
        public virtual DbSet<EmailNotification> EmailNotification { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Security.Configuration.GetConnectionString("DefaultConnection"))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);

        }
    }
}
