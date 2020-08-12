using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        //use memberId to find the library
        public async Task<List<Library>> FindLibraryByMemberId(string memberId)
        {
            var result = _repo.GetAll().Where(x => x.MemberId.Equals(memberId)).ToList();
            return await Task.FromResult(result);
        }
        public async Task<List<Library>> FindLibraryByUriAndMemberId(string uri, string memberId)
        {
            var result = _repo.GetAll().Where(x => x.MemberId.Equals(memberId) && x.Uri.Equals(uri)).ToList();
            return await Task.FromResult(result);
        }
        public async void LibraryCreateOrDelete(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var memberId = turnContext.Activity.From.Id;
            var obj = JObject.FromObject(turnContext.Activity.Value).ToObject<ViewModels.MsteamsValue>();

            if (obj.Option.Equals("Create"))
            {
                var uri = obj.Url;
                var LibraryItem = await FindLibraryByUriAndMemberId(uri, memberId);

                if (LibraryItem.Count.Equals(0))
                    CreateLibraryItem(memberId, obj.Url, obj.Name);
            }
            else if (obj.Option.Equals("Delete"))
            {
                var LibraryId = obj.LibraryId;

                Guid guid;
                Guid.TryParse(LibraryId.ToString(), out guid);
                DeleteLibraryItem(guid);

                var libraryCard = await GetLibraryCard(turnContext);

                var activity = MessageFactory.Attachment(libraryCard);
                activity.Id = turnContext.Activity.ReplyToId;

                await turnContext.UpdateActivityAsync(activity, cancellationToken);
            }
        }
        public async Task<Attachment> GetLibraryCard(ITurnContext turnContext)
        {
            var memberId = turnContext.Activity.From.Id;

            var Name = turnContext.Activity.From.Name;
            var libraries = await FindLibraryByMemberId(memberId);
            var libraryCard = Service.LibraryService.CreateAdaptiveCardAttachment(libraries, Name);

            return libraryCard;
        }
        public static Attachment CreateAdaptiveCardAttachment(List<Library> library, string Name)
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "LibraryCard.json" };
            var pathsItem = new[] { ".", "Resources", "LibraryCardItem.json" };

            var libraryCardJson = File.ReadAllText(Path.Combine(paths));
            var libraryCardItemJson = File.ReadAllText(Path.Combine(pathsItem));

            var myCard = JsonConvert.DeserializeObject<AdaptiveCard>(libraryCardJson);

            ((AdaptiveTextBlock)((AdaptiveColumnSet)myCard.Body[0]).Columns[1].Items[0]).Text = Name;


            library.ForEach(item =>
            {

                var columnSet = JsonConvert.DeserializeObject<AdaptiveColumnSet>(libraryCardItemJson);
                myCard.Body.Add(columnSet);
                ((AdaptiveTextBlock)columnSet.Columns[1].Items[0]).Text = item.LibraryName;
                ((AdaptiveTextBlock)columnSet.Columns[1].Items[1]).Text = item.Uri;
                ((AdaptiveSubmitAction)((AdaptiveActionSet)columnSet.Columns[2].Items[0]).Actions[0]).Data = new Data()
                {
                    msteams = new Msteams()
                    {
                        type = "invoke",
                        value = new MsteamsValue()
                        {
                            Name = item.LibraryName,
                            Url = item.Uri,
                            Option = "Delete",
                            LibraryId = item.LibraryId
                        }
                    }
                };
            });
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = myCard
            };

            return adaptiveCardAttachment;
        }

    }
}