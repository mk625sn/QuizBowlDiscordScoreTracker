using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuizBowlDiscordScoreTracker;

namespace QuizBowlDiscordScoreTrackerUnitTests
{
    [TestClass]
    public class GameStateTests
    {
        [TestMethod]
        public void ReaderIdNotSetInConstructor()
        {
            // This isn't that useful of a test, but use it as a proof of concept.
            GameState gameState = new GameState();
            Assert.IsNull(gameState.ReaderId, "No reader should be assigned on initialization.");
        }

        [TestMethod]
        public void TryGetNextPlayerFalseWhenQueueIsEmpty()
        {
            GameState gameState = new GameState();
            Assert.IsFalse(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                "There should be no next player if no one was added to the queue.");
        }

        [TestMethod]
        public void CannotAddReaderToQueue()
        {
            GameState gameState = new GameState
            {
                ReaderId = 123
            };
            Assert.IsFalse(
                gameState.AddPlayer(gameState.ReaderId.Value, "Reader"),
                "Adding the reader to the queue should not be possible.");
        }

        [TestMethod]
        public void ReaderIdPersists()
        {
            const ulong readerId = 123;
            GameState gameState = new GameState
            {
                ReaderId = readerId
            };
            Assert.AreEqual(readerId, gameState.ReaderId, "Reader Id is not persisted.");
        }

        [TestMethod]
        public void CannotAddSamePlayerTwiceToQueue()
        {
            const ulong id = 1234;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Adding the player the first time should succeed.");
            Assert.IsFalse(gameState.AddPlayer(id, "Player"), "Adding the player the second time should fail.");
        }

        [TestMethod]
        public void FirstAddedPlayerIsTopOfQueue()
        {
            const ulong firstId = 1;
            const ulong secondId = 2;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "Adding the player the first time should succeed.");
            Assert.IsTrue(gameState.AddPlayer(secondId, "Player2"), "Adding the player the second time should succeed.");
            Assert.IsTrue(gameState.TryGetNextPlayer(out ulong nextPlayerId), "There should be a player in the queue.");
            Assert.AreEqual(firstId, nextPlayerId, "The player first in the queue should be the first one added.");
        }

        [TestMethod]
        public void PlayerOrderInQueue()
        {
            ulong[] ids = new ulong[] { 1, 2, 3, 4 };
            GameState gameState = new GameState();
            foreach (ulong id in ids)
            {
                Assert.IsTrue(gameState.AddPlayer(id, $"Player {id}"), $"Should be able to add {id} to the queue.");
            }

            foreach (ulong id in ids)
            {
                Assert.IsTrue(
                    gameState.TryGetNextPlayer(out ulong nextPlayerId),
                    $"Should be able to get a player from the queue (which should match ID {id}.");
                Assert.AreEqual(id, nextPlayerId, "Unexpected ID from the queue.");
                gameState.ScorePlayer(0);
            }

            Assert.IsFalse(
                gameState.TryGetNextPlayer(out _), "No players should be left in the queue.");
        }

