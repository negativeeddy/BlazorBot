using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBot.Client.Bot
{
    public class BlazorAdapter : BotAdapter
    {
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ConversationEventArgs> OnActivityPublished;

        public async Task ProcessActivityAsync(string message, BotCallbackHandler callback = null)
        {
            if (message == null)
            {
                return;
            }

            // Performing the conversion from console text to an Activity for
            // which the system handles all messages (from all unique services).
            // All processing is performed by the broader bot pipeline on the Activity
            // object.
            var activity = new Activity()
            {
                Text = message,

                // Note on ChannelId:
                // The Bot Framework channel is identified by a unique ID.
                // For example, "skype" is a common channel to represent the Skype service.
                // We are inventing a new channel here.
                ChannelId = "console",
                From = new ChannelAccount(id: "user", name: "User1"),
                Recipient = new ChannelAccount(id: "bot", name: "Bot"),
                Conversation = new ConversationAccount(id: "Convo1"),
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Type = ActivityTypes.Message,
            };

            using (var context = new TurnContext(this, activity))
            {
                await this.RunPipelineAsync(context, callback, default(CancellationToken)).ConfigureAwait(false);
            }
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext context, Activity[] activities, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

            var responses = new ResourceResponse[activities.Length];

            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];

                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                        {
                            OnActivityPublished?.Invoke(this, new ConversationEventArgs { Activity = activity });
                        }

                        break;

                    case ActivityTypesEx.Delay:
                        {
                            // The Activity Schema doesn't have a delay type build in, so it's simulated
                            // here in the Bot. This matches the behavior in the Node connector.
                            int delayMs = (int)((Activity)activity).Value;
                            await Task.Delay(delayMs).ConfigureAwait(false);
                        }

                        break;

                    case ActivityTypes.Trace:
                        // Do not send trace activities unless you know that the client needs them.
                        // For example: BF protocol only sends Trace Activity when talking to emulator channel.
                        break;

                    default:
                        Console.WriteLine("Bot: activity type: {0}", activity.Type);
                        break;
                }

                responses[index] = new ResourceResponse(activity.Id);
            }

            return responses;
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
