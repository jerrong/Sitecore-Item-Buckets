namespace Sitecore.ItemBucket.Kernel.Util
{
    public class SearchField
    {
        public SearchField()
        {
        }

        public SearchField(string storageType, string indexType, string vectorType, string boost)
        {
            this.SetStorageType(storageType);
            this.SetIndexType(indexType);
            this.SetVectorType(vectorType);
            this.SetBoost(boost);
        }

        #region Public Properties

        public Lucene.Net.Documents.Field.Store StorageType
        {
            get; set;
        }

        public Lucene.Net.Documents.Field.Index IndexType
        {
            get; set;
        }

        public Lucene.Net.Documents.Field.TermVector VectorType
        {
            get; set;
        }

        public float Boost
        {
            get; set;
        }

        #endregion

        #region Setters

        public void SetStorageType(string storageType)
        {
            switch (storageType)
            {
                case "YES":
                    {
                        this.StorageType = Lucene.Net.Documents.Field.Store.YES;
                        break;
                    }

                case "NO":
                    {
                        this.StorageType = Lucene.Net.Documents.Field.Store.NO;
                        break;
                    }

                case "COMPRESS":
                    {
                        this.StorageType = Lucene.Net.Documents.Field.Store.COMPRESS;
                        break;
                    }

                default:
                    break;
            }
        }

        public void SetIndexType(string indexType)
        {
            switch (indexType)
            {
                case "NO":
                    {
                        this.IndexType = Lucene.Net.Documents.Field.Index.NO;
                        break;
                    }

                case "NO_NORMS":
                    {
                        this.IndexType = Lucene.Net.Documents.Field.Index.NO_NORMS;
                        break;
                    }

                case "TOKENIZED":
                    {
                        this.IndexType = Lucene.Net.Documents.Field.Index.TOKENIZED;
                        break;
                    }

                case "UN_TOKENIZED":
                    {
                        this.IndexType = Lucene.Net.Documents.Field.Index.UN_TOKENIZED;
                        break;
                    }

                default:
                    break;
            }
        }

        public void SetVectorType(string vectorType)
        {
            switch (vectorType)
            {
                case "NO":
                    {
                        this.VectorType = Lucene.Net.Documents.Field.TermVector.NO;
                        break;
                    }

                case "WITH_OFFSETS":
                    {
                        this.VectorType = Lucene.Net.Documents.Field.TermVector.WITH_OFFSETS;
                        break;
                    }

                case "WITH_POSITIONS":
                    {
                        this.VectorType = Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS;
                        break;
                    }

                case "WITH_POSITIONS_OFFSETS":
                    {
                        this.VectorType = Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS;
                        break;
                    }

                case "YES":
                    {
                        this.VectorType = Lucene.Net.Documents.Field.TermVector.YES;
                        break;
                    }

                default:
                    break;
            }
        }

        public void SetBoost(string boost)
        {
            float boostReturn;

            if (float.TryParse(boost, out boostReturn))
            {
                this.Boost = boostReturn;
            }

            this.Boost = 1;
        }

        #endregion
    }
}