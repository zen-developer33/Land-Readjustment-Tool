using Land_Pooling_Policy_Manager.UI;
using Land_Pooling_Policy_Manager.Services;

namespace Land_Pooling_Policy_Manager.UI.Forms
{
    public sealed partial class frmPolicyClauseDiagram : Form
    {
        private readonly PolicyManagerService _service;
        private readonly int _policySetId;
        private readonly int _clauseId;
        private readonly string _clauseTitle;
        private readonly bool _editable;

        public frmPolicyClauseDiagram(
            PolicyManagerService service,
            int policySetId,
            int clauseId,
            string clauseTitle,
            bool editable)
        {
            _service = service;
            _policySetId = policySetId;
            _clauseId = clauseId;
            _clauseTitle = clauseTitle;
            _editable = editable;
            InitializeComponent();
            RecordFormTheme.Apply(this);
            lblClause.Text = clauseTitle;
            btnAttachImage.Enabled = editable;
        }

        private async void frmPolicyClauseDiagram_Load(object? sender, EventArgs e)
        {
            await LoadImageAsync();
        }

        private async void btnAttachImage_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new()
            {
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                UseWaitCursor = true;
                await RunServiceAsync(() => _service.AddAttachmentAsync(
                    _policySetId,
                    _clauseId,
                    dialog.FileName,
                    _clauseTitle));
                await LoadImageAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Clause Diagram", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task LoadImageAsync()
        {
            try
            {
                byte[]? imageData = await RunServiceAsync(() =>
                    _service.GetPolicyAttachmentImageDataAsync(_policySetId, _clauseId));
                Image? image = null;
                if (imageData is { Length: > 0 })
                {
                    image = await Task.Run(() =>
                    {
                        using MemoryStream stream = new(imageData);
                        return Image.FromStream(stream);
                    });
                }

                // ZoomPanPanel.Image setter disposes the previous image and
                // resets the zoom-to-fit, so we hand the new image to it directly.
                imageViewer.Image = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Clause Diagram", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<T> RunServiceAsync<T>(Func<Task<T>> operation)
        {
            return await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }

        private async Task RunServiceAsync(Func<Task> operation)
        {
            await Task.Run(async () => await _service.RunExclusiveAsync(operation).ConfigureAwait(false));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Clearing Image on the viewer disposes the bitmap; Dispose on the
            // panel itself fires via the parent disposal chain.
            imageViewer.Image = null;
            base.OnFormClosed(e);
        }
    }
}
