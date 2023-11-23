
using System.Net.Mail;
using System.Net;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Drawing;
using IltscheduleMailReminderSchedule.Interface;
using IltscheduleMailReminderSchedule.Model;
using IltscheduleMailReminderSchedule.Helper;
//using EFCore.BulkExtensions;



namespace IltscheduleMailReminderSchedule.Repository
{
    public class MailMessageAlert : IMailMessageAlert
    {
        private static int CACHE_EXPIRED_TIMEOUT = 30;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static EmpoweredTLSContext _dbContext;

        public MailMessageAlert(EmpoweredTLSContext customDbContext)
        {
            _dbContext = customDbContext;

        }

        public async Task<int> SendEmailAsyncNew(List<EmailNotification> lstemailNotifications, string orgCode, MailServerConfiguration mailConfiguration)
        {

            #region "Send all Emails in Bulk"

            bool emailContainsImages = CheckIfMailContainsImages(lstemailNotifications[0].Message, orgCode);
            List<string> lstImagesPath = new List<string>();
            if (emailContainsImages)
                lstImagesPath = SaveImagesToDisk(lstemailNotifications[0].Message, orgCode);

            // InsertBulkInDatabase(orgCode, "[Notification].[EmailNotification]", ListToDatatable(lstemailNotifications), new SQLMapping().GetEmailNotificationColumnMapping());
            // lstemailNotifications =await AddBulkDataAsync(lstemailNotifications);

            foreach (var item in lstemailNotifications)
            {
                try
                {
                    item.CreatedDate = DateTime.UtcNow;
                    var mimeMessage = new MailMessage();

                    if (mailConfiguration != null && !string.IsNullOrEmpty(mailConfiguration.HostName))
                    {
                        mimeMessage.From = new MailAddress(mailConfiguration.FromEmailName, "", System.Text.Encoding.UTF8);
                        mimeMessage.To.Add(new MailAddress(item.ToEmail));

                        if (!String.IsNullOrEmpty(item.CC))
                        {
                            string[] CC;
                            CC = item.CC.Split(',');
                            foreach (string ccmail in CC)
                            {
                                if (string.IsNullOrEmpty(ccmail))
                                    continue;
                                string ccEmail = ccmail.Trim();
                                mimeMessage.CC.Add(new MailAddress(ccEmail, ccEmail));
                            }
                        }

                        mimeMessage.IsBodyHtml = true;
                        mimeMessage.Subject = item.Subject;
                        mimeMessage.Body = item.Message;

                        if (emailContainsImages)
                        {
                            mimeMessage.Body = ReplaceImagesTags(item.Message, orgCode);
                            GetEmailBodyWithImages(lstImagesPath, ref mimeMessage);
                        }

                        GetSmtpClient(mailConfiguration).SendMailAsync(mimeMessage).GetAwaiter().GetResult();

                    }
                    item.Description = "Success";
                    item.Status = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Exception While sending email :- {0} OrgCode :- {1}", Utilities.GetDetailedException(ex), orgCode));
                    item.Description = ex.Message;
                    item.Status = false;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(item.CC))
                        item.CC = Security.Encrypt(item.CC);
                    item.Message = (item.Message);
                    item.Subject = (item.Subject);
                    item.ToEmail = Security.Encrypt(item.ToEmail);
                }
            }
            #endregion

            //UpdateBulkInDatabase(orgCode, "[dbo].Update_EmailNotification", ListToDatatable(lstemailNotifications));
            //lstemailNotifications=await UpdateBulkDataAsync(lstemailNotifications);
            InsertBulkInDatabase(orgCode, "[Notification].[EmailNotification]", ListToDatatable(lstemailNotifications), new SQLMapping().GetEmailNotificationColumnMapping());
            return 1;
        }

        public static bool SendMail(MailMessage mail, MailServerConfiguration config)
        {
            bool isMailSend = true;
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = config.HostName;
                if (config.Port != 0)
                    smtpClient.Port = config.Port;
                smtpClient.EnableSsl = Convert.ToBoolean(config.UseSecureConnection);
                smtpClient.UseDefaultCredentials = false;
                if (config.UserName.ToString() != "")
                    smtpClient.Credentials = new NetworkCredential(config.UserName, config.UserPassword);

                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
            return isMailSend;
        }

