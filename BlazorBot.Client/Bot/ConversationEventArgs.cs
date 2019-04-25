using Microsoft.Bot.Schema;
using System;

namespace BlazorBot.Client.Bot
{
    public class ConversationEventArgs : EventArgs
    {
        public Activity Activity { get; set; }
    }
}