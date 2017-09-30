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
            return !_saving;
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
                this.Invoke((MethodInvoker)delegate
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

        private bool _saving;
        private void SaveButton_Click(object sender, EventArgs e)
        {
            _saving = true;
            _category.Name = NameTextbox.Text;
            _category.Description = DescriptionTextbox.Text;
            _session.SaveChanges();
            _saving = false;
        }

        private void UnsubscribeButton_Click(object sender, EventArgs e)
        {
            _session.Dispose();
            _subscription.Dispose();
            ToggleEditing();
        }
    }
}
