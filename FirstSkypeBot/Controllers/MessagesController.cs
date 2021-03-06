﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
//7895d748-6edb-4c64-aa08-4a8087d4fe6a
//ZiLGTK04BSjkMFtm1Dnxd5L
namespace FirstSkypeBot
{
    interface ITimeStorage
    {
        DateTime? Start { get; set; }
        DateTime? BreakStart { get; set; }
        List<TimeSpan> Breaks { get; }
        DateTime? End { get; set; }
    }
    class Storage : ITimeStorage
    {
        private static DateTime? _start;
        private static DateTime? _end;
        private static DateTime? _breakStart;
        private static List<TimeSpan> _breaks = new List<TimeSpan>();

        public DateTime? End
        {
            get { return _end; }
            set { _end = value; }
        }
        DateTime? ITimeStorage.Start
        {
            get { return _start; }
            set { _start = value; }
        }
        DateTime? ITimeStorage.BreakStart
        {
            get { return _breakStart; }
            set { _breakStart = value; }
        }
        List<TimeSpan> ITimeStorage.Breaks
        {
            get { return _breaks; }
        }

        [BotAuthentication]
        public class MessagesController : ApiController
        {
            private readonly ITimeStorage _storage;
            public MessagesController()
            {
                _storage = new Storage();
            }

            /// <summary>
            /// POST: api/Messages
            /// Receive a message from a user and reply to it
            /// </summary>
            public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    // calculate something for us to return
                    string text = activity.Text.ToLower();
                    string replyText = "";

                    if (text == "start" || text == "s")
                    {
                        _storage.Start = DateTime.Now;
                        replyText = "Start time was set to " + DateTime.Now.ToShortTimeString();
                    }
                    else if (text == "break start" || text == "bs")
                    {
                        _storage.BreakStart = DateTime.Now;
                        replyText = "Break start time was set to " + DateTime.Now.ToShortTimeString();

                    }
                    else if ((text == "break end" || text == "be") && _storage.BreakStart.HasValue)
                    {
                        _storage.Breaks.Add(DateTime.Now - _storage.BreakStart.Value);
                        replyText = "Break end time was set to " + DateTime.Now.ToShortTimeString();

                    }
                    else if (text == "end" || text == "e")
                    {
                        _storage.End = DateTime.Now;
                        if (_storage.Start.HasValue)
                        {
                            var duration = (DateTime.Now - _storage.Start.Value);
                            foreach (var breakTime in _storage.Breaks)
                            {
                                duration -= breakTime;
                            }
                            replyText = string.Format("Start: {0} Break: {1}m End: {2} Duration: {3}h",
                                _storage.Start.Value.ToShortTimeString(),
                                _storage.Breaks.Select(x => x.TotalMinutes).Sum().ToString("N0"),
                                DateTime.Now.ToShortTimeString(),
                                duration.TotalHours.ToString("N2"));
                        }
                    }
                    // return our reply to the user
                    Activity reply = activity.CreateReply(replyText);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    HandleSystemMessage(activity);
                }
                var response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }

            private Activity HandleSystemMessage(Activity message)
            {
                if (message.Type == ActivityTypes.DeleteUserData)
                {
                    // Implement user deletion here
                    // If we handle user deletion, return a real message
                }
                else if (message.Type == ActivityTypes.ConversationUpdate)
                {
                    // Handle conversation state changes, like members being added and removed
                    // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                    // Not available in all channels
                }
                else if (message.Type == ActivityTypes.ContactRelationUpdate)
                {
                    // Handle add/remove from contact lists
                    // Activity.From + Activity.Action represent what happened
                }
                else if (message.Type == ActivityTypes.Typing)
                {
                    // Handle knowing tha the user is typing
                }
                else if (message.Type == ActivityTypes.Ping)
                {
                }

                return null;
            }
        }
    }
}