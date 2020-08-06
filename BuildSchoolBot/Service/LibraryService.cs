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
        private EGRepository<Library> _repo;
        public LibraryService(EGRepository<Library> repo)
        {
            _repo = repo;
        }
        public void CreateLibraryItem(string memberId, string Uri, string libraryName)
        {
            var entity = new Library()
            {
                LibraryId = Guid.NewGuid(),
                Uri = Uri,
                MemberId = memberId,
                LibraryName = libraryName
            };

            _repo.Create(entity);
            _repo.context.SaveChanges();
        }
        public void DeleteLibraryItem(Guid libraryId)
        {

            var entity = _repo.GetAll().FirstOrDefault(x => x.LibraryId.Equals(libraryId));

            _repo.Delete(entity);
            _repo.context.SaveChanges();
        }
        public async Task<List<Library>> FindLibraryByMemberId(string memberId)
        {
            var result = _repo.GetAll().Where(x => x.MemberId == memberId).ToList();
            return await Task.FromResult(result);
        }
        public static Attachment CreateAdaptiveCardAttachment(List<Library> library, string Name)
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "LibraryCard.json" };
            var pathsItem = new[] { ".", "Resources", "LibraryCardItem.json" };
            var libraryCardJson = File.ReadAllText(Path.Combine(paths));

            var libraryCardItemJson = File.ReadAllText(Path.Combine(pathsItem));
            var obj = JsonConvert.DeserializeObject<dynamic>(libraryCardJson);
            var objItem = JsonConvert.DeserializeObject<dynamic>(libraryCardItemJson);
            var card = AdaptiveCards.AdaptiveCard.FromJson(libraryCardJson).Card;

            obj.body[0].columns[1].items[0].text.Value = Name;

            library.ForEach(item =>
            {
                obj.body.Add(objItem);
                objItem.columns[1].items[0].text.Value = item.LibraryName;
                objItem.columns[1].items[1].text.Value = item.Uri;
                objItem.columns[2].items[0].actions[0].data.msteams.value.Value = "{\"LibraryId\":\"" + item.LibraryId + "\"}";
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