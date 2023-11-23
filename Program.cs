using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Xml;
using IltscheduleMailReminderSchedule.Interface;
using IltscheduleMailReminderSchedule.Repository;
using MandatoryLearningReminder.Repository;
using IltscheduleMailReminderSchedule.Model;

namespace IltscheduleMailReminderSchedule
{
    class Program
    {
        // public static DbContext db = new EmpoweredTLSContext();

        public static IConfigurationRoot Configuration;
        public static string OrgnaizationCode;
        public static string EmpoweredLmsPath;
        public static string EncryptionKey;
        public static string ApiGatewayWwwroot;

        //Log4Net instance 
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            IConfigurationRoot configuration = builder.Build();

            //Log4Net Config file Configuration.
            XmlDocument log4netConfig = new XmlDocument();

            log4netConfig.Load(File.OpenRead("log4net.config"));

            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                       typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            _logger.Info("Logging Process start");
            try
            {
                Console.WriteLine("Program Start" + DateTime.Now.ToString());
                _logger.Info("Program Start" + DateTime.Now.ToString());

                string ConnectionString = configuration.GetConnectionString("DefaultConnection");
                _logger.Info(ConnectionString.ToString());

                //string ConnectionString1 = configuration.GetConnectionString("DefaultHrmsConnection");
                //_logger.Info(ConnectionString1.ToString());

                ApiGatewayWwwroot = configuration["ApiGatewayWwwroot"];
                _logger.Info(ApiGatewayWwwroot.ToString());

                OrgnaizationCode = Configuration["OrgnaizationCode"];
                Console.WriteLine(OrgnaizationCode);
                _logger.Info(OrgnaizationCode.ToString());

                EmpoweredLmsPath = configuration["EmpoweredLmsPath"];
                EncryptionKey = configuration["EncryptionKey"];
                var services = new ServiceCollection();
                services.AddScoped<ImportData>();

                services.AddScoped<IMailMessageAlert, MailMessageAlert>();
                services.AddDbContext<EmpoweredTLSContext>(options => options.UseSqlServer(ConnectionString));
                
                ServiceProvider serviceProvider = services.BuildServiceProvider();

                ImportData testService = serviceProvider.GetService<ImportData>();

                List<IltscheduleMailReminder> iltscheduleMailReminders =  await testService.ImportFile();

                var reminderCount = await testService.GetManditoryLearningReminder(iltscheduleMailReminders);

                _logger.Info("event executed" + reminderCount);

                Console.WriteLine("Program End" + DateTime.Now.ToString());
                _logger.Info("Program End" + DateTime.Now.ToString());

                _logger.Info("Logging Process end");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.Error("Error in Main :" + ex.ToString());

            }
        }
    }
}