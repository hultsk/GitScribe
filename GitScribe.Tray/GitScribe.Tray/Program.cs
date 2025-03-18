using GitScribe.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitScribe.Tray
{
   internal static class Program
   {
      private static NotifyIcon _trayIcon;
      private static IHost _host;
      private static GitScibreService _service;

      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main(string[] args)
      {
         // To customize application configuration such as set high DPI settings or default font
         ApplicationConfiguration.Initialize();

         // Create the host using the service's CreateHostBuilder but modifying it
         // to not use Windows Service (since we're running as a tray app)
         _host = CreateHostBuilder(args).Build();

         // Get the service
         _service = _host.Services.GetRequiredService<GitScibreService>();

         // Initialize tray icon
         InitializeTrayIcon();

         // Start the host
         _host.Start();

         // Run application message loop without a main form
         Application.Run();

         // Stop the host when application exits
         _host.StopAsync().Wait();
      }

      private static void InitializeTrayIcon()
      {
         _trayIcon = new NotifyIcon
         {
            Icon = SystemIcons.Application,
            Text = "GitScribe Service",
            Visible = true
         };

         // Create context menu
         var contextMenu = new ContextMenuStrip();

         // Status menu item
         var statusMenuItem = new ToolStripMenuItem("Service Status: Running");
         statusMenuItem.Enabled = false;
         contextMenu.Items.Add(statusMenuItem);

         contextMenu.Items.Add(new ToolStripSeparator());

         // View repositories menu item
         var viewReposMenuItem = new ToolStripMenuItem("View Repositories", null, (sender, e) =>
         {
            var repoForm = new RepositoriesForm(_service);
            repoForm.Show();
         });
         contextMenu.Items.Add(viewReposMenuItem);

         // Refresh menu item
         var refreshMenuItem = new ToolStripMenuItem("Refresh", null, (sender, e) =>
         {
            // Update status might go here in the future
            MessageBox.Show("Service information refreshed", "GitScribe",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
         });
         contextMenu.Items.Add(refreshMenuItem);

         contextMenu.Items.Add(new ToolStripSeparator());

         // Start service menu item
         var startMenuItem = new ToolStripMenuItem("Start Service", null, async (sender, e) =>
         {
            await _service.StartAsync(CancellationToken.None);
            statusMenuItem.Text = "Service Status: Running";
            MessageBox.Show("Service started", "GitScribe",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
         });
         contextMenu.Items.Add(startMenuItem);

         // Stop service menu item
         var stopMenuItem = new ToolStripMenuItem("Stop Service", null, async (sender, e) =>
         {
            await _service.StopAsync(CancellationToken.None);
            statusMenuItem.Text = "Service Status: Stopped";
            MessageBox.Show("Service stopped", "GitScribe",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
         });
         contextMenu.Items.Add(stopMenuItem);

         contextMenu.Items.Add(new ToolStripSeparator());

         // Exit menu item
         var exitMenuItem = new ToolStripMenuItem("Exit", null, (sender, e) =>
         {
            _trayIcon.Visible = false;
            Application.Exit();
         });
         contextMenu.Items.Add(exitMenuItem);

         _trayIcon.ContextMenuStrip = contextMenu;

         // Set up double-click to open repositories form
         _trayIcon.DoubleClick += (sender, e) =>
         {
            var repoForm = new RepositoriesForm(_service);
            repoForm.Show();
         };
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              // We don't use UseWindowsService here since this is a tray app
              .ConfigureServices((hostContext, services) =>
              {
                 // Reuse your existing configuration extension
                 services.AddRepositoryManager(hostContext.Configuration);

                 // Register as singleton instead of hosted service
                 services.AddSingleton<GitScibreService>();
              });
   }
}