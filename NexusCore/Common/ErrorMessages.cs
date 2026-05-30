using System;
using Newtonsoft.Json;

namespace NexusCore.Common
{
    public class ErrorMessages
    {
        private static Dictionary<int, string> ErrorMessageCollection = new Dictionary<int, string>();

        protected ErrorMessages()
        {
            //ErrorMessageCollection = new Dictionary<int, string>();
        }

        public static void LoadErrorMessages()
        {
            //using (StreamReader r = new StreamReader("Common//ErrorMessages.json"))
            var path = Path.Combine(AppContext.BaseDirectory, "Common", "ErrorMessages.json");
            using (StreamReader r = new StreamReader(path))


            {
                string json = r.ReadToEnd();
                List<MessageModel> messages = JsonConvert.DeserializeObject<List<MessageModel>>(json);

                foreach (var item in messages)
                {
                    //ErrorMessageCollection.Add(item.Code, item.Message);
                    ErrorMessageCollection[Convert.ToInt32(item.Code)] = item.Message;

                }
            }
        }

        public static string GetMessage(StatusCodes statusCode)
        {
            if (ErrorMessageCollection.TryGetValue(Convert.ToInt32(statusCode), out string Message))
            {
                return Message;
            }
            else
            {
                return "No Message Found!!";
            }
        }
    }

    public class MessageModel
    {
        //public int Code { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}

