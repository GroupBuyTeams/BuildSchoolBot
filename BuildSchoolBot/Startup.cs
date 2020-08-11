// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using Quartz.Spi;
using Quartz;
using Quartz.Impl;
using BuildSchoolBot.Dialogs;
using BuildSchoolBot.Bots;
using BuildSchoolBot.Scheduler.Jobs;
using BuildSchoolBot.Scheduler;
using BuildSchoolBot.Models;
using BuildSchoolBot.Service;
using BuildSchoolBot.Repository;

namespace BuildSchoolBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>();
            services.AddSingleton<HistoryDialog>();
            services.AddSingleton<AddressDialogs>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, EchoBot<MainDialog>>();


            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Schedulers
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<StartBuy>();
            services.AddSingleton<StopBuy>();
            services.AddSingleton<NoteBuy>();
            services.AddHostedService<QuartzHostedService>();

            services.AddTransient<TeamsBuyContext>();
            services.AddTransient<EGRepository<Library>>();
            services.AddTransient<EGRepository<Payment>>();
            services.AddTransient<LibraryService>();
            services.AddTransient<OrderfoodServices>();
            services.AddTransient<OrderDetailService>();
            services.AddTransient<OrderService>();
            services.AddTransient<CreateCardService>();
            services.AddTransient<OrganizeStructureService>();
            services.AddTransient<PayMentService>();
            services.AddTransient<MenuService>();
            services.AddTransient<MenuDetailService>();

            services.AddTransient<HistoryService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }

    }
}
