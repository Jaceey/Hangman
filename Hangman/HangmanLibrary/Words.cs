/*
 * Program:         WordsLibrary.dll
 * Module:          Words.cs
 * Author:          Jaceey Tuck
 * Date:            March 28, 2019
 * Description:     Implementation class for the IWordsEx interface (and also 
 *                  indirectly, for IWords).
 *                  This class implements its interfaces IMPLICITLY which means 
 *                  the interface methods are also available to a client via the
 *                  object's own class interface.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;    // File class
using System.ServiceModel;

namespace HangmanLibrary
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void OutputWinner(string winner);

    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IWords
    {
        int Count { get; }
        [OperationContract]
        bool NewPlayer(string name);
        [OperationContract(IsOneWay = true)]
        void Leave(string name);
        [OperationContract]
        string[] GetAllUsers();
        [OperationContract] string NewWord();

        [OperationContract]
        List<Hangman> GetPlayerList();
        [OperationContract]
        void SetPlayerList(List<Hangman> players);

        [OperationContract]
        void AddPoints(string name);

        [OperationContract]
        void SetWordState(string name);

        [OperationContract]
        void ResetWordState();

        [OperationContract]
        void AddWordCount(string name);

        [OperationContract]
        void CheckPlayerStatus();


    }
    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Words : IWords
    {        
        private Dictionary<string, ICallback> callbacks = new Dictionary<string, ICallback>();
        private List<string> users = new List<string>();
        List<Hangman> playerList = new List<Hangman>();

        // Private data members
        private const string wordsFile = "Words.txt";
        public List<string> allWords = null;
        public string currentWord = "";

        // Constructor
        public Words()
        {
            // Outputs to server
            Console.WriteLine("Loading words from words.txt");
            LoadFile();
            NewWord();
        }
               
        void LoadFile()
        {
            // Get current directory
            string currentDir = Directory.GetCurrentDirectory();
            // Get parent directory
            string par = Directory.GetParent(currentDir).Parent.FullName;

            var path = Path.Combine(par, "WordFile", wordsFile);

            using (TextReader reader = File.OpenText(path))
            {
                allWords = new List<string>();
                string s = "";
                while((s = reader.ReadLine()) != null)
                {
                    if (s.Length >= 3)
                        allWords.Add(s.ToUpper());
                }
            }
            // Outputs to server
            Console.WriteLine(Count + " words added");
        }

        // Public methods/properties
        public bool NewPlayer(string name)
        {
            if(callbacks.ContainsKey(name.ToUpper()))
            {
                return false;
            }
            else
            {
                ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
                callbacks.Add(name.ToUpper(), cb);

                Hangman player = new Hangman
                {
                    _PlayerName = name.ToUpper()
                };

                playerList.Add(player);
                users.Add(name.ToUpper());
                return true;
            }
        }

        
        public List<Hangman> GetPlayerList()
        {
            return playerList;
        }

        public void SetPlayerList(List<Hangman> players)
        {
            playerList = players;
        }

        // Updates players if they exit
        public void Leave(string name)
        {
            if (callbacks.ContainsKey(name.ToUpper()))
            {
                callbacks.Remove(name.ToUpper());
                users.Remove(name.ToUpper());

                foreach (var item in playerList)
                {
                    if (item._PlayerName == name.ToUpper())
                    {
                        playerList.Remove(item);
                        Console.WriteLine("Player: " + item._PlayerName + " has left");
                    }
                }
            }
        }
                
        public void CheckPlayerStatus()
        {
            bool gameOver = false;
            // Determine if all players words are solved
            foreach (var item in playerList)
            {
                if (item._FinishedWord == true)
                    gameOver = true;
                else
                {
                    gameOver = false;
                    break;
                }
            }

            if (gameOver == true)
                UpdateAllUsers();
            
        }


        public string[] GetAllUsers()
        {
            return users.ToArray<string>();
        }

        // Add points to each player
        public void AddPoints(string name)
        {
            foreach (var item in playerList)
            {
                if (item._PlayerName == name.ToUpper())
                {
                    item._TotalPoints += 5;
                    Console.WriteLine("Added 5 points to Player: " + item._PlayerName + " Total Points: " + item._TotalPoints);
                }
            }
        }

        public void SetWordState(string name)
        {
            foreach (var item in playerList)
            {
                if (item._PlayerName == name.ToUpper())
                {
                    item._FinishedWord = true;
                }
            }
        }

        public void ResetWordState()
        {
            foreach (var item in playerList)
            {
                item._FinishedWord = false;
            }
        }

        private string GetWinner()
        {
            int highestScore = 0;
            List<string> winnerNames = new List<string>();

            // Determine who has the highest score
            foreach (var item in playerList)
            {
                if (item._TotalPoints > highestScore)
                {
                    highestScore = item._TotalPoints;
                    winnerNames.Clear();
                    winnerNames.Add(item._PlayerName);
                }
                else if (item._TotalPoints == highestScore)
                {
                    winnerNames.Add(item._PlayerName);
                }
            }

            string winner = "";

            if (winnerNames.Count > 2)
            {
                foreach (var item in winnerNames)
                {
                    if (item != winnerNames.Last())
                        winner += item + " & ";
                    else
                        winner += item;
                }

                winner += " have all won with " + highestScore + " points!";
            }
            else if (winnerNames.Count > 1)
            {
                winner = winnerNames[0] + " and " + winnerNames[1] + " have tied at " + highestScore + " points!";
            }
            else
            {
                winner = winnerNames[0] + " is the winner at " + highestScore + " points";
            }
            
            return winner;
            
        }
        private void UpdateAllUsers()
        {
            string winner = GetWinner();
            foreach (ICallback item in callbacks.Values)
            {
                item.OutputWinner(winner);
                Console.WriteLine("Winner updated!");
            }
        }


        public string NewWord()
        {
            // Outputs to server
            Console.WriteLine("New word in play");

            currentWord = allWords[RandomNumber()];
            Console.WriteLine(currentWord);

            return currentWord;
        }


        public int Count
        {
            get
            {
                if (allWords == null)
                    return 0;
                return allWords.Count;
            }
        }

        private int RandomNumber()
        {
            Random random = new Random();
            return random.Next(0, allWords.Count);
        }

        public void AddWordCount(string name)
        {
            foreach (var item in playerList)
            {
                if (item._PlayerName == name.ToUpper())
                {
                    item.NumWordsGuessed++;
                }
            }
        }
    }
}
