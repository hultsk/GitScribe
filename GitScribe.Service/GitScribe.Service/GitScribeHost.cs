namespace GitScribe.Service;

public class GitScribeHost
{
   public static void Main(string[] args)
   {
      // Create the host and run the worker in the background
      CreateHostBuilder(args).Build().Run();
   }

   public static IHostBuilder CreateHostBuilder(string[] args) =>
       Host.CreateDefaultBuilder(args)
           .ConfigureServices((hostContext, services) =>
           {
              // Add the background worker that will host the gRPC server
              services.AddHostedService<GitScribeService>();
           });
}