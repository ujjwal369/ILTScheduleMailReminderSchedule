
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;

using Microsoft.Extensions.Configuration;
using NETCore.Encrypt;
using IltscheduleMailReminderSchedule;
using IltscheduleMailReminderSchedule.Interface;
using IltscheduleMailReminderSchedule.Helper;
using IltscheduleMailReminderSchedule.Model;
using ILTScheduleMailReminderSchedule.Model;

namespace MandatoryLearningReminder.Repository
{
    public class ImportData
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static EmpoweredTLSContext _dbContext;
        private IMailMessageAlert _message;
        public ImportData(EmpoweredTLSContext customDbContext, IMailMessageAlert message)
        {
            _dbContext = customDbContext;
            _message = message;
        }
        public async Task<List<IltscheduleMailReminder>> ImportFile(string ReminderTemplate = null)
        {
            List<IltscheduleMailReminder> iltscheduleMailReminders = new List<IltscheduleMailReminder>();

            try
            {
                using (var dbContext = _message.GetDbContext())
                {
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "GetIltScheduleMailRemainder";
                            cmd.CommandType = CommandType.StoredProcedure;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            if (dt.Rows.Count <= 0)
                            {
                                reader.Dispose();
                                connection.Close();
                                return iltscheduleMailReminders;
                            }
                            foreach (DataRow row in dt.Rows)
                            {
                                IltscheduleMailReminder iltscheduleMailReminder = new IltscheduleMailReminder();
                                iltscheduleMailReminder.CreatedBy = int.Parse(row["CreatedBy"].ToString());
                                iltscheduleMailReminder.ModifiedBy = int.Parse(row["ModifiedBy"].ToString());
                                iltscheduleMailReminder.CreatedDate = Convert.ToDateTime(row["CreatedDate"]);
                                iltscheduleMailReminder.ModifiedDate = Convert.ToDateTime(row["ModifiedDate"]);
                                iltscheduleMailReminder.CourseId = int.Parse(row["CourseId"].ToString());
                                iltscheduleMailReminder.ModuleId = int.Parse(row["ModuleId"].ToString());
                                iltscheduleMailReminder.ScheduleId = int.Parse(row["ScheduleId"].ToString());
                                iltscheduleMailReminder.FirstRemDays = int.Parse(row["FirstRemDays"].ToString());
                                iltscheduleMailReminder.SecondRemDays = int.Parse(row["SecondRemDays"].ToString());
                                iltscheduleMailReminder.ThirdRemDays = int.Parse(row["ThirdRemDays"].ToString());
                                iltscheduleMailReminder.FourthRemDays = int.Parse(row["FourthRemDays"].ToString());
                                iltscheduleMailReminder.FirstRemDays = int.Parse(row["FirstRemDays"].ToString());
                                iltscheduleMailReminder.FirstRemTemplate = row["FirstRemTemplate"].ToString();
                                iltscheduleMailReminder.SecondRemTemplate = row["SecondRemTemplate"].ToString();
                                iltscheduleMailReminder.ThirdRemTemplate = row["ThirdRemTemplate"].ToString();
                                iltscheduleMailReminder.FourthRemTemplate = row["FourthRemTemplate"].ToString();
                                iltscheduleMailReminder.FifthRemTemplate = row["FifthRemTemplate"].ToString();
                                iltscheduleMailReminder.FirstManagerInCC = Convert.ToBoolean(row["FirstManagerInCC"].ToString());
                                iltscheduleMailReminder.SecondManagerInCC = Convert.ToBoolean(row["SecondManagerInCC"].ToString());
                                iltscheduleMailReminder.ThirdManagerInCC = Convert.ToBoolean(row["ThirdManagerInCC"].ToString());
                                iltscheduleMailReminder.FourthManagerInCC = Convert.ToBoolean(row["FourthManagerInCC"].ToString());
                                iltscheduleMailReminder.CourseImage = row["CourseImage"].ToString();
                                iltscheduleMailReminder.CourseName = row["Title"].ToString();
                                iltscheduleMailReminders.Add(iltscheduleMailReminder);
                            }

                        }
                    }
                }
                return iltscheduleMailReminders;
            }
            catch (Exception ex)
            {
                return iltscheduleMailReminders;

            }

        }


        public async Task<int> GetManditoryLearningReminder(List<IltscheduleMailReminder> iltscheduleMailReminders)
        {
            int i = 0;
            foreach (IltscheduleMailReminder iltmail in iltscheduleMailReminders)
            {
                try
                {
                    using (var dbContext = _message.GetDbContext())
                    {
                        using (var connection = dbContext.Database.GetDbConnection())
                        {
                            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                connection.Open();
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandText = "GetAllIltUsersForEmailRemainder";
                                cmd.Parameters.Add(new SqlParameter("@ScheduleID", SqlDbType.Int) { Value = iltmail.ScheduleId });
                                cmd.Parameters.Add(new SqlParameter("@CourseID", SqlDbType.Int) { Value = iltmail.CourseId });
                                cmd.CommandType = CommandType.StoredProcedure;
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                DataTable dt = new DataTable();
                                dt.Load(reader);
                                if (dt.Rows.Count <= 0)
                                {
                                    reader.Dispose();
                                    connection.Close();
                                    return i;
                                }
                                foreach (DataRow row in dt.Rows)
                                {
                                    i++;
                                    UserForEmailRemainder userForEmailRemainder = new UserForEmailRemainder();
                                    userForEmailRemainder.EmailAddress = Security.Decrypt(row["EmailId"].ToString());
                                    userForEmailRemainder.UserName = row["EmailId"].ToString();
                                    if (DateTime.Now.Date == (iltmail.CreatedDate.AddDays((double)iltmail.FirstRemDays)))
                                    {
                                        await this.EmailReminder(TemplateTitles.FirstReminderEmail, iltmail, userForEmailRemainder.UserName, userForEmailRemainder.EmailAddress);
                                        Task.CompletedTask.Wait();
                                        i++;
                                    }
                                    else if (DateTime.Now.Date == (iltmail.CreatedDate.AddDays((double)iltmail.SecondRemDays)))
                                    {
                                        await this.EmailReminder(TemplateTitles.SecondReminderEmail, iltmail, userForEmailRemainder.UserName, userForEmailRemainder.EmailAddress);
                                        Task.CompletedTask.Wait();
                                        i++;
                                    }
                                    else if (DateTime.Now.Date == (iltmail.CreatedDate.AddDays((double)iltmail.ThirdRemDays)))
                                    {
                                        await this.EmailReminder(TemplateTitles.ThirdReminderEmail, iltmail, userForEmailRemainder.UserName, userForEmailRemainder.EmailAddress);
                                        Task.CompletedTask.Wait();
                                        i++;
                                    }
                                    else if (DateTime.Now.Date == (iltmail.CreatedDate.AddDays((double)iltmail.FourthRemDays)))
                                    {
                                        await this.EmailReminder(TemplateTitles.ForthReminderEmail, iltmail, userForEmailRemainder.UserName, userForEmailRemainder.EmailAddress);
                                        Task.CompletedTask.Wait();
                                        i++;
                                    }
                                    else if (DateTime.Now.Date == (iltmail.CreatedDate.AddDays((double)iltmail.FifthRemDays )))
                                    {
                                        await this.EmailReminder(TemplateTitles.FifthReminderEmail, iltmail, userForEmailRemainder.UserName, userForEmailRemainder.EmailAddress);
                                        Task.CompletedTask.Wait();
                                        i++;
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception in function GetManditoryLearningReminder  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), Program.OrgnaizationCode));

                }
            }

            return i;
        }



        public async Task<int> EmailReminder(string ReminderTemplate, IltscheduleMailReminder iltscheduleMailReminder, string userName, string emailAddress)
        {

            int i = 0;

            try
            {

                try
                {
                    MailTemplateDesigner masterTemplateFile = await _dbContext.MailTemplateDesigner.Where(A => A.EventTitle.ToLower().Contains(TemplateTitles.FirstReminderEmail.ToLower()) && A.Status == true).FirstOrDefaultAsync();


                    if (masterTemplateFile == null)
                    {
                        _logger.Error(string.Format("Exception in function GetTemplateByTitle :- {0} for client :- {1}. Template  :- {2}", "Template not present", Program.OrgnaizationCode, TemplateTitles.FirstReminderEmail));
                        return i;
                    }
                    string RegardName = _message.GetRegardsName(Program.OrgnaizationCode);

                    MailServerConfiguration MailConfiguration = await _message.GetMailConfiguration();

                    List<EmailNotification> emailNotificationList = new List<EmailNotification>();
                    int emailCount = 0;

                    MailTemplateDesigner templateDesigner = (MailTemplateDesigner)masterTemplateFile.Shallowcopy();
                    try
                    {
                        if (templateDesigner != null)
                        {
                            #region "Template Creation"
                            templateDesigner.TemplateContent = templateDesigner.TemplateContent.Replace(Token.UserName, userName, StringComparison.OrdinalIgnoreCase);
                            templateDesigner.TemplateContent = templateDesigner.TemplateContent.Replace(Token.RegardName, RegardName, StringComparison.OrdinalIgnoreCase);
                            //   templateDesigner.TemplateContent = templateDesigner.TemplateContent.Replace(Token.AppUrl, Program.EmpoweredLmsPath, StringComparison.OrdinalIgnoreCase);
                            templateDesigner.TemplateContent = templateDesigner.TemplateContent.Replace(Token.CourseTitle, iltscheduleMailReminder.CourseName, StringComparison.OrdinalIgnoreCase);
                            templateDesigner.TemplateContent = templateDesigner.TemplateContent.Replace(Token.CourseDueDate, iltscheduleMailReminder.CreatedDate.ToString(), StringComparison.OrdinalIgnoreCase);
                            templateDesigner.MailSubject = templateDesigner.MailSubject.Replace(Token.CourseTitle, iltscheduleMailReminder.CourseName, StringComparison.OrdinalIgnoreCase);
                            templateDesigner.TemplateContent = templateDesigner.TemplateContent.Replace(Token.CourseUrl, Program.EmpoweredLmsPath + "/myCourseModule/" + iltscheduleMailReminder.CourseId, StringComparison.OrdinalIgnoreCase);


                            //var courseImage = await _dbContext.IltscheduleMailReminder.Where(A => A.CourseId == iltscheduleMailReminder.CourseId).Select(a => a.CourseImage).FirstOrDefaultAsync();

                            //if (courseImage != null)
                            //{

                            //    templateDesigner.TemplateContent = templateDesigner.TemplateContent.Replace(Token.CourseImage, "<img style=" + "width:300px" + ";" + " src=" + courseImage.ToString() + " /> ");
                            //}


                            #endregion

                            //#region "Create Email Object"
                            //string CCmailId = null;
                            //if (iltscheduleMailReminder.FifthManagerInCC)
                            //    CCmailId = iltscheduleMailReminder.ema;



                            EmailNotification emailNotification = new EmailNotification();
                            emailNotification.Subject = (templateDesigner.MailSubject);
                            emailNotification.Message = (templateDesigner.TemplateContent);
                            emailNotification.ToEmail = (emailAddress);
                            emailNotification.CourseId = iltscheduleMailReminder.CourseId;
                            // emailNotification.CC = CCmailId;
                            emailNotification.EventTitle = TemplateTitles.FirstReminderEmail;
                            // emailNotification.Description = "processing";

                            emailNotificationList.Add((emailNotification));

                            emailCount++;
                            
                                _ = _message.SendEmailAsyncNew(emailNotificationList, Program.OrgnaizationCode, MailConfiguration);
                                emailNotificationList.Clear();
                            

                        }
                    }
                    catch (Exception ex)
                    { _logger.Error(string.Format("Exception in function EmailReminder_First while sending multiple emails :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), Program.OrgnaizationCode)); }
                }

                catch (Exception ex)
                { _logger.Error(string.Format("Exception in function EmailReminder_First  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), Program.OrgnaizationCode)); }

            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception in function EmailReminder_First  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), Program.OrgnaizationCode));
            }
            return i;

        }

    }
}


