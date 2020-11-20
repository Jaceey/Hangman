/*
 * Program:         WordsLibrary.dll
 * Module:          Hangman.cs
 * Author:          Jaceey Tuck
 * Date:            March 28, 2019
 * Description:     Helper class to keep track of the players information
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;  // WCF types
using System.Runtime.Serialization;

namespace HangmanLibrary
{
    public class Hangman 
    {
        public int _TotalPoints { get; set; }
        public string _PlayerName { get; set; }
        public bool _FinishedWord { get; set; }
        public int _NumberGuesses { get; set; }
        public int NumWordsGuessed { get; set; }

        public Hangman()
        {
            NumWordsGuessed = 1;
            _TotalPoints = 0;
            _NumberGuesses = 0;
            _FinishedWord = false;
        }

    }
}