        [TestMethod]
        public void PlayerOnSameTeamSkippedInQueue()
        {
            ulong[] ids = new ulong[] { 1, 3, 2, 5, 4 };
            GameState gameState = new GameState();
            foreach (ulong id in ids)
            {
                ulong teamId = 100 + (id % 2);
                Assert.IsTrue(
                    gameState.AddPlayer(id, $"Player {id}", teamId), $"Should be able to add {id} to the queue.");
            }

            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                $"Should be able to get a player from the queue (which should be for the first team)");
            Assert.AreEqual(1u, nextPlayerId, "Unexpected ID from the queue.");
            gameState.ScorePlayer(0);
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out nextPlayerId),
                $"Should be able to get a player from the queue (which should be for the second team)");
            Assert.AreEqual(2u, nextPlayerId, "Unexpected ID from the queue after getting the 2nd player.");
            gameState.ScorePlayer(0);

            Assert.IsFalse(
                gameState.TryGetNextPlayer(out _),
                "No players should be left in the queue, since they are on the same team.");
        }

        [TestMethod]
        public void CanWithdrawPlayerOnTopOfQueue()
        {
            const ulong id = 1234;
            GameState gameState = new GameState();

            Assert.IsTrue(gameState.AddPlayer(id, $"Player {id}"), "Adding the player should succeed.");
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                "There should be a player in the queue.");
            Assert.AreEqual(id, nextPlayerId, "Id of the next player should be ours.");
            Assert.IsTrue(gameState.WithdrawPlayer(id), "Withdrawing the same player should succeed.");
            Assert.IsFalse(
                gameState.TryGetNextPlayer(out _),
                "There should be no player in the queue when they withdrew.");
        }

        [TestMethod]
        public void CanWithdrawPlayerInMiddleOfQueue()
        {
            const ulong firstId = 1;
            const ulong secondId = 22;
            const ulong thirdId = 333;
            GameState gameState = new GameState();

            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "Adding the first player should succeed.");
            Assert.IsTrue(gameState.AddPlayer(secondId, "Player2"), "Adding the second player should succeed.");
            Assert.IsTrue(gameState.AddPlayer(thirdId, "Player3"), "Adding the third player should succeed.");
            Assert.IsTrue(gameState.WithdrawPlayer(secondId), "Withdrawing the second player should succeed.");
            Assert.IsTrue(gameState.WithdrawPlayer(firstId), "Withdrawing the first player should succeed.");
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                "There should be a player in the queue.");
            Assert.AreEqual(thirdId, nextPlayerId, "Id of the next player should be the third player's.");
        }

        [TestMethod]
        public void CannotWithdrawPlayerNotInQueue()
        {
            const ulong id = 1234;
            GameState gameState = new GameState();
            gameState.AddPlayer(id, "Player");
            Assert.IsFalse(gameState.WithdrawPlayer(id + 1), "Should not be able to withdraw player who is not in the queue.");
        }

        [TestMethod]
        public void CannotWithdrawSamePlayerInQueueTwiceInARow()
        {
            const ulong id = 1234;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Adding player should succeed.");
            Assert.IsTrue(gameState.WithdrawPlayer(id), "First withdrawal should succeed.");
            Assert.IsFalse(gameState.WithdrawPlayer(id), "Second withdrawal should fail.");
        }

        [TestMethod]
        public void CanWithdrawSamePlayerInQueueTwice()
        {
            const ulong id = 1234;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "First add should succeed.");
            Assert.IsTrue(gameState.WithdrawPlayer(id), "First withdrawal should succeed.");
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Second add should succeed.");
            Assert.IsTrue(gameState.WithdrawPlayer(id), "Second withdrawal should succeed.");
        }

        [TestMethod]
        public void ClearCurrentRoundClearsQueueAndKeepsReader()
        {
            const ulong id = 1234;
            const ulong readerId = 12345;
            GameState gameState = new GameState
            {
                ReaderId = readerId
            };

            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ClearCurrentRound();
            Assert.IsFalse(gameState.TryGetNextPlayer(out _), "Queue should have been cleared.");
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed after clear.");
            Assert.AreEqual(readerId, gameState.ReaderId, "Reader should remain the same.");
        }

        [TestMethod]
        public void ClearAllClearsQueueAndReader()
        {
            const ulong id = 1234;
            const ulong readerId = 12345;
            GameState gameState = new GameState
            {
                ReaderId = readerId
            };

            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ClearAll();
            Assert.IsFalse(gameState.TryGetNextPlayer(out ulong _), "Queue should have been cleared.");
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed after clear.");
            Assert.IsNull(gameState.ReaderId, "Reader should be cleared.");
        }

        [TestMethod]
        public void NextQuestionClearsQueueAndKeepsReader()
        {
            const ulong id = 1234;
            const ulong readerId = 12345;
            GameState gameState = new GameState
            {
                ReaderId = readerId
            };

            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ScorePlayer(-5);
            gameState.NextQuestion();
            Assert.IsFalse(gameState.TryGetNextPlayer(out ulong _), "Queue should have been cleared.");
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed after clear.");
            Assert.AreEqual(readerId, gameState.ReaderId, "Reader should remain the same.");
            IDictionary<PlayerTeamPair, ScoringSplitOnScoreAction> lastSplits = gameState.GetLastScoringSplits();
            Assert.AreEqual(1, lastSplits.Count, "Unexpected number of scores.");

            KeyValuePair<PlayerTeamPair, ScoringSplitOnScoreAction> splitPair = lastSplits.First();
            Assert.AreEqual(id, splitPair.Key.PlayerId, "Unexpected ID for the score.");
            Assert.AreEqual(-5, splitPair.Value.Split.Points, "Unexpected point total for the score.");
        }

        [TestMethod]
        public void CannotAddPlayerAfterNeg()
        {
            const ulong id = 1;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ScorePlayer(-5);
            Assert.IsFalse(gameState.TryGetNextPlayer(out ulong _), "Queue should have been cleared.");
            Assert.IsFalse(gameState.AddPlayer(id, "Player"), "Add should fail after a neg.");
        }

        [TestMethod]
        public void CannotAddPlayerAfterZeroPointBuzz()
        {
            const ulong id = 1;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ScorePlayer(0);
            Assert.IsFalse(gameState.TryGetNextPlayer(out ulong _), "Queue should have been cleared.");
            Assert.IsFalse(gameState.AddPlayer(id, "Player"), "Add should fail after a no penalty buzz.");
        }

        [TestMethod]
        public void CanAddPlayerAfterCorrectBuzz()
        {
            const ulong id = 1;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ScorePlayer(10);
            Assert.IsFalse(gameState.TryGetNextPlayer(out ulong _), "Queue should have been cleared.");
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should suceed after correct buzz.");
        }

        [TestMethod]
        public void NegScoredCorrectly()
        {
            const ulong id = 123;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ScorePlayer(-5);
            IDictionary<PlayerTeamPair, ScoringSplitOnScoreAction> lastSplits = gameState.GetLastScoringSplits();
            Assert.AreEqual(1, lastSplits.Count, "Only one player should have a score.");
            KeyValuePair<PlayerTeamPair, ScoringSplitOnScoreAction> splitPair = lastSplits.First();

            Assert.AreEqual(id, splitPair.Key.PlayerId, "Unexpected ID.");
            Assert.AreEqual(-5, splitPair.Value.Split.Points, "Unexpected score.");
        }

        [TestMethod]
        public void CorrectBuzzScoredCorrectly()
        {
            const ulong id = 123;
            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(id, "Player"), "Add should succeed.");
            gameState.ScorePlayer(10);
            IDictionary<PlayerTeamPair, ScoringSplitOnScoreAction> lastSplits = gameState.GetLastScoringSplits();
            Assert.AreEqual(1, lastSplits.Count, "Only one player should have a score.");
            KeyValuePair<PlayerTeamPair, ScoringSplitOnScoreAction> splitPair = lastSplits.First();

            Assert.AreEqual(id, splitPair.Key.PlayerId, "Unexpected ID.");
            Assert.AreEqual(10, splitPair.Value.Split.Points, "Unexpected score.");
        }

        [TestMethod]
        public void MultipleBuzzesWithCorrectScore()
        {
            const ulong id = 123;
            int[] points = new int[] { 10, -5, 15 };
            GameState gameState = new GameState();
            foreach (int point in points)
            {
                Assert.IsTrue(gameState.AddPlayer(id, "Player"), $"Add should succeed for point total {point}.");
                gameState.ScorePlayer(point);
                if (point <= 0)
                {
                    gameState.NextQuestion();
                }
            }

            IDictionary<PlayerTeamPair, ScoringSplitOnScoreAction> lastSplits = gameState.GetLastScoringSplits();
            Assert.AreEqual(1, lastSplits.Count, "Only one player should have a score.");
            KeyValuePair<PlayerTeamPair, ScoringSplitOnScoreAction> splitPair = lastSplits.First();

            Assert.AreEqual(id, splitPair.Key.PlayerId, "Unexpected ID.");
            Assert.AreEqual(points.Sum(), splitPair.Value.Split.Points, "Unexpected score.");
        }

        [TestMethod]
        public void DifferentPlayersInQueueScoredCorrectly()
        {
            const ulong firstId = 1;
            const ulong secondId = 22;

            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "Add for first player should succeed.");
            Assert.IsTrue(gameState.AddPlayer(secondId, "Player2"), "Add for second player should succeed.");
            gameState.ScorePlayer(-5);
            gameState.ScorePlayer(10);

            IDictionary<PlayerTeamPair, ScoringSplitOnScoreAction> lastSplits = gameState.GetLastScoringSplits();
            Assert.AreEqual(2, lastSplits.Count, "Two players should have scored.");

            KeyValuePair<PlayerTeamPair, ScoringSplitOnScoreAction> scoreGrouping = lastSplits
                .FirstOrDefault(pair => pair.Key.PlayerId == firstId);
            Assert.IsNotNull(scoreGrouping, "We should have a pair which relates to the first player.");
            Assert.AreEqual(-5, scoreGrouping.Value.Split.Points, "The first player should have negged.");

            scoreGrouping = lastSplits.FirstOrDefault(pair => pair.Key.PlayerId == secondId);
            Assert.IsNotNull(scoreGrouping, "We should have a pair which relates to the second player.");
            Assert.AreEqual(10, scoreGrouping.Value.Split.Points, "The second player should have negged.");
        }

        [TestMethod]
        public void PlayerOnSameTeamSkippedOnWrongBuzz()
        {
            const ulong firstPlayerId = 1;
            const ulong secondPlayerId = 2;
            const ulong otherTeamPlayerId = 3;
            const ulong firstTeamId = 11;
            const ulong secondTeamId = 12;

            GameState gameState = new GameState();
            Assert.IsTrue(
                gameState.AddPlayer(firstPlayerId, "Player1", firstTeamId), "Add should succeed the first time");
            Assert.IsTrue(
                gameState.AddPlayer(secondPlayerId, "Player2", firstTeamId), "Add should succeed the second time");
            Assert.IsTrue(
                gameState.AddPlayer(otherTeamPlayerId, "Player3", secondTeamId), "Add should succeed the third time");

            gameState.ScorePlayer(0);
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId), "There should be another player in the queue");
            Assert.AreEqual(otherTeamPlayerId, nextPlayerId, "Player on the other team should be prompted next");
            gameState.ScorePlayer(0);

            Assert.IsFalse(gameState.TryGetNextPlayer(out _), "No other players should be taken from the queue");
        }

        [TestMethod]
        public void TeamIncludedInScore()
        {
            const ulong firstPlayerId = 1;
            const ulong secondPlayerId = 3;
            const ulong firstTeamId = 11;
            const ulong secondTeamId = 12;

            GameState gameState = new GameState();
            Assert.IsTrue(
                gameState.AddPlayer(firstPlayerId, "Player1", firstTeamId), "Add should succeed the first time");
            gameState.ScorePlayer(10);

            Assert.IsTrue(
                gameState.AddPlayer(secondPlayerId, "Player3", secondTeamId), "Add should succeed the third time");
            gameState.ScorePlayer(15);

            IDictionary<PlayerTeamPair, ScoringSplitOnScoreAction> lastSplits = gameState.GetLastScoringSplits();
            PlayerTeamPair firstPair = new PlayerTeamPair(firstPlayerId, firstTeamId);
            Assert.IsTrue(
                lastSplits.TryGetValue(firstPair, out ScoringSplitOnScoreAction split),
                "Couldn't find split for the first player");
            Assert.AreEqual(10, split.Split.Points, "Unexpected score for the first player");
            Assert.AreEqual(firstTeamId, split.Action.Buzz.TeamId, "Unexpected team ID for the first player's buzz");

            PlayerTeamPair secondPair = new PlayerTeamPair(secondPlayerId, secondTeamId);
            Assert.IsTrue(
                lastSplits.TryGetValue(secondPair, out split), "Couldn't find split for the second player");
            Assert.AreEqual(15, split.Split.Points, "Unexpected score for the second player");
            Assert.AreEqual(secondTeamId, split.Action.Buzz.TeamId, "Unexpected team ID for the second player's buzz");
        }

        [TestMethod]
        public void UndoOnNoScoreDoesNothing()
        {
            const ulong firstId = 1;

            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "Add should succeed.");
            Assert.IsFalse(gameState.Undo(out _), "Undo should return false.");
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                "We should still have a player in the buzz queue.");
            Assert.AreEqual(firstId, nextPlayerId, "Next player should be the first one.");
        }

        [TestMethod]
        public void UndoNeggedQuestion()
        {
            TestUndoRestoresState(-5);
        }

        [TestMethod]
        public void UndoNoPenaltyQuestion()
        {
            TestUndoRestoresState(0);
        }

        [TestMethod]
        public void UndoCorrectQuestion()
        {
            TestUndoRestoresState(10);
        }

        [TestMethod]
        public void UndoNeggedQuestionWithTeams()
        {
            TestUndoRestoresStateWithTeams(-5);
        }

        [TestMethod]
        public void UndoNoPenaltyQuestionWithTeams()
        {
            TestUndoRestoresStateWithTeams(0);
        }

        [TestMethod]
        public void UndoCorrectQuestionWithTeams()
        {
            TestUndoRestoresStateWithTeams(10);
        }

        [TestMethod]
        public void UndoPersistsBetweenQuestions()
        {
            const ulong firstId = 1;
            const ulong secondId = 2;

            GameState gameState = new GameState();
            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "First add should succeed.");
            Assert.IsTrue(gameState.AddPlayer(secondId, "Player2"), "Second add should succeed.");

            gameState.ScorePlayer(10);
            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "First add in second question should succeed.");
            gameState.ScorePlayer(15);

            Assert.IsTrue(gameState.Undo(out ulong firstUndoId), "First undo should succeed.");
            Assert.AreEqual(firstId, firstUndoId, "First ID returned by undo is incorrect.");
            Assert.IsTrue(gameState.Undo(out ulong secondUndoId), "Second undo should succeed.");
            Assert.AreEqual(firstId, secondUndoId, "Second ID returned by undo is incorrect.");

            gameState.ScorePlayer(-5);
            Assert.IsTrue(gameState.TryGetNextPlayer(out ulong nextPlayerId), "There should be a player in the queue.");
            Assert.AreEqual(secondId, nextPlayerId, "Wrong player in queue.");
        }

        [TestMethod]
        public void UndoAndWithdrawPromptsNextPlayerOnTeam()
        {
            const ulong firstUserId = 1;
            const ulong secondUserId = 2;
            const ulong teamId = 1212;
            GameState gameState = new GameState();

            Assert.IsTrue(
                gameState.AddPlayer(firstUserId, $"Player {firstUserId}", teamId),
                "Adding the first player should succeed.");
            Assert.IsTrue(
                gameState.AddPlayer(secondUserId, $"Player {secondUserId}", teamId),
                "Adding the second player should succeed.");
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                "There should be a player in the queue.");
            Assert.AreEqual(firstUserId, nextPlayerId, "Id of the next player should be ours.");
            gameState.ScorePlayer(-5);

            Assert.IsFalse(
                gameState.TryGetNextPlayer(out _),
                "We shouldn't get any other players in the queue since they're on the same team");
            Assert.IsTrue(gameState.Undo(out nextPlayerId), "Undo should've succeeded");
            Assert.AreEqual(firstUserId, nextPlayerId, "Player returned by Undo should be the first one");

            Assert.IsTrue(gameState.WithdrawPlayer(firstUserId), "Withdrawing the first player should succeed.");
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out nextPlayerId), "There should be another player in the queue.");
            Assert.AreEqual(secondUserId, nextPlayerId, "Second player should be prompted");
        }

        public static void TestUndoRestoresState(int pointsFromBuzz)
        {
            const ulong firstId = 1;
            const ulong secondId = 2;
            const int firstPointsFromBuzz = 10;

            GameState gameState = new GameState();
            // To make sure we're not just clearing the field, give the first player points
            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "First add should succeed.");
            gameState.ScorePlayer(firstPointsFromBuzz);

            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1"), "First add in second question should succeed.");
            Assert.IsTrue(gameState.AddPlayer(secondId, "Player2"), "Second add in second question should succeed.");

            gameState.ScorePlayer(pointsFromBuzz);
            IDictionary<ulong, int> scores = gameState.GetLastScoringSplits()
                .ToDictionary(lastSplitPair => lastSplitPair.Key.PlayerId,
                lastSplitPair => lastSplitPair.Value.Split.Points);
            Assert.IsTrue(scores.TryGetValue(firstId, out int score), "Unable to get score for the first player.");
            Assert.AreEqual(pointsFromBuzz + firstPointsFromBuzz, score, "Incorrect score.");

            Assert.IsTrue(gameState.Undo(out ulong id), "Undo should return true.");
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                "We should still have a player in the buzz queue.");
            Assert.AreEqual(firstId, nextPlayerId, "Next player should be the first one.");

            scores = gameState.GetLastScoringSplits()
                .ToDictionary(lastSplitPair => lastSplitPair.Key.PlayerId,
                lastSplitPair => lastSplitPair.Value.Split.Points);
            Assert.IsTrue(
                scores.TryGetValue(firstId, out int scoreAfterUndo),
                "Unable to get score for the first player after undo.");
            Assert.AreEqual(firstPointsFromBuzz, scoreAfterUndo, "Incorrect score after undo.");

            Assert.IsFalse(
                gameState.AddPlayer(firstId, "Player1"),
                "First player already buzzed, so we shouldn't be able to add them again.");
            Assert.IsFalse(
                gameState.AddPlayer(secondId, "Player2"),
                "Second player already buzzed, so we shouldn't be able to add them again.");

            gameState.ScorePlayer(0);
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong finalPlayerId),
                "Buzz queue should have two players after an undo.");
            Assert.AreEqual(secondId, finalPlayerId, "Next player should be the second one.");
        }

        public static void TestUndoRestoresStateWithTeams(int pointsFromBuzz)
        {
            const ulong firstId = 1;
            const ulong secondId = 2;
            const ulong firstTeamId = 1001;
            const ulong secondTeamId = 1002;
            const int firstPointsFromBuzz = 10;

            GameState gameState = new GameState();
            // To make sure we're not just clearing the field, give the first player points
            Assert.IsTrue(gameState.AddPlayer(firstId, "Player1", firstTeamId), "First add should succeed.");
            gameState.ScorePlayer(firstPointsFromBuzz);

            Assert.IsTrue(
                gameState.AddPlayer(firstId, "Player1", firstTeamId), "First add in second question should succeed.");
            Assert.IsTrue(
                gameState.AddPlayer(secondId, "Player2", secondTeamId), "Second add in second question should succeed.");

            gameState.ScorePlayer(pointsFromBuzz);
            IDictionary<ulong, int> scores = gameState.GetLastScoringSplits()
                .ToDictionary(lastSplitPair => lastSplitPair.Key.PlayerId,
                    lastSplitPair => lastSplitPair.Value.Split.Points);
            Assert.IsTrue(scores.TryGetValue(firstId, out int score), "Unable to get score for the first player.");
            Assert.AreEqual(pointsFromBuzz + firstPointsFromBuzz, score, "Incorrect score.");

            Assert.IsTrue(gameState.Undo(out ulong id), "Undo should return true.");
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong nextPlayerId),
                "We should still have a player in the buzz queue.");
            Assert.AreEqual(firstId, nextPlayerId, "Next player should be the first one.");

            scores = gameState.GetLastScoringSplits()
                .ToDictionary(lastSplitPair => lastSplitPair.Key.PlayerId,
                    lastSplitPair => lastSplitPair.Value.Split.Points);
            Assert.IsTrue(
                scores.TryGetValue(firstId, out int scoreAfterUndo),
                "Unable to get score for the first player after undo.");
            Assert.AreEqual(firstPointsFromBuzz, scoreAfterUndo, "Incorrect score after undo.");

            Assert.IsFalse(
                gameState.AddPlayer(firstId, "Player1", firstTeamId),
                "First player already buzzed, so we shouldn't be able to add them again.");
            Assert.IsFalse(
                gameState.AddPlayer(secondId, "Player2", secondTeamId),
                "Second player already buzzed, so we shouldn't be able to add them again.");

            gameState.ScorePlayer(0);
            Assert.IsTrue(
                gameState.TryGetNextPlayer(out ulong finalPlayerId),
                "Buzz queue should have two players after an undo.");
            Assert.AreEqual(secondId, finalPlayerId, "Next player should be the second one.");
        }

        // TODO: Add tests for Bot. We'd want to create another class that implements the event handlers, but has different arguments
        // which don't require Discord-specific classes.
    }
}
