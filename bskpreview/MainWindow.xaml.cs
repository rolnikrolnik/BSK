using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace bskpreview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainController mainController;

        public MainWindow()
        {
            InitializeComponent();
            // FOR TESTING PURPOSES
            //this.EncryptionSourceFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\interfejs2.JPG";
            //this.EncryptionDestinationFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\szyfrogram";
            //this.DecriptionSourceFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\szyfrogram";
            //this.DecryptionDestinationFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\odszyfrowane";
        }

        #region Button Methods

        private void CipherButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ProgressBar.IsIndeterminate = true;
                this.PassSettingsForEncryption();
                this.ProgressBar.IsIndeterminate = false;
                MessageBox.Show("Plik zaszyfrowany poprawnie!", "Sukces");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DecipherButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ProgressBar.IsIndeterminate = true;
                this.PassSettingsForDecryption();
                this.ProgressBar.IsIndeterminate = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {

            var sourceButton = ((e.Source) as Button);

            if (sourceButton == this.EncryptionSourceFileButton)
            {
                var saveFileDialog = new OpenFileDialog();
                if (saveFileDialog.ShowDialog() != true) return;
                this.EncryptionSourceFileTextBox.Text = saveFileDialog.FileName;
            }
            else if (sourceButton == this.EncryptionDestinationFileButton)
            {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() != true) return;
                this.EncryptionDestinationFileTextBox.Text = saveFileDialog.FileName;
                saveFileDialog.OpenFile().Close();
            }
            else if (sourceButton == this.DecriptionSourceFileButton)
            {
                var saveFileDialog = new OpenFileDialog();
                if (saveFileDialog.ShowDialog() != true) return;
                this.DecriptionSourceFileTextBox.Text = saveFileDialog.FileName;
                this.ListPossibleIdentities();
            }
            else if (sourceButton == this.DecritpionDestinationFileButton)
            {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() != true) return;
                this.DecryptionDestinationFileTextBox.Text = saveFileDialog.FileName;
                saveFileDialog.OpenFile().Close();
            }
        }

        private void AddReceiverButton_Click(object sender, RoutedEventArgs e)
        {
            const string initialPath = @"C:\Users\Rolnik\Desktop\STUDIA\s06\BSK\projekt\Odbiorcy";
            var openFileDialog = new OpenFileDialog { Multiselect = true};
            if(Directory.Exists(initialPath))
                openFileDialog.InitialDirectory = initialPath;
            if (openFileDialog.ShowDialog() != true) return;
            foreach (var receiver in openFileDialog.FileNames.Select(fileName => new RSAPublicKey
            {
                Username = System.IO.Path.GetFileNameWithoutExtension(fileName),
                PathToKey = openFileDialog.FileName
            }))
            {
                this.ReceiversListBox.Items.Add(receiver);
            }
            this.EncryptButton.IsEnabled = true;
        }

        private void DeleteReceiverButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ReceiversListBox.SelectedItem != null)
            {
                this.ReceiversListBox.Items.Remove(this.ReceiversListBox.SelectedItem);
            }
            if (this.ReceiversListBox.Items.Count == 0) this.EncryptButton.IsEnabled = false;
        }

        private void ConfirmIdentityButton_Click(object sender, RoutedEventArgs e)
        {
            this.PasswordTextBox.IsEnabled = true;
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;
        }

        private void IdentietiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.PasswordTextBox.IsEnabled = false;
            this.ConfirmIdentityButton.IsEnabled = true;
            this.PasswordTextBox.Password = string.Empty;
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.DecryptButton.IsEnabled = true;
            if (this.PasswordTextBox.Password == string.Empty) this.DecryptButton.IsEnabled = false;
        }

        #endregion

        #region Private methods

        private void ListPossibleIdentities()
        {
            var sourcePath = this.DecriptionSourceFileTextBox.Text;
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException("Ścieżki do plików niepoprawne - wybierz plik i ponów akcję!");
            }
            this.mainController = new MainController
            {
                DecryptionSourceFilePath = sourcePath
            };

            foreach (var identity in this.mainController.GetPossibleIdentities())
            {
                this.IdentietiesListBox.Items.Add(identity.Name);
            }
            this.IdentietiesListBox.SelectedIndex = 0;
        }

        private void PassSettingsForEncryption()
        {
            var sourcePath = this.EncryptionSourceFileTextBox.Text;
            var destinationPath = this.EncryptionDestinationFileTextBox.Text;
            if (string.IsNullOrEmpty(sourcePath) ||
                string.IsNullOrEmpty(destinationPath))
            {
                throw new ArgumentException("Ścieżki do plików niepoprawne - wybierz plik i ponów akcję!");
            }

            int keySize;
            Int32.TryParse(((ComboBoxItem)this.KeySizeComboBox.SelectedValue).Content.ToString(), out keySize);
            int blockSize;
            Int32.TryParse(((ComboBoxItem)this.BlockSizeComboBox.SelectedValue).Content.ToString(),
                out blockSize);
            int feedbackSize;
            Int32.TryParse(((ComboBoxItem)this.FeedbackdSizeComboBox.SelectedValue).Content.ToString(),
                out feedbackSize);
            var cipherMode = ((ComboBoxItem)this.ModeComboBox.SelectedValue).Content.ToString();

            this.mainController = new MainController
            {
                EncryptionSourceFilePath = sourcePath,
                EncryptionDestinationFilePath = destinationPath,
                KeySize = keySize,
                BlockSize = blockSize,
                FeedbackSize = feedbackSize,
                CipherMode = cipherMode,
                ReceiversRsaPublicKeys = this.ReceiversListBox.Items.OfType<RSAPublicKey>().ToList()
            };

            this.mainController.EncryptFile();
        }

        private void PassSettingsForDecryption()
        {
            var sourcePath = this.DecriptionSourceFileTextBox.Text;
            var destinationPath = this.DecryptionDestinationFileTextBox.Text;
            if (string.IsNullOrEmpty(sourcePath) ||
                string.IsNullOrEmpty(destinationPath))
            {
                throw new ArgumentException("Ścieżki do plików niepoprawne - wybierz plik i ponów akcję!");
            }

            this.mainController = new MainController
            {
                DecryptionSourceFilePath = sourcePath,
                DecryptionDestinationFilePath = destinationPath,
                Password = this.PasswordTextBox.Password,
                Identity = this.IdentietiesListBox.SelectedItem.ToString()
            };

            try
            {
                this.mainController.DecryptFile();
            }
            catch
            {}
        }

        #endregion

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = this.ModeComboBox.SelectedItem as ComboBoxItem;
            if (comboBoxItem != null
                && (comboBoxItem.Content.ToString() == "CFB"
                    || comboBoxItem.Content.ToString() == "OFB"))
            {
                if (this.FeedbackdSizeComboBox != null
                    && this.FeedbackSizeLabel != null)
                    this.FeedbackSizeLabel.Visibility = Visibility.Visible;
                var feedbackdSizeComboBox = this.FeedbackdSizeComboBox;
                if (feedbackdSizeComboBox != null) feedbackdSizeComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                if (this.FeedbackdSizeComboBox != null
                    && this.FeedbackSizeLabel != null)
                    this.FeedbackSizeLabel.Visibility = Visibility.Collapsed;
                var feedbackdSizeComboBox = this.FeedbackdSizeComboBox;
                if (feedbackdSizeComboBox != null) feedbackdSizeComboBox.Visibility = Visibility.Collapsed;
            }
        }
    }
}
