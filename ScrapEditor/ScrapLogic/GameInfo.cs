using System;
using System.Collections.Generic;

namespace ScrapEditor.ScrapLogic
{
    public class GameInfo
    {
        public string Id { get; set; }
        public long InternalId { get; set; }
        public string Console { get; set; }
        public List<RegionalInfo<string>> Names { get; set; }
        public string Editor { get; set; }
        public string Developer { get; set; }
        public string NbPlayers { get; set; }
        public string Classification { get; set; }
        public string Genres { get; set; }
        public List<RegionalInfo<string>> ReleaseDate { get; set; }
        public string Style { get; set; }
        public List<RegionalInfo<string>> Description { get; set; }
        public List<RegionalInfo<GameImage>> Images { get; set; }
        public DateTime ScrapDate { get; set; }
        public DateTime LastEditTime { get; set; }
        public string Link { get; set; }
        public string Provider { get; set; }
    }

    public class Edit
    {
        public string Username { get; set; }
        
        public string FieldName { get; set; }
        
        public DateTime EditDate { get; set; }
        
        public string OldValue { get; set; }
        
        public string NewValue { get; set; }
    }
    public class Game
    {
        public string Id { get; set; }
        public Game()
        {
            ScrapInfos = new List<GameInfo>();
            Edits = new List<Edit>();
        }
        public long ScrapEditorId { get; set; }
        public List<GameInfo> ScrapInfos { get; set; }
        public GameInfo SavedInfo { get; set; }
        public bool IsUploadedToScreenScraper { get; set; }
        public long ScreenScraperId { get; set; }
        public List<Edit> Edits { get; set; }
    }
}