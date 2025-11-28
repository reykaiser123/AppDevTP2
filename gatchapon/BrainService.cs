using Firebase.Database;
using Firebase.Database.Query;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

namespace gatchapon
{
    public class BrainService
    {
        private readonly FirebaseClient _firebaseClient;
        private List<QuestionItem> _localKnowledgeBase = new List<QuestionItem>();
        // 🛑 ADD: List of terms to filter against
        private readonly List<string> _forbiddenWords = new List<string>
       {
        "badword1", "swearword2", "inappropriatephrase3" // Replace with actual profanity/harsh terms
        // Ensure this list is lowercase for case-insensitive checking
    };
        public BrainService()
        {
            _firebaseClient = new FirebaseClient("https://gatchapon-d7cd9-default-rtdb.firebaseio.com/");
            ListenForKnowledgeUpdates();
        }

        private void ListenForKnowledgeUpdates()
        {
            _firebaseClient
                .Child("knowledge_base")
                .AsObservable<QuestionItem>()
                .Subscribe(d =>
                {
                    if (d.Object != null)
                    {
                        var existing = _localKnowledgeBase.FirstOrDefault(q => q.Question == d.Object.Question);
                        if (existing == null)
                        {
                            _localKnowledgeBase.Add(d.Object);
                        }
                    }
                });
        }
        private bool ContainsInappropriateContent(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            string lowerText = text.ToLower();

            // Check if the input contains any of the forbidden phrases
            return _forbiddenWords.Any(word => lowerText.Contains(word));
        }

        // --- UPDATED: GET ANSWER WITH CHARACTER FILTERING ---
        // charName is added here to enable personality filtering
        public async Task<string> GetAnswerAsync(string userQuestion, string charName)
        {
            if (_localKnowledgeBase.Count == 0)
            {
                var items = await _firebaseClient.Child("knowledge_base").OnceAsync<QuestionItem>();
                foreach (var i in items) _localKnowledgeBase.Add(i.Object);
            }

            // FILTER: Select knowledge specific to this character OR general knowledge
            var relevantKnowledge = _localKnowledgeBase
                .Where(k =>
                    string.IsNullOrEmpty(k.Character) || // General knowledge
                    string.Equals(k.Character, charName, StringComparison.OrdinalIgnoreCase) // Character specific
                );

            // Find the best match from the RELEVANT subset
            var bestMatch = relevantKnowledge
                .Select(k => new { Item = k, Score = CalculateSimilarity(userQuestion.ToLower(), k.Question.ToLower()) })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (bestMatch != null && bestMatch.Score > 0.6)
            {
                return bestMatch.Item.Answer;
            }

            return null;
        }

        // --- UPDATED: TEACH WITH CHARACTER TAGGING ---
        // charName is added here to tag the owner of the new knowledge
        // --- UPDATED: TEACH WITH CHARACTER TAGGING AND FILTERING ---
        public async Task<bool> TeachAsync(string question, string answer, string charName) // Changed return type to bool
        {
            if (ContainsInappropriateContent(question) || ContainsInappropriateContent(answer))
            {
                // Return false if filtering failed
                return false;
            }

            // If content is clean, proceed with learning
            var newItem = new QuestionItem { Question = question, Answer = answer, Character = charName };
            await _firebaseClient.Child("knowledge_base").PostAsync(newItem);

            // Return true if learning was successful
            return true;
        }
        // --- MATH: (Levenshtein Distance calculation methods remain the same) ---
        private double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return 1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length));
        }

        private int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0) return m;
            if (m == 0) return n;
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }

    // --- ESSENTIAL MODEL (Now requires the Character tag) ---
    // If you define this here, you must delete it from your Models folder!
    public class QuestionItem
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Character { get; set; } // Added for personality
    }
}