namespace Sitecore.ItemBucket.Kernel.Kernel.Search.DidYouMean
{
    using System.Collections.Generic;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Search;

    public class DidYouMean
    {
        private static List<string> SuggestSimilar(string term)
        {
            var similarWordsList = new List<string>();
            using (var context = new SortableIndexSearchContext(SearchManager.GetIndex(Constants.Index.Name)))
            {
                // create the spell checker
                /*var spell = new SpellChecker.Net.Search.Spell.SpellChecker(context.Searcher.GetIndexReader().Directory());

                // get 2 similar words
                string[] similarWords = spell.SuggestSimilar(term, 2);

                // show the similar words
                for (int wordIndex = 0; wordIndex < similarWords.Length; wordIndex++)
                {
                    similarWordsList.Add(similarWords[wordIndex]);
                }*/
            }
            return similarWordsList;
        }

        public static List<string> RequestSimilarWord(string originalWord)
        {
            return SuggestSimilar(originalWord);
        }
    }
}
