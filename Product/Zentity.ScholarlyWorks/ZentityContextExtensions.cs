// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using Zentity.Core;
using System.Data.Objects;

namespace Zentity.ScholarlyWorks
{
    /// <summary>
    /// The class to host extension methods for <see cref="Zentity.Core.ZentityContext"/> class.
    /// </summary>
    public static class ZentityContextExtensions
    {
        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Audio"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Audio"/> objects.</returns>
        public static ObjectQuery<Audio> Audios(this ZentityContext context)
        {
            return context.Resources.OfType<Audio>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Book"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Book"/> objects.</returns>
        public static ObjectQuery<Book> Books(this ZentityContext context)
        {
            return context.Resources.OfType<Book>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Booklet"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Booklet"/> objects.</returns>
        public static ObjectQuery<Booklet> Booklets(this ZentityContext context)
        {
            return context.Resources.OfType<Booklet>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.CategoryNode"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.CategoryNode"/> objects.</returns>
        public static ObjectQuery<CategoryNode> CategoryNodes(this ZentityContext context)
        {
            return context.Resources.OfType<CategoryNode>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Chapter"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Chapter"/> objects.</returns>
        public static ObjectQuery<Chapter> Chapters(this ZentityContext context)
        {
            return context.Resources.OfType<Chapter>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Code"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Code"/> objects.</returns>
        public static ObjectQuery<Code> AllCode(this ZentityContext context)
        {
            return context.Resources.OfType<Code>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Contact"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Contact"/> objects.</returns>
        public static ObjectQuery<Contact> Contacts(this ZentityContext context)
        {
            return context.Resources.OfType<Contact>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Data"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Data"/> objects.</returns>
        public static ObjectQuery<Data> AllData(this ZentityContext context)
        {
            return context.Resources.OfType<Data>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Download"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Download"/> objects.</returns>
        public static ObjectQuery<Download> Downloads(this ZentityContext context)
        {
            return context.Resources.OfType<Download>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.ElectronicSource"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.ElectronicSource"/> objects.</returns>
        public static ObjectQuery<ElectronicSource> ElectronicSources(this ZentityContext context)
        {
            return context.Resources.OfType<ElectronicSource>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Email"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Email"/> objects.</returns>
        public static ObjectQuery<Email> Emails(this ZentityContext context)
        {
            return context.Resources.OfType<Email>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Experiment"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Experiment"/> objects.</returns>
        public static ObjectQuery<Experiment> Experiments(this ZentityContext context)
        {
            return context.Resources.OfType<Experiment>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Image"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Image"/> objects.</returns>
        public static ObjectQuery<Image> Images(this ZentityContext context)
        {
            return context.Resources.OfType<Image>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Journal"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Journal"/> objects.</returns>
        public static ObjectQuery<Journal> Journals(this ZentityContext context)
        {
            return context.Resources.OfType<Journal>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.JournalArticle"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.JournalArticle"/> objects.</returns>
        public static ObjectQuery<JournalArticle> JournalArticles(this ZentityContext context)
        {
            return context.Resources.OfType<JournalArticle>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Lecture"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Lecture"/> objects.</returns>
        public static ObjectQuery<Lecture> Lectures(this ZentityContext context)
        {
            return context.Resources.OfType<Lecture>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Letter"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Letter"/> objects.</returns>
        public static ObjectQuery<Letter> Letters(this ZentityContext context)
        {
            return context.Resources.OfType<Letter>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Manual"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Manual"/> objects.</returns>
        public static ObjectQuery<Manual> Manuals(this ZentityContext context)
        {
            return context.Resources.OfType<Manual>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Media"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Media"/> objects.</returns>
        public static ObjectQuery<Media> Medias(this ZentityContext context)
        {
            return context.Resources.OfType<Media>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Organization"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Organization"/> objects.</returns>
        public static ObjectQuery<Organization> Organizations(this ZentityContext context)
        {
            return context.Resources.OfType<Organization>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Patent"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Patent"/> objects.</returns>
        public static ObjectQuery<Patent> Patents(this ZentityContext context)
        {
            return context.Resources.OfType<Patent>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Person"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Person"/> objects.</returns>
        public static ObjectQuery<Person> People(this ZentityContext context)
        {
            return context.Resources.OfType<Person>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.PersonalCommunication"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.PersonalCommunication"/> objects.</returns>
        public static ObjectQuery<PersonalCommunication> PersonalCommunications(this ZentityContext context)
        {
            return context.Resources.OfType<PersonalCommunication>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Proceedings"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Proceedings"/> objects.</returns>
        public static ObjectQuery<Proceedings> AllProceedings(this ZentityContext context)
        {
            return context.Resources.OfType<Proceedings>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.ProceedingsArticle"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.ProceedingsArticle"/> objects.</returns>
        public static ObjectQuery<ProceedingsArticle> ProceedingsArticles(this ZentityContext context)
        {
            return context.Resources.OfType<ProceedingsArticle>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Publication"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Publication"/> objects.</returns>
        public static ObjectQuery<Publication> Publications(this ZentityContext context)
        {
            return context.Resources.OfType<Publication>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.ScholarlyWork"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.ScholarlyWork"/> objects.</returns>
        public static ObjectQuery<ScholarlyWork> ScholarlyWorks(this ZentityContext context)
        {
            return context.Resources.OfType<ScholarlyWork>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.ScholarlyWorkContainer"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.ScholarlyWorkContainer"/> objects.</returns>
        public static ObjectQuery<ScholarlyWorkContainer> ScholarlyWorkContainers(this ZentityContext context)
        {
            return context.Resources.OfType<ScholarlyWorkContainer>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.ScholarlyWorkItem"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.ScholarlyWorkItem"/> objects.</returns>
        public static ObjectQuery<ScholarlyWorkItem> ScholarlyWorkItems(this ZentityContext context)
        {
            return context.Resources.OfType<ScholarlyWorkItem>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Software"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Software"/> objects.</returns>
        public static ObjectQuery<Software> Softwares(this ZentityContext context)
        {
            return context.Resources.OfType<Software>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Tag"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Tag"/> objects.</returns>
        public static ObjectQuery<Tag> Tags(this ZentityContext context)
        {
            return context.Resources.OfType<Tag>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.TechnicalReport"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.TechnicalReport"/> objects.</returns>
        public static ObjectQuery<TechnicalReport> TechnicalReports(this ZentityContext context)
        {
            return context.Resources.OfType<TechnicalReport>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Thesis"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Thesis"/> objects.</returns>
        public static ObjectQuery<Thesis> AllThesis(this ZentityContext context)
        {
            return context.Resources.OfType<Thesis>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.ThesisMsc"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.ThesisMsc"/> objects.</returns>
        public static ObjectQuery<ThesisMsc> AllThesisMsc(this ZentityContext context)
        {
            return context.Resources.OfType<ThesisMsc>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.ThesisPhD"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.ThesisPhD"/> objects.</returns>
        public static ObjectQuery<ThesisPhD> AllThesisPhD(this ZentityContext context)
        {
            return context.Resources.OfType<ThesisPhD>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Tutorial"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Tutorial"/> objects.</returns>
        public static ObjectQuery<Tutorial> Tutorials(this ZentityContext context)
        {
            return context.Resources.OfType<Tutorial>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Unpublished"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Unpublished"/> objects.</returns>
        public static ObjectQuery<Unpublished> AllUnpublished(this ZentityContext context)
        {
            return context.Resources.OfType<Unpublished>();
        }

        /// <summary>
        /// Returns a query against the store for all <see cref="Zentity.ScholarlyWorks.Video"/> objects. 
        /// </summary>
        /// <param name="context">Context for the store.</param>
        /// <returns>Query against the store for all <see cref="Zentity.ScholarlyWorks.Video"/> objects.</returns>
        public static ObjectQuery<Video> Videos(this ZentityContext context)
        {
            return context.Resources.OfType<Video>();
        }
    }
}
