// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.ScholarlyWorks
{
    internal static class MaxLengths
    {
        // Person.
        internal const int lastName = 256;
        internal const int firstName = 256;
        internal const int middleName = 256;

        // Book
        internal const int changeHistory = 2147483647;//max
        internal const int isbn = 256;

        // Tag
        internal const int name = 50;

        // Email
        internal const int subject = 256;

        // Audio
        internal const int codec = 4000;
        internal const int mode = 4000;

        // Download
        internal const int versionInformation = 256;
        internal const int copyright = 4000;
        internal const int hardwareRequirements = 4000;
        internal const int eula = 1024;
        internal const int operatingSystem = 4000;
        internal const int downloadRequirements = 4000;
        internal const int systemRequirements = 4000;
        internal const int language = 128;
        internal const int license = 4000;

        // Code
        internal const int programmingLanguage = 256;
        internal const int technology = 256;

        // Lecture
        internal const int series = 256;
        internal const int audience = 4000;
        internal const int venue = 1024;
        
        // Video
        internal const int resolution = 256;
        internal const int director = 256;
        internal const int aspectRatio = 64;
        internal const int scanningMethod = 256;
        internal const int colorModel = 64;

        // Email
        internal const int email = 2048;

        // Journal
        internal const int journalName = 256;

        // ScholarlyWork
        internal const int notes = 2147483647;//max
        internal const int scholarlyWorkAbstract = 2147483647;//max

        // Proceedings
        internal const int eventName = 1024;

        // PersonalCommunication
        internal const int from = 1024;
        internal const int to = 4000;

        // ScholarlyWorkItem
        internal const int scope = 128;

        // Experiment
        internal const int status = 4000;
        internal const int report = 2147483647;//max
        internal const int plan = 2147483647;//max
        internal const int experimentName = 1024;

        // ElectronicSource
        internal const int reference = 4000;

        // JournalArticle
        internal const int journal = 256;

        // Publication
        internal const int doi = 256;
        internal const int pages = 1024;
        internal const int institution = 1024;
        internal const int publisherAddress = 1024;
        internal const int location = 1024;
        internal const int bookTitle = 256;
        internal const int volume = 256;
        internal const int catalogNumber = 1024;
        internal const int publisherUri = 1024;
        internal const int chapter = 256;
        internal const int number = 256;
        internal const int publisher = 256;
        internal const int edition = 256;
        internal const int organization = 1024;

        // TechnicalReport
        internal const int category = 256;
    }
}
