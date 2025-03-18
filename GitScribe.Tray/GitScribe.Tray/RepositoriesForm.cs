using GitScribe.Service;
using LibGit2Sharp;

namespace GitScribe.Tray
{
   public partial class RepositoriesForm : Form
   {
      private readonly GitScibreService m_service;
      private ListBox m_repositoriesListBox;
      private Button m_refreshButton;
      private Button m_viewDetailsButton;

      public RepositoriesForm(GitScibreService service)
      {
         m_service = service;
         SetupComponents();
         LoadRepositories();
      }

      private void SetupComponents()
      {
         Text = "GitScribe Repositories";
         Size = new Size(500, 400);
         StartPosition = FormStartPosition.CenterScreen;

         m_repositoriesListBox = new ListBox
         {
            Dock = DockStyle.Fill,
            DisplayMember = "Name"
         };

         var panel = new Panel
         {
            Dock = DockStyle.Bottom,
            Height = 50
         };

         m_refreshButton = new Button
         {
            Text = "Refresh",
            Width = 100,
            Location = new Point(10, 15)
         };
         m_refreshButton.Click += (sender, e) => LoadRepositories();

         m_viewDetailsButton = new Button
         {
            Text = "View Details",
            Width = 100,
            Location = new Point(120, 15)
         };
         m_viewDetailsButton.Click += (sender, e) => ViewRepositoryDetails();

         panel.Controls.Add(m_refreshButton);
         panel.Controls.Add(m_viewDetailsButton);

         Controls.Add(m_repositoriesListBox);
         Controls.Add(panel);
      }

      private void LoadRepositories()
      {
         m_repositoriesListBox.Items.Clear();
         var repositoryNames = m_service.GetRepositories();

         foreach (var name in repositoryNames)
         {
            if (name != null)
            {
               m_repositoriesListBox.Items.Add(name);
            }
         }
      }

      private void ViewRepositoryDetails()
      {
         if (m_repositoriesListBox.SelectedItem is string repositoryName)
         {
            var repositoryInformation = m_service.GetRepositoryInformation(repositoryName);
            var repositoryStatus = m_service.GetRepositoryStatus(repositoryInformation.WorkingDirectory);
            var detailsForm = new RepositoryDetailsForm(repositoryName, repositoryInformation, repositoryStatus);
            detailsForm.Show();
         }
         else
         {
            MessageBox.Show("Please select a repository first", "GitScribe",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
      }
   }
}

public class RepositoryDetailsForm : Form
{
   private readonly string _repositoryName;
   private readonly RepositoryInformation _repository;
   private readonly RepositoryStatus _repositoryStatus;

   public RepositoryDetailsForm(string repositoryName, RepositoryInformation repository, RepositoryStatus repositoryStatus)
   {
      _repositoryName = repositoryName;
      _repository = repository;
      _repositoryStatus = repositoryStatus;
      InitializeComponent();
   }

   private void InitializeComponent()
   {
      Text = $"Repository: {_repositoryName}";
      Size = new Size(500, 400);
      StartPosition = FormStartPosition.CenterScreen;

      var panel = new TableLayoutPanel
      {
         Dock = DockStyle.Fill,
         ColumnCount = 2,
         RowCount = 0,
         AutoSize = true,
         Padding = new Padding(10)
      };

      AddPropertyRow(panel, "Path", _repository.WorkingDirectory);

      if (_repositoryStatus.Untracked.Any())
      {
         AddPropertyRow(panel, "Untracked", _repositoryStatus.Untracked.Count().ToString());
      }

      if (_repositoryStatus.Modified.Any())
      {
         AddPropertyRow(panel, "Modified", _repositoryStatus.Modified.Count().ToString());
      }

      if (_repositoryStatus.Added.Any())
      {
         AddPropertyRow(panel, "Added", _repositoryStatus.Added.Count().ToString());
      }

      if (_repositoryStatus.Removed.Any())
      {
         AddPropertyRow(panel, "Removed", _repositoryStatus.Removed.Count().ToString());
      }

      Controls.Add(panel);
   }

   private void AddPropertyRow(TableLayoutPanel panel, string propertyName, string propertyValue)
   {
      panel.RowCount++;
      int rowIndex = panel.RowCount - 1;

      var label = new Label
      {
         Text = propertyName + ":",
         AutoSize = true,
         Font = new Font(Font, FontStyle.Bold)
      };

      var valueLabel = new Label
      {
         Text = propertyValue ?? "N/A",
         AutoSize = true
      };

      panel.Controls.Add(label, 0, rowIndex);
      panel.Controls.Add(valueLabel, 1, rowIndex);
   }
}
