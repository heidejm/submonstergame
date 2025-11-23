using System;
using System.Collections.Generic;

namespace SubGame.Core.AI
{
    /// <summary>
    /// Represents a deck of behavior cards for a monster type.
    /// Cards can appear multiple times to weight probability.
    /// </summary>
    public class AIDeck
    {
        private readonly List<BehaviorCard> _cards = new List<BehaviorCard>();
        private readonly Random _random;

        /// <summary>
        /// Gets all cards in the deck.
        /// </summary>
        public IReadOnlyList<BehaviorCard> Cards => _cards;

        /// <summary>
        /// Gets the number of cards in the deck.
        /// </summary>
        public int Count => _cards.Count;

        /// <summary>
        /// Creates a new AI deck.
        /// </summary>
        /// <param name="random">Random number generator (null for default)</param>
        public AIDeck(Random random = null)
        {
            _random = random ?? new Random();
        }

        /// <summary>
        /// Adds a card to the deck.
        /// </summary>
        /// <param name="card">The card to add</param>
        /// <param name="copies">Number of copies to add (affects draw probability)</param>
        public void AddCard(BehaviorCard card, int copies = 1)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            for (int i = 0; i < copies; i++)
            {
                _cards.Add(card);
            }
        }

        /// <summary>
        /// Draws a random card from the deck.
        /// </summary>
        /// <returns>A randomly selected card, or null if deck is empty</returns>
        public BehaviorCard DrawCard()
        {
            if (_cards.Count == 0)
                return null;

            int index = _random.Next(_cards.Count);
            return _cards[index];
        }

        /// <summary>
        /// Removes all cards from the deck.
        /// </summary>
        public void Clear()
        {
            _cards.Clear();
        }
    }

    /// <summary>
    /// Builder for creating AI decks with a fluent API.
    /// </summary>
    public class AIDeckBuilder
    {
        private readonly AIDeck _deck;

        public AIDeckBuilder(Random random = null)
        {
            _deck = new AIDeck(random);
        }

        /// <summary>
        /// Adds a card to the deck.
        /// </summary>
        /// <param name="card">The card to add</param>
        /// <param name="copies">Number of copies (affects probability)</param>
        public AIDeckBuilder WithCard(BehaviorCard card, int copies = 1)
        {
            _deck.AddCard(card, copies);
            return this;
        }

        /// <summary>
        /// Builds and returns the deck.
        /// </summary>
        public AIDeck Build()
        {
            return _deck;
        }
    }
}
