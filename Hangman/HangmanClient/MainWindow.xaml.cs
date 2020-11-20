using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HangmanLibrary;
using System.ServiceModel;  // WCF

namespace HangmanClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        private IWords wordInstance = null;
        public Words words;
        public string word = "";
        public int NumOfGuesses = 0;
        
        // Used to store each players info
        public Hangman playerInfo;
        public System.Windows.Threading.DispatcherTimer dispatcherTimer;
        public int TimerCount;
        public bool NextWord = false;
        public MainWindow()
        {
            // Setup a timer
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            TimerCount = 0;

            words = new Words();
            word = words.currentWord;
            

            InitializeComponent();

        }

        // Update scoreboard 
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //dispatcherTimer = sender as System.Windows.Threading.DispatcherTimer;
            TimerCount++;
            if (TimerCount % 5 == 0)
            {
                SetPlayers();

                // Check if winner needs to be implemented
                wordInstance.CheckPlayerStatus();
            }
        }

        // Button click event for all the letters
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            CheckWord(button.Content.ToString());
        }

        // Check for letter in word
        private void CheckWord(string Letter)
        {
            SetPlayers();
            List<int> indexes = new List<int>();

            char[] tempWord = word.ToCharArray();
            char[] label = wordTxt.Text.ToCharArray();
            char l = Char.Parse(Letter);

            if (tempWord.Contains(l))
            {
                // find letter index
                for (int i = 0; i < tempWord.Length; i++)
                {
                    if (l == tempWord[i])
                    {
                        indexes.Add(i);
                    }
                }


                for (int i = 0; i < indexes.Count; i++)
                {
                    // * 2, label is twice the length after formatting
                    label[indexes[i] * 2] = l;

                    // update scoreboard
                    wordInstance.AddPoints(tbName.Text);
                    SetPlayers();
                }
            }
            else
            {
                NumOfGuesses++;
                ChangePicture();

                if (NumOfGuesses == 10)
                {
                    winnerTxt.Visibility = Visibility.Visible;
                    wordInstance.SetWordState(tbName.Text);
                    MessageBox.Show("Word not solved!\nThe word was: " + word);
                    buttonPanel.IsEnabled = false;
                }
            }
            
            wordTxt.Text = new string(label);
            CheckWin();
        }

        // Update Hangman picture
        private void ChangePicture()
        {
            HangmanImg.Source = new BitmapImage(new Uri("\\images\\" + NumOfGuesses + ".jpg", UriKind.Relative));
        }

        // Check is the word has been solved
        private void CheckWin()
        {
            string label = wordTxt.Text.ToString();

            // Determine if word has any letters left to be guessed
            if (!label.Contains('_'))
            {
                winnerTxt.Visibility = Visibility.Visible;
                wordInstance.SetWordState(tbName.Text);
                MessageBox.Show("You Solved The Word!!");
                buttonPanel.IsEnabled = false;
            }
        }

        // Set word to randomly selected word
        private void SetWord(string word)
        {
            string textHolder = "";
            
            // Replace letters with _
            for (int i = 0; i < word.Length; i++)
            {
                textHolder += "_ ";
            }

            wordTxt.Text = textHolder;
        }

        // Assign player new word
        private void NewWord_Click(object sender, RoutedEventArgs e)
        {
            // Reset all values
            word = words.NewWord();
            wordInstance.AddWordCount(tbName.Text);
            wordInstance.ResetWordState();
            buttonPanel.IsEnabled = true;
            ResetButtons();
            NumOfGuesses = 0;
            ChangePicture();
            SetWord(word);
            SetPlayers();            
            winnerTxt.Visibility = Visibility.Hidden;
            winnerTxt.Text = "Determining Winner";
            NextWord = false;
            NewWordBtn.IsEnabled = false;
        }

        // Enable all letter buttons
        private void ResetButtons()
        {
            btnA.IsEnabled = true;
            btnB.IsEnabled = true;
            btnC.IsEnabled = true;
            btnD.IsEnabled = true;
            btnE.IsEnabled = true;
            btnF.IsEnabled = true;
            btnG.IsEnabled = true;
            btnH.IsEnabled = true;
            btnI.IsEnabled = true;
            btnJ.IsEnabled = true;
            btnK.IsEnabled = true;
            btnL.IsEnabled = true;
            btnM.IsEnabled = true;
            btnN.IsEnabled = true;
            btnO.IsEnabled = true;
            btnP.IsEnabled = true;
            btnQ.IsEnabled = true;
            btnR.IsEnabled = true;
            btnS.IsEnabled = true;
            btnT.IsEnabled = true;
            btnU.IsEnabled = true;
            btnV.IsEnabled = true;
            btnW.IsEnabled = true;
            btnX.IsEnabled = true;
            btnY.IsEnabled = true;
            btnZ.IsEnabled = true;
        }

        // Set player login information
        private void OnNameClick(object sender, RoutedEventArgs e)
        {
            // Start timer
            dispatcherTimer.Start();
            if (tbName.Text != "")
            {
                ConnectToHangman();
                SetWord(word);
            }
        }

        // Output scoreboard
        private void SetPlayers()
        {
            var users = wordInstance.GetPlayerList();

            PlayerBoard.Children.Clear();
            ScoreBoard.Children.Clear();
            WordCount.Children.Clear();

            foreach (var item in users)
            {
                // Output Player Name
                Label name = new Label
                {
                    Content = item._PlayerName,
                    Width = 70
                };
                // Output Player score
                Label score = new Label
                {
                    Content = item._TotalPoints,
                    Width = 70
                };
                // Output Player word count
                Label wordCount = new Label
                {
                    Content = "Word#: " + item.NumWordsGuessed,
                    Width = 70
                };

                PlayerBoard.Children.Add(name);
                ScoreBoard.Children.Add(score);
                WordCount.Children.Add(wordCount);
            }
        }

        // Connect to server
        private void ConnectToHangman()
        {
            try
            {
                // Configure the ABCs of using the MessageBoard service
                DuplexChannelFactory<IWords> channel = new DuplexChannelFactory<IWords>(this, "HangmanEndpoint");

                // Activate a MessageBoard object
                wordInstance = channel.CreateChannel();

                if (wordInstance.NewPlayer(tbName.Text))
                {
                    // Hide login, so they cant login again
                    LoginPanel.Visibility = Visibility.Hidden;
                    buttonPanel.IsEnabled = true;

                    // Set player board
                    SetPlayers();
                }
                else
                {
                    // New player name rejected by the service so nullify service proxies
                    wordInstance = null;
                    MessageBox.Show("ERROR: Username in use. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (wordInstance != null)
                wordInstance.Leave(tbName.Text);
        }


        private delegate void GuiUpdateDelegate(string winner);
        public void OutputWinner(string winner)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    winnerTxt.Text = winner;
                    NewWordBtn.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                this.Dispatcher.BeginInvoke(new GuiUpdateDelegate(OutputWinner), new object[] { winner });
        }
    }
}
