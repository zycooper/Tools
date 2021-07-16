using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMS_Dev_Tool
{
    //send Email
    public class EmailSourceUnit
    {
        public string SourceName { get; set; }
        public string TableUrl { get; set; }
        public string ChartUrl { get; set; }
    }
    public class AttachmentUnit
    {
        public string _attachment_filename;
        public MemoryStream _attachment;
        public AttachmentUnit(DataTable data,string filename = "RawData.csv")
        {
            _attachment_filename = filename;

            DataTable data_csv = new DataTable();

            for (int i = 0; i < data.Columns.Count; i++)
            {
                data_csv.Columns.Add(data.Columns[i].ColumnName);
            }

            //escape comma from dt
            for (int j = 0; j < data.Rows.Count; j++)
            {
                DataRow dr = data_csv.NewRow();

                for (int i = 0; i < data.Columns.Count; i++)
                {
                    if (!string.IsNullOrEmpty(data.Rows[j][i].ToString()))
                    {
                        string value = data.Rows[j][i].ToString().Contains(",") ? "\"" + data.Rows[j][i].ToString() + "\"" : data.Rows[j][i].ToString();
                        dr[i] = value.Replace( @"
","");
                    }
                }

                data_csv.Rows.Add(dr);
            }

            //convert dt into stream                     

            _attachment = new MemoryStream(Encoding.GetEncoding("iso-8859-1").GetBytes(data_csv.ToCSV()));
        }
    }
    public static class Extensions
    {
        public static string ToCSV(this DataTable table)
        {
            var result = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
            }

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(row[i].ToString());
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
                }
            }

            return result.ToString();
        }
    }
    static public class EmailSend
    {
        /// <summary>
        /// Final EmailBody result
        /// </summary>
        static public string EmailBody;        
        static private LinkedResource Img_resource(string url, string id)
        {
            //fix base64
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(url, url.Length);
            sbText.Replace("\r\n", string.Empty); sbText.Replace(" ", string.Empty);

            Byte[] bitmapData = Convert.FromBase64String(sbText.ToString());
            MemoryStream streamBitmap = new MemoryStream(bitmapData);
            var imageToInline = new LinkedResource(streamBitmap, MediaTypeNames.Image.Jpeg);
            imageToInline.ContentId = id;

            return imageToInline;
        }
        static public void Send(
            string Subject, 
            string Body, 
            string FromEmail, 
            string FromTitle, 
            List<string> ToList, 
            List<string> BccList = null,
            string EmailServer = "")
        {
            SmtpClient client = new SmtpClient();
            MailMessage msg = new MailMessage
            {
                From = new MailAddress(FromEmail, FromTitle, System.Text.Encoding.UTF8)
            };

            if (ToList.Count > 0)
            {
                ToList.ForEach(x => msg.To.Add(x));

                //add Bcc
                if (BccList != null)
                {
                    if (BccList.Count > 0)
                    {
                        BccList.ForEach(x => msg.Bcc.Add(x));
                    }
                }
               
                client.Host = EmailServer;                

                msg.IsBodyHtml = true;
                msg.Subject = Subject;
                msg.Body = Body;

                client.Send(msg);
            }
        }
        static public void Send(
            string _subject,
            List<EmailSourceUnit> SourceList,
            List<string> ToList,
            List<string> CCList = null,
            List<string> BccList = null,
            AttachmentUnit attachment = null,
            string _fromTitle = "Default Email",
            string _fromEmailAddress = "yuanzhang998@gmail.com",
            string EmailServer = "")
        {
            SmtpClient client = new SmtpClient();
            MailMessage msg = new MailMessage
            {
                From = new MailAddress(_fromEmailAddress, _fromTitle, System.Text.Encoding.UTF8)
            };

            if (ToList.Count > 0)
            {
                ToList.ForEach(x => msg.To.Add(x));

                //add CC
                if (CCList != null)
                {
                    if (CCList.Count > 0)
                    {
                        CCList.ForEach(x => msg.CC.Add(x));
                    }
                }

                //add Bcc
                if (BccList != null)
                {
                    if (BccList.Count > 0)
                    {
                        BccList.ForEach(x => msg.Bcc.Add(x));
                    }
                }

                client.Host = EmailServer;

                msg.IsBodyHtml = true;
                msg.Subject = _subject;

                /*Email Content Below*/

                //populate SourceName for chart cid: remove all the non alphanueric characters
                Regex rgx = new Regex("[^A-Za-z0-9]+");
                string img_str = @"";

                foreach (var source in SourceList)
                {
                    if (!string.IsNullOrEmpty(source.ChartUrl))
                    {
                        img_str += @"<hr>
                                    <h4>" + /*Unit Name*/ source.SourceName + @":</h4>
                                    <img src=""cid:" +  /*Unit Name without non alphanumeric characters*/ rgx.Replace(source.SourceName, "") + @""">
                                    " +/*html table*/ source.TableUrl + @"";
                    }
                    else
                    {
                        img_str += @"<hr>
                                    <h4>" + /*Unit Name*/ source.SourceName + @":</h4>" +/*html table*/ source.TableUrl + @"";
                    }
                }

                EmailBody = @"<html>
                                    <p>Dear All,<p>
                                    <p>Please see today’s " + _subject + @" below.</p>
                                    " + img_str + @"
                                    <p>Best Regards,</p>                                  
                                  </html>";

                AlternateView view = AlternateView.CreateAlternateViewFromString(EmailBody, null, "text/html");

                foreach (var source in SourceList)
                {
                    if (!string.IsNullOrEmpty(source.ChartUrl))
                    {
                        view.LinkedResources.Add(Img_resource(source.ChartUrl, rgx.Replace(source.SourceName, "")));
                    }
                }

                msg.AlternateViews.Add(view);

                //add csv attachment
                if (attachment != null)
                {
                    Attachment atmt = new Attachment(attachment._attachment, new ContentType("text/csv"));
                    atmt.Name = attachment._attachment_filename;

                    msg.Attachments.Add(atmt);
                }

                client.Send(msg);
            }
        }
        static public void Send(
           string _subject,
           List<EmailSourceUnit> SourceList,
           List<string> ToList,
           List<AttachmentUnit> attachment_list,
           List<string> CCList = null,
           List<string> BccList = null,           
           string _fromTitle = "Report Automation",
           string _fromEmailAddress = "yuanzhang998@gmail.com",
           string EmailServer = "")
        {
            SmtpClient client = new SmtpClient();
            MailMessage msg = new MailMessage
            {
                From = new MailAddress(_fromEmailAddress, _fromTitle, System.Text.Encoding.UTF8)
            };

            if (ToList.Count > 0)
            {
                ToList.ForEach(x => msg.To.Add(x));

                //add CC
                if (CCList != null)
                {
                    if (CCList.Count > 0)
                    {
                        CCList.ForEach(x => msg.CC.Add(x));
                    }
                }

                //add Bcc
                if (BccList != null)
                {
                    if (BccList.Count > 0)
                    {
                        BccList.ForEach(x => msg.Bcc.Add(x));
                    }
                }
              
                //change setting according to Mireya Vazques
                client.Host = EmailServer;            

                msg.IsBodyHtml = true;
                msg.Subject = _subject;

                /*Email Content Below*/

                //populate SourceName for chart cid: remove all the non alphanueric characters
                Regex rgx = new Regex("[^A-Za-z0-9]+");
                string img_str = @"";

                foreach (var source in SourceList)
                {
                    if (!string.IsNullOrEmpty(source.ChartUrl))
                    {
                        img_str += @"<hr>
                                    <h4>" + /*Unit Name*/ source.SourceName + @":</h4>
                                    <img src=""cid:" +  /*Unit Name without non alphanumeric characters*/ rgx.Replace(source.SourceName, "") + @""">
                                    " +/*html table*/ source.TableUrl + @"";
                    }
                    else
                    {
                        img_str += @"<hr>
                                    <h4>" + /*Unit Name*/ source.SourceName + @":</h4>" +/*html table*/ source.TableUrl + @"";
                    }
                }

                EmailBody = @"<html>
                                    <p>Dear All,<p>
                                    <p>Please see today’s " + _subject + @" below.</p>
                                    " + img_str + @"
                                    <p>Best Regards,</p>
                                  </html>";

                AlternateView view = AlternateView.CreateAlternateViewFromString(EmailBody, null, "text/html");

                foreach (var source in SourceList)
                {
                    if (!string.IsNullOrEmpty(source.ChartUrl))
                    {
                        view.LinkedResources.Add(Img_resource(source.ChartUrl, rgx.Replace(source.SourceName, "")));
                    }
                }

                msg.AlternateViews.Add(view);

                //add csv attachment
                if (attachment_list.Count != 0)
                {
                    foreach (AttachmentUnit attachment in attachment_list)
                    {
                        Attachment atmt = new Attachment(attachment._attachment, new ContentType("text/csv"));
                        atmt.Name = attachment._attachment_filename;

                        msg.Attachments.Add(atmt);
                    }                   
                }

                client.Send(msg);
            }
        }
    }
}