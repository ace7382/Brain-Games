// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

//.........................
//.....Generated Class.....
//.........................
//.......Do not edit.......
//.........................

using UnityEngine;
// ReSharper disable All

namespace Doozy.Runtime.Signals
{
    public partial class Signal
    {
        public static bool Send(StreamId.GameManagement id, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), message);
        public static bool Send(StreamId.GameManagement id, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), signalSource, message);
        public static bool Send(StreamId.GameManagement id, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), signalProvider, message);
        public static bool Send(StreamId.GameManagement id, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), signalSender, message);
        public static bool Send<T>(StreamId.GameManagement id, T signalValue, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), signalValue, message);
        public static bool Send<T>(StreamId.GameManagement id, T signalValue, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), signalValue, signalSource, message);
        public static bool Send<T>(StreamId.GameManagement id, T signalValue, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), signalValue, signalProvider, message);
        public static bool Send<T>(StreamId.GameManagement id, T signalValue, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.GameManagement), id.ToString(), signalValue, signalSender, message);

        public static bool Send(StreamId.PathPuzzle id, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), message);
        public static bool Send(StreamId.PathPuzzle id, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), signalSource, message);
        public static bool Send(StreamId.PathPuzzle id, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), signalProvider, message);
        public static bool Send(StreamId.PathPuzzle id, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), signalSender, message);
        public static bool Send<T>(StreamId.PathPuzzle id, T signalValue, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), signalValue, message);
        public static bool Send<T>(StreamId.PathPuzzle id, T signalValue, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), signalValue, signalSource, message);
        public static bool Send<T>(StreamId.PathPuzzle id, T signalValue, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), signalValue, signalProvider, message);
        public static bool Send<T>(StreamId.PathPuzzle id, T signalValue, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.PathPuzzle), id.ToString(), signalValue, signalSender, message);

        public static bool Send(StreamId.QuitConfirmation id, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), message);
        public static bool Send(StreamId.QuitConfirmation id, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), signalSource, message);
        public static bool Send(StreamId.QuitConfirmation id, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), signalProvider, message);
        public static bool Send(StreamId.QuitConfirmation id, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), signalSender, message);
        public static bool Send<T>(StreamId.QuitConfirmation id, T signalValue, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), signalValue, message);
        public static bool Send<T>(StreamId.QuitConfirmation id, T signalValue, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), signalValue, signalSource, message);
        public static bool Send<T>(StreamId.QuitConfirmation id, T signalValue, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), signalValue, signalProvider, message);
        public static bool Send<T>(StreamId.QuitConfirmation id, T signalValue, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.QuitConfirmation), id.ToString(), signalValue, signalSender, message);

        public static bool Send(StreamId.Trivia id, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), message);
        public static bool Send(StreamId.Trivia id, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), signalSource, message);
        public static bool Send(StreamId.Trivia id, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), signalProvider, message);
        public static bool Send(StreamId.Trivia id, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), signalSender, message);
        public static bool Send<T>(StreamId.Trivia id, T signalValue, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), signalValue, message);
        public static bool Send<T>(StreamId.Trivia id, T signalValue, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), signalValue, signalSource, message);
        public static bool Send<T>(StreamId.Trivia id, T signalValue, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), signalValue, signalProvider, message);
        public static bool Send<T>(StreamId.Trivia id, T signalValue, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.Trivia), id.ToString(), signalValue, signalSender, message);

        public static bool Send(StreamId.TutorialScreen id, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), message);
        public static bool Send(StreamId.TutorialScreen id, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), signalSource, message);
        public static bool Send(StreamId.TutorialScreen id, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), signalProvider, message);
        public static bool Send(StreamId.TutorialScreen id, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), signalSender, message);
        public static bool Send<T>(StreamId.TutorialScreen id, T signalValue, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), signalValue, message);
        public static bool Send<T>(StreamId.TutorialScreen id, T signalValue, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), signalValue, signalSource, message);
        public static bool Send<T>(StreamId.TutorialScreen id, T signalValue, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), signalValue, signalProvider, message);
        public static bool Send<T>(StreamId.TutorialScreen id, T signalValue, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.TutorialScreen), id.ToString(), signalValue, signalSender, message);

        public static bool Send(StreamId.WordScramble id, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), message);
        public static bool Send(StreamId.WordScramble id, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), signalSource, message);
        public static bool Send(StreamId.WordScramble id, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), signalProvider, message);
        public static bool Send(StreamId.WordScramble id, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), signalSender, message);
        public static bool Send<T>(StreamId.WordScramble id, T signalValue, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), signalValue, message);
        public static bool Send<T>(StreamId.WordScramble id, T signalValue, GameObject signalSource, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), signalValue, signalSource, message);
        public static bool Send<T>(StreamId.WordScramble id, T signalValue, SignalProvider signalProvider, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), signalValue, signalProvider, message);
        public static bool Send<T>(StreamId.WordScramble id, T signalValue, Object signalSender, string message = "") => SignalsService.SendSignal(nameof(StreamId.WordScramble), id.ToString(), signalValue, signalSender, message);   
    }

    public partial class StreamId
    {
        public enum GameManagement
        {
            CountdownEnded,
            DisableExitLevelButton,
            GameSetup,
            LeaveTitleScreen,
            LeaveTutorialScreen,
            LevelEnded,
            LoadLevelScene,
            PlayNextLevel,
            ReplayCurrentLevel,
            StartLevel,
            UnloadGameScenes
        }

        public enum PathPuzzle
        {
            PathPuzzleSetup,
            TileRotated
        }

        public enum QuitConfirmation
        {
            BackToGame,
            ExitLevel,
            Popup
        }

        public enum Trivia
        {
            AnswerChosen,
            EndGame,
            TriviaSetup
        }

        public enum TutorialScreen
        {
            PageLoaded
        }
        public enum WordScramble
        {
            EndGame,
            TileClicked,
            WordScrambleSetup
        }         
    }
}
