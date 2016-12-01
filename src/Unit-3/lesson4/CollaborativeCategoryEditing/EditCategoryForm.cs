using System;
using System.Reactive.Linq;
using System.Windows.Forms;

using Raven.Abstractions.Data;
using Raven.Client;

namespace CollaborativeCategoryEditing
{
    public partial class EditCategoryForm : Form
    {
        public EditCategoryForm()
        {
            InitializeComponent();
        }

        private IDocumentSession _session;
        private Category _category;
        private IDisposable _subscription;
        private Etag _localEtag;

        private void LoadAndSubscribeButton_Click(object sender, EventArgs e)
        {
            _session = DocumentStoreHolder.Store.OpenSession();
            _category = _session.Load<Category>(CategoryIdTextbox.Text);

            if (_category == null)
            {
                MessageBox.Show("Category not found!");
                _session.Dispose();
            }
            else
            {
                NameTextbox.Text = _category.Name;
                DescriptionTextbox.Text = _category.Description;

                _localEtag = _session.Advanced.GetEtagFor(_category);
                _subscription = DocumentStoreHolder.Store
                    .Changes()
                    .ForDocument(CategoryIdTextbox.Text)
                    .Where(DocumentChangedByOtherUser)
                    .Subscribe(DocumentChangedOnServer);

                ToggleEditing();
            }
        }

        private bool DocumentChangedByOtherUser(DocumentChangeNotification change)
        {
            if (_savesCount == 0) return true;
            if (change.Etag.Restarts != _localEtag.Restarts) return true;

            var numberOfServerChanges = change.Etag.Changes - _localEtag.Changes;
            return (numberOfServerChanges > _savesCount);
        }

        private void DocumentChangedOnServer(DocumentChangeNotification change)
        {
            var shouldRefresh = MessageBox.Show(
                "Document was changed on the server. Would like to refresh?",
                "Alert", MessageBoxButtons.YesNo
            ) == DialogResult.Yes;

            if (shouldRefresh)
            {
                _session.Advanced.Refresh(_category);
                _savesCount = 0;
                _localEtag = _session.Advanced.GetEtagFor(_category);
                this.Invoke((MethodInvoker) delegate
                {
                    NameTextbox.Text = _category.Name;
                    DescriptionTextbox.Text = _category.Description;
                });
            }
        }

        private void ToggleEditing()
        {
            var isEditing = NameTextbox.Enabled;

            // toggle Load and Subscribe feature
            CategoryIdTextbox.Enabled = isEditing;
            CategoryIdLabel.Enabled = isEditing;
            LoadAndSubscribeButton.Enabled = isEditing;

            // toggle Edit feature
            NameTextbox.Enabled = !isEditing;
            NameLabel.Enabled = !isEditing;
            DescriptionTextbox.Enabled = !isEditing;
            DescriptionLabel.Enabled = !isEditing;
            SaveButton.Enabled = !isEditing;
            UnsubscribeButton.Enabled = !isEditing;
        }

        private int _savesCount = 0;
        private void SaveButton_Click(object sender, EventArgs e)
        {
            _category.Name = NameTextbox.Text;
            _category.Description = DescriptionTextbox.Text;
            _savesCount++;
            _session.SaveChanges();
        }

        private void UnsubscribeButton_Click(object sender, EventArgs e)
        {
            _session.Dispose();
            _subscription.Dispose();
            ToggleEditing();
        }
    }
}
