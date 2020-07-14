using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System.Net;
using HtmlAgilityPack;
using Serilog;
using WorkingWithWorker.Models;


namespace WorkingWithWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly string _urlLog;

        public Worker(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _urlLog = configuration["UrlLog"];
            _serviceConfiguration = new ServiceConfiguration();
            new ConfigureFromConfigurationOptions<ServiceConfiguration>(
                configuration.GetSection("ServiceConfiguration"))
                    .Configure(_serviceConfiguration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);


                foreach (string cidade in _serviceConfiguration.Cidades)
                {
                    _logger.Information($"Acessando site: {_serviceConfiguration.UrlFonte + cidade }");

                    var resultado = new Temperatura();
                    resultado.Horario = DateTime.Now;
                    resultado.Cidade = cidade;
                    resultado.GrausCelsius = GetTemperature(_serviceConfiguration.UrlFonte + cidade);

                    // log levels
                    // _logger.Debug();
                    // _logger.Error();
                    // _logger.Fatal();
                    // _logger.Information();
                    // _logger.Verbose();
                    // _logger.Warning();
                    _logger.Information("Temperatura registrada: " + resultado.GrausCelsius);
                }

                await Task.Delay(_serviceConfiguration.Intervalo, stoppingToken);
            }
        }

        private int GetTemperature(String UrlSite)
        {
            HtmlDocument html = new HtmlDocument();
            using (WebClient client = new WebClient())
            {
                string htmlCode = client.DownloadString(UrlSite);
                html.LoadHtml(htmlCode);
            }

            string testDivSelector = "//div[@class='_flex']/span[@class='-helvetica -gray -font-70 _margin-l-30 _center']";
            var divString = html.DocumentNode.SelectSingleNode(testDivSelector).InnerHtml.ToString().Replace("º", String.Empty).Split("\n")[1];

            return Int16.Parse(divString);
        }
    }
}
