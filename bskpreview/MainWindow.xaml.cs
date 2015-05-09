using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            this.EncryptionSourceFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\projekt\\interfejs2.JPG";
            this.EncryptionDestinationFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\projekt\\szyfrogram";
            this.DecriptionSourceFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\projekt\\szyfrogram";
            this.DecryptionDestinationFileTextBox.Text = "C:\\Users\\Rolnik\\Desktop\\STUDIA\\s06\\BSK\\projekt\\odszyfrowane";
        }

        #region Button Methods

        private void CipherButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressBar.IsIndeterminate = true;
            this.PassSettingsForEncryption();
            this.ProgressBar.IsIndeterminate = false;   
            MessageBox.Show("Plik zaszyfrowany poprawnie!", "Sukces");    
        }

        private void DecipherButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressBar.IsIndeterminate = true;
            //var decryptedBytes = this.DecryptFile();
            //this.SaveFile(this.DecryptionDestinationFileTextBox.Text, decryptedBytes);
            this.ProgressBar.IsIndeterminate = false;
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() != true) return;
            var sourceButton = ((e.Source) as Button);

            if (sourceButton == this.EncryptionSourceFileButton)
                this.EncryptionSourceFileTextBox.Text = saveFileDialog.FileName;
            else if (sourceButton == this.EncryptionDestinationFileButton)
            {
                this.EncryptionDestinationFileTextBox.Text = saveFileDialog.FileName;
                saveFileDialog.OpenFile().Close();
                
            }
            else if (sourceButton == this.DecriptionSourceFileButton)
                this.DecriptionSourceFileTextBox.Text = saveFileDialog.FileName;
            else if (sourceButton == this.DecritpionDestinationFileButton)
            {
                this.DecryptionDestinationFileTextBox.Text = saveFileDialog.FileName;
                saveFileDialog.OpenFile().Close();
            }
        }

        private void PassSettingsForEncryption()
        {
            var sourcePath = this.EncryptionSourceFileTextBox.Text;
            var destinationPath = this.EncryptionDestinationFileTextBox.Text;
            if (string.IsNullOrEmpty(sourcePath) || 
                string.IsNullOrEmpty(destinationPath) )
            {
                throw new ArgumentNullException("Plik nie wybrany!");
            }

            int keySize;
            Int32.TryParse(((ComboBoxItem) this.KeySizeComboBox.SelectedValue).Content.ToString(), out keySize);
            int feedbackSize;
            Int32.TryParse(((ComboBoxItem) this.FeedbackSizeComboBox.SelectedValue).Content.ToString(),
                out feedbackSize);
            var cipherMode = ((ComboBoxItem) this.ModeComboBox.SelectedValue).Content.ToString();

            this.mainController = new MainController()
            {
                EncryptionSourceFilePath = sourcePath,
                EncryptionDestinationFilePath = destinationPath,
                KeySize = keySize,
                BlockSize = feedbackSize,
                CipherMode = cipherMode
            };
            
            this.mainController.EncryptFile();
        }

        #endregion

        #region Helper Methods

        //private byte[] DecryptFile()
        //{
        //    byte[] decryptedBytes;
        //    try
        //    {
        //        var filePath = this.DecriptionSourceFileTextBox.Text;

        //        if (string.IsNullOrEmpty(filePath))
        //        {
        //            throw new ArgumentNullException("Plik nie wybrany!");
        //        }
        //        var cipherMode = (this.ModeComboBox.SelectedValue as ComboBoxItem).Content.ToString();
        //        decryptedBytes = cipherClass.DecryptFile(filePath, cipherMode);
        //    }
        //    catch (Exception exception)
        //    {
        //        MessageBox.Show(exception.Message);
        //        decryptedBytes = null;
        //    }
        //    return decryptedBytes;
        //}

        //private byte[] EncryptFile()
        //{
        //    byte[] encrypteBytes;
        //    try
        //    {
        //        var filePath = this.EncryptionSourceFileTextBox.Text;
        //        if (string.IsNullOrEmpty(filePath))
        //        {
        //            throw new ArgumentNullException("Plik nie wybrany!");
        //        }

        //        int keySize;
        //        Int32.TryParse((this.KeySizeComboBox.SelectedValue as ComboBoxItem).Content.ToString(), out keySize);
        //        int FeedbackSize;
        //        Int32.TryParse((this.FeedbackSizeComboBox.SelectedValue as ComboBoxItem).Content.ToString(),
        //            out FeedbackSize);
        //        var cipherMode = (this.ModeComboBox.SelectedValue as ComboBoxItem).Content.ToString();

        //        encrypteBytes = cipherClass.EncryptFile(filePath, keySize, FeedbackSize, cipherMode);
        //    }
        //    catch (Exception exception)
        //    {
        //        MessageBox.Show(exception.Message);
        //        encrypteBytes = null;
        //    }
        //    return encrypteBytes;
        //}

        //private void SaveFile(string filePath, byte[] encryptedBytes)
        //{

        //    try
        //    {
        //        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException();
        //        this.fileController.SaveFile(filePath, encryptedBytes);
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message);
        //    }
        //}

        #endregion
        
    }
}
