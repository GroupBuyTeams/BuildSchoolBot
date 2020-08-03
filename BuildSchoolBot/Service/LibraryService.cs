using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace BuildSchoolBot.Service
{
    public class LibraryService
    {
        private TeamsBuyContext _context;
        private EGRepository<Library> _repo;
        public LibraryService(TeamsBuyContext context, EGRepository<Library> repo)
        {
            _context = context;
            _repo = repo;
        }
        public void CreateLibraryItem(Guid memberId, string Uri, string libraryName)
        {
            var entity = new Library()
            {
                LibraryId = Guid.NewGuid(),
                Uri = Uri,
                MemberId = memberId,
                LibraryName = libraryName
            };

            _repo.Create(entity);
            _context.SaveChanges();
        }
        public void DeleteLibraryItem(Guid libraryId)
        {

            var entity = _repo.GetAll().FirstOrDefault(x => x.LibraryId.Equals(libraryId));

            _repo.Delete(entity);
            _context.SaveChanges();
        }
        public async Task<List<Library>> FindLibraryByMemberId(Guid memberId)
        {
            var result = _repo.GetAll().Where(x => x.MemberId == memberId).ToList();
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
            var card = AdaptiveCards.AdaptiveCard.FromJson(libraryCardJson).Card;
            library.ForEach(item =>
            {
                objItem.columns[1].items[0].text.Value = item.LibraryName;
                objItem.columns[1].items[1].text.Value = item.Uri;
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