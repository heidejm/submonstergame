using System;
using System.Collections.Generic;
using SubGame.Core.AI.Actions;
using SubGame.Core.AI.Conditions;
using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI
{
    /// <summary>
    /// Controls monster AI behavior by managing decks and executing turns.
    /// </summary>
    public class MonsterAIController
    {
        private readonly Dictionary<Guid, AIDeck> _monsterDecks = new Dictionary<Guid, AIDeck>();
        private readonly AIContext _context = new AIContext();
        private readonly Random _random;

        /// <summary>
        /// Default deck used when a monster doesn't have a specific deck assigned.
        /// </summary>
        public AIDeck DefaultDeck { get; set; }

        /// <summary>
        /// Event fired when a card is drawn.
        /// Parameters: monster, card drawn
        /// </summary>
        public event Action<IEntity, BehaviorCard> OnCardDrawn;

        /// <summary>
        /// Event fired when a monster completes its AI turn.
        /// Parameters: monster
        /// </summary>
        public event Action<IEntity> OnTurnComplete;

        /// <summary>
        /// Creates a new MonsterAIController.
        /// </summary>
        /// <param name="random">Random number generator (null for default)</param>
        public MonsterAIController(Random random = null)
        {
            _random = random ?? new Random();
        }

        /// <summary>
        /// Assigns a specific deck to a monster.
        /// </summary>
        /// <param name="monsterId">ID of the monster</param>
        /// <param name="deck">Deck to assign</param>
        public void AssignDeck(Guid monsterId, AIDeck deck)
        {
            _monsterDecks[monsterId] = deck;
        }

        /// <summary>
        /// Gets the deck for a monster (returns default if none assigned).
        /// </summary>
        /// <param name="monsterId">ID of the monster</param>
        /// <returns>The monster's deck or the default deck</returns>
        public AIDeck GetDeck(Guid monsterId)
        {
            return _monsterDecks.TryGetValue(monsterId, out var deck) ? deck : DefaultDeck;
        }

        /// <summary>
        /// Executes an AI turn for a monster.
        /// Draws a card and executes its behavior.
        /// </summary>
        /// <param name="monster">The monster taking its turn</param>
        /// <param name="state">Current game state</param>
        /// <returns>True if any action was taken</returns>
        public bool ExecuteTurn(IEntity monster, IGameState state)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            // Get the deck for this monster
            var deck = GetDeck(monster.Id);
            if (deck == null || deck.Count == 0)
            {
                // No deck, monster does nothing
                OnTurnComplete?.Invoke(monster);
                return false;
            }

            // Reset context for this turn
            _context.Reset();

            // Draw a card
            var card = deck.DrawCard();
            if (card == null)
            {
                OnTurnComplete?.Invoke(monster);
                return false;
            }

            OnCardDrawn?.Invoke(monster, card);

            // Execute the card's behavior
            bool actionTaken = card.Execute(state, monster, _context);

            OnTurnComplete?.Invoke(monster);

            return actionTaken;
        }

        /// <summary>
        /// Creates a default aggressive deck for testing.
        /// </summary>
        /// <returns>A basic aggressive behavior deck</returns>
        public static AIDeck CreateDefaultAggressiveDeck(Random random = null)
        {
            var deck = new AIDeck(random);

            // Card 1: "Lunge Attack" - Attack if in range, else move toward target
            var lungeAttack = new BehaviorCard(
                "Lunge Attack",
                new List<ConditionalBranch>
                {
                    // IF target in attack range THEN attack
                    new ConditionalBranch(
                        new TargetInAttackRangeCondition(TargetSelector.Nearest),
                        new AttackAction()
                    ),
                    // ELSE IF can reach target THEN move toward it
                    new ConditionalBranch(
                        new CanReachTargetCondition(TargetSelector.Nearest),
                        new MoveTowardTargetAction()
                    )
                },
                new List<IAIAction> { new IdleAction() }, // Fallback: idle
                "Aggressively pursue and attack the nearest target"
            );

            // Card 2: "Frenzy" - Attack if in range (no movement)
            var frenzy = new BehaviorCard(
                "Frenzy",
                new List<ConditionalBranch>
                {
                    new ConditionalBranch(
                        new TargetInAttackRangeCondition(TargetSelector.Nearest),
                        new AttackAction()
                    )
                },
                new List<IAIAction> { new IdleAction() },
                "Attack if possible, otherwise do nothing"
            );

            // Card 3: "Stalk" - Move toward target (no attack this turn)
            var stalk = new BehaviorCard(
                "Stalk",
                new List<ConditionalBranch>
                {
                    new ConditionalBranch(
                        new CanReachTargetCondition(TargetSelector.Nearest),
                        new MoveTowardTargetAction()
                    )
                },
                new List<IAIAction> { new IdleAction() },
                "Move closer to prey without attacking"
            );

            // Add cards to deck with different weights
            deck.AddCard(lungeAttack, 3);  // Most common
            deck.AddCard(frenzy, 2);
            deck.AddCard(stalk, 1);

            return deck;
        }
    }
}
