using Microsoft.AspNetCore.SignalR;
using Quartz;
using System;
using System.Threading.Tasks;
using BlazorApp1.Hubs;

namespace BlazorApp1.Services
{
    public class ReminderService : IJob
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public ReminderService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", "Time to take your medication!");
        }
    }
}