        private bool CheckIfMailContainsImages(string htmlBody, string orgCode)
        {
            try
            {
                var regexImages = new Regex(@"(<\s*img)(.*?)(>)", RegexOptions.Compiled);
                var matchedImages = regexImages.Matches(htmlBody);

                //Email does not contain images so return the html string without Modification
                if (matchedImages.Count == 0)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception in function CheckIfMailContainsImages  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), orgCode));
                return false;
            }
        }
        private string ReplaceImagesTags(string htmlBody, string orgCode)
        {
            try
            {
                var regexImages = new Regex(@"(<\s*img)(.*?)(>)", RegexOptions.Compiled);
                var matchedImages = regexImages.Matches(htmlBody);
                for (int i = 0; i < matchedImages.Count; i++)
                {
                    htmlBody = htmlBody.Replace(matchedImages[i].Value, @"<img src=""cid:{" + i + @"}""/>");
                }
                return htmlBody;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception in function ReplaceImagesTags  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), orgCode));
                return htmlBody;
            }
        }
        private List<string> SaveImagesToDisk(string htmlString, string orgCode)
        {
            var regexImages = new Regex(@"(<\s*img)(.*?)(>)", RegexOptions.Compiled);
            var matchedImages = regexImages.Matches(htmlString);

            var regexImageExtension = new Regex(@"(data:image)(.*?)(;)", RegexOptions.Compiled);
            var matchImageExtension = regexImageExtension.Matches(htmlString);
            string replacedString = htmlString;
            List<string> lstImagePaths = new List<string>();

            for (int i = 0; i < matchedImages.Count; i++)
            {
                replacedString = replacedString.Replace(matchedImages[i].Value, @"<img src=""cid:{" + i + @"}""/>");
                string extension = matchImageExtension[i].Value.Replace("data: image / ", "");
                extension = extension.Replace(";", "").Split("/")[1];

                string[] splitImageCode = matchedImages[i].Value.Split(",");
                string base64string = splitImageCode[1].Replace(">", "");
                base64string = base64string.Replace("\"", "");
                base64string = base64string.Remove(base64string.Length - 1);


                lstImagePaths.Add(SaveImageOnDisk(base64string, extension, orgCode));
            }
            return lstImagePaths;
        }

        private string SaveImageOnDisk(string binaryData, string imageExtension, string orgCode)
        {
            string imagePath = string.Empty;
            try
            {
                byte[] imageBytes = Convert.FromBase64String(binaryData);
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                ms.Write(imageBytes, 0, imageBytes.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);

                try
                {
                    if (!Directory.Exists(Program.ApiGatewayWwwroot + @"\" + orgCode))
                        Directory.CreateDirectory(Program.ApiGatewayWwwroot + @"\" + orgCode);
                }
                catch (Exception)
                { }
                if (imageExtension == "")
                    imageExtension = "png";

                imagePath = Program.ApiGatewayWwwroot + @"\" + orgCode + @"\Image_" + new Random().Next(1, 99999) + "_" + DateTime.Now.ToString().Replace(":", "_").Replace("-", "_").Replace("/", "_").Replace(@"\", "_");

                switch (imageExtension.ToUpper())
                {
                    case "JPEG":
                    case "JPG":
                        image.Save(imagePath + ".jpeg", ImageFormat.Jpeg);
                        imagePath += ".jpeg";
                        break;
                    case "PNG":
                        image.Save(imagePath + ".png", ImageFormat.Png);
                        imagePath += ".png";
                        break;
                    default:
                        break;
                }
            }

            catch (Exception ex)
            { _logger.Error(string.Format("Exception in function SaveImageOnDisk  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), orgCode)); }
            return imagePath;
        }
        private static void GetEmailBodyWithImages(List<string> lstImagePaths, ref MailMessage mailMessage)
        {
            for (int j = 0; j < lstImagePaths.Count; j++)
            {
                var inlineLogo = new LinkedResource(lstImagePaths[j], "image/png");
                inlineLogo.ContentId = j.ToString();
                mailMessage.Body = mailMessage.Body.Replace("{" + j + "}", j.ToString());
                var view = AlternateView.CreateAlternateViewFromString(mailMessage.Body, null, "text/html");
                view.LinkedResources.Add(inlineLogo);
                mailMessage.AlternateViews.Add(view);
            }
        }
        public async Task<MailServerConfiguration> GetMailConfiguration()
        {
            try
            {
                MailServerConfiguration mailServerConfiguration = new MailServerConfiguration();

                mailServerConfiguration = await _dbContext.MailServerConfiguration.Where(a => a.IsActive == true && a.IsDeleted == false).FirstOrDefaultAsync();
                return mailServerConfiguration;
            }
            catch (Exception ex)
            { _logger.Error(string.Format("Exception in function GetMailConfigurationAsync :- {0} Organization Code :- {1} ", ex.Message.ToString(), Program.OrgnaizationCode)); }
            return null;
        }
        public static System.Net.Mail.SmtpClient GetSmtpClient(MailServerConfiguration mailConfiguration)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = mailConfiguration.HostName;
            if (mailConfiguration.Port != 0)
                smtpClient.Port = mailConfiguration.Port;
            smtpClient.EnableSsl = Convert.ToBoolean(mailConfiguration.UseSecureConnection);
            smtpClient.UseDefaultCredentials = false;
            if (mailConfiguration.UserName.ToString() != "")
                smtpClient.Credentials = new NetworkCredential(mailConfiguration.UserName, mailConfiguration.UserPassword);


            return smtpClient;
        }
        public EmpoweredTLSContext GetDbContext()
        {
            string ConnectionString = Security.Configuration.GetConnectionString("DefaultConnection");

            DbContextOptionsBuilder<EmpoweredTLSContext> optionsBuilder = new DbContextOptionsBuilder<EmpoweredTLSContext>();
            optionsBuilder
                .UseSqlServer(ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new EmpoweredTLSContext(optionsBuilder.Options);
        }
        public void InsertBulkInDatabase(string orgCode, string tableName, DataTable datatable, Dictionary<string, string> columnMapping)
        {
            try
            {

                using (var dbContext = GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    using (SqlConnection sqlConnection = new SqlConnection(connection.ConnectionString))
                    {
                        sqlConnection.Open();

                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection))
                        {
                            bulkCopy.DestinationTableName = tableName;
                            try
                            {
                                #region "Add Datatable to Sql Column Mapping"
                                if (columnMapping != null)
                                {
                                    foreach (var item in columnMapping)
                                    {
                                        bulkCopy.ColumnMappings.Add(item.Key, item.Value);
                                    }
                                }
                                #endregion
                                bulkCopy.BulkCopyTimeout = 0;
                                bulkCopy.WriteToServer(datatable);
                            }
                            catch (Exception ex)
                            { _logger.Error(string.Format("Exception in function InsertBulkInDatabase  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), orgCode)); }
                        }
                    }
                }


            }
            catch (Exception ex)
            { _logger.Error(string.Format("Exception in function InsertBulkInDatabase  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), orgCode)); }
        }

        public async Task<List<EmailNotification>> AddBulkDataAsync(List<EmailNotification> lstemailNotifications)
        {
            try
            {
                using (var dbContext = GetDbContext())
                {
                    await dbContext.AddRangeAsync(lstemailNotifications);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception in function UpdateBulkInDatabase  :- {0}  ", Utilities.GetDetailedException(ex)));
            }

            return lstemailNotifications;
        }

        public async Task<List<EmailNotification>> UpdateBulkDataAsync(List<EmailNotification> lstemailNotifications)
        {
            try
            {
                using (var dbContext = GetDbContext())
                {

                    dbContext.UpdateRange(lstemailNotifications);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            { _logger.Error(string.Format("Exception in function UpdateBulkInDatabase  :- {0}  ", Utilities.GetDetailedException(ex))); }

            return lstemailNotifications;
        }
        public void UpdateBulkInDatabase(string orgCode, string spName, DataTable datatable)
        {
            try
            {
                using (var dbContext = GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    using (SqlConnection con = new SqlConnection(connection.ConnectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(spName))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = con;
                            cmd.Parameters.AddWithValue("@tblEmailNotification", datatable);
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            { _logger.Error(string.Format("Exception in function UpdateBulkInDatabase  :- {0} Organization Code :- {1} ", Utilities.GetDetailedException(ex), orgCode)); }
        }


        private static DataTable ListToDatatable(List<EmailNotification> lstNotifications)
        {
            DataTable dataTable = new SQLMapping().GetEmailNotificationTable();
            try
            {
                DataRow dataRow = null;
                foreach (var item in lstNotifications)
                {
                    dataRow = dataTable.NewRow();
                    dataRow["CustomerCode"] = item.CustomerCode;
                    dataRow["Description"] = item.Description;
                    dataRow["Message"] = item.Message;
                    dataRow["Status"] = item.Status;
                    dataRow["Subject"] = item.Subject;
                    dataRow["CreatedDate"] = item.CreatedDate;
                    dataRow["CC"] = item.CC;
                    dataRow["CourseId"] = item.CourseId;
                    dataRow["UserId"] = item.UserId;
                    dataRow["EventTitle"] = item.EventTitle;
                    dataRow["ToEmail"] = item.ToEmail == null ? "ToEmail not present" : item.ToEmail;
                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception)
            {
            }
            return dataTable;
        }

        public string GetRegardsName(string OrganizationCode)
        {
            try
            {
                //var _db = new EmpoweredTLSContext();
                //string RegardName = this._db.MasterPageSetting.OrderByDescending(p => p.Id).Select(p => p.RenameSystem).FirstOrDefault();
                //return String.IsNullOrEmpty(RegardName) ? "Admin" : RegardName;
                return "Admin";
            }
            catch (Exception ex)
            { _logger.Error(string.Format("Exception in function GetRegardsName :- {0}   ", Utilities.GetDetailedException(ex))); }
            return "";
        }
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {


                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        object value = dr[column.ColumnName];
                        if (value == DBNull.Value)
                            value = null;
                        if (pro.Name == "EmailID")
                        {
                            pro.SetValue(obj, Security.Decrypt(value.ToString()), null);
                        }
                        else
                        {
                            pro.SetValue(obj, value, null);
                        }
                    }

                    else
                        continue;
                }
            }
            return obj;
        }
        //public async Task<List<IltscheduleMailReminder>> GetEmailData(string ReminderTemplate)
        //{
        //    List<IltscheduleMailReminder> data = new List<IltscheduleMailReminder>();
        //    DataTable dt = new DataTable();
        //    try

        //    {


        //        using (var dbcontextOrg = GetDbContext())
        //        {
        //            dbcontextOrg.Database.OpenConnection();
        //            using (var cmd = dbcontextOrg.Database.GetDbConnection().CreateCommand())
        //            {
        //                cmd.CommandText = "GetUserRemainderMail";
        //                cmd.CommandTimeout = 0;
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.Add(new SqlParameter("@ReminderTemplate", SqlDbType.NVarChar) { Value = ReminderTemplate });
        //                DbDataReader reader = cmd.ExecuteReader();
        //                dt.Load(reader);
        //                reader.Dispose();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    if (dt.Rows.Count > 0)
        //    {
        //        data = ConvertDataTable<IltscheduleMailReminder>(dt);
        //    }
        //    return data;
        //}

        public async Task<List<ConfigurableParameter>> GetUserConfigurationValueAsync(string[] str_arr, string orgCode, string defaultValue = "")
        {

            List<ConfigurableParameter> configValues = new List<ConfigurableParameter>();
            List<ConfigurableParameter> EmailconfigValues = new List<ConfigurableParameter>();
            try
            {

                configValues = await _dbContext.ConfigurableParameter.Where(a => a.ParameterType == "EMAIL").ToListAsync();


                foreach (var item in str_arr)
                {
                    ConfigurableParameter configValue = new ConfigurableParameter();
                    configValue = configValues.Where(a => a.Code == item.ToString()).FirstOrDefault();
                    if (configValue != null)
                    {

                        EmailconfigValues.Add(configValue);
                    }
                    else
                    {
                        configValue.Code = item.ToString();
                        configValue.Value = defaultValue;

                        configValues.Add(configValue);
                    }

                }

                //_logger.Debug(string.Format("Value for Config {0} for Organization {1} is {2} ", configurationCode, orgCode, configValue));
            }
            catch (System.Exception ex)
            {
                _logger.Error(string.Format("Exception in function GetConfigurationValueAsync :- {0} for Organisation {1}", Utilities.GetDetailedException(ex), orgCode));
                return null;
            }
            return configValues;
        }

    }
}
