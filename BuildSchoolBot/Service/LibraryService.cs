using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildSchoolBot.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace BuildSchoolBot.Service
{
    public class LibraryService
    {
        private TeamsBuyContext _db;
        public LibraryService(TeamsBuyContext db)
        {
            _db = db;
        }
        public void CreateLibraryItem(Guid memberId, string Uri)
        {
            var entity = new Library()
            {
                LibraryId = Guid.NewGuid(),
                Uri = Uri,
                MemberId = memberId
            };

            _db.Library.Add(entity);
            _db.SaveChanges();
        }
        public void DeleteLibraryItem(Guid libraryId)
        {

            var entity = _db.Library.FirstOrDefault(x => x.LibraryId.Equals(libraryId));

            _db.Library.Remove(entity);
            _db.SaveChanges();
        }
        public async Task<List<Library>> FindLibraryByMemberId(Guid memberId)
        {
            var result = _db.Library.Where(x => x.MemberId == memberId).ToList();
            return await Task.FromResult(result);
        }
        public static Attachment CreateAdaptiveCardAttachment(List<Library> library)
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "LibraryCard.json" };
            var pathsItem = new[] { ".", "Resources", "LibraryCardItem.json" };
            var libraryCardJson = File.ReadAllText(Path.Combine(paths));
            var libraryCardItemJson = File.ReadAllText(Path.Combine(pathsItem));
            var obj = JsonConvert.DeserializeObject<dynamic>(libraryCardJson);
            var objItem = JsonConvert.DeserializeObject<dynamic>(libraryCardItemJson);

            library.ForEach(item =>
            {
                objItem.columns[1].items[0].text.Value = item.Uri;
                objItem.columns[2].items[0].actions[0].data.msteams.value.Value = "{\"LibraryId\":\"" + item.LibraryId + "\"}";
                obj.body.Add(objItem);
            });

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = obj
            };

            return adaptiveCardAttachment;
        }

    }
}